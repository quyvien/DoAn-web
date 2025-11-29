function getValue(id) {
    return document.getElementById(id).value.trim();
}

function showError(id, message) {
    document.getElementById(id + "_error").innerHTML = message;
}

function clearErrors() {
    document.querySelectorAll(".error").forEach(e => e.innerHTML = "");
}

document.getElementById("formLogin").addEventListener("submit", function (e) {
    e.preventDefault();
    clearErrors();

    // Tài khoản đúng (hardcode)
    const correctUser = "admin";
    const correctPass = "12345678";

    const user = getValue("loginUsername");
    const pass = getValue("loginPassword");

    let valid = true;

    // Kiểm tra tồn tại tên đăng nhập
    if (user !== correctUser) {
        valid = false;
        showError("loginUsername", "Tên đăng nhập không tồn tại!");
    }
    // Nếu tên đúng mới kiểm tra mật khẩu
    else if (pass !== correctPass) {
        valid = false;
        showError("loginPassword", "Mật khẩu không chính xác!");
    }











    if (valid) {
        alert("Đăng nhập thành công!");
        window.location.href = "/Default/Index";
    }



});
