/* =========================================================================================
   Author:          Sailen Mondal
   Create Date:     2026-11-03
   Description:     Database creation and table schema setup for Food Delivery Application.
                    Defines Users, Categories, MenuItems, Orders, and OrderItems tables
                    with relational constraints, indexes, and performance optimizations.
   Database/Module: FoodDeliveryDb / dbo
   -----------------------------------------------------------------------------------------
   Revision History:
   Date         Author          Description
   ----------   -------------   ------------------------------------------------------------
   2026-11-03   Sailen Mondal   Initial creation.
   ========================================================================================= */

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'FoodDeliveryDb')
BEGIN
    CREATE DATABASE FoodDeliveryDb;
END
GO

USE FoodDeliveryDb;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- 1. Users Table
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        UserId INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(150) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
        PasswordHash NVARCHAR(256) NOT NULL,
        PasswordSalt NVARCHAR(256) NOT NULL,
        PhoneNumber NVARCHAR(20) NOT NULL,
        Address NVARCHAR(300) NOT NULL,
        Role NVARCHAR(20) NOT NULL CONSTRAINT DF_Users_Role DEFAULT 'Customer', -- 'Customer' or 'Admin'
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT GETDATE()
    );
END
GO

-- 2. Categories Table
IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories (
        CategoryId INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL CONSTRAINT UQ_Categories_Name UNIQUE,
        DisplayOrder INT NOT NULL CONSTRAINT DF_Categories_DisplayOrder DEFAULT 0,
        IsActive BIT NOT NULL CONSTRAINT DF_Categories_IsActive DEFAULT 1
    );
END
GO

-- 3. MenuItems Table
IF OBJECT_ID('dbo.MenuItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.MenuItems (
        ItemId INT IDENTITY(1,1) PRIMARY KEY,
        CategoryId INT NOT NULL CONSTRAINT FK_MenuItems_Categories FOREIGN KEY REFERENCES dbo.Categories(CategoryId),
        Name NVARCHAR(150) NOT NULL,
        Description NVARCHAR(500) NULL,
        Price DECIMAL(10,2) NOT NULL,
        ImageUrl NVARCHAR(500) NULL,
        IsVeg BIT NOT NULL CONSTRAINT DF_MenuItems_IsVeg DEFAULT 1,
        IsAvailable BIT NOT NULL CONSTRAINT DF_MenuItems_IsAvailable DEFAULT 1,
        Rating DECIMAL(2,1) NOT NULL CONSTRAINT DF_MenuItems_Rating DEFAULT 4.5,
        PrepTimeMinutes INT NOT NULL CONSTRAINT DF_MenuItems_PrepTime DEFAULT 20,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_MenuItems_CreatedAt DEFAULT GETDATE()
    );
END
GO

-- Index on MenuItems for category and availability lookups
IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_MenuItems_Category_Available')
BEGIN
    CREATE NONCLUSTERED INDEX IX_MenuItems_Category_Available 
    ON dbo.MenuItems(CategoryId, IsAvailable)
    INCLUDE (Name, Price, ImageUrl, IsVeg, Rating, PrepTimeMinutes);
END
GO

-- 4. Orders Table
IF OBJECT_ID('dbo.Orders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders (
        OrderId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL CONSTRAINT FK_Orders_Users FOREIGN KEY REFERENCES dbo.Users(UserId),
        DeliveryAddress NVARCHAR(300) NOT NULL,
        ContactPhone NVARCHAR(20) NOT NULL,
        PaymentMethod NVARCHAR(50) NOT NULL CONSTRAINT DF_Orders_PaymentMethod DEFAULT 'Cash on Delivery',
        TotalAmount DECIMAL(10,2) NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Orders_Status DEFAULT 'Pending', -- 'Pending', 'Accepted', 'Rejected'
        PlacedAt DATETIME NOT NULL CONSTRAINT DF_Orders_PlacedAt DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL
    );
END
GO

-- Index on Orders for today's orders & auto-rejection checks
IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Orders_PlacedAt_Status')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Orders_PlacedAt_Status 
    ON dbo.Orders(PlacedAt, Status)
    INCLUDE (UserId, TotalAmount, DeliveryAddress, ContactPhone);
END
GO

-- 5. OrderItems Table
IF OBJECT_ID('dbo.OrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems (
        OrderItemId INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL CONSTRAINT FK_OrderItems_Orders FOREIGN KEY REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
        ItemId INT NOT NULL CONSTRAINT FK_OrderItems_MenuItems FOREIGN KEY REFERENCES dbo.MenuItems(ItemId),
        ItemName NVARCHAR(150) NOT NULL,
        UnitPrice DECIMAL(10,2) NOT NULL,
        Quantity INT NOT NULL,
        TotalPrice AS (UnitPrice * Quantity) PERSISTED
    );
END
GO
