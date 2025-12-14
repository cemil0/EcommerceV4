USE ECommerceDB;
GO

-- Find the user
SELECT Id, Email, UserName 
FROM AspNetUsers 
WHERE Email = 'cemil@example.com';
GO
