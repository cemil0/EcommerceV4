USE ECommerceDB;
GO

SET QUOTED_IDENTIFIER ON;
GO

-- Assign Admin role to cemil@example.com
DECLARE @UserId NVARCHAR(450) = 'c0358afc-f911-44ee-9930-53cca17be743';
DECLARE @AdminRoleId NVARCHAR(450) = 'd950e034-93ce-4466-9945-38fe5b2e9a15';

-- Assign role
IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @AdminRoleId)
BEGIN
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@UserId, @AdminRoleId);
    PRINT 'Admin role assigned successfully!';
END

-- Show result
SELECT u.Email, r.Name as Role 
FROM AspNetUsers u 
JOIN AspNetUserRoles ur ON u.Id = ur.UserId 
JOIN AspNetRoles r ON ur.RoleId = r.Id 
WHERE u.Email = 'cemil@example.com';
GO
