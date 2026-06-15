-- Localize MakeForYou marketplace data to Vietnamese.
-- Reimagines the catalog as Vietnamese handmade craft shops/products,
-- and translates remaining English UI-facing strings (notifications,
-- chat, reviews, posts, customization labels) to Vietnamese.

SET NOCOUNT ON;
BEGIN TRAN;

-- 1) Categories
UPDATE Categories SET Name = N'Đan Len & Dệt May Thủ Công' WHERE CategoryId = 1;
UPDATE Categories SET Name = N'Gốm Sứ Thủ Công'            WHERE CategoryId = 2;
UPDATE Categories SET Name = N'Đồ Gỗ Mỹ Nghệ & Gia Dụng'   WHERE CategoryId = 3;
UPDATE Categories SET Name = N'Tranh Vẽ & Tranh In Nghệ Thuật' WHERE CategoryId = 4;
UPDATE Categories SET Name = N'Hoa Ép & Nghệ Thuật Hoa'    WHERE CategoryId = 5;

-- 2) Seller account owner names (Users.FullName for sellers 1-5)
UPDATE Users SET FullName = N'Nguyễn Thị Lan'  WHERE UserId = 1;
UPDATE Users SET FullName = N'Trần Văn Hùng'   WHERE UserId = 2;
UPDATE Users SET FullName = N'Lê Văn Mộc'      WHERE UserId = 3;
UPDATE Users SET FullName = N'Phạm Thị Hồng'   WHERE UserId = 4;
UPDATE Users SET FullName = N'Đỗ Thị Hoa'      WHERE UserId = 5;

-- 3) Seller shop profiles
UPDATE Sellers
SET ShopName = N'Len Mộc Handmade',
    Bio = N'Nghệ nhân đan len thủ công với hơn 8 năm kinh nghiệm, chuyên các sản phẩm khăn choàng và mũ len họa tiết thổ cẩm vùng Tây Bắc.',
    SkillDescription = N'Thiết kế họa tiết riêng, phối màu theo yêu cầu, nhận đan số lượng nhỏ theo đơn đặt hàng.'
WHERE SellerId = 1;

UPDATE Sellers
SET ShopName = N'Gốm Bát Tràng An Nhiên',
    Bio = N'Xưởng gốm gia truyền tại làng nghề Bát Tràng, chuyên chế tác gốm sứ thủ công với men rạn và họa tiết truyền thống.',
    SkillDescription = N'Gốm gia dụng: cốc, bát, chậu cây với men rạn hoa văn cổ điển, nhận khắc tên và lời chúc theo yêu cầu.'
WHERE SellerId = 2;

UPDATE Sellers
SET ShopName = N'Mộc Tâm - Đồ Gỗ Thủ Công',
    Bio = N'Xưởng mộc thủ công chuyên chế tác đồ gỗ trang trí và quà tặng từ gỗ óc chó, gỗ xoan đào tự nhiên.',
    SkillDescription = N'Tranh gỗ khắc laser, móc khóa, đồ trang trí nội thất nhỏ, nhận khắc tên và thiết kế riêng theo yêu cầu.'
WHERE SellerId = 3;

UPDATE Sellers
SET ShopName = N'Tranh Dân Gian Phương Nam',
    Bio = N'Xưởng tranh chuyên phục dựng và sáng tác tranh dân gian Đông Hồ kết hợp phong cách hiện đại.',
    SkillDescription = N'Tranh in giclée trên giấy mỹ thuật, vẽ chân dung thú cưng theo phong cách dân gian, nhận đặt vẽ theo yêu cầu.'
WHERE SellerId = 4;

UPDATE Sellers
SET ShopName = N'Hoa Ép Mộc Lan',
    Bio = N'Chuyên chế tác tranh hoa ép và đồ lưu niệm từ hoa tươi ép khô, nguyên liệu thu hái từ vườn hoa địa phương.',
    SkillDescription = N'Tranh hoa ép khung gỗ theo yêu cầu, khung lưu niệm hoa cưới, bookmark hoa ép thủ công.'
WHERE SellerId = 5;

-- 4) Products - reimagined as Vietnamese handicrafts (kept aligned with
--    existing customization groups/options per product)
UPDATE Products SET
    Title = N'Khăn Choàng Len Thổ Cẩm Đan Tay',
    Description = N'Khăn choàng len đan tay theo họa tiết thổ cẩm vùng Tây Bắc, mềm mại và ấm áp. Có thể chọn chất liệu len thường hoặc len cừu cao cấp.'
