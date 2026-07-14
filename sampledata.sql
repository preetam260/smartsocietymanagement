-- =========================================================
-- SMART SOCIETY - LEAN TEST DATASET
-- 1 Admin, 3 Owners, 2 Security, 3 Residents + minimal
-- supporting records for every module.
--
-- Roles:
--   1 = Admin, 2 = Resident, 3 = Security, 4 = Owner
-- =========================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ================= USERS (9) =================

INSERT INTO "Users" ("Id","Name","Email","PasswordHash","PhoneNumber","Role","IsActive","IsDeleted","CreatedAt","UpdatedAt")
VALUES (
  gen_random_uuid(), 'Super Admin', 'admin@smartsociety.com',
  '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i',
  '9999999999', 1, true, false, NOW(), NOW()
);

INSERT INTO "Users" ("Id","Name","Email","PasswordHash","PhoneNumber","Role","IsActive","IsDeleted","CreatedAt","UpdatedAt")
VALUES (
  gen_random_uuid(), 'Owner One', 'owner1@example.com',
  '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i',
  '9000000001', 4, true, false, NOW(), NOW()
);

INSERT INTO "Users" ("Id","Name","Email","PasswordHash","PhoneNumber","Role","IsActive","IsDeleted","CreatedAt","UpdatedAt")
VALUES (
  gen_random_uuid(), 'Owner Two', 'owner2@example.com',
  '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i',
  '9000000002', 4, true, false, NOW(), NOW()
);

INSERT INTO "Users" ("Id","Name","Email","PasswordHash","PhoneNumber","Role","IsActive","IsDeleted","CreatedAt","UpdatedAt")
VALUES (
  gen_random_uuid(), 'Owner Three', 'owner3@example.com',
  '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i',
  '9000000003', 4, true, false, NOW(), NOW()
);

INSERT INTO "Users" ("Id","Name","Email","PasswordHash","PhoneNumber","Role","IsActive","IsDeleted","CreatedAt","UpdatedAt")
VALUES (
  gen_random_uuid(), 'Security One', 'security1@example.com',
  '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i',
  '9000000011', 3, true, false, NOW(), NOW()
);

INSERT INTO "Users" ("Id","Name","Email","PasswordHash","PhoneNumber","Role","IsActive","IsDeleted","CreatedAt","UpdatedAt")
VALUES (
  gen_random_uuid(), 'Security Two', 'security2@example.com',
  '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i',
  '9000000012', 3, true, false, NOW(), NOW()
);

INSERT INTO "Users" ("Id","Name","Email","PasswordHash","PhoneNumber","Role","IsActive","IsDeleted","CreatedAt","UpdatedAt")
VALUES (
  gen_random_uuid(), 'Resident One', 'resident1@example.com',
  '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i',
  '9000000021', 2, true, false, NOW(), NOW()
);

INSERT INTO "Users" ("Id","Name","Email","PasswordHash","PhoneNumber","Role","IsActive","IsDeleted","CreatedAt","UpdatedAt")
VALUES (
  gen_random_uuid(), 'Resident Two', 'resident2@example.com',
  '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i',
  '9000000022', 2, true, false, NOW(), NOW()
);

INSERT INTO "Users" ("Id","Name","Email","PasswordHash","PhoneNumber","Role","IsActive","IsDeleted","CreatedAt","UpdatedAt")
VALUES (
  gen_random_uuid(), 'Resident Three', 'resident3@example.com',
  '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i',
  '9000000023', 2, true, false, NOW(), NOW()
);

select 'Users created' as status;
select * from "Users";

-- ================= APARTMENTS (3 - one per owner) =================

