/* =========================================================================================
   Author:          Sailen Mondal
   Create Date:     2026-11-03
   Description:     Stored Procedures for Food Delivery Application covering User authentication,
                    Category and MenuItem management, and Order placement, auto-rejection,
                    and status transitions.
   Database/Module: FoodDeliveryDb / dbo
   -----------------------------------------------------------------------------------------
   Revision History:
   Date         Author          Description
   ----------   -------------   ------------------------------------------------------------
   2026-11-03   Sailen Mondal   Initial creation.
   2026-11-03   Sailen Mondal   Updated ItemsSummary formatting to Unicode Rupee symbol (₹) and enhanced midnight boundary handling.
   ========================================================================================= */

USE FoodDeliveryDb;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- 1. usp_User_Register
CREATE OR ALTER PROCEDURE dbo.usp_User_Register
    @FullName NVARCHAR(100),
    @Email NVARCHAR(150),
    @PasswordHash NVARCHAR(256),
    @PasswordSalt NVARCHAR(256),
    @PhoneNumber NVARCHAR(20),
    @Address NVARCHAR(300),
    @Role NVARCHAR(20) = 'Customer',
    @NewUserId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email)
        BEGIN
            SET @NewUserId = -1; -- Indicates email already registered
            RETURN;
        END

        INSERT INTO dbo.Users (FullName, Email, PasswordHash, PasswordSalt, PhoneNumber, Address, Role, CreatedAt)
        VALUES (@FullName, @Email, @PasswordHash, @PasswordSalt, @PhoneNumber, @Address, @Role, GETDATE());

        SET @NewUserId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- 2. usp_User_GetByEmail
CREATE OR ALTER PROCEDURE dbo.usp_User_GetByEmail
    @Email NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT 
            UserId,
            FullName,
            Email,
            PasswordHash,
            PasswordSalt,
            PhoneNumber,
            Address,
            Role,
            CreatedAt
        FROM dbo.Users
        WHERE Email = @Email;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- 3. usp_User_GetById
CREATE OR ALTER PROCEDURE dbo.usp_User_GetById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT 
            UserId,
            FullName,
            Email,
            PhoneNumber,
            Address,
            Role,
            CreatedAt
        FROM dbo.Users
        WHERE UserId = @UserId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- 4. usp_Category_GetAll
CREATE OR ALTER PROCEDURE dbo.usp_Category_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT 
            CategoryId,
            Name,
            DisplayOrder,
            IsActive
        FROM dbo.Categories
        WHERE IsActive = 1
        ORDER BY DisplayOrder ASC, Name ASC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- 5. usp_MenuItem_GetAll
CREATE OR ALTER PROCEDURE dbo.usp_MenuItem_GetAll
    @CategoryId INT = NULL,
    @IsVeg BIT = NULL,
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT 
            m.ItemId,
            m.CategoryId,
            c.Name AS CategoryName,
            m.Name,
            m.Description,
            m.Price,
            m.ImageUrl,
            m.IsVeg,
            m.IsAvailable,
            m.Rating,
            m.PrepTimeMinutes,
            m.CreatedAt
        FROM dbo.MenuItems m
        INNER JOIN dbo.Categories c ON m.CategoryId = c.CategoryId
        WHERE m.IsAvailable = 1
          AND (@CategoryId IS NULL OR m.CategoryId = @CategoryId)
          AND (@IsVeg IS NULL OR m.IsVeg = @IsVeg)
          AND (@SearchTerm IS NULL OR m.Name LIKE '%' + @SearchTerm + '%' OR m.Description LIKE '%' + @SearchTerm + '%')
        ORDER BY m.ItemId ASC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- 6. usp_MenuItem_GetAdminAll
CREATE OR ALTER PROCEDURE dbo.usp_MenuItem_GetAdminAll
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT 
            m.ItemId,
            m.CategoryId,
            c.Name AS CategoryName,
            m.Name,
            m.Description,
            m.Price,
            m.ImageUrl,
            m.IsVeg,
            m.IsAvailable,
            m.Rating,
            m.PrepTimeMinutes,
            m.CreatedAt
        FROM dbo.MenuItems m
        INNER JOIN dbo.Categories c ON m.CategoryId = c.CategoryId
        ORDER BY m.CategoryId ASC, m.ItemId ASC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- 7. usp_MenuItem_GetById
CREATE OR ALTER PROCEDURE dbo.usp_MenuItem_GetById
    @ItemId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT 
            m.ItemId,
            m.CategoryId,
            c.Name AS CategoryName,
            m.Name,
            m.Description,
            m.Price,
            m.ImageUrl,
            m.IsVeg,
            m.IsAvailable,
            m.Rating,
            m.PrepTimeMinutes,
            m.CreatedAt
        FROM dbo.MenuItems m
        INNER JOIN dbo.Categories c ON m.CategoryId = c.CategoryId
        WHERE m.ItemId = @ItemId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- 8. usp_MenuItem_ToggleAvailability
CREATE OR ALTER PROCEDURE dbo.usp_MenuItem_ToggleAvailability
    @ItemId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.MenuItems
        SET IsAvailable = CASE WHEN IsAvailable = 1 THEN 0 ELSE 1 END
        WHERE ItemId = @ItemId;

        SELECT IsAvailable FROM dbo.MenuItems WHERE ItemId = @ItemId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- 9. usp_MenuItem_UpdatePrice
