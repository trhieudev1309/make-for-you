-- Seed avatar/cover images for Seller 1 ("Len Mộc Handmade" / Thổ Cẩm Lan Hương)
-- using the static demo assets under wwwroot/images/artisans/1/.

SET NOCOUNT ON;

UPDATE Sellers
SET AvatarUrl = '/images/artisans/1/avatar.webp',
    CoverImageUrl = '/images/artisans/1/cover.jpg'
WHERE SellerId = 1;
