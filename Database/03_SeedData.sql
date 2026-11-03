/* =========================================================================================
   Author:          Sailen Mondal
   Create Date:     2026-11-03
   Description:     Seed Data for Food Delivery Application.
                    Populates Categories, authentic Indian and fusion MenuItems with ₹ pricing,
                    default Admin and Customer accounts, and initial seed orders.
   Database/Module: FoodDeliveryDb / dbo
   -----------------------------------------------------------------------------------------
   Revision History:
   Date         Author          Description
   ----------   -------------   ------------------------------------------------------------
   2026-11-03   Sailen Mondal   Initial creation.
   ========================================================================================= */

USE FoodDeliveryDb;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

SET NOCOUNT ON;

-- 1. Insert Categories
IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
BEGIN
    INSERT INTO dbo.Categories (Name, DisplayOrder, IsActive) VALUES
    (N'Biryani & Rice Bowls', 1, 1),
    (N'Pizzas & Breads', 2, 1),
    (N'Burgers & Wraps', 3, 1),
    (N'Indian Curries', 4, 1),
    (N'Snacks & Starters', 5, 1),
    (N'Beverages & Desserts', 6, 1);
END
GO

-- 2. Insert MenuItems (Indian & Popular Dishes)
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems)
BEGIN
    DECLARE @CatBiryani INT = (SELECT CategoryId FROM dbo.Categories WHERE Name = N'Biryani & Rice Bowls');
    DECLARE @CatPizza INT = (SELECT CategoryId FROM dbo.Categories WHERE Name = N'Pizzas & Breads');
    DECLARE @CatBurger INT = (SELECT CategoryId FROM dbo.Categories WHERE Name = N'Burgers & Wraps');
    DECLARE @CatCurry INT = (SELECT CategoryId FROM dbo.Categories WHERE Name = N'Indian Curries');
    DECLARE @CatSnack INT = (SELECT CategoryId FROM dbo.Categories WHERE Name = N'Snacks & Starters');
    DECLARE @CatDrink INT = (SELECT CategoryId FROM dbo.Categories WHERE Name = N'Beverages & Desserts');

    INSERT INTO dbo.MenuItems (CategoryId, Name, Description, Price, ImageUrl, IsVeg, IsAvailable, Rating, PrepTimeMinutes) VALUES
    -- Biryani & Rice
    (@CatBiryani, N'Hyderabadi Chicken Dum Biryani', N'Slow-cooked fragrant basmati rice with succulent marinated chicken, saffron and authentic spices.', 299.00, N'/Content/images/dishes/dish-1.jpg', 0, 1, 4.8, 25),
    (@CatBiryani, N'Royal Paneer Tikka Biryani', N'Layers of spiced long grain basmati rice, tandoori charred paneer cubes and caramelized onions.', 249.00, N'/Content/images/dishes/dish-2.jpg', 1, 1, 4.7, 20),
    
    -- Pizzas & Breads
    (@CatPizza, N'Farmhouse Supreme Veg Pizza', N'Hand-tossed crust topped with spiced tomato sauce, mozzarella, bell peppers, sweet corn, and black olives.', 289.00, N'/Content/images/dishes/dish-3.jpg', 1, 1, 4.7, 20),
    (@CatPizza, N'Classic Margherita Pizza', N'Crispy crust loaded with San Marzano tomato puree, creamy fresh mozzarella, and aromatic basil leaves.', 219.00, N'/Content/images/dishes/dish-4.jpg', 1, 1, 4.6, 15),
    (@CatPizza, N'Spicy Chicken Pepperoni Pizza', N'Generously layered with smoked chicken pepperoni slices, gooey melted cheese, and chili flakes.', 349.00, N'/Content/images/dishes/dish-5.jpg', 0, 1, 4.9, 22),
    (@CatPizza, N'Cheese Garlic Breadsticks', N'Freshly baked artisan breadsticks stuffed with herbs, garlic butter, and overflowing cheddar cheese.', 139.00, N'/Content/images/dishes/dish-6.jpg', 1, 1, 4.5, 12),

    -- Burgers & Wraps
    (@CatBurger, N'Crispy Chicken Zinger Burger', N'Signature golden crispy chicken breast fillet, pickled gherkins, creamy mayo, and crunchy iceberg lettuce.', 189.00, N'/Content/images/dishes/dish-7.jpg', 0, 1, 4.8, 15),
    (@CatBurger, N'Maharaja Double Veg Burger', N'Double spiced potato & corn patties, cheddar cheese slice, fresh tomatoes, and spicy tandoori mayo.', 159.00, N'/Content/images/dishes/dish-8.jpg', 1, 1, 4.6, 15),
    (@CatBurger, N'Kolkata Kathi Chicken Roll', N'Flaky paratha wrap filled with charred chicken tikka pieces, crisp red onions, green chilies, and lime juice.', 149.00, N'/Content/images/dishes/dish-9.jpg', 0, 1, 4.7, 12),

    -- Indian Curries
    (@CatCurry, N'Butter Chicken with 2 Naan', N'Tender roasted chicken simmered in a velvety buttery tomato gravy, served with fluffy garlic butter naan.', 319.00, N'/Content/images/dishes/dish-10.jpg', 0, 1, 4.9, 25),
    (@CatCurry, N'Paneer Butter Masala Combo', N'Soft paneer cubes in rich cashew-tomato makhani gravy with aromatic spices, served with 2 butter rotis.', 249.00, N'/Content/images/dishes/dish-11.jpg', 1, 1, 4.8, 20),
    (@CatCurry, N'Dal Makhani Royal Meal Box', N'Slow-cooked black lentils simmered overnight with cream and white butter, served with jeera rice.', 199.00, N'/Content/images/dishes/dish-12.jpg', 1, 1, 4.7, 18),

    -- Snacks & Starters
    (@CatSnack, N'Delhi Style Samosa Chaat (2 pcs)', N'Crushed crispy potato samosas topped with spicy chole, tangy tamarind chutney, mint dip, and crunchy sev.', 89.00, N'/Content/images/dishes/dish-13.jpg', 1, 1, 4.6, 10),
    (@CatSnack, N'Peri Peri Crinkle Fries', N'Extra crispy crinkle-cut golden potatoes tossed in hot African bird''s eye chili seasoning with garlic dip.', 119.00, N'/Content/images/dishes/dish-14.jpg', 1, 1, 4.5, 10),

    -- Beverages & Desserts
    (@CatDrink, N'Punjabi Mango Lassi (Chilled 350ml)', N'Thick sweet dahi blended with fresh Alphonso mango pulp and cardamom, garnished with chopped pistachios.', 99.00, N'/Content/images/dishes/dish-15.jpg', 1, 1, 4.9, 5),
    (@CatDrink, N'Cold Coffee with Vanilla Ice Cream', N'Rich chilled espresso shaken with whole milk and topped with a scoop of Madagascar vanilla bean ice cream.', 129.00, N'/Content/images/dishes/dish-16.jpg', 1, 1, 4.8, 5),
    (@CatDrink, N'Warm Gulab Jamun with Rabdi (2 pcs)', N'Melt-in-mouth milk dumplings soaked in saffron sugar syrup, accompanied by thickened malai rabdi.', 119.00, N'/Content/images/dishes/dish-17.jpg', 1, 1, 4.9, 5);
