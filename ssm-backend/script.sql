select * from 'Users where "Email" = 'admin@smartsociety.com';

select * from "Users";

DELETE FROM "Announcements";
DELETE FROM "Bookings";
DELETE FROM "MaintenanceBills";
DELETE FROM "Notifications";
DELETE FROM "VisitorEntries";
DELETE FROM "Visitors";
DELETE FROM "Residents";
DELETE FROM "Facilities";
DELETE FROM "Apartments";

DELETE FROM "Users"
WHERE "Email" != 'admin@smartsociety.com';


UPDATE "MaintenanceBills" SET "DueDate" = '2025-01-01' WHERE "Id" = 'a91ccc3f-5f93-4b2e-a9df-ec753a94adad';

