/* =========================================================================================
   Author:          Sailen Mondal
   Create Date:     2026-11-03
   Description:     Stored procedure to retrieve order details by OrderId with strict row-level
                    authorization check (Anti-IDOR). Returns record only if user is Admin or the order owner.
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

CREATE OR ALTER PROCEDURE dbo.usp_Order_GetById
    @OrderId INT,
    @UserId INT = NULL,
    @IsAdmin BIT = 0
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
    WHERE o.OrderId = @OrderId
      AND (@IsAdmin = 1 OR o.UserId = @UserId);
END
GO
