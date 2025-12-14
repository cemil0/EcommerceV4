USE ECommerceDB;
GO

-- Check if user exists
SELECT Id, Email, UserName, EmailConfirmed 
FROM AspNetUsers 
WHERE Email = 'admin@example.com' OR UserName = 'admin@example.com';
GO