WHERE ProductId = 1;

UPDATE Products SET
    Title = N'Mũ Len Beret Đan Tay',
    Description = N'Mũ len beret đan tay thủ công, kiểu dáng thanh lịch, giữ ấm tốt cho mùa đông. Có 3 kích cỡ S, M, L.'
WHERE ProductId = 2;

UPDATE Products SET
    Title = N'Bộ Mũ Len Đôi Cho Cặp Đôi Theo Yêu Cầu',
    Description = N'Cặp mũ len đan tay theo yêu cầu, có thể thêu tên hoặc ký hiệu riêng cho hai người. Chọn màu sắc và kích cỡ theo sở thích.'
WHERE ProductId = 3;

UPDATE Products SET
    Title = N'Cốc Gốm Bát Tràng Khắc Tên Tặng Người Thân',
    Description = N'Cốc gốm Bát Tràng nung thủ công, men rạn hoa văn cổ điển. Khắc dòng chữ dành tặng Bố, Mẹ, Ông hoặc Bà - món quà ý nghĩa cho gia đình.'
WHERE ProductId = 4;

UPDATE Products SET
    Title = N'Chậu Cây Gốm Bát Tràng Mini',
    Description = N'Chậu cây nhỏ xinh làm thủ công từ gốm Bát Tràng, có lỗ thoát nước, phù hợp trồng sen đá và cây mini để bàn.'
WHERE ProductId = 5;

UPDATE Products SET
    Title = N'Tranh Gỗ Khắc Laser Trang Trí Tường Theo Yêu Cầu',
    Description = N'Tranh gỗ tự nhiên khắc laser theo thiết kế hoặc hình ảnh yêu cầu, hoàn thiện bằng dầu tự nhiên an toàn. Nhiều kích thước từ nhỏ đến lớn để trang trí không gian sống.'
WHERE ProductId = 6;

UPDATE Products SET
    Title = N'Móc Khóa Gỗ Khắc Hình Thủ Công',
    Description = N'Móc khóa gỗ tự nhiên được tiện và khắc thủ công theo hình trái tim, hình tròn hoặc cỏ bốn lá. Món quà lưu niệm nhỏ xinh, ý nghĩa.'
WHERE ProductId = 7;

UPDATE Products SET
    Title = N'Tranh Đông Hồ Hoa Lá Khổ Lớn',
    Description = N'Tranh in giclée trên giấy mỹ thuật theo phong cách dân gian Đông Hồ với họa tiết hoa lá. Phiên bản giới hạn, có chữ ký và đánh số.'
WHERE ProductId = 8;

UPDATE Products SET
    Title = N'Tranh Chân Dung Thú Cưng Vẽ Theo Phong Cách Dân Gian',
    Description = N'Tranh chân dung thú cưng được vẽ theo yêu cầu, lấy cảm hứng từ phong cách tranh dân gian Việt Nam. Giao file kỹ thuật số độ phân giải cao, có thể chọn in khung khi đặt hàng.'
WHERE ProductId = 9;

UPDATE Products SET
    Title = N'Tranh Hoa Ép Khung Gỗ Handmade',
    Description = N'Tranh hoa ép thủ công từ hoa lan, hoa nhài hoặc hoa hồng tươi, ép khô và lồng trong khung gỗ hoặc khung nhựa. Sẵn sàng để treo trang trí.'
WHERE ProductId = 10;

UPDATE Products SET
    Title = N'Khung Lưu Niệm Hoa Cưới Ép Khô',
    Description = N'Lưu giữ bó hoa cưới của bạn dưới dạng khung lưu niệm hoa ép khô tinh tế. Có thể chọn màu khung trắng, nâu hoặc đen, nhận làm theo kích thước yêu cầu.'
WHERE ProductId = 11;

-- 5) Customization group/option labels still in English
UPDATE CustomizationGroups SET Title = N'Dành Cho Ai?' WHERE CustomizationGroupId = 7;