CREATE OR ALTER PROCEDURE dbo.usp_MenuItem_UpdatePrice
    @ItemId INT,
    @NewPrice DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.MenuItems
        SET Price = @NewPrice
        WHERE ItemId = @ItemId;

        SELECT @@ROWCOUNT AS RowsAffected;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- 10. usp_Order_Create
-- Ingests multi-item payload via XML in an atomic transaction
CREATE OR ALTER PROCEDURE dbo.usp_Order_Create
    @UserId INT,
    @DeliveryAddress NVARCHAR(300),
    @ContactPhone NVARCHAR(20),
    @PaymentMethod NVARCHAR(50),
    @TotalAmount DECIMAL(10,2),
    @OrderItemsXml XML,
    @NewOrderId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.Orders (
            UserId, 
            DeliveryAddress, 
            ContactPhone, 
            PaymentMethod, 
            TotalAmount, 
            Status, 
            PlacedAt
        )
        VALUES (
            @UserId, 
            @DeliveryAddress, 
            @ContactPhone, 
            @PaymentMethod, 
            @TotalAmount, 
            'Pending', 
            GETDATE()
        );

        SET @NewOrderId = SCOPE_IDENTITY();

        INSERT INTO dbo.OrderItems (
            OrderId, 
            ItemId, 
            ItemName, 
            UnitPrice, 
            Quantity
        )
        SELECT 
            @NewOrderId,
            T.c.value('@ItemId', 'INT'),
            T.c.value('@ItemName', 'NVARCHAR(150)'),
            T.c.value('@UnitPrice', 'DECIMAL(10,2)'),
            T.c.value('@Quantity', 'INT')
        FROM @OrderItemsXml.nodes('/items/item') AS T(c);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- 11. usp_Order_GetTodayOrdersByStatus
-- Retrieves today's orders partitioned by status ('Pending', 'Accepted', 'Rejected')
CREATE OR ALTER PROCEDURE dbo.usp_Order_GetTodayOrdersByStatus
    @Status NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT 
            o.OrderId,
            o.UserId,
            u.FullName AS CustomerName,
            u.Email AS CustomerEmail,
            o.ContactPhone,
            o.DeliveryAddress,
            o.PaymentMethod,
            o.TotalAmount,
            o.Status,
            o.PlacedAt,
            o.UpdatedAt,
            ItemsSummary = STRING_AGG(CONCAT(oi.ItemName, N' x ', oi.Quantity, N' (', NCHAR(8377), FORMAT(oi.UnitPrice, '0.##'), N')'), N', ')
        FROM dbo.Orders o
        INNER JOIN dbo.Users u ON o.UserId = u.UserId
        LEFT JOIN dbo.OrderItems oi ON o.OrderId = oi.OrderId
        WHERE o.Status = @Status
          AND (o.Status = 'Pending' OR CAST(ISNULL(o.UpdatedAt, o.PlacedAt) AS DATE) = CAST(GETDATE() AS DATE))
        GROUP BY 
            o.OrderId, 
            o.UserId, 
            u.FullName, 
            u.Email, 
            o.ContactPhone, 
            o.DeliveryAddress, 
            o.PaymentMethod, 
            o.TotalAmount, 
            o.Status, 
            o.PlacedAt, 
            o.UpdatedAt
        ORDER BY o.PlacedAt DESC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- 12. usp_Order_UpdateStatus
-- Updates an order's status to 'Accepted' or 'Rejected' only if it is currently 'Pending'
CREATE OR ALTER PROCEDURE dbo.usp_Order_UpdateStatus
    @OrderId INT,
    @NewStatus NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Orders
        SET Status = @NewStatus,
            UpdatedAt = GETDATE()
        WHERE OrderId = @OrderId
          AND Status = 'Pending';

        SELECT @@ROWCOUNT AS RowsAffected;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- 13. usp_Order_AutoRejectPending
-- Automatically marks pending orders older than 5 minutes as 'Rejected'
CREATE OR ALTER PROCEDURE dbo.usp_Order_AutoRejectPending
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Orders
        SET Status = 'Rejected',
            UpdatedAt = GETDATE()
        WHERE Status = 'Pending'
          AND PlacedAt <= DATEADD(MINUTE, -5, GETDATE());

        SELECT @@ROWCOUNT AS AutoRejectedCount;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- 14. usp_Order_GetCustomerHistory
-- Returns past orders for a specific customer
CREATE OR ALTER PROCEDURE dbo.usp_Order_GetCustomerHistory
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT 
            o.OrderId,
            o.DeliveryAddress,
            o.ContactPhone,
            o.PaymentMethod,
            o.TotalAmount,
            o.Status,
            o.PlacedAt,
            o.UpdatedAt,
            ItemsSummary = STRING_AGG(CONCAT(oi.ItemName, N' x ', oi.Quantity, N' (', NCHAR(8377), FORMAT(oi.UnitPrice, '0.##'), N')'), N', ')
        FROM dbo.Orders o
        LEFT JOIN dbo.OrderItems oi ON o.OrderId = oi.OrderId
        WHERE o.UserId = @UserId
        GROUP BY 
            o.OrderId, 
            o.DeliveryAddress, 
            o.ContactPhone, 
            o.PaymentMethod, 
            o.TotalAmount, 
            o.Status, 
            o.PlacedAt, 
            o.UpdatedAt
        ORDER BY o.PlacedAt DESC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
