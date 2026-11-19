PCStore - DU AN ASP.NET CORE MVC .NET 8 (KHUNG GIAO DIEN)
======================================================

1. MO DU AN
- Giai nen file ZIP.
- Mo PCStore.sln (Visual Studio 2022) hoac mo PCStore/PCStore.csproj.
- Can cai .NET 8 SDK + ASP.NET and web development.
- F5 / Ctrl+F5 de chay website.
- Trang User: http://localhost:5108/
- Trang Admin: http://localhost:5108/Admin
- Co the dung 'dotnet run --project PCStore/PCStore.csproj --launch-profile http' (chay tu thu muc chua PCStore.sln)

2. CAC THU MUC CHINH
- Controllers/: dieu huong giao dien trang User (Home, Products, PcBuilder, Cart, Checkout, Account, Orders).
- Models/: 13 POCO Model tuong ung 13 bang SQL Server (KHONG co Entity Framework).
- ViewModels/: AdminModuleViewModel phuc vu bo cuc trang Admin.
- Views/Shared/_Layout_user.cshtml: khung chung trang User.
- Areas/Admin/Controllers/: dieu huong giao dien Admin.
- Areas/Admin/Views/Shared/_Layout_admin.cshtml: khung chung Admin.
- Areas/Admin/Views/Shared/_AdminModule.cshtml: giao dien bang quan ly dung chung.
- wwwroot/css/site.css: mau sac, giao dien, responsive, hover.
- wwwroot/js/site.js: mo/dong menu responsive.
- wwwroot/images/banners/ va products/: vi tri dat anh that khi co.
- Data/, Services/, Repositories/: thu muc cho tung thanh vien bo sung SQL/ADO.NET va xu ly nghiep vu.
- Database/computer_store_db_SQLServer.sql: script tao 13 bang SQL Server (ban da chuyen doi).

3. PHAN CONG 4 NGUOI
- NV1: User & Security, Admin Dashboard, Program.cs, layout, SQL connection.
- NV2: Catalog, Products, Categories, Attributes, Suppliers, Imports.
- NV3: PcBuilder, kiem tra tuong thich, tinh tong, xuat bao gia PDF/Excel.
- NV4: Cart, Checkout, Orders, duyet don va tru kho.

4. BANG MAU DUNG THEO ANH
- #02A4EF : mau chinh
- #EF4D02 : nut hanh dong, CTA
- #02EFC3 : diem nhan xanh mint
- #022EEF : xanh royal
- #015F8A : xanh dam
- #59CAFE : xanh nhat
- #3D8EB4 : xanh phu

5. QUAN TRONG - DAY CHI LA KHUNG GIAO DIEN
- Chua ket noi SQL Server va chua truy van du lieu.
- Chua CRUD, dang nhap that, phan quyen, gio hang, Build PC, thanh toan, gui email.
- Nut nghiep vu chua hoat dong duoc vo hieu hoa.
- /Admin CHUA DUOC BAO VE, chi dung de phat trien giao dien: KHONG trien khai cong khai.
- Gia, so lieu Dashboard va bieu do deu la bo cuc minh hoa, khong phai du lieu that.
- Dat connection string tai appsettings.json, sau do cai Microsoft.Data.SqlClient khi code ADO.NET.
- File database SQL Server da duoc chep kem; chay bang SSMS neu muon tao bang.
- Anh/banner cu tu template thoi trang va quan ly nhan su khong duoc dua vao ban giao dien PC nay.

6. CACH DOI ANH
- Chep anh vao wwwroot/images/products/ hay wwwroot/images/banners/.
- Sua file .cshtml tuong ung, thay the span.picture-placeholder bang the img co src nhu:
  <img src="~/images/products/ten-anh.jpg" alt="San pham" />
- Hero va banner phu dang la gradient CSS khong can anh, co the thay anh theo y muon.

7. GHI CHU KIEM TRA
- Cac tap tin duoc xem va kiem tra tinh hop le cua cau truc; chua build dotnet truc tiep trong moi truong tao file (khong co .NET SDK).
