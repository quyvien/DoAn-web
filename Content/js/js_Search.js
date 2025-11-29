

// dam bao trang du lieu da tai xong truoc khi thuc hien tim kiem
$(document).ready(function () {

    // Lắng nghe sự kiện 'keyup' (khi gõ phím)
    $('#live-search-box').on('keyup', function () {

        var $this = $(this); // Cache lại đối tượng textbox
        var query = $this.val(); // Lấy nội dung đang gõ
        var resultsDiv = $('#search-results');

        // === ĐỌC URL TỪ THUỘC TÍNH "data-url" CỦA HTML ===
        var url = $this.data('url');

        if (query.length > 2) { // Chỉ tìm khi gõ hơn 2 ký tự

            // Dùng biến "url" này để gọi AJAX
            $.get(url, { searchString: query }, function (data) {

                // Đổ HTML (từ PartialView) vào div
                resultsDiv.html(data);
                resultsDiv.show(); // Hiển thị div
            });
        } else {
            resultsDiv.hide(); // Ẩn div nếu xóa chữ
            resultsDiv.html(''); // Dọn sạch
        }
    });

    // (Tùy chọn) Ẩn kết quả khi click ra ngoài
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.search-box').length) {
            $('#search-results').hide();
        }
    });
});