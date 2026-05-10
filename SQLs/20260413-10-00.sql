
-- Seed sellers, categories and products for MakeForYou (SQL Server)
-- Run in the application database. This script inserts categories first,
-- then users + sellers, then products referencing sellers and categories.
-- Adjust emails/passwords/images as needed for your environment.

SET NOCOUNT ON;

-- 1) Categories
DECLARE @CatKnitting bigint, @CatPottery bigint, @CatWoodwork bigint, @CatIllustration bigint, @CatFloral bigint;

INSERT INTO dbo.Categories (Name) VALUES (N'Knitting & Textiles');
SET @CatKnitting = SCOPE_IDENTITY();

INSERT INTO dbo.Categories (Name) VALUES (N'Pottery & Ceramics');
SET @CatPottery = SCOPE_IDENTITY();

INSERT INTO dbo.Categories (Name) VALUES (N'Woodwork & Homewares');
SET @CatWoodwork = SCOPE_IDENTITY();

INSERT INTO dbo.Categories (Name) VALUES (N'Illustration & Prints');
SET @CatIllustration = SCOPE_IDENTITY();

INSERT INTO dbo.Categories (Name) VALUES (N'Pressed Flowers & Floral Art');
SET @CatFloral = SCOPE_IDENTITY();


-- 2) Sellers (Users -> Sellers). SellerId == Users.UserId
-- Seller 1: Lina Craftworks (hand-knit scarves, hats)
DECLARE @SellerLina bigint;
INSERT INTO dbo.Users (FullName, Email, PasswordHash, Phone, Role, CreatedAt, Status)
VALUES (N'Lina Craftworks', N'lina@craftworks.example', N'{HASHED_PW_1}', N'+1-555-0101', 2, GETUTCDATE(), 1);
SET @SellerLina = SCOPE_IDENTITY();

INSERT INTO dbo.Sellers (SellerId, Bio, SkillDescription, PriceRange, AvailabilityStatus, AverageRating, TotalReviews)
VALUES (@SellerLina,
        N'Hand-knitter specialising in alpaca and merino scarves and cosy accessories.',
        N'Pattern design, custom colours, small-batch production.',
        2, -- price range index (example)
        1, -- available
        5,
        124);


-- Seller 2: Stone & Clay Studio (pottery)
DECLARE @SellerStone bigint;
INSERT INTO dbo.Users (FullName, Email, PasswordHash, Phone, Role, CreatedAt, Status)
VALUES (N'Stone & Clay Studio', N'hello@stoneandclay.example', N'{HASHED_PW_2}', N'+1-555-0102', 2, GETUTCDATE(), 1);
SET @SellerStone = SCOPE_IDENTITY();

INSERT INTO dbo.Sellers (SellerId, Bio, SkillDescription, PriceRange, AvailabilityStatus, AverageRating, TotalReviews)
VALUES (@SellerStone,
        N'Wheel-thrown and hand-built ceramics inspired by Scandinavian design.',
        N'Functional pottery: mugs, bowls, planters with reactive glazes.',
        3,
        1,
        5,
        89);


-- Seller 3: WillowWood (woodworker)
DECLARE @SellerWillow bigint;
INSERT INTO dbo.Users (FullName, Email, PasswordHash, Phone, Role, CreatedAt, Status)
VALUES (N'WillowWood Workshop', N'shop@willowwood.example', N'{HASHED_PW_3}', N'+1-555-0103', 2, GETUTCDATE(), 1);
SET @SellerWillow = SCOPE_IDENTITY();

INSERT INTO dbo.Sellers (SellerId, Bio, SkillDescription, PriceRange, AvailabilityStatus, AverageRating, TotalReviews)
VALUES (@SellerWillow,
        N'Handmade kitchenware and home accents crafted from reclaimed walnut and maple.',
        N'Boards, utensils, small furniture and personalised engraving available.',
        4,
        1,
        5,
        204);


-- Seller 4: Aurora Prints (illustration & prints)
DECLARE @SellerAurora bigint;
INSERT INTO dbo.Users (FullName, Email, PasswordHash, Phone, Role, CreatedAt, Status)
VALUES (N'Aurora Prints', N'hello@auroraprints.example', N'{HASHED_PW_4}', N'+1-555-0104', 2, GETUTCDATE(), 1);
SET @SellerAurora = SCOPE_IDENTITY();

INSERT INTO dbo.Sellers (SellerId, Bio, SkillDescription, PriceRange, AvailabilityStatus, AverageRating, TotalReviews)
VALUES (@SellerAurora,
        N'Limited edition giclée prints and illustrated stationery.',
        N'Original watercolour and digital prints, custom portrait commissions.',
        2,
        1,
        4,
        57);


-- Seller 5: Bloom & Press (pressed flower frames)
DECLARE @SellerBloom bigint;
INSERT INTO dbo.Users (FullName, Email, PasswordHash, Phone, Role, CreatedAt, Status)
VALUES (N'Bloom & Press', N'orders@bloomandpress.example', N'{HASHED_PW_5}', N'+1-555-0105', 2, GETUTCDATE(), 1);
SET @SellerBloom = SCOPE_IDENTITY();

