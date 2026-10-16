# DoAn-web

Website bán hàng xây dựng bằng ASP.NET MVC 5, Entity Framework 6 và SQL Server.

## Chức năng chính

- Danh mục, tìm kiếm và chi tiết sản phẩm.
- Giỏ hàng, flash sale, mã giảm giá và đặt hàng.
- Đăng ký, đăng nhập và lịch sử đơn hàng.
- Khu vực quản trị sản phẩm, danh mục, khách hàng, tài khoản và đơn hàng.

## Yêu cầu

- Visual Studio 2022 với workload **ASP.NET and web development**.
- .NET Framework 4.7.2.
- SQL Server và cơ sở dữ liệu `MyStore2026` có cấu trúc khớp với mô hình tại `Models/MyStore2026Model.edmx`.

## Cài đặt

1. Clone repository và mở `DoAn-web.sln` bằng Visual Studio.
2. Cập nhật chuỗi kết nối `MyStore2026Entities` trong `Web.config` theo SQL Server trên máy.
3. Khôi phục NuGet packages, sau đó build solution.
4. Chạy dự án bằng IIS Express.

Có thể khôi phục package và kiểm tra build từ Developer PowerShell:

```powershell
msbuild DoAn-web.sln /t:Restore /p:RestorePackagesConfig=true
msbuild DoAn-web.sln /t:Build /p:Configuration=Release
```

## Phân quyền và mật khẩu

- Vai trò `A` dùng cho quản trị viên, vai trò `C` dùng cho khách hàng.
- Mật khẩu mới được lưu bằng PBKDF2 kèm salt.
- Tài khoản cũ còn mật khẩu dạng thuần sẽ tự chuyển sang bản băm sau lần đăng nhập thành công đầu tiên.

Không commit chuỗi kết nối chứa tài khoản/mật khẩu thật hoặc dữ liệu bí mật lên repository.