END
GO

-- 3. Insert Users (Admin and Customers)
-- Salt: Rm9vZGx5S2l0Y2hlblNhbHRfMjAyNg== (FoodlyKitchenSalt_2026)
-- Admin Hash (for AdminPassword@123): tzVeqodv0mhJbggC8X9t7JJLcVIOWCd717r+OkSMre8=
-- Customer Hash (for Password@123): tdx+qBXOc4qvqTfhkBc87eezfpgGvjFXHa3U9y4DakA=
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'admin@foodly.in')
BEGIN
    INSERT INTO dbo.Users (FullName, Email, PasswordHash, PasswordSalt, PhoneNumber, Address, Role, CreatedAt)
    VALUES (N'Store Administrator', N'admin@foodly.in', N'tzVeqodv0mhJbggC8X9t7JJLcVIOWCd717r+OkSMre8=', N'Rm9vZGx5S2l0Y2hlblNhbHRfMjAyNg==', N'9876543200', N'Foodly HQ, MG Road, Bengaluru, Karnataka 560001', N'Admin', GETDATE());
END
GO

-- Also insert admin@example.com alias to match the PDF mockups
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'admin@example.com')
BEGIN
    INSERT INTO dbo.Users (FullName, Email, PasswordHash, PasswordSalt, PhoneNumber, Address, Role, CreatedAt)
    VALUES (N'Admin Manager', N'admin@example.com', N'tzVeqodv0mhJbggC8X9t7JJLcVIOWCd717r+OkSMre8=', N'Rm9vZGx5S2l0Y2hlblNhbHRfMjAyNg==', N'9876543201', N'Central Hub, Sector 18, Noida, Uttar Pradesh 201301', N'Admin', GETDATE());
END
GO

-- Sample Customers
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'rahul@example.in')
BEGIN
    INSERT INTO dbo.Users (FullName, Email, PasswordHash, PasswordSalt, PhoneNumber, Address, Role, CreatedAt)
    VALUES (N'Rahul Sharma', N'rahul@example.in', N'tdx+qBXOc4qvqTfhkBc87eezfpgGvjFXHa3U9y4DakA=', N'Rm9vZGx5S2l0Y2hlblNhbHRfMjAyNg==', N'9876543210', N'Flat 402, Green Glen Layout, Bellandur, Bengaluru, Karnataka 560103', N'Customer', GETDATE());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'priya@example.in')
