param([string]$BaseUrl = 'http://localhost:63723')
$ErrorActionPreference = 'Stop'
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
function Get-Page([string]$path) {
    Invoke-WebRequest ($BaseUrl + $path) -WebSession $session -UseBasicParsing -TimeoutSec 30
}
function Get-Token($page) {
    $match = [regex]::Match($page.Content, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"')
    if (-not $match.Success) { throw 'Missing anti-forgery token' }
    return $match.Groups[1].Value
}
$root = Get-Page '/'
if ($root.BaseResponse.ResponseUri.AbsolutePath -ne '/') { throw 'Homepage redirected' }
$catalog = Get-Page '/Product/Index'
$paths = [regex]::Matches($catalog.Content, '/Product/Details/\d+') | ForEach-Object { $_.Value } | Select-Object -Unique
$productId = $null
foreach ($path in $paths) {
    $detail = Get-Page $path
    $stock = [regex]::Match($detail.Content, 'max="(\d+)"')
    if ($stock.Success -and [int]$stock.Groups[1].Value -ge 2) {
        $productId = [int]($path.Split('/')[-1])
        break
    }
}
if ($null -eq $productId) { throw 'Need a product with at least two units for this smoke test' }
$cart = Invoke-WebRequest ($BaseUrl + '/Cart/AddToCart') -Method Post -WebSession $session -UseBasicParsing -Body @{
    productID = $productId; quantity = 1; __RequestVerificationToken = (Get-Token $detail)
}
$token = Get-Token $cart
function Change-Quantity([int]$change) {
    Invoke-RestMethod ($BaseUrl + '/Cart/ChangeQuantityAjax') -Method Post -WebSession $session -Body @{
        productID = $productId; change = $change; __RequestVerificationToken = $token
    }
}
$changed = Change-Quantity 1
if (-not $changed.success -or $changed.quantity -ne 2 -or $changed.cartTotal -ne $changed.itemTotal -or $changed.itemTotal -ne 2 * $changed.unitPrice) {
    throw 'Quantity or totals are incorrect'
}
$overflow = Change-Quantity ([int]::MaxValue)
if ($overflow.success) { throw 'Overflow request was accepted' }
$after = Change-Quantity 0
if ($after.quantity -ne 2) { throw 'Rejected request changed the cart' }
$cart = Get-Page '/Cart/Index'
if (-not $cart.Content.Contains('id="cart-total"')) { throw 'Missing live total target' }
$null = Invoke-WebRequest ($BaseUrl + '/Cart/RemoveFromCart') -Method Post -WebSession $session -UseBasicParsing -Body @{
    productID = $productId; __RequestVerificationToken = (Get-Token $cart)
}
Write-Output 'PASS: homepage, add, update, totals, overflow rejection and remove (session only; no order created)'
