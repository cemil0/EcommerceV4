USE ECommerceDB;
GO

SET QUOTED_IDENTIFIER ON;
GO

-- Get user and role IDs
DECLARE @UserId NVARCHAR(450) = '046eef3c-d7f5-4518-8b3b-4b2aa1f71672'; -- admin@test.com
DECLARE @RoleId NVARCHAR(450) = 'd950e034-93ce-4466-9945-38fe5b2e9a15'; -- Admin role

-- Assign role to user if not already assigned
IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
BEGIN
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@UserId, @RoleId);
    PRINT 'Admin role assigned successfully!';
END
ELSE
BEGIN
    PRINT 'User already has Admin role.';
END

-- Show result
SELECT u.Email, r.Name as Role 
FROM AspNetUsers u 
JOIN AspNetUserRoles ur ON u.Id = ur.UserId 
JOIN AspNetRoles r ON ur.RoleId = r.Id 
WHERE u.Email = 'admin@test.com';
GO
