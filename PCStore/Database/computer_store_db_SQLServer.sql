/*
    DATABASE: computer_store_db
    HE QUAN TRI: Microsoft SQL Server (T-SQL)
    DU AN: ASP.NET Core Web App (Model-View-Controller)

    Chuyen doi tu file MySQL computer_store_db.sql do nhom cung cap.
    - Giu nguyen 13 bang va ten cot de de anh xa sang Model C#.
    - AUTO_INCREMENT -> IDENTITY(1,1)
    - VARCHAR du lieu van ban -> NVARCHAR (ho tro tieng Viet)
    - BOOLEAN -> BIT; ENUM -> NVARCHAR + CHECK
    - TEXT -> NVARCHAR(MAX); CURRENT_TIMESTAMP -> SYSDATETIME()
    - ON DELETE RESTRICT -> ON DELETE NO ACTION
    - Prebuilt_components: FK component_id dung NO ACTION thay cho CASCADE
      de tranh loi multiple cascade paths cua SQL Server.
    - Categories: FK parent_id (tu tham chieu) dung NO ACTION thay cho SET NULL
      de tranh loi Msg 1785 cua SQL Server; xu ly SET NULL cho danh muc con trong code C#.

    Cach dung: Mo file trong SSMS, chon Connect toi SQL Server, bam Execute.
    Script khong xoa database hay du lieu cu neu chay lai.
*/

USE [master];
GO

IF DB_ID(N'computer_store_db') IS NULL
BEGIN
    CREATE DATABASE [computer_store_db];
END;
GO

USE [computer_store_db];
GO

-- ============================================================
-- 1. TAI KHOAN VA PHAN QUYEN
-- ============================================================

IF OBJECT_ID(N'dbo.roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.roles (
        id INT IDENTITY(1,1) NOT NULL,
        role_name NVARCHAR(50) NOT NULL,
        CONSTRAINT PK_roles PRIMARY KEY (id),
        CONSTRAINT UQ_roles_role_name UNIQUE (role_name)
    );
END;
GO

IF OBJECT_ID(N'dbo.users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.users (
        id BIGINT IDENTITY(1,1) NOT NULL,
        username NVARCHAR(50) NOT NULL,
        [password] NVARCHAR(255) NOT NULL,
        full_name NVARCHAR(100) NOT NULL,
        email NVARCHAR(100) NOT NULL,
        phone NVARCHAR(20) NULL,
        [address] NVARCHAR(255) NULL,
        is_active BIT NOT NULL CONSTRAINT DF_users_is_active DEFAULT (1),
        CONSTRAINT PK_users PRIMARY KEY (id),
        CONSTRAINT UQ_users_username UNIQUE (username),
        CONSTRAINT UQ_users_email UNIQUE (email)
    );
END;
GO

IF OBJECT_ID(N'dbo.user_roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.user_roles (
        user_id BIGINT NOT NULL,
        role_id INT NOT NULL,
        CONSTRAINT PK_user_roles PRIMARY KEY (user_id, role_id),
        CONSTRAINT FK_user_roles_users FOREIGN KEY (user_id)
            REFERENCES dbo.users(id) ON DELETE CASCADE,
        CONSTRAINT FK_user_roles_roles FOREIGN KEY (role_id)
            REFERENCES dbo.roles(id) ON DELETE CASCADE
    );
END;
GO

-- ============================================================
-- 2. DANH MUC VA SAN PHAM
-- ============================================================

IF OBJECT_ID(N'dbo.categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.categories (
        id INT IDENTITY(1,1) NOT NULL,
        [name] NVARCHAR(100) NOT NULL,
        parent_id INT NULL,
        CONSTRAINT PK_categories PRIMARY KEY (id),
        CONSTRAINT CK_categories_not_self CHECK (parent_id IS NULL OR parent_id <> id),
        CONSTRAINT FK_categories_parent FOREIGN KEY (parent_id)
            REFERENCES dbo.categories(id) ON DELETE NO ACTION
    );