BEGIN
    INSERT INTO dbo.Users (FullName, Email, PasswordHash, PasswordSalt, PhoneNumber, Address, Role, CreatedAt)
    VALUES (N'Priya Patel', N'priya@example.in', N'tdx+qBXOc4qvqTfhkBc87eezfpgGvjFXHa3U9y4DakA=', N'Rm9vZGx5S2l0Y2hlblNhbHRfMjAyNg==', N'9823456781', N'B-12, Mayur Colony, Kothrud, Pune, Maharashtra 411038', N'Customer', GETDATE());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'john@example.com')
BEGIN
    INSERT INTO dbo.Users (FullName, Email, PasswordHash, PasswordSalt, PhoneNumber, Address, Role, CreatedAt)
    VALUES (N'John Doe', N'john@example.com', N'tdx+qBXOc4qvqTfhkBc87eezfpgGvjFXHa3U9y4DakA=', N'Rm9vZGx5S2l0Y2hlblNhbHRfMjAyNg==', N'9911223344', N'House 15, Lake View Road, Indiranagar, Bengaluru, Karnataka 560038', N'Customer', GETDATE());
END
GO

-- 4. Insert Initial Seed Orders for Today
IF NOT EXISTS (SELECT 1 FROM dbo.Orders)
BEGIN
    DECLARE @RahulId INT = (SELECT UserId FROM dbo.Users WHERE Email = N'rahul@example.in');
    DECLARE @PriyaId INT = (SELECT UserId FROM dbo.Users WHERE Email = N'priya@example.in');
    DECLARE @JohnId INT = (SELECT UserId FROM dbo.Users WHERE Email = N'john@example.com');

    DECLARE @ItemBiryaniId INT = (SELECT ItemId FROM dbo.MenuItems WHERE Name = N'Hyderabadi Chicken Dum Biryani');
    DECLARE @ItemPizzaId INT = (SELECT ItemId FROM dbo.MenuItems WHERE Name = N'Classic Margherita Pizza');
    DECLARE @ItemFriesId INT = (SELECT ItemId FROM dbo.MenuItems WHERE Name = N'Peri Peri Crinkle Fries');
    DECLARE @ItemLassiId INT = (SELECT ItemId FROM dbo.MenuItems WHERE Name = N'Punjabi Mango Lassi (Chilled 350ml)');

    -- Order 1: Recent Pending Order (placed 2 mins ago)
    INSERT INTO dbo.Orders (UserId, DeliveryAddress, ContactPhone, PaymentMethod, TotalAmount, Status, PlacedAt)
    VALUES (@RahulId, N'Flat 402, Green Glen Layout, Bellandur, Bengaluru', N'9876543210', N'Cash on Delivery', 518.00, N'Pending', DATEADD(MINUTE, -2, GETDATE()));
    DECLARE @Ord1 INT = SCOPE_IDENTITY();

    INSERT INTO dbo.OrderItems (OrderId, ItemId, ItemName, UnitPrice, Quantity) VALUES
    (@Ord1, @ItemBiryaniId, N'Hyderabadi Chicken Dum Biryani', 299.00, 1),
    (@Ord1, @ItemPizzaId, N'Classic Margherita Pizza', 219.00, 1);

    -- Order 2: Accepted Order (placed 25 mins ago)
    INSERT INTO dbo.Orders (UserId, DeliveryAddress, ContactPhone, PaymentMethod, TotalAmount, Status, PlacedAt, UpdatedAt)
    VALUES (@PriyaId, N'B-12, Mayur Colony, Kothrud, Pune', N'9823456781', N'UPI on Delivery', 218.00, N'Accepted', DATEADD(MINUTE, -25, GETDATE()), DATEADD(MINUTE, -20, GETDATE()));
    DECLARE @Ord2 INT = SCOPE_IDENTITY();

    INSERT INTO dbo.OrderItems (OrderId, ItemId, ItemName, UnitPrice, Quantity) VALUES
    (@Ord2, @ItemFriesId, N'Peri Peri Crinkle Fries', 119.00, 1),
    (@Ord2, @ItemLassiId, N'Punjabi Mango Lassi (Chilled 350ml)', 99.00, 1);

    -- Order 3: An older pending order placed 8 mins ago (ready to test 5-min auto-rejection mechanism!)
    INSERT INTO dbo.Orders (UserId, DeliveryAddress, ContactPhone, PaymentMethod, TotalAmount, Status, PlacedAt)
    VALUES (@JohnId, N'House 15, Lake View Road, Indiranagar, Bengaluru', N'9911223344', N'Cash on Delivery', 299.00, N'Pending', DATEADD(MINUTE, -8, GETDATE()));
    DECLARE @Ord3 INT = SCOPE_IDENTITY();

    INSERT INTO dbo.OrderItems (OrderId, ItemId, ItemName, UnitPrice, Quantity) VALUES
    (@Ord3, @ItemBiryaniId, N'Hyderabadi Chicken Dum Biryani', 299.00, 1);
END
GO
