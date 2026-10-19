param([string]$AssemblyPath = (Join-Path $PSScriptRoot '..\bin\DoAn-web.dll'))
$ErrorActionPreference = 'Stop'
$assemblyFile = (Resolve-Path $AssemblyPath).Path
[void][Reflection.Assembly]::LoadFrom((Join-Path (Split-Path $assemblyFile) 'EntityFramework.dll'))
[void][Reflection.Assembly]::LoadFrom($assemblyFile)

function Assert-True($condition, [string]$message) {
    if (-not $condition) { throw $message }
}

# Kiểm tra quy tắc hủy mà không cần kết nối cơ sở dữ liệu.
$order = New-Object DoAn_web.Models.Order
$order.PaymentStatus = 'Chờ xử lý'
$order.ShippingStatus = 'Chờ lấy hàng'
Assert-True ([DoAn_web.Services.OrderService]::CanCustomerCancel($order)) 'Don cho xu ly phai huy duoc'
foreach ($status in @('Đang vận chuyển', 'Đã giao thành công', 'khong-hop-le', '')) {
    $order.ShippingStatus = $status
    Assert-True (-not [DoAn_web.Services.OrderService]::CanCustomerCancel($order)) 'Khong duoc huy don ngoai trang thai cho lay hang'
}
$order.ShippingStatus = 'Chờ lấy hàng'
$order.PaymentStatus = 'Đã thanh toán'
Assert-True (-not [DoAn_web.Services.OrderService]::CanCustomerCancel($order)) 'Don da thanh toan can quy trinh hoan tien rieng'
foreach ($status in @('Đã hủy', 'Đã Huỷ !')) {
    $order.PaymentStatus = $status
    Assert-True ([DoAn_web.Services.OrderService]::IsCancelled($order)) 'Phai nhan dien trang thai huy cu'
    Assert-True (-not [DoAn_web.Services.OrderService]::CanCustomerCancel($order)) 'Khong duoc huy lan hai'
}
Assert-True (-not [DoAn_web.Services.OrderService]::CanCustomerCancel($null)) 'Don null khong the huy'
Write-Output 'PASS: order cancellation regression checks'

$password = 'MatKhauThuNghiem-123!'
$hash = [DoAn_web.Security.PasswordHasher]::Hash($password)
$secondHash = [DoAn_web.Security.PasswordHasher]::Hash($password)
Assert-True ($hash -ne $secondHash) 'Moi mat khau phai co salt ngau nhien'
Assert-True ([DoAn_web.Security.PasswordHasher]::Verify($password, $hash)) 'Mat khau dung phai duoc chap nhan'
Assert-True (-not [DoAn_web.Security.PasswordHasher]::Verify('sai', $hash)) 'Mat khau sai phai bi tu choi'
Assert-True ([DoAn_web.Security.PasswordHasher]::Verify($password, $password)) 'Tai khoan cu phai dang nhap duoc de nang cap'
foreach ($invalid in @(
    'PBKDF2$100000$$',
    'PBKDF2$100000$AA==$AA==',
    'PBKDF2$100000$!$!',
    ($hash -replace '\$100000\$', '$2147483647$'),
    ($hash -replace '\$100000\$', '$0$'),
    ($hash + '$extra')
)) {
    Assert-True (-not [DoAn_web.Security.PasswordHasher]::Verify($password, $invalid)) 'Ban bam hong phai bi tu choi ma khong phat sinh exception'
}
Write-Output 'PASS: password verification regression checks'
