
-- ===============================================
-- TRIGGERS - SISTEMA INVENBANK (CORREGIDOS)
-- Propósito: Auditoría automática y validaciones
-- ===============================================

USE InvenBank_db;
GO

-- ===============================================
-- 1. Función auxiliar para obtener el UserId del contexto
-- ===============================================
CREATE OR ALTER FUNCTION fn_GetCurrentUserId()
RETURNS INT
AS
BEGIN
    DECLARE @UserId INT;
    IF EXISTS (SELECT * FROM sys.dm_exec_sessions WHERE session_id = @@SPID AND context_info IS NOT NULL)
    BEGIN
        SELECT @UserId = CAST(context_info AS INT) 
        FROM sys.dm_exec_sessions 
        WHERE session_id = @@SPID;
    END
    RETURN @UserId;
END
GO

-- ===============================================
-- 2. TRIGGERS UpdatedAt
-- ===============================================
CREATE OR ALTER TRIGGER tr_Users_UpdatedAt ON Users AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users SET UpdatedAt = GETDATE()
    FROM Users u INNER JOIN inserted i ON u.Id = i.Id
    WHERE u.UpdatedAt = i.UpdatedAt;
END
GO

CREATE OR ALTER TRIGGER tr_Products_UpdatedAt ON Products AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Products SET UpdatedAt = GETDATE()
    FROM Products p INNER JOIN inserted i ON p.Id = i.Id
    WHERE p.UpdatedAt = i.UpdatedAt;
END
GO

CREATE OR ALTER TRIGGER tr_ProductSuppliers_UpdatedAt ON ProductSuppliers AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE ProductSuppliers SET UpdatedAt = GETDATE()
    FROM ProductSuppliers ps INNER JOIN inserted i ON ps.Id = i.Id
    WHERE ps.UpdatedAt = i.UpdatedAt;
END
GO

CREATE OR ALTER TRIGGER tr_Orders_UpdatedAt ON Orders AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Orders SET UpdatedAt = GETDATE()
    FROM Orders o INNER JOIN inserted i ON o.Id = i.Id
    WHERE o.UpdatedAt = i.UpdatedAt;
END
GO

-- ===============================================
-- 3. Auditoría para Users
-- ===============================================
CREATE OR ALTER TRIGGER tr_Users_Audit
ON Users
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserId INT = dbo.fn_GetCurrentUserId();

    -- Validar que el UserId exista
    IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = @UserId)
    BEGIN
        -- Buscar el ID del usuario "Sistema Automático"
        SELECT @UserId = Id FROM Users WHERE Email = 'sistema@invenbank.local';
    END

    DECLARE @Operation VARCHAR(10);

    IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
        SET @Operation = 'UPDATE';
    ELSE IF EXISTS (SELECT * FROM inserted)
        SET @Operation = 'INSERT';
    ELSE
        SET @Operation = 'DELETE';

    IF @Operation = 'INSERT'
    BEGIN
        INSERT INTO AuditLogs (TableName, Operation, RecordId, UserId, NewValues)
        SELECT 
            'Users', 'INSERT', i.Id, @UserId,
            (SELECT i.Email AS Email, i.FirstName, i.LastName, i.RoleId, i.IsActive FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
        FROM inserted i;
    END

    IF @Operation = 'UPDATE'
    BEGIN
        INSERT INTO AuditLogs (TableName, Operation, RecordId, UserId, OldValues, NewValues)
        SELECT 
            'Users', 'UPDATE', i.Id, @UserId,
            (SELECT d.Email, d.FirstName, d.LastName, d.RoleId, d.IsActive FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
            (SELECT i.Email, i.FirstName, i.LastName, i.RoleId, i.IsActive FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
        FROM inserted i
        INNER JOIN deleted d ON i.Id = d.Id;
    END

    IF @Operation = 'DELETE'
    BEGIN
        INSERT INTO AuditLogs (TableName, Operation, RecordId, UserId, OldValues)
        SELECT 
            'Users', 'DELETE', d.Id, @UserId,
            (SELECT d.Email, d.FirstName, d.LastName, d.RoleId, d.IsActive FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
        FROM deleted d;
    END
END
GO

