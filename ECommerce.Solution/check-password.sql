USE ECommerceDB;
GO

-- Create new admin user with known password
-- Password: Admin123! (hashed)
DECLARE @UserId NVARCHAR(450) = NEWID();
DECLARE @AdminRoleId NVARCHAR(450) = 'd950e034-93ce-4466-9945-38fe5b2e9a15';

-- Insert user (you'll need to hash the password properly via API)
-- For now, let's just verify the existing user's password hash

SELECT Id, Email, PasswordHash 
FROM AspNetUsers 
WHERE Email = 'admin@test.com';
GO