INSERT INTO "Apartments"
("Id","OwnerId","Block","Floor","Number","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    (SELECT "Id" FROM "Users" WHERE "Email" = 'owner1@example.com'),
    'A', 1, '101', false, NOW(), NOW();

INSERT INTO "Apartments"
("Id","OwnerId","Block","Floor","Number","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    (SELECT "Id" FROM "Users" WHERE "Email" = 'owner2@example.com'),
    'A', 2, '102', false, NOW(), NOW();

INSERT INTO "Apartments"
("Id","OwnerId","Block","Floor","Number","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    (SELECT "Id" FROM "Users" WHERE "Email" = 'owner3@example.com'),
    'B', 1, '201', false, NOW(), NOW();

select 'Apartments created' as status;
select * from "Apartments";

-- ================= RESIDENTS (3 - one per apartment) =================

INSERT INTO "Residents"
("Id","UserId","ApartmentId","MoveInDate","VehicleNumber","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    u."Id",
    a."Id",
    NOW() - INTERVAL '6 months',
    'TS09AB1234',
    false,
    NOW(),
    NOW()
FROM
(
    SELECT "Id" FROM "Users" WHERE "Email" = 'resident1@example.com' LIMIT 1
) u
CROSS JOIN LATERAL
(
    SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 1 LIMIT 1
) a;

INSERT INTO "Residents"
("Id","UserId","ApartmentId","MoveInDate","VehicleNumber","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    u."Id",
    a."Id",
    NOW() - INTERVAL '3 months',
    'TS09AB5678',
    false,
    NOW(),
    NOW()
FROM
(
    SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1
) u
CROSS JOIN LATERAL
(
    SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 2 LIMIT 1
) a;

INSERT INTO "Residents"
("Id","UserId","ApartmentId","MoveInDate","VehicleNumber","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    u."Id",
    a."Id",
    NOW() - INTERVAL '1 month',
    'TS09AB9012',
    false,
    NOW(),
    NOW()
FROM
(
    SELECT "Id" FROM "Users" WHERE "Email" = 'resident3@example.com' LIMIT 1
) u
CROSS JOIN LATERAL
(
    SELECT "Id" FROM "Apartments" WHERE "Block" = 'B' AND "Floor" = 1 LIMIT 1
) a;

select 'Residents created' as status;
select * from "Residents";

-- ================= FACILITIES (3) =================

INSERT INTO "Facilities"
("Id","Name","Description","HourlyRate","Capacity","IsActive","IsDeleted","CreatedAt","UpdatedAt")
VALUES
(gen_random_uuid(),'Club House','Community events hall',500,100,true,false,NOW(),NOW()),
(gen_random_uuid(),'Gym','Fitness center',200,40,true,false,NOW(),NOW()),
(gen_random_uuid(),'Swimming Pool','Olympic size pool',300,50,true,false,NOW(),NOW());

select 'Facilities created' as status;
select * from "Facilities";

-- ================= BOOKINGS (4) =================

INSERT INTO "Bookings"
("Id","FacilityId","UserId","Date","StartTime","EndTime","TotalCost","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    f."Id",
    u."Id",
    CURRENT_DATE + 2,
    NOW() + INTERVAL '2 hours',
    NOW() + INTERVAL '4 hours',
    1000,
    2,
    false,
    NOW(),
    NOW()
FROM generate_series(1,1) i
CROSS JOIN LATERAL (
    SELECT "Id" FROM "Facilities" WHERE "Name" = 'Club House' LIMIT 1
) f
CROSS JOIN LATERAL (
    SELECT "Id" FROM "Users" WHERE "Email" = 'resident1@example.com' LIMIT 1
) u;

INSERT INTO "Bookings"
("Id","FacilityId","UserId","Date","StartTime","EndTime","TotalCost","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    f."Id",
    u."Id",
    CURRENT_DATE + 1,
    NOW() + INTERVAL '1 hour',
    NOW() + INTERVAL '2 hours',
    200,
    1,
    false,
    NOW(),
    NOW()
FROM generate_series(1,1) i
CROSS JOIN LATERAL (
    SELECT "Id" FROM "Facilities" WHERE "Name" = 'Gym' LIMIT 1
) f
CROSS JOIN LATERAL (
    SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1
) u;

INSERT INTO "Bookings"
("Id","FacilityId","UserId","Date","StartTime","EndTime","TotalCost","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    f."Id",
    u."Id",
    CURRENT_DATE + 3,
    NOW() + INTERVAL '3 hours',
    NOW() + INTERVAL '5 hours',
    600,
    2,
    false,
    NOW(),
    NOW()
FROM generate_series(1,1) i
CROSS JOIN LATERAL (
    SELECT "Id" FROM "Facilities" WHERE "Name" = 'Swimming Pool' LIMIT 1
) f
CROSS JOIN LATERAL (
    SELECT "Id" FROM "Users" WHERE "Email" = 'resident3@example.com' LIMIT 1
) u;

INSERT INTO "Bookings"
("Id","FacilityId","UserId","Date","StartTime","EndTime","TotalCost","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    f."Id",
    u."Id",
    CURRENT_DATE + 5,
    NOW() + INTERVAL '6 hours',
    NOW() + INTERVAL '7 hours',
    200,
    3,
    false,
    NOW(),
    NOW()
FROM generate_series(1,1) i
CROSS JOIN LATERAL (
    SELECT "Id" FROM "Facilities" WHERE "Name" = 'Gym' LIMIT 1
) f
CROSS JOIN LATERAL (
    SELECT "Id" FROM "Users" WHERE "Email" = 'resident1@example.com' LIMIT 1
) u;

select 'Bookings created' as status;
select * from "Bookings";

-- ================= BILLS (3 - one per apartment) =================

INSERT INTO "Bills"
("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    a."Id",
    u."Id",
    'July 2026',
    3000,
    0,
    CURRENT_DATE + 10,
    1,
    false,
    false,
    NOW(),
    NOW()
FROM
(
    SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 1 LIMIT 1
) a
CROSS JOIN LATERAL
(
    SELECT "Id" FROM "Users" WHERE "Email" = 'resident1@example.com' LIMIT 1
) u;

INSERT INTO "Bills"
("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    a."Id",
    u."Id",
    'June 2026',
    3000,
    150,
    CURRENT_DATE - 5,
    3,
    false,
    false,
    NOW(),
    NOW()
FROM
(
    SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 2 LIMIT 1
) a
CROSS JOIN LATERAL
(
    SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1
) u;

INSERT INTO "Bills"
("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    a."Id",
    u."Id",
    'June 2026',
    3000,
    0,
    CURRENT_DATE - 10,
    2,
    false,
    false,
    NOW(),
    NOW()
FROM
(
    SELECT "Id" FROM "Apartments" WHERE "Block" = 'B' AND "Floor" = 1 LIMIT 1
) a
CROSS JOIN LATERAL
(
    SELECT "Id" FROM "Users" WHERE "Email" = 'resident3@example.com' LIMIT 1
) u;

select 'Bills created' as status;
select * from "Bills";

-- ================= ANNOUNCEMENTS (2) =================

INSERT INTO "Announcements"
("Id","Title","Content","Audience","IsPinned","ExpiresAt","IsDeleted","CreatedAt","UpdatedAt")
VALUES
(gen_random_uuid(),'Water Supply Interruption','Water supply will be interrupted on Sunday 10AM-2PM.',2,true, NOW() + INTERVAL '7 days', false, NOW(), NOW()),
(gen_random_uuid(),'Society AGM','Society AGM scheduled for next Sunday, 11AM.',2,false, NOW() + INTERVAL '10 days', false, NOW(), NOW());

select 'Announcements created' as status;
select * from "Announcements";

-- ================= NOTIFICATIONS (6 - 2 per resident) =================

INSERT INTO "Notifications"
("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    u."Id",
    'Bill Due Reminder',
    'Your maintenance bill for July is due on the 15th.',
    false,
    false,
    NOW() - INTERVAL '1 hour',
    NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email" = 'resident1@example.com' LIMIT 1) u;

INSERT INTO "Notifications"
("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    u."Id",
    'Booking Confirmed',
    'Your booking for the Club House is confirmed.',
    true,
    false,
    NOW() - INTERVAL '1 day',
    NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email" = 'resident1@example.com' LIMIT 1) u;

INSERT INTO "Notifications"
("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    u."Id",
    'Payment Failed',
    'Your payment for the June bill could not be processed.',
    false,
    false,
    NOW() - INTERVAL '2 hours',
    NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1) u;

INSERT INTO "Notifications"
("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    u."Id",
    'Booking Pending',
    'Your Gym booking request is awaiting approval.',
    true,
    false,
    NOW() - INTERVAL '3 days',
    NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1) u;

INSERT INTO "Notifications"
("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    u."Id",
    'Payment Successful',
    'Maintenance bill for June paid successfully.',
    true,
    false,
    NOW() - INTERVAL '4 days',
    NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email" = 'resident3@example.com' LIMIT 1) u;

INSERT INTO "Notifications"
("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    u."Id",
    'Visitor Registered',
    'Your visitor has been pre-registered.',
    false,
    false,
    NOW() - INTERVAL '5 hours',
    NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email" = 'resident3@example.com' LIMIT 1) u;

select 'Notifications created' as status;
select * from "Notifications";

-- ================= VISITORS (3 - one per apartment) =================

INSERT INTO "Visitors"
("Id","Name","Email","Purpose","ApartmentId","QrToken","ETA","ExpiresAt","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    'Rahul Sharma',
    'rahul.sharma@example.com',
    'Guest Visit',
    a."Id",
    md5(random()::text),
    NOW() + INTERVAL '1 hour',
    NOW() + INTERVAL '6 hours',
    2,
    false,
    NOW(),
    NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 1 LIMIT 1) a;

INSERT INTO "Visitors"
("Id","Name","Email","Purpose","ApartmentId","QrToken","ETA","ExpiresAt","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    'Priya Nair',
    'priya.nair@example.com',
    'Courier',
    a."Id",
    md5(random()::text),
    NOW() + INTERVAL '2 hours',
    NOW() + INTERVAL '7 hours',
    1,
    false,
    NOW(),
    NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 2 LIMIT 1) a;

INSERT INTO "Visitors"
("Id","Name","Email","Purpose","ApartmentId","QrToken","ETA","ExpiresAt","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    'Zomato Delivery',
    'delivery@example.com',
    'Food Delivery',
    a."Id",
    md5(random()::text),
    NOW() + INTERVAL '30 minutes',
    NOW() + INTERVAL '2 hours',
    2,
    false,
    NOW(),
    NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block" = 'B' AND "Floor" = 1 LIMIT 1) a;

select 'Visitors created' as status;
select * from "Visitors";

-- ================= VISITOR ENTRIES (2) =================

INSERT INTO "VisitorEntries"
("Id","VisitorId","CheckinTime","CheckoutTime","StaffId","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    v."Id",
    NOW() - INTERVAL '2 hours',
    NOW() - INTERVAL '30 minutes',
    s."Id",
    false,
    NOW(),
    NOW()
FROM
(
    SELECT "Id" FROM "Visitors" WHERE "Name" = 'Rahul Sharma' LIMIT 1
) v
CROSS JOIN LATERAL
(
    SELECT "Id" FROM "Users" WHERE "Email" = 'security1@example.com' LIMIT 1
) s;

INSERT INTO "VisitorEntries"
("Id","VisitorId","CheckinTime","CheckoutTime","StaffId","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    v."Id",
    NOW() - INTERVAL '20 minutes',
    NULL,
    s."Id",
    false,
    NOW(),
    NOW()
FROM
(
    SELECT "Id" FROM "Visitors" WHERE "Name" = 'Zomato Delivery' LIMIT 1
) v
CROSS JOIN LATERAL
(
    SELECT "Id" FROM "Users" WHERE "Email" = 'security2@example.com' LIMIT 1
) s;

select 'Visitor Entries created' as status;
select * from "VisitorEntries";

INSERT INTO "Complaints"
("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    u."Id",
    a."Id",
    'Water Leakage',
    'There is a water leak near the kitchen sink.',
    1,
    NULL,
    false,
    NOW(),
    NOW()
FROM
(
    SELECT "Id" FROM "Users" WHERE "Email" = 'resident1@example.com' LIMIT 1
) u
CROSS JOIN LATERAL
(
    SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 1 LIMIT 1
) a;

INSERT INTO "Complaints"
("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    u."Id",
    a."Id",
    'Lift Problem',
    'The lift in Block A is making a loud noise.',
    2,
    'Technician has been assigned.',
    false,
    NOW(),
    NOW()
FROM
(
    SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1
) u
CROSS JOIN LATERAL
(
    SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 2 LIMIT 1
) a;

INSERT INTO "Complaints"
("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","IsDeleted","CreatedAt","UpdatedAt")
SELECT
    gen_random_uuid(),
    u."Id",
    a."Id",
    'Parking Issue',
    'Someone parked in my allotted spot.',
    3,
    'Issue resolved by security.',
    false,
    NOW(),
    NOW()
FROM
(
    SELECT "Id" FROM "Users" WHERE "Email" = 'resident3@example.com' LIMIT 1
) u
CROSS JOIN LATERAL
(
    SELECT "Id" FROM "Apartments" WHERE "Block" = 'B' AND "Floor" = 1 LIMIT 1
) a;

INSERT INTO "Notifications"
("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Bill Overdue', 'Your maintenance bill for June is now overdue.', false, false, NOW() - INTERVAL '5 hours', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1) u;

INSERT INTO "Notifications"
("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Booaking Cancelled', 'Your booking for the Gym on Fri 6AM was cancelled.', true, false, NOW() - INTERVAL '1 day', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1) u;

INSERT INTO "Notifications"
("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Visitor Checked In', 'Your visitor has checked in at the gate.', false, false, NOW() - INTERVAL '2 days', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1) u;

INSERT INTO "Notifications"
("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Complaint Update', 'Your complaint about the lift has been marked In Progress.', false, false, NOW() - INTERVAL '3 days', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1) u;

INSERT INTO "Notifications"
("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'New Announcement', 'Water supply will be interrupted on Sunday 10AM-2PM.', true, false, NOW() - INTERVAL '4 days', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1) u;

INSERT INTO "Notifications"
("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Bill Generated', 'Your maintenance bill for July has been generated.', false, false, NOW() - INTERVAL '6 days', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1) u;

INSERT INTO "Notifications"
("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Visitor Checked Out', 'Your visitor has checked out.', true, false, NOW() - INTERVAL '7 days', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1) u;

INSERT INTO "Notifications"
("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Complaint Received', 'Your complaint about parking has been logged.', true, false, NOW() - INTERVAL '8 days', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1) u;

select 'More notifications created for resident2' as status;

INSERT INTO "Bills"
("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'May 2026', 3000, 0, CURRENT_DATE - 40, 2, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 2 LIMIT 1) a
CROSS JOIN LATERAL (SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1) u;

INSERT INTO "Bills"
("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'April 2026', 2800, 100, CURRENT_DATE - 70, 2, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 2 LIMIT 1) a
CROSS JOIN LATERAL (SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1) u;

INSERT INTO "Bills"
("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'August 2026', 3200, 0, CURRENT_DATE + 30, 1, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 2 LIMIT 1) a
CROSS JOIN LATERAL (SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1) u;

INSERT INTO "Bills"
("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'March 2026', 2900, 0, CURRENT_DATE - 100, 2, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 2 LIMIT 1) a
CROSS JOIN LATERAL (SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1) u;

select 'More bills created for resident2 apartment' as status;

INSERT INTO "Announcements"
("Id","Title","Content","Audience","IsPinned","ExpiresAt","IsDeleted","CreatedAt","UpdatedAt")
VALUES
(gen_random_uuid(),'Diwali Celebration','Society Diwali celebration in the clubhouse this Saturday 6PM.',2,false, NOW() + INTERVAL '5 days', false, NOW(), NOW()),
(gen_random_uuid(),'Fire Drill Notice','Mandatory fire safety drill scheduled for next Tuesday.',2,true, NOW() + INTERVAL '8 days', false, NOW(), NOW()),
(gen_random_uuid(),'Parking Rules Update','Updated parking allocation rules now in effect. Please review.',2,false, NOW() + INTERVAL '15 days', false, NOW(), NOW());

select 'More announcements created' as status;

INSERT INTO "Complaints"
("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", 'Power Issue', 'Frequent power fluctuations in the evening.', 1, NULL, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1) u
CROSS JOIN LATERAL (SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 2 LIMIT 1) a;

INSERT INTO "Complaints"
("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", 'Water Leakage', 'Ceiling seepage near the balcony after rain.', 2, 'Plumber scheduled for inspection.', false, NOW(), NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1) u
CROSS JOIN LATERAL (SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 2 LIMIT 1) a;

INSERT INTO "Complaints"
("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", 'Noise Complaint', 'Loud construction noise early morning on weekends.', 3, 'Issue addressed with the contractor.', false, NOW(), NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email" = 'resident2@example.com' LIMIT 1) u
CROSS JOIN LATERAL (SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 2 LIMIT 1) a;

select 'More complaints created for resident2' as status;

INSERT INTO "Visitors"
("Id","Name","Email","Purpose","ApartmentId","QrToken","ETA","ExpiresAt","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), 'Amit Verma', 'amit.verma@example.com', 'Guest Visit', a."Id", md5(random()::text), NOW() + INTERVAL '1 hour', NOW() + INTERVAL '5 hours', 2, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 2 LIMIT 1) a;

INSERT INTO "Visitors"
("Id","Name","Email","Purpose","ApartmentId","QrToken","ETA","ExpiresAt","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), 'Swiggy Delivery', 'swiggy.delivery@example.com', 'Food Delivery', a."Id", md5(random()::text), NOW() + INTERVAL '20 minutes', NOW() + INTERVAL '1 hour', 3, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 2 LIMIT 1) a;

INSERT INTO "Visitors"
("Id","Name","Email","Purpose","ApartmentId","QrToken","ETA","ExpiresAt","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), 'Amazon Courier', 'amazon.courier@example.com', 'Courier', a."Id", md5(random()::text), NOW() + INTERVAL '3 hours', NOW() + INTERVAL '8 hours', 1, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 2 LIMIT 1) a;

INSERT INTO "Visitors"
("Id","Name","Email","Purpose","ApartmentId","QrToken","ETA","ExpiresAt","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), 'Sneha Reddy', 'sneha.reddy@example.com', 'Guest Visit', a."Id", md5(random()::text), NOW() + INTERVAL '4 hours', NOW() + INTERVAL '9 hours', 2, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block" = 'A' AND "Floor" = 2 LIMIT 1) a;

select 'More visitors created for resident2 apartment' as status;

SELECT 'Additional resident2 data inserted successfully!' AS message;

SELECT * FROM "Bookings";


select * from "Apartments";