END;
GO

IF OBJECT_ID(N'dbo.products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.products (
        id BIGINT IDENTITY(1,1) NOT NULL,
        sku NVARCHAR(50) NOT NULL,
        [name] NVARCHAR(255) NOT NULL,
        price DECIMAL(15, 2) NOT NULL,
        stock_quantity INT NOT NULL CONSTRAINT DF_products_stock DEFAULT (0),
        thumbnail NVARCHAR(255) NULL,
        product_type NVARCHAR(20) NOT NULL,
        category_id INT NULL,
        CONSTRAINT PK_products PRIMARY KEY (id),
        CONSTRAINT UQ_products_sku UNIQUE (sku),
        CONSTRAINT CK_products_type CHECK (product_type IN (N'COMPONENT', N'PREBUILT_PC')),
        CONSTRAINT CK_products_price CHECK (price >= 0),
        CONSTRAINT CK_products_stock CHECK (stock_quantity >= 0),
        CONSTRAINT FK_products_categories FOREIGN KEY (category_id)
            REFERENCES dbo.categories(id) ON DELETE SET NULL
    );
END;
GO

-- ============================================================
-- 3. BUILD PC VA CAU THANH PC NGUYEN BO
-- ============================================================

IF OBJECT_ID(N'dbo.attributes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.attributes (
        id INT IDENTITY(1,1) NOT NULL,
        [name] NVARCHAR(100) NOT NULL,
        CONSTRAINT PK_attributes PRIMARY KEY (id),
        CONSTRAINT UQ_attributes_name UNIQUE ([name])
    );
END;
GO