INSERT INTO dbo.Sellers (SellerId, Bio, SkillDescription, PriceRange, AvailabilityStatus, AverageRating, TotalReviews)
VALUES (@SellerBloom,
        N'Pressed-flower art and botanical home decor made from locally sourced botanicals.',
        N'Custom framed pieces, wedding keepsakes and botanical bookmarks.',
        2,
        1,
        5,
        38);


-- 3) Products
-- Lina Craftworks products
INSERT INTO dbo.Products (SellerId, CategoryId, Title, Description, ImageUrl, Price, CreatedAt)
VALUES 
(@SellerLina, @CatKnitting, N'Alpaca Infinity Scarf - Soft Grey', N'Hand-knitted alpaca infinity scarf. Lightweight, warm with a soft drape. Available in small-batch colours.', N'https://cdn.example.com/images/lina/scarf-grey.jpg', 65, GETUTCDATE()),
(@SellerLina, @CatKnitting, N'Chunky Merino Beanie', N'Chunky knit merino beanie with fold-over cuff. Perfect for winter.', N'https://cdn.example.com/images/lina/beanie-merino.jpg', 35, GETUTCDATE()),
(@SellerLina, @CatKnitting, N'Custom Couple Hats', N'Custom-sized matching hats — choose yarn colour and initials embroidery.', N'https://cdn.example.com/images/lina/couple-hats.jpg', 50, GETUTCDATE());

-- Stone & Clay Studio products
INSERT INTO dbo.Products (SellerId, CategoryId, Title, Description, ImageUrl, Price, CreatedAt)
VALUES
(@SellerStone, @CatPottery, N'Ceramic Coffee Mug - Speckle Glaze', N'12oz wheel-thrown mug with comfortable handle and reactive speckle glaze. Dishwasher safe.', N'https://cdn.example.com/images/stone/mug-speckle.jpg', 28, GETUTCDATE()),
(@SellerStone, @CatPottery, N'Small Planter - Raw Clay', N'Hand-built raw clay planter with drainage hole. Ideal for succulents.', N'https://cdn.example.com/images/stone/planter-small.jpg', 22, GETUTCDATE());

-- WillowWood products
INSERT INTO dbo.Products (SellerId, CategoryId, Title, Description, ImageUrl, Price, CreatedAt)
VALUES
(@SellerWillow, @CatWoodwork, N'Walnut Cutting Board - End Grain', N'End-grain walnut cutting board, oil-finished. Sizes: 12"x8", 16"x10". Custom engraving available.', N'https://cdn.example.com/images/willow/board-walnut.jpg', 95, GETUTCDATE()),
(@SellerWillow, @CatWoodwork, N'Hand-turned Wooden Spoon', N'Comfortable hand-turned beechwood spoon, food-safe finish.', N'https://cdn.example.com/images/willow/spoon.jpg', 18, GETUTCDATE());

-- Aurora Prints products
INSERT INTO dbo.Products (SellerId, CategoryId, Title, Description, ImageUrl, Price, CreatedAt)
VALUES
(@SellerAurora, @CatIllustration, N'Nebula Floral Print - A3', N'Limited edition A3 giclée print on archival paper. Signed and numbered (50).', N'https://cdn.example.com/images/aurora/nebula-floral-a3.jpg', 45, GETUTCDATE()),
(@SellerAurora, @CatIllustration, N'Custom Pet Portrait (Digital)', N'Digital pet portrait commission delivered as high-res PNG/JPEG. Add printed options at checkout.', N'https://cdn.example.com/images/aurora/pet-portrait-sample.jpg', 70, GETUTCDATE());

-- Bloom & Press products
INSERT INTO dbo.Products (SellerId, CategoryId, Title, Description, ImageUrl, Price, CreatedAt)
VALUES
(@SellerBloom, @CatFloral, N'Pressed-Flower Frame - Meadow', N'8"x10" pressed-flower frame featuring seasonal meadow flowers. Ready to hang.', N'https://cdn.example.com/images/bloom/frame-meadow.jpg', 55, GETUTCDATE()),
(@SellerBloom, @CatFloral, N'Wedding Bouquet Keepsake', N'Preserve a small bridal bouquet as a framed keepsake. Custom sizes available.', N'https://cdn.example.com/images/bloom/wedding-keepsake.jpg', 120, GETUTCDATE());


-- Optional: Verify inserts
SELECT TOP(200) p.ProductId, p.Title, p.Price, p.CreatedAt, s.SellerId, u.FullName AS SellerName, c.Name AS Category
FROM dbo.Products p
JOIN dbo.Sellers s ON p.SellerId = s.SellerId
JOIN dbo.Users u ON s.SellerId = u.UserId
LEFT JOIN dbo.Categories c ON p.CategoryId = c.CategoryId
ORDER BY p.CreatedAt DESC;

SET NOCOUNT OFF;

UPDATE Users 
SET Role = 2, Status = 0 
WHERE UserId = 10008;

UPDATE Products SET Status = 1;