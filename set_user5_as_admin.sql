-- Remove admin role from user 7
DELETE FROM "UserRoles" 
WHERE "UserId" = 7 AND "RoleId" = 2;

-- Verify: Should only show user 5 as admin now
SELECT u."Id", u."FullName", u."Email", r."Name" as "Role"
FROM "Users" u
INNER JOIN "UserRoles" ur ON u."Id" = ur."UserId"
INNER JOIN "Roles" r ON ur."RoleId" = r."Id"
WHERE r."Name" = 'Admin';
