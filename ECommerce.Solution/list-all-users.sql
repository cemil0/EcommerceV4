USE ECommerceDB;
GO

-- Show all users
SELECT Id, Email, UserName, EmailConfirmed, PhoneNumber
FROM AspNetUsers;
GO

-- Show all roles
SELECT Id, Name, NormalizedName
FROM AspNetRoles;
GO
