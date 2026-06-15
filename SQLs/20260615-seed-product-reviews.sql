-- Seed 4 demo reviews (with photos) for Product 1 ("Khăn Choàng Len Thổ Cẩm Đan Tay").
-- Each review is backed by a real completed Order + OrderItem so the
-- Products/Details review list reflects genuine purchase history.

SET NOCOUNT ON;
BEGIN TRAN;

DECLARE @OrderId BIGINT;

-- Review 1: Phan Cẩm Tú, 5*, 03/03/2026
INSERT INTO Orders (BuyerId, SellerId, OrderDescription, AgreedPrice, Status, CreatedAt, CompletedAt, IsPaid, IsSellerPaid)
VALUES (7, 1, N'Khăn Choàng Len Thổ Cẩm Đan Tay', 150000, 7, '2026-03-03T10:00:00', '2026-03-03T10:00:00', 1, 1);
SET @OrderId = SCOPE_IDENTITY();

INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price, HasCustomization, IsCustomizationResolved)
VALUES (@OrderId, 1, 1, 150000, 0, 0);

INSERT INTO Reviews (OrderId, BuyerId, SellerId, Rating, Comment, CreatedAt, ImageUrl, ProductId)
VALUES (@OrderId, 7, 1, 5, N'Shop chuẩn bị và giao hàng nhanh, đóng gói cẩn thận đẹp. Khăn đẹp lắm mn ưi, k dày cũng k mỏng quá', '2026-03-03T10:00:00', '/images/reviews/review-1.webp', 1);

-- Review 2: Đỗ Thị Dương, 5*, 15/02/2026
INSERT INTO Orders (BuyerId, SellerId, OrderDescription, AgreedPrice, Status, CreatedAt, CompletedAt, IsPaid, IsSellerPaid)
VALUES (6, 1, N'Khăn Choàng Len Thổ Cẩm Đan Tay', 150000, 7, '2026-02-15T10:00:00', '2026-02-15T10:00:00', 1, 1);
SET @OrderId = SCOPE_IDENTITY();

INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price, HasCustomization, IsCustomizationResolved)
VALUES (@OrderId, 1, 1, 150000, 0, 0);

INSERT INTO Reviews (OrderId, BuyerId, SellerId, Rating, Comment, CreatedAt, ImageUrl, ProductId)
VALUES (@OrderId, 6, 1, 5, N'Khăn đẹp cũng dày dặn. Mang khăn đi tà xùa chuup ảnh xinh lắm. Đáng mua nha', '2026-02-15T10:00:00', '/images/reviews/review-2.webp', 1);

-- Review 3: Nguyễn Minh Khang, 4*, 28/01/2026
INSERT INTO Orders (BuyerId, SellerId, OrderDescription, AgreedPrice, Status, CreatedAt, CompletedAt, IsPaid, IsSellerPaid)
VALUES (7, 1, N'Khăn Choàng Len Thổ Cẩm Đan Tay', 150000, 7, '2026-01-28T10:00:00', '2026-01-28T10:00:00', 1, 1);
SET @OrderId = SCOPE_IDENTITY();

INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price, HasCustomization, IsCustomizationResolved)
VALUES (@OrderId, 1, 1, 150000, 0, 0);

INSERT INTO Reviews (OrderId, BuyerId, SellerId, Rating, Comment, CreatedAt, ImageUrl, ProductId)
VALUES (@OrderId, 7, 1, 4, N'Trời ơi chất lượng đẹp thật sự ạ. Sờ chất lhawn lhas chắc chắn và không có chỉ thừa .Dày dặn và dờ khá êm ạ. Nên mua nha mn ơi', '2026-01-28T10:00:00', '/images/reviews/review-3.webp', 1);

-- Review 4: Trần Bảo Ngọc, 5*, 09/01/2026
INSERT INTO Orders (BuyerId, SellerId, OrderDescription, AgreedPrice, Status, CreatedAt, CompletedAt, IsPaid, IsSellerPaid)
VALUES (6, 1, N'Khăn Choàng Len Thổ Cẩm Đan Tay', 150000, 7, '2026-01-09T10:00:00', '2026-01-09T10:00:00', 1, 1);
SET @OrderId = SCOPE_IDENTITY();

INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price, HasCustomization, IsCustomizationResolved)
VALUES (@OrderId, 1, 1, 150000, 0, 0);

INSERT INTO Reviews (OrderId, BuyerId, SellerId, Rating, Comment, CreatedAt, ImageUrl, ProductId)
VALUES (@OrderId, 6, 1, 5, N'Trùi ui mê Hà Giang quá nên mua về trước để lên lịch diiii 😍', '2026-01-09T10:00:00', '/images/reviews/review-4.webp', 1);

COMMIT;
