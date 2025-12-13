-- Replace 'your.email@gmail.com' with your actual Google account email
UPDATE "Users" 
SET 
    "Email" = 'your.email@gmail.com',
    "NormalizedEmail" = 'YOUR.EMAIL@GMAIL.COM',
    "UserName" = 'your.email@gmail.com',
    "NormalizedUserName" = 'YOUR.EMAIL@GMAIL.COM'
WHERE "Id" = 7;

-- Verify the change
SELECT "Id", "FullName", "Email" FROM "Users" WHERE "Id" = 7;