IF OBJECT_ID(N'dbo.product_attributes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.product_attributes (
        product_id BIGINT NOT NULL,
        attribute_id INT NOT NULL,
        [value] NVARCHAR(255) NOT NULL,
        CONSTRAINT PK_product_attributes PRIMARY KEY (product_id, attribute_id),
        CONSTRAINT FK_product_attributes_products FOREIGN KEY (product_id)
            REFERENCES dbo.products(id) ON DELETE CASCADE,
        CONSTRAINT FK_product_attributes_attributes FOREIGN KEY (attribute_id)
            REFERENCES dbo.attributes(id) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'dbo.prebuilt_components', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.prebuilt_components (
        pc_id BIGINT NOT NULL,
        component_id BIGINT NOT NULL,
        quantity INT NOT NULL CONSTRAINT DF_prebuilt_components_quantity DEFAULT (1),
        CONSTRAINT PK_prebuilt_components PRIMARY KEY (pc_id, component_id),
        CONSTRAINT CK_prebuilt_components_quantity CHECK (quantity > 0),
        CONSTRAINT CK_prebuilt_components_not_self CHECK (pc_id <> component_id),
        CONSTRAINT FK_prebuilt_components_pc FOREIGN KEY (pc_id)
            REFERENCES dbo.products(id) ON DELETE CASCADE,
        CONSTRAINT FK_prebuilt_components_component FOREIGN KEY (component_id)
            REFERENCES dbo.products(id) ON DELETE NO ACTION
    );
END;
GO

-- ============================================================
-- 4. DON HANG VA CHI TIET DON HANG
-- ============================================================

IF OBJECT_ID(N'dbo.orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.orders (
        id BIGINT IDENTITY(1,1) NOT NULL,
        user_id BIGINT NULL,                 -- Khach vang lai duoc de NULL
        receiver_name NVARCHAR(100) NOT NULL,
        receiver_phone NVARCHAR(20) NOT NULL,
        receiver_email NVARCHAR(100) NULL,
        province NVARCHAR(100) NOT NULL,
        district NVARCHAR(100) NOT NULL,
        address_detail NVARCHAR(255) NOT NULL,
        order_note NVARCHAR(MAX) NULL,
        order_date DATETIME2(0) NOT NULL
            CONSTRAINT DF_orders_order_date DEFAULT (SYSDATETIME()),
        total_amount DECIMAL(15, 2) NOT NULL,
        payment_method NVARCHAR(50) NOT NULL,
        [status] NVARCHAR(20) NOT NULL
            CONSTRAINT DF_orders_status DEFAULT (N'PENDING'),
        CONSTRAINT PK_orders PRIMARY KEY (id),
        CONSTRAINT CK_orders_total CHECK (total_amount >= 0),
        CONSTRAINT CK_orders_status CHECK ([status] IN
            (N'PENDING', N'CONFIRMED', N'SHIPPING', N'COMPLETED', N'CANCELLED')),
        CONSTRAINT FK_orders_users FOREIGN KEY (user_id)
            REFERENCES dbo.users(id) ON DELETE SET NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.order_details', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.order_details (
        id BIGINT IDENTITY(1,1) NOT NULL,
        order_id BIGINT NOT NULL,
        product_id BIGINT NOT NULL,
        quantity INT NOT NULL,
        unit_price DECIMAL(15, 2) NOT NULL,
        CONSTRAINT PK_order_details PRIMARY KEY (id),
        CONSTRAINT CK_order_details_quantity CHECK (quantity > 0),
        CONSTRAINT CK_order_details_price CHECK (unit_price >= 0),
        CONSTRAINT FK_order_details_orders FOREIGN KEY (order_id)
            REFERENCES dbo.orders(id) ON DELETE CASCADE,
        CONSTRAINT FK_order_details_products FOREIGN KEY (product_id)
            REFERENCES dbo.products(id) ON DELETE NO ACTION
    );
END;
GO

-- ============================================================
-- 5. NHA CUNG CAP VA NHAP KHO
-- ============================================================

IF OBJECT_ID(N'dbo.suppliers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.suppliers (
        id INT IDENTITY(1,1) NOT NULL,
        [name] NVARCHAR(255) NOT NULL,
        phone NVARCHAR(20) NOT NULL,
        email NVARCHAR(100) NULL,
        [address] NVARCHAR(255) NULL,
        CONSTRAINT PK_suppliers PRIMARY KEY (id)
    );
END;
GO

IF OBJECT_ID(N'dbo.import_orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.import_orders (
        id BIGINT IDENTITY(1,1) NOT NULL,
        supplier_id INT NOT NULL,
        user_id BIGINT NOT NULL,             -- Nguoi lap phieu nhap
        import_date DATETIME2(0) NOT NULL
            CONSTRAINT DF_import_orders_date DEFAULT (SYSDATETIME()),
        total_amount DECIMAL(15, 2) NOT NULL,
        note NVARCHAR(MAX) NULL,
        CONSTRAINT PK_import_orders PRIMARY KEY (id),
        CONSTRAINT CK_import_orders_total CHECK (total_amount >= 0),
        CONSTRAINT FK_import_orders_suppliers FOREIGN KEY (supplier_id)
            REFERENCES dbo.suppliers(id) ON DELETE NO ACTION,
        CONSTRAINT FK_import_orders_users FOREIGN KEY (user_id)
            REFERENCES dbo.users(id) ON DELETE NO ACTION
    );
END;
GO

IF OBJECT_ID(N'dbo.import_order_details', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.import_order_details (
        id BIGINT IDENTITY(1,1) NOT NULL,
        import_order_id BIGINT NOT NULL,
        product_id BIGINT NOT NULL,
        quantity INT NOT NULL,
        import_price DECIMAL(15, 2) NOT NULL,
        CONSTRAINT PK_import_order_details PRIMARY KEY (id),
        CONSTRAINT CK_import_order_details_quantity CHECK (quantity > 0),
        CONSTRAINT CK_import_order_details_price CHECK (import_price >= 0),
        CONSTRAINT FK_import_order_details_import_orders FOREIGN KEY (import_order_id)
            REFERENCES dbo.import_orders(id) ON DELETE CASCADE,
        CONSTRAINT FK_import_order_details_products FOREIGN KEY (product_id)
            REFERENCES dbo.products(id) ON DELETE NO ACTION
    );
END;
GO

-- ============================================================
-- 6. CHI MUC PHUC VU TRUY VAN, LOC SAN PHAM VA DON HANG
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.user_roles') AND name = N'IX_user_roles_role_id')
    CREATE INDEX IX_user_roles_role_id ON dbo.user_roles(role_id);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.categories') AND name = N'IX_categories_parent_id')
    CREATE INDEX IX_categories_parent_id ON dbo.categories(parent_id);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.products') AND name = N'IX_products_category_type_price')
    CREATE INDEX IX_products_category_type_price ON dbo.products(category_id, product_type, price);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.product_attributes') AND name = N'IX_product_attributes_attribute_id')
    CREATE INDEX IX_product_attributes_attribute_id ON dbo.product_attributes(attribute_id);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.prebuilt_components') AND name = N'IX_prebuilt_components_component_id')
    CREATE INDEX IX_prebuilt_components_component_id ON dbo.prebuilt_components(component_id);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.orders') AND name = N'IX_orders_user_date')
    CREATE INDEX IX_orders_user_date ON dbo.orders(user_id, order_date);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.orders') AND name = N'IX_orders_status_date')
    CREATE INDEX IX_orders_status_date ON dbo.orders([status], order_date);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.order_details') AND name = N'IX_order_details_order_id')
    CREATE INDEX IX_order_details_order_id ON dbo.order_details(order_id);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.order_details') AND name = N'IX_order_details_product_id')
    CREATE INDEX IX_order_details_product_id ON dbo.order_details(product_id);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.import_orders') AND name = N'IX_import_orders_supplier_id')
    CREATE INDEX IX_import_orders_supplier_id ON dbo.import_orders(supplier_id);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.import_orders') AND name = N'IX_import_orders_user_id')
    CREATE INDEX IX_import_orders_user_id ON dbo.import_orders(user_id);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.import_order_details') AND name = N'IX_import_order_details_import_order_id')
    CREATE INDEX IX_import_order_details_import_order_id ON dbo.import_order_details(import_order_id);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.import_order_details') AND name = N'IX_import_order_details_product_id')
    CREATE INDEX IX_import_order_details_product_id ON dbo.import_order_details(product_id);
GO

-- ============================================================
-- 7. QUYEN MAC DINH (KHONG TAO TAI KHOAN / MAT KHAU ADMIN MAC DINH)
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM dbo.roles WHERE role_name = N'ROLE_ADMIN')
    INSERT INTO dbo.roles(role_name) VALUES (N'ROLE_ADMIN');
IF NOT EXISTS (SELECT 1 FROM dbo.roles WHERE role_name = N'ROLE_USER')
    INSERT INTO dbo.roles(role_name) VALUES (N'ROLE_USER');
GO

-- ============================================================
-- 8. KIEM TRA SAU KHI TAO DATABASE
-- ============================================================

SELECT [name] AS table_name
FROM sys.tables
WHERE schema_id = SCHEMA_ID(N'dbo')
ORDER BY [name];

SELECT COUNT(*) AS total_tables
FROM sys.tables
WHERE schema_id = SCHEMA_ID(N'dbo');
GO

/*
    GHI CHU NGHIEP VU:
    1. File nay tao schema, constraints, indexes va 2 role; chua co tai khoan Admin.
       Ung dung C# phai bam mat khau bang BCrypt / PasswordHasher truoc khi INSERT.
    2. Chuc nang nhap kho (cong stock_quantity), duyet don (tru stock_quantity),
       Build PC (kiem tra tuong thich) can duoc xu ly trong Service/Repository C#
       voi SQL transaction; KHONG co trigger tu dong trong file nay.
    3. Chuc nang bao hanh trong so do nghiep vu chua co bang trong SQL goc,
       nen khong tu y them bang moi vao ban chuyen doi nay.
*/