UPDATE CustomizationOptions SET OptionValue = N'Bố' WHERE CustomizationOptionId = 16;
UPDATE CustomizationOptions SET OptionValue = N'Mẹ' WHERE CustomizationOptionId = 17;
UPDATE CustomizationOptions SET OptionValue = N'Ông' WHERE CustomizationOptionId = 18;
UPDATE CustomizationOptions SET OptionValue = N'Bà' WHERE CustomizationOptionId = 19;

-- 6) Seller post sample content
UPDATE SellerPosts SET
    Title = N'Bộ sưu tập khăn len thổ cẩm mùa đông 2026',
    Content = N'Len Mộc Handmade vừa ra mắt bộ sưu tập khăn choàng và mũ len họa tiết thổ cẩm mới cho mùa đông năm nay. Mỗi sản phẩm đều được đan tay tỉ mỉ, sử dụng len cừu cao cấp giữ ấm tốt. Ghé shop để đặt hàng và nhận ưu đãi đặc biệt cho 50 khách đầu tiên!'
WHERE PostId = 2;

-- 7) Review sample content
UPDATE Reviews SET
    Comment = N'Sản phẩm rất đẹp, đóng gói cẩn thận và giao hàng đúng hẹn. Mình rất hài lòng, chắc chắn sẽ ủng hộ shop lần sau!'
WHERE ReviewId = 1;

-- 8) Chat message sample conversation
UPDATE ChatMessages SET Message = N'Chào shop, mình muốn hỏi về sản phẩm khăn len ạ' WHERE MessageId = 1;
UPDATE ChatMessages SET Message = N'Khăn có thể đan thêm họa tiết theo yêu cầu không ạ?' WHERE MessageId = 2;
UPDATE ChatMessages SET Message = N'Chào bạn, shop có nhận đan theo yêu cầu nhé' WHERE MessageId = 3;
UPDATE ChatMessages SET Message = N'Bạn gửi giúp mình mẫu họa tiết và kích thước mong muốn nha' WHERE MessageId = 4;
UPDATE ChatMessages SET Message = N'Thời gian hoàn thành khoảng 5-7 ngày làm việc bạn nhé' WHERE MessageId = 5;
UPDATE ChatMessages SET Message = N'Dạ vâng, mình cảm ơn shop nhiều ạ' WHERE MessageId = 6;
UPDATE ChatMessages SET Message = N'Mình đặt hàng luôn bây giờ nhé' WHERE MessageId = 7;
UPDATE ChatMessages SET Message = N'Chào shop, tranh Đông Hồ khổ 45x90cm còn hàng không ạ?' WHERE MessageId = 8;

-- 9) Notifications - translate the two generated message templates
-- 9a) "New order #X" / "You have received a new order (#X) from <Buyer>."
UPDATE n
SET n.Title = N'Đơn hàng mới #' + CAST(n.OrderId AS NVARCHAR(20)),
    n.Message = N'Bạn vừa nhận được đơn hàng mới (#' + CAST(n.OrderId AS NVARCHAR(20)) + N') từ ' + u.FullName + N'.'
FROM Notifications n
JOIN Orders o ON n.OrderId = o.OrderId
JOIN Buyers b ON o.BuyerId = b.BuyerId
JOIN Users u ON b.BuyerId = u.UserId
WHERE n.Title LIKE 'New order #%';

-- 9b) "Order placed #X" / "Your order (#X) has been placed and sent to the seller <Shop>."
UPDATE n
SET n.Title = N'Đặt hàng thành công #' + CAST(n.OrderId AS NVARCHAR(20)),
    n.Message = N'Đơn hàng (#' + CAST(n.OrderId AS NVARCHAR(20)) + N') của bạn đã được đặt và gửi đến cửa hàng ' + ISNULL(s.ShopName, su.FullName) + N'.'
FROM Notifications n
JOIN Orders o ON n.OrderId = o.OrderId
JOIN Sellers s ON o.SellerId = s.SellerId
JOIN Users su ON s.SellerId = su.UserId
WHERE n.Title LIKE 'Order placed #%';

COMMIT;

-- Verification
SELECT CategoryId, Name FROM Categories ORDER BY CategoryId;
SELECT ProductId, Title, Price FROM Products ORDER BY ProductId;
SELECT SellerId, ShopName FROM Sellers ORDER BY SellerId;
SELECT TOP 5 NotificationId, Title, Message FROM Notifications ORDER BY NotificationId;
