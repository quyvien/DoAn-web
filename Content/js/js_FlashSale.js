// 1. Lấy chuỗi thời gian từ C#
var endTimeString = "@endTimeStr";

console.log("Flash Sale End Time: " + endTimeString); // Bấm F12 xem console có hiện ngày giờ không

if (endTimeString && endTimeString !== "") {
    // Javascript đọc định dạng ISO
    var countDownDate = new Date(endTimeString).getTime();

    var x = setInterval(function () {
        var now = new Date().getTime();
        var distance = countDownDate - now;

        // Tính toán
        var hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        var minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
        var seconds = Math.floor((distance % (1000 * 60)) / 1000);

        // Xử lý hiển thị số 0 đằng trước (01, 02...)
        document.getElementById("h").innerHTML = hours < 10 ? "0" + hours : hours;
        document.getElementById("m").innerHTML = minutes < 10 ? "0" + minutes : minutes;
        document.getElementById("s").innerHTML = seconds < 10 ? "0" + seconds : seconds;

        // Nếu hết giờ hoặc dữ liệu sai (NaN)
        if (distance < 0 || isNaN(distance)) {
            // Nếu NaN thì hiện 00:00:00 cho đẹp thay vì NaN
            if (isNaN(distance)) {
                document.getElementById("h").innerHTML = "00";
                document.getElementById("m").innerHTML = "00";
                document.getElementById("s").innerHTML = "00";
            } else {
                clearInterval(x);
                document.querySelector(".countdown-timer").innerHTML = "ĐÃ KẾT THÚC";
            }
        }
    }, 1000);
}