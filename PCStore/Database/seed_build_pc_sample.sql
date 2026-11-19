/*
    DU LIEU MAU CHO MODULE BUILD PC (NGUOI 3)
    - Idempotent: chay nhieu lan khong bi trung du lieu.
    - Gom du 8 nhom linh kien (CPU, Mainboard, RAM, VGA, SSD, PSU, Case, Tan nhiet)
      va cac thuoc tinh ky thuat (Socket, Chuan RAM, So khe RAM, TDP, Cong suat nguon,
      Nguon khuyen nghi, Form Factor...) de test day du cac tinh huong tuong thich.
*/

USE [computer_store_db];
GO

-- 1. DANH MUC LINH KIEN
DECLARE @catNames TABLE ([name] NVARCHAR(100));
INSERT INTO @catNames VALUES
    (N'CPU'),
    (N'Mainboard'),
    (N'RAM'),
    (N'VGA'),
    (N'SSD'),
    (N'PSU'),
    (N'Case'),
    (N'Tản nhiệt');

INSERT INTO dbo.categories ([name], parent_id)
SELECT cn.[name], NULL
FROM @catNames cn
WHERE NOT EXISTS (SELECT 1 FROM dbo.categories c WHERE c.[name] = cn.[name]);

DECLARE @catCpu INT = (SELECT TOP 1 id FROM dbo.categories WHERE [name] = N'CPU');
DECLARE @catMain INT = (SELECT TOP 1 id FROM dbo.categories WHERE [name] = N'Mainboard');
DECLARE @catRam INT = (SELECT TOP 1 id FROM dbo.categories WHERE [name] = N'RAM');
DECLARE @catVga INT = (SELECT TOP 1 id FROM dbo.categories WHERE [name] = N'VGA');
DECLARE @catSsd INT = (SELECT TOP 1 id FROM dbo.categories WHERE [name] = N'SSD');
DECLARE @catPsu INT = (SELECT TOP 1 id FROM dbo.categories WHERE [name] = N'PSU');
DECLARE @catCase INT = (SELECT TOP 1 id FROM dbo.categories WHERE [name] = N'Case');
DECLARE @catCooler INT = (SELECT TOP 1 id FROM dbo.categories WHERE [name] = N'Tản nhiệt');

-- 2. THUOC TINH KY THUAT
DECLARE @attrNames TABLE ([name] NVARCHAR(100));
INSERT INTO @attrNames VALUES
    (N'Socket'),
    (N'Chuẩn RAM'),
    (N'Số khe RAM'),
    (N'Bus RAM'),
    (N'Dung lượng'),
    (N'TDP'),
    (N'Công suất nguồn'),
    (N'Nguồn khuyến nghị'),
    (N'Form Factor'),
    (N'Chuẩn kết nối');

INSERT INTO dbo.attributes ([name])
SELECT an.[name]
FROM @attrNames an
WHERE NOT EXISTS (SELECT 1 FROM dbo.attributes a WHERE a.[name] = an.[name]);

DECLARE @aSocket INT = (SELECT id FROM dbo.attributes WHERE [name] = N'Socket');
DECLARE @aRamType INT = (SELECT id FROM dbo.attributes WHERE [name] = N'Chuẩn RAM');
DECLARE @aRamSlots INT = (SELECT id FROM dbo.attributes WHERE [name] = N'Số khe RAM');
DECLARE @aBusRam INT = (SELECT id FROM dbo.attributes WHERE [name] = N'Bus RAM');
DECLARE @aCapacity INT = (SELECT id FROM dbo.attributes WHERE [name] = N'Dung lượng');
DECLARE @aTdp INT = (SELECT id FROM dbo.attributes WHERE [name] = N'TDP');
DECLARE @aPsuWatt INT = (SELECT id FROM dbo.attributes WHERE [name] = N'Công suất nguồn');
DECLARE @aRecPsu INT = (SELECT id FROM dbo.attributes WHERE [name] = N'Nguồn khuyến nghị');
DECLARE @aFormFactor INT = (SELECT id FROM dbo.attributes WHERE [name] = N'Form Factor');
DECLARE @aInterface INT = (SELECT id FROM dbo.attributes WHERE [name] = N'Chuẩn kết nối');

-- 3. SAN PHAM LINH KIEN MAU
IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'CPU-I5-13400F')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'CPU-I5-13400F', N'Intel Core i5-13400F (10 Nhân 16 Luồng)', 4790000, 12, NULL, N'COMPONENT', @catCpu);

IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'CPU-I7-14700K')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'CPU-I7-14700K', N'Intel Core i7-14700K (20 Nhân 28 Luồng)', 10490000, 6, NULL, N'COMPONENT', @catCpu);

IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'CPU-R5-7600')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'CPU-R5-7600', N'AMD Ryzen 5 7600 (6 Nhân 12 Luồng, AM5)', 5190000, 9, NULL, N'COMPONENT', @catCpu);

IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'CPU-R7-7800X3D')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'CPU-R7-7800X3D', N'AMD Ryzen 7 7800X3D (Hết hàng - Test lọc kho)', 11290000, 0, NULL, N'COMPONENT', @catCpu);

IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'MAIN-B760M-D4')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'MAIN-B760M-D4', N'ASUS PRIME B760M-A WIFI D4', 3290000, 10, NULL, N'COMPONENT', @catMain);

IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'MAIN-Z790-D5')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'MAIN-Z790-D5', N'MSI PRO Z790-P WIFI DDR5', 5690000, 7, NULL, N'COMPONENT', @catMain);

IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'MAIN-B650M-AM5')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'MAIN-B650M-AM5', N'GIGABYTE B650M GAMING X AX (AM5 DDR5)', 4490000, 8, NULL, N'COMPONENT', @catMain);

IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'RAM-D4-16G-3200')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'RAM-D4-16G-3200', N'Kingston Fury Beast 16GB DDR4 3200MHz', 990000, 20, NULL, N'COMPONENT', @catRam);

IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'RAM-D5-16G-5600')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'RAM-D5-16G-5600', N'Corsair Vengeance 16GB DDR5 5600MHz', 1490000, 16, NULL, N'COMPONENT', @catRam);

IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'VGA-RTX4060-8G')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'VGA-RTX4060-8G', N'ASUS Dual GeForce RTX 4060 OC 8GB', 8290000, 11, NULL, N'COMPONENT', @catVga);

IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'VGA-RTX4070S-12G')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'VGA-RTX4070S-12G', N'MSI GeForce RTX 4070 SUPER 12G VENTUS 2X', 16990000, 5, NULL, N'COMPONENT', @catVga);

IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'SSD-NVME-1TB')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'SSD-NVME-1TB', N'Samsung 980 PRO 1TB NVMe PCIe Gen4', 2490000, 18, NULL, N'COMPONENT', @catSsd);

IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'SSD-NVME-500G')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'SSD-NVME-500G', N'WD Blue SN580 500GB NVMe Gen4', 1190000, 22, NULL, N'COMPONENT', @catSsd);

IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'PSU-450W-BRONZE')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'PSU-450W-BRONZE', N'Cooler Master MWE 450W 80 Plus Bronze', 950000, 10, NULL, N'COMPONENT', @catPsu);

IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'PSU-650W-BRONZE')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'PSU-650W-BRONZE', N'MSI MAG A650BN 650W 80 Plus Bronze', 1390000, 14, NULL, N'COMPONENT', @catPsu);

IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'PSU-850W-GOLD')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'PSU-850W-GOLD', N'Corsair RM850e 850W 80 Plus Gold Modular', 3190000, 7, NULL, N'COMPONENT', @catPsu);

IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'CASE-MATX-AIR')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'CASE-MATX-AIR', N'DeepCool CH370 Micro-ATX Airflow', 1290000, 9, NULL, N'COMPONENT', @catCase);

IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'CASE-ATX-MID')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'CASE-ATX-MID', N'NZXT H5 Flow Mid-Tower ATX', 2290000, 6, NULL, N'COMPONENT', @catCase);

IF NOT EXISTS (SELECT 1 FROM dbo.products WHERE sku = N'COOL-AK400')
    INSERT INTO dbo.products (sku, [name], price, stock_quantity, thumbnail, product_type, category_id)
    VALUES (N'COOL-AK400', N'DeepCool AK400 Air Cooler', 690000, 15, NULL, N'COMPONENT', @catCooler);

