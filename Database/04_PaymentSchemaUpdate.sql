/* =========================================================================================
   Author:          Sailen Mondal
   Create Date:     2026-11-03
   Description:     Database schema upgrade adding payment status, transaction reference IDs,
                    masked payment metadata, and idempotency constraints to dbo.Orders, along
                    with updated stored procedures for secure order creation and retrieval.
   Database/Module: FoodDeliveryDb / dbo
   -----------------------------------------------------------------------------------------
   Revision History:
   Date         Author          Description
   ----------   -------------   ------------------------------------------------------------
   2026-11-03   Sailen Mondal   Initial creation of payment schema update and stored procedures.
   ========================================================================================= */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE FoodDeliveryDb;
GO

SET NOCOUNT ON;

-- 1. Add Payment Columns to dbo.Orders
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Orders') AND name = 'PaymentStatus')
BEGIN
    ALTER TABLE dbo.Orders ADD PaymentStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_Orders_PaymentStatus DEFAULT 'Pending';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Orders') AND name = 'PaymentTransactionId')
BEGIN
    ALTER TABLE dbo.Orders ADD PaymentTransactionId NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Orders') AND name = 'MaskedPaymentDetails')
BEGIN
    ALTER TABLE dbo.Orders ADD MaskedPaymentDetails NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Orders') AND name = 'IdempotencyKey')
BEGIN
    ALTER TABLE dbo.Orders ADD IdempotencyKey NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'UQ_Orders_IdempotencyKey' AND object_id = OBJECT_ID('dbo.Orders'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Orders_IdempotencyKey 
    ON dbo.Orders(IdempotencyKey) 
    WHERE IdempotencyKey IS NOT NULL;
END
GO

-- 2. Upgrade dbo.usp_Order_Create Stored Procedure
CREATE OR ALTER PROCEDURE dbo.usp_Order_Create
    @UserId INT,
    @DeliveryAddress NVARCHAR(300),
    @ContactPhone NVARCHAR(20),
    @PaymentMethod NVARCHAR(50),
    @PaymentStatus NVARCHAR(20) = 'Pending',
    @PaymentTransactionId NVARCHAR(100) = NULL,
    @MaskedPaymentDetails NVARCHAR(100) = NULL,
    @IdempotencyKey NVARCHAR(100) = NULL,
    @TotalAmount DECIMAL(10,2),
    @OrderItemsXml XML
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Idempotency Check: If already placed with this key, return existing OrderId
    IF @IdempotencyKey IS NOT NULL
    BEGIN
        DECLARE @ExistingOrderId INT = (SELECT TOP 1 OrderId FROM dbo.Orders WHERE IdempotencyKey = @IdempotencyKey);
        IF @ExistingOrderId IS NOT NULL
        BEGIN
            SELECT @ExistingOrderId AS NewOrderId;
            RETURN;
        END
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Insert parent order record
        INSERT INTO dbo.Orders 
        (
            UserId, 
            DeliveryAddress, 
            ContactPhone, 
            PaymentMethod, 
            PaymentStatus,
            PaymentTransactionId,
            MaskedPaymentDetails,
            IdempotencyKey,
            TotalAmount, 
            Status, 
            PlacedAt, 
            UpdatedAt
        )
        VALUES 
        (
            @UserId, 
            @DeliveryAddress, 
            @ContactPhone, 
            @PaymentMethod, 
            @PaymentStatus,
            @PaymentTransactionId,
            @MaskedPaymentDetails,
            @IdempotencyKey,
            @TotalAmount, 
            'Pending', 
            GETDATE(), 
            NULL
        );

        DECLARE @NewOrderId INT = SCOPE_IDENTITY();

        -- Shred XML into order items
        INSERT INTO dbo.OrderItems (OrderId, ItemId, ItemName, UnitPrice, Quantity)
        SELECT 
            @NewOrderId,
            T.Item.value('@ItemId', 'INT'),
            T.Item.value('@ItemName', 'NVARCHAR(150)'),
            T.Item.value('@UnitPrice', 'DECIMAL(10,2)'),
            T.Item.value('@Quantity', 'INT')
        FROM @OrderItemsXml.nodes('/*/*') AS T(Item);

        COMMIT TRANSACTION;

        SELECT @NewOrderId AS NewOrderId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- 3. Upgrade dbo.usp_Order_GetCustomerHistory
CREATE OR ALTER PROCEDURE dbo.usp_Order_GetCustomerHistory
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        o.OrderId,
        o.UserId,
        o.DeliveryAddress,
        o.ContactPhone,
        o.PaymentMethod,
        o.PaymentStatus,
        o.PaymentTransactionId,
        o.MaskedPaymentDetails,
        o.TotalAmount,
        o.Status,
        o.PlacedAt,
        o.UpdatedAt,
        u.FullName AS CustomerName,
        u.Email AS CustomerEmail,
        STUFF((
            SELECT ', ' + CAST(oi.Quantity AS VARCHAR(10)) + 'x ' + oi.ItemName
            FROM dbo.OrderItems oi
            WHERE oi.OrderId = o.OrderId
            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS ItemsSummary
    FROM dbo.Orders o
    INNER JOIN dbo.Users u ON o.UserId = u.UserId
    WHERE o.UserId = @UserId
    ORDER BY o.PlacedAt DESC;
END
GO

-- 4. Upgrade dbo.usp_Order_GetTodayOrdersByStatus
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
            o.PaymentStatus,
            o.PaymentTransactionId,
            o.MaskedPaymentDetails,
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
            o.PaymentStatus,
            o.PaymentTransactionId,
            o.MaskedPaymentDetails,
            o.TotalAmount, 
            o.Status, 
            o.PlacedAt, 
            o.UpdatedAt
        ORDER BY o.PlacedAt DESC;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO
