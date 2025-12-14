USE ECommerceDB;
GO

SET QUOTED_IDENTIFIER ON;
GO

-- Get user ID
DECLARE @UserId NVARCHAR(450);
SELECT @UserId = Id FROM AspNetUsers WHERE Email = 'admin@example.com';

-- Create Admin role if not exists
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());
END

-- Get role ID
DECLARE @RoleId NVARCHAR(450);
SELECT @RoleId = Id FROM AspNetRoles WHERE Name = 'Admin';

-- Assign role to user
IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
BEGIN
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@UserId, @RoleId);
END

-- Show result
SELECT u.Email, r.Name as Role 
FROM AspNetUsers u 
JOIN AspNetUserRoles ur ON u.Id = ur.UserId 
JOIN AspNetRoles r ON ur.RoleId = r.Id 
WHERE u.Email = 'admin@example.com';
GO