-- 4. GAN THUOC TINH KY THUAT CHO TUNG SAN PHAM
DECLARE @pairs TABLE (sku NVARCHAR(50), attr_id INT, [val] NVARCHAR(255));
INSERT INTO @pairs VALUES
    -- CPU
    (N'CPU-I5-13400F', @aSocket, N'LGA1700'),
    (N'CPU-I5-13400F', @aRamType, N'DDR4 / DDR5'),
    (N'CPU-I5-13400F', @aTdp, N'65W'),

    (N'CPU-I7-14700K', @aSocket, N'LGA1700'),
    (N'CPU-I7-14700K', @aRamType, N'DDR4 / DDR5'),
    (N'CPU-I7-14700K', @aTdp, N'125W'),

    (N'CPU-R5-7600', @aSocket, N'AM5'),
    (N'CPU-R5-7600', @aRamType, N'DDR5'),
    (N'CPU-R5-7600', @aTdp, N'65W'),

    -- Mainboard
    (N'MAIN-B760M-D4', @aSocket, N'LGA1700'),
    (N'MAIN-B760M-D4', @aRamType, N'DDR4'),
    (N'MAIN-B760M-D4', @aRamSlots, N'4 khe'),
    (N'MAIN-B760M-D4', @aFormFactor, N'mATX'),

    (N'MAIN-Z790-D5', @aSocket, N'LGA1700'),
    (N'MAIN-Z790-D5', @aRamType, N'DDR5'),
    (N'MAIN-Z790-D5', @aRamSlots, N'4 khe'),
    (N'MAIN-Z790-D5', @aFormFactor, N'ATX'),

    (N'MAIN-B650M-AM5', @aSocket, N'AM5'),
    (N'MAIN-B650M-AM5', @aRamType, N'DDR5'),
    (N'MAIN-B650M-AM5', @aRamSlots, N'4 khe'),
    (N'MAIN-B650M-AM5', @aFormFactor, N'mATX'),

    -- RAM
    (N'RAM-D4-16G-3200', @aRamType, N'DDR4'),
    (N'RAM-D4-16G-3200', @aCapacity, N'16GB'),
    (N'RAM-D4-16G-3200', @aBusRam, N'3200MHz'),

    (N'RAM-D5-16G-5600', @aRamType, N'DDR5'),
    (N'RAM-D5-16G-5600', @aCapacity, N'16GB'),
    (N'RAM-D5-16G-5600', @aBusRam, N'5600MHz'),

    -- VGA
    (N'VGA-RTX4060-8G', @aTdp, N'115W'),
    (N'VGA-RTX4060-8G', @aRecPsu, N'550W'),
    (N'VGA-RTX4060-8G', @aCapacity, N'8GB GDDR6'),

    (N'VGA-RTX4070S-12G', @aTdp, N'220W'),
    (N'VGA-RTX4070S-12G', @aRecPsu, N'650W'),
    (N'VGA-RTX4070S-12G', @aCapacity, N'12GB GDDR6X'),

    -- SSD
    (N'SSD-NVME-1TB', @aCapacity, N'1TB'),
    (N'SSD-NVME-1TB', @aInterface, N'M.2 NVMe PCIe Gen4'),

    (N'SSD-NVME-500G', @aCapacity, N'500GB'),
    (N'SSD-NVME-500G', @aInterface, N'M.2 NVMe PCIe Gen4'),

    -- PSU
    (N'PSU-450W-BRONZE', @aPsuWatt, N'450W'),
    (N'PSU-650W-BRONZE', @aPsuWatt, N'650W'),
    (N'PSU-850W-GOLD', @aPsuWatt, N'850W'),

    -- Case
    (N'CASE-MATX-AIR', @aFormFactor, N'mATX / Mini-ITX'),
    (N'CASE-ATX-MID', @aFormFactor, N'ATX / mATX / Mini-ITX'),

    -- Cooler
    (N'COOL-AK400', @aSocket, N'LGA1700 / AM5 / AM4'),
    (N'COOL-AK400', @aTdp, N'220W');

INSERT INTO dbo.product_attributes (product_id, attribute_id, [value])
SELECT p.id, pr.attr_id, pr.[val]
FROM @pairs pr
INNER JOIN dbo.products p ON p.sku = pr.sku
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.product_attributes pa
    WHERE pa.product_id = p.id AND pa.attribute_id = pr.attr_id
);
GO

SELECT c.[name] AS category_name, COUNT(p.id) AS total_products
FROM dbo.categories c
LEFT JOIN dbo.products p ON p.category_id = c.id
GROUP BY c.[name]
ORDER BY c.[name];
GO
