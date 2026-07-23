-- =========================================================
-- SMART SOCIETY — ENRICHED DEMO DATASET
--
-- Roles:  1 = Admin,  2 = Resident,  3 = Security,  4 = Owner
-- =========================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ===================== USERS (16) =====================
-- Password for all: password123
-- BCrypt hash: $2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i

-- 1 Admin
INSERT INTO "Users" ("Id","Name","Email","PasswordHash","PhoneNumber","Role","IsActive","IsDeleted","CreatedAt","UpdatedAt")
VALUES (gen_random_uuid(), 'Super Admin', 'admin@smartsociety.com',
  '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i',
  '9999999999', 1, true, false, NOW(), NOW());

-- 4 Owners
INSERT INTO "Users" ("Id","Name","Email","PasswordHash","PhoneNumber","Role","IsActive","IsDeleted","CreatedAt","UpdatedAt") VALUES
(gen_random_uuid(), 'Rajesh Kumar',   'owner1@example.com', '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i', '9000000001', 4, true, false, NOW(), NOW()),
(gen_random_uuid(), 'Meena Iyer',     'owner2@example.com', '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i', '9000000002', 4, true, false, NOW(), NOW()),
(gen_random_uuid(), 'Vikram Singh',   'owner3@example.com', '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i', '9000000003', 4, true, false, NOW(), NOW()),
(gen_random_uuid(), 'Sunita Reddy',   'owner4@example.com', '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i', '9000000004', 4, true, false, NOW(), NOW());

-- 3 Security Staff
INSERT INTO "Users" ("Id","Name","Email","PasswordHash","PhoneNumber","Role","IsActive","IsDeleted","CreatedAt","UpdatedAt") VALUES
(gen_random_uuid(), 'Ramesh Guard',   'security1@example.com', '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i', '9000000011', 3, true, false, NOW(), NOW()),
(gen_random_uuid(), 'Sunil Guard',    'security2@example.com', '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i', '9000000012', 3, true, false, NOW(), NOW()),
(gen_random_uuid(), 'Babu Guard',     'security3@example.com', '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i', '9000000013', 3, true, false, NOW(), NOW());

-- 8 Residents
INSERT INTO "Users" ("Id","Name","Email","PasswordHash","PhoneNumber","Role","IsActive","IsDeleted","CreatedAt","UpdatedAt") VALUES
(gen_random_uuid(), 'Arjun Nair',     'resident1@example.com', '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i', '9000000021', 2, true, false, NOW(), NOW()),
(gen_random_uuid(), 'Priya Sharma',   'resident2@example.com', '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i', '9000000022', 2, true, false, NOW(), NOW()),
(gen_random_uuid(), 'Karthik Rao',    'resident3@example.com', '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i', '9000000023', 2, true, false, NOW(), NOW()),
(gen_random_uuid(), 'Divya Menon',    'resident4@example.com', '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i', '9000000024', 2, true, false, NOW(), NOW()),
(gen_random_uuid(), 'Rohit Patel',    'resident5@example.com', '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i', '9000000025', 2, true, false, NOW(), NOW()),
(gen_random_uuid(), 'Ananya Das',     'resident6@example.com', '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i', '9000000026', 2, true, false, NOW(), NOW()),
(gen_random_uuid(), 'Nikhil Joshi',   'resident7@example.com', '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i', '9000000027', 2, true, false, NOW(), NOW()),
(gen_random_uuid(), 'Sneha Kapoor',   'resident8@example.com', '$2a$11$LTCKs7IrEIIT55pyXyxPU.Qe7sLcMF9I9nd/xQPk.aqRf2OLOyV5i', '9000000028', 2, true, false, NOW(), NOW());

SELECT 'Users created' AS status;

-- ===================== APARTMENTS (8 across 3 blocks) =====================

INSERT INTO "Apartments" ("Id","OwnerId","Block","Floor","Number","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), (SELECT "Id" FROM "Users" WHERE "Email"='owner1@example.com'), 'A', 1, '101', false, NOW(), NOW();
INSERT INTO "Apartments" ("Id","OwnerId","Block","Floor","Number","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), (SELECT "Id" FROM "Users" WHERE "Email"='owner1@example.com'), 'A', 2, '201', false, NOW(), NOW();
INSERT INTO "Apartments" ("Id","OwnerId","Block","Floor","Number","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), (SELECT "Id" FROM "Users" WHERE "Email"='owner2@example.com'), 'A', 3, '301', false, NOW(), NOW();
INSERT INTO "Apartments" ("Id","OwnerId","Block","Floor","Number","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), (SELECT "Id" FROM "Users" WHERE "Email"='owner2@example.com'), 'B', 1, '101', false, NOW(), NOW();
INSERT INTO "Apartments" ("Id","OwnerId","Block","Floor","Number","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), (SELECT "Id" FROM "Users" WHERE "Email"='owner3@example.com'), 'B', 2, '201', false, NOW(), NOW();
INSERT INTO "Apartments" ("Id","OwnerId","Block","Floor","Number","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), (SELECT "Id" FROM "Users" WHERE "Email"='owner3@example.com'), 'B', 3, '301', false, NOW(), NOW();
INSERT INTO "Apartments" ("Id","OwnerId","Block","Floor","Number","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), (SELECT "Id" FROM "Users" WHERE "Email"='owner4@example.com'), 'C', 1, '101', false, NOW(), NOW();
INSERT INTO "Apartments" ("Id","OwnerId","Block","Floor","Number","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), (SELECT "Id" FROM "Users" WHERE "Email"='owner4@example.com'), 'C', 2, '201', false, NOW(), NOW();

SELECT 'Apartments created' AS status;

-- ===================== RESIDENTS (8 active) =====================

INSERT INTO "Residents" ("Id","UserId","ApartmentId","MoveInDate","VehicleNumber","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", NOW() - INTERVAL '8 months', 'TS09AB1001', false, NOW(), NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident1@example.com') u
CROSS JOIN LATERAL (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='101') a;

INSERT INTO "Residents" ("Id","UserId","ApartmentId","MoveInDate","VehicleNumber","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", NOW() - INTERVAL '6 months', 'TS09AB2002', false, NOW(), NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident2@example.com') u
CROSS JOIN LATERAL (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='201') a;

INSERT INTO "Residents" ("Id","UserId","ApartmentId","MoveInDate","VehicleNumber","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", NOW() - INTERVAL '4 months', 'TS09AB3003', false, NOW(), NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident3@example.com') u
CROSS JOIN LATERAL (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='301') a;

INSERT INTO "Residents" ("Id","UserId","ApartmentId","MoveInDate","VehicleNumber","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", NOW() - INTERVAL '10 months', 'TS09AB4004', false, NOW(), NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident4@example.com') u
CROSS JOIN LATERAL (SELECT "Id" FROM "Apartments" WHERE "Block"='B' AND "Number"='101') a;

INSERT INTO "Residents" ("Id","UserId","ApartmentId","MoveInDate","VehicleNumber","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", NOW() - INTERVAL '3 months', 'TS09AB5005', false, NOW(), NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident5@example.com') u
CROSS JOIN LATERAL (SELECT "Id" FROM "Apartments" WHERE "Block"='B' AND "Number"='201') a;

INSERT INTO "Residents" ("Id","UserId","ApartmentId","MoveInDate","VehicleNumber","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", NOW() - INTERVAL '5 months', 'TS09AB6006', false, NOW(), NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident6@example.com') u
CROSS JOIN LATERAL (SELECT "Id" FROM "Apartments" WHERE "Block"='B' AND "Number"='301') a;

INSERT INTO "Residents" ("Id","UserId","ApartmentId","MoveInDate","VehicleNumber","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", NOW() - INTERVAL '7 months', 'TS09AB7007', false, NOW(), NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident7@example.com') u
CROSS JOIN LATERAL (SELECT "Id" FROM "Apartments" WHERE "Block"='C' AND "Number"='101') a;

INSERT INTO "Residents" ("Id","UserId","ApartmentId","MoveInDate","VehicleNumber","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", NOW() - INTERVAL '2 months', 'TS09AB8008', false, NOW(), NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident8@example.com') u
CROSS JOIN LATERAL (SELECT "Id" FROM "Apartments" WHERE "Block"='C' AND "Number"='201') a;

SELECT 'Residents created' AS status;

-- ===================== FACILITIES (5) =====================

INSERT INTO "Facilities" ("Id","Name","Description","HourlyRate","Capacity","IsActive","IsDeleted","CreatedAt","UpdatedAt") VALUES
(gen_random_uuid(), 'Club House',     'Community events hall with stage and seating',  500, 100, true, false, NOW(), NOW()),
(gen_random_uuid(), 'Gym',            'Fully equipped fitness center with cardio zone', 200, 40,  true, false, NOW(), NOW()),
(gen_random_uuid(), 'Swimming Pool',  'Olympic-size pool with changing rooms',          300, 50,  true, false, NOW(), NOW()),
(gen_random_uuid(), 'Tennis Court',   'Floodlit synthetic turf court',                  400, 4,   true, false, NOW(), NOW()),
(gen_random_uuid(), 'Party Hall',     'AC banquet hall for private events',              800, 80,  true, false, NOW(), NOW());

SELECT 'Facilities created' AS status;

-- ===================== BOOKINGS (10) =====================

-- Confirmed bookings
INSERT INTO "Bookings" ("Id","FacilityId","UserId","Date","StartTime","EndTime","SeatsBooked","TotalCost","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), f."Id", u."Id", CURRENT_DATE + 2, NOW() + INTERVAL '2 hours', NOW() + INTERVAL '4 hours', 2, 1000, 2, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Facilities" WHERE "Name"='Club House') f CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident1@example.com') u;

INSERT INTO "Bookings" ("Id","FacilityId","UserId","Date","StartTime","EndTime","SeatsBooked","TotalCost","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), f."Id", u."Id", CURRENT_DATE + 3, NOW() + INTERVAL '6 hours', NOW() + INTERVAL '8 hours', 1, 400, 2, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Facilities" WHERE "Name"='Gym') f CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident2@example.com') u;

INSERT INTO "Bookings" ("Id","FacilityId","UserId","Date","StartTime","EndTime","SeatsBooked","TotalCost","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), f."Id", u."Id", CURRENT_DATE + 1, NOW() + INTERVAL '3 hours', NOW() + INTERVAL '5 hours', 1, 600, 2, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Facilities" WHERE "Name"='Swimming Pool') f CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident3@example.com') u;

-- Held bookings (waiting for payment)
INSERT INTO "Bookings" ("Id","FacilityId","UserId","Date","StartTime","EndTime","SeatsBooked","TotalCost","Status","HoldExpiresAt","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), f."Id", u."Id", CURRENT_DATE + 4, NOW() + INTERVAL '10 hours', NOW() + INTERVAL '12 hours', 2, 800, 5, NOW() + INTERVAL '10 minutes', false, NOW(), NOW()
FROM (SELECT "Id" FROM "Facilities" WHERE "Name"='Tennis Court') f CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident4@example.com') u;

INSERT INTO "Bookings" ("Id","FacilityId","UserId","Date","StartTime","EndTime","SeatsBooked","TotalCost","Status","HoldExpiresAt","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), f."Id", u."Id", CURRENT_DATE + 5, NOW() + INTERVAL '4 hours', NOW() + INTERVAL '8 hours', 3, 2400, 5, NOW() + INTERVAL '10 minutes', false, NOW(), NOW()
FROM (SELECT "Id" FROM "Facilities" WHERE "Name"='Party Hall') f CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident5@example.com') u;

-- Cancelled booking
INSERT INTO "Bookings" ("Id","FacilityId","UserId","Date","StartTime","EndTime","SeatsBooked","TotalCost","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), f."Id", u."Id", CURRENT_DATE - 2, NOW() - INTERVAL '24 hours', NOW() - INTERVAL '22 hours', 1, 200, 3, false, NOW() - INTERVAL '3 days', NOW()
FROM (SELECT "Id" FROM "Facilities" WHERE "Name"='Gym') f CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident6@example.com') u;

-- Completed bookings
INSERT INTO "Bookings" ("Id","FacilityId","UserId","Date","StartTime","EndTime","SeatsBooked","TotalCost","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), f."Id", u."Id", CURRENT_DATE - 5, NOW() - INTERVAL '5 days', NOW() - INTERVAL '5 days' + INTERVAL '2 hours', 2, 600, 4, false, NOW() - INTERVAL '6 days', NOW()
FROM (SELECT "Id" FROM "Facilities" WHERE "Name"='Swimming Pool') f CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident7@example.com') u;

INSERT INTO "Bookings" ("Id","FacilityId","UserId","Date","StartTime","EndTime","SeatsBooked","TotalCost","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), f."Id", u."Id", CURRENT_DATE - 3, NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days' + INTERVAL '3 hours', 1, 1500, 4, false, NOW() - INTERVAL '4 days', NOW()
FROM (SELECT "Id" FROM "Facilities" WHERE "Name"='Club House') f CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident1@example.com') u;

-- Expired bookings
INSERT INTO "Bookings" ("Id","FacilityId","UserId","Date","StartTime","EndTime","SeatsBooked","TotalCost","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), f."Id", u."Id", CURRENT_DATE - 1, NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day' + INTERVAL '1 hour', 1, 400, 6, false, NOW() - INTERVAL '2 days', NOW()
FROM (SELECT "Id" FROM "Facilities" WHERE "Name"='Tennis Court') f CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident8@example.com') u;

INSERT INTO "Bookings" ("Id","FacilityId","UserId","Date","StartTime","EndTime","SeatsBooked","TotalCost","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), f."Id", u."Id", CURRENT_DATE + 1, NOW() + INTERVAL '1 hour', NOW() + INTERVAL '3 hours', 2, 1600, 6, false, NOW() - INTERVAL '1 day', NOW()
FROM (SELECT "Id" FROM "Facilities" WHERE "Name"='Party Hall') f CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident2@example.com') u;

SELECT 'Bookings created' AS status;

-- ===================== BILLS (24 across apartments and periods) =====================

-- Block A-101 bills (resident1)
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'March 2026', 3000, 0, CURRENT_DATE - 120, 2, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='101') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident1@example.com') u;
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'April 2026', 3000, 0, CURRENT_DATE - 90, 2, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='101') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident1@example.com') u;
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'May 2026', 3000, 0, CURRENT_DATE - 60, 2, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='101') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident1@example.com') u;
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'June 2026', 3000, 0, CURRENT_DATE - 30, 2, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='101') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident1@example.com') u;
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'July 2026', 3000, 0, CURRENT_DATE + 10, 1, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='101') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident1@example.com') u;

-- Block A-201 bills (resident2) - has overdue
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'April 2026', 3000, 100, CURRENT_DATE - 90, 2, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='201') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident2@example.com') u;
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'May 2026', 3000, 0, CURRENT_DATE - 60, 2, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='201') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident2@example.com') u;
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'June 2026', 3000, 150, CURRENT_DATE - 5, 3, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='201') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident2@example.com') u;
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'July 2026', 3000, 0, CURRENT_DATE + 15, 1, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='201') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident2@example.com') u;
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'August 2026', 3200, 0, CURRENT_DATE + 45, 1, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='201') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident2@example.com') u;

-- Block A-301 bills (resident3)
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'June 2026', 3000, 0, CURRENT_DATE - 10, 2, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='301') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident3@example.com') u;
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'July 2026', 3000, 0, CURRENT_DATE + 12, 1, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='301') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident3@example.com') u;

-- Block B-101 bills (resident4) - has overdue with escalating penalties
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'April 2026', 3000, 300, CURRENT_DATE - 90, 3, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='B' AND "Number"='101') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident4@example.com') u;
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'May 2026', 3000, 150, CURRENT_DATE - 60, 3, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='B' AND "Number"='101') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident4@example.com') u;
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'June 2026', 3000, 0, CURRENT_DATE - 25, 2, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='B' AND "Number"='101') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident4@example.com') u;
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'July 2026', 3000, 0, CURRENT_DATE + 8, 1, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='B' AND "Number"='101') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident4@example.com') u;

-- Block B-201, B-301, C-101, C-201 bills (residents 5-8) - one each
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'July 2026', 2800, 0, CURRENT_DATE + 14, 1, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='B' AND "Number"='201') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident5@example.com') u;
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'July 2026', 2800, 0, CURRENT_DATE + 14, 2, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='B' AND "Number"='301') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident6@example.com') u;
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'June 2026', 3000, 150, CURRENT_DATE - 15, 3, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='C' AND "Number"='101') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident7@example.com') u;
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'July 2026', 3000, 0, CURRENT_DATE + 20, 1, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='C' AND "Number"='101') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident7@example.com') u;
INSERT INTO "Bills" ("Id","ApartmentId","BilledToUserId","Period","BaseAmount","Penalty","DueDate","Status","IsVacantRate","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), a."Id", u."Id", 'July 2026', 2800, 0, CURRENT_DATE + 18, 5, false, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='C' AND "Number"='201') a CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='resident8@example.com') u;

SELECT 'Bills created' AS status;

-- ===================== ANNOUNCEMENTS (6) =====================

INSERT INTO "Announcements" ("Id","Title","Content","Audience","IsPinned","ExpiresAt","IsDeleted","CreatedAt","UpdatedAt") VALUES
(gen_random_uuid(), 'Water Supply Interruption',   'Water supply will be interrupted on Sunday 10 AM – 2 PM for tank cleaning.',                    2, true,  NOW() + INTERVAL '7 days',  false, NOW(), NOW()),
(gen_random_uuid(), 'Society AGM Notice',           'Annual General Meeting scheduled for this Sunday at 11 AM in the Club House. All owners please attend.', 2, true, NOW() + INTERVAL '10 days', false, NOW(), NOW()),
(gen_random_uuid(), 'Diwali Celebration',            'Society Diwali celebration in the clubhouse this Saturday at 6 PM. Snacks and fireworks arranged.', 2, false, NOW() + INTERVAL '5 days',  false, NOW(), NOW()),
(gen_random_uuid(), 'Fire Drill Notice',             'Mandatory fire safety drill scheduled for next Tuesday at 10 AM. All residents must participate.', 2, true,  NOW() + INTERVAL '8 days',  false, NOW(), NOW()),
(gen_random_uuid(), 'Parking Rules Update',          'Updated parking allocation rules are now in effect. Visitors must use Basement 2 only. Please review the notice board.', 2, false, NOW() + INTERVAL '15 days', false, NOW(), NOW()),
(gen_random_uuid(), 'Maintenance Window',            'Elevator maintenance scheduled for Block A on Wednesday 9 AM – 12 PM. Please use stairs.',     2, false, NOW() + INTERVAL '3 days',  false, NOW(), NOW());

SELECT 'Announcements created' AS status;

-- ===================== VISITORS (12) =====================

-- Approved visitors (ready to check in)
INSERT INTO "Visitors" ("Id","Name","Email","Purpose","ApartmentId","QrToken","ETA","ExpiresAt","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), 'Rahul Sharma', 'rahul.sharma@example.com', 'Guest Visit', a."Id", md5(random()::text), NOW() + INTERVAL '1 hour', NOW() + INTERVAL '25 hours', 2, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='101') a;

INSERT INTO "Visitors" ("Id","Name","Email","Purpose","ApartmentId","QrToken","ETA","ExpiresAt","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), 'Amazon Courier', 'delivery@amazon.com', 'Courier', a."Id", md5(random()::text), NOW() + INTERVAL '30 minutes', NOW() + INTERVAL '2 hours', 2, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='201') a;

INSERT INTO "Visitors" ("Id","Name","Email","Purpose","ApartmentId","QrToken","ETA","ExpiresAt","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), 'Neha Gupta', 'neha.gupta@example.com', 'Guest Visit', a."Id", md5(random()::text), NOW() + INTERVAL '3 hours', NOW() + INTERVAL '27 hours', 2, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='B' AND "Number"='201') a;

-- Checked-in visitors
INSERT INTO "Visitors" ("Id","Name","Email","Purpose","ApartmentId","QrToken","ETA","ExpiresAt","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), 'Zomato Delivery', 'delivery@zomato.com', 'Food Delivery', a."Id", md5(random()::text), NOW() - INTERVAL '20 minutes', NOW() + INTERVAL '2 hours', 3, false, NOW() - INTERVAL '25 minutes', NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='B' AND "Number"='101') a;

INSERT INTO "Visitors" ("Id","Name","Email","Purpose","ApartmentId","QrToken","ETA","ExpiresAt","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), 'Plumber - Raju', 'raju.plumber@example.com', 'Maintenance', a."Id", md5(random()::text), NOW() - INTERVAL '1 hour', NOW() + INTERVAL '5 hours', 3, false, NOW() - INTERVAL '2 hours', NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='301') a;

-- Checked-out visitors
INSERT INTO "Visitors" ("Id","Name","Email","Purpose","ApartmentId","QrToken","ETA","ExpiresAt","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), 'Priya Nair', 'priya.nair@example.com', 'Guest Visit', a."Id", md5(random()::text), NOW() - INTERVAL '6 hours', NOW() - INTERVAL '1 hour', 4, false, NOW() - INTERVAL '8 hours', NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='201') a;

INSERT INTO "Visitors" ("Id","Name","Email","Purpose","ApartmentId","QrToken","ETA","ExpiresAt","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), 'Swiggy Delivery', 'delivery@swiggy.com', 'Food Delivery', a."Id", md5(random()::text), NOW() - INTERVAL '4 hours', NOW() - INTERVAL '2 hours', 4, false, NOW() - INTERVAL '5 hours', NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='C' AND "Number"='101') a;

-- Expired visitors
INSERT INTO "Visitors" ("Id","Name","Email","Purpose","ApartmentId","QrToken","ETA","ExpiresAt","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), 'Amit Verma', 'amit.verma@example.com', 'Guest Visit', a."Id", md5(random()::text), NOW() - INTERVAL '2 days', NOW() - INTERVAL '1 day', 5, false, NOW() - INTERVAL '3 days', NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='C' AND "Number"='201') a;

INSERT INTO "Visitors" ("Id","Name","Email","Purpose","ApartmentId","QrToken","ETA","ExpiresAt","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), 'Flipkart Courier', 'courier@flipkart.com', 'Courier', a."Id", md5(random()::text), NOW() - INTERVAL '3 days', NOW() - INTERVAL '2 days', 5, false, NOW() - INTERVAL '4 days', NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='B' AND "Number"='301') a;

-- Denied visitor
INSERT INTO "Visitors" ("Id","Name","Email","Purpose","ApartmentId","QrToken","ETA","ExpiresAt","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), 'Unknown Person', 'unknown@example.com', 'Soliciting', a."Id", md5(random()::text), NOW() - INTERVAL '1 day', NOW() - INTERVAL '12 hours', 6, false, NOW() - INTERVAL '2 days', NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='101') a;

-- Pending visitor
INSERT INTO "Visitors" ("Id","Name","Email","Purpose","ApartmentId","QrToken","ETA","ExpiresAt","Status","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), 'Sneha Reddy', 'sneha.reddy@example.com', 'Guest Visit', a."Id", md5(random()::text), NOW() + INTERVAL '5 hours', NOW() + INTERVAL '29 hours', 1, false, NOW(), NOW()
FROM (SELECT "Id" FROM "Apartments" WHERE "Block"='C' AND "Number"='201') a;

SELECT 'Visitors created' AS status;

-- ===================== VISITOR ENTRIES (6) =====================

-- Entries for checked-in visitors
INSERT INTO "VisitorEntries" ("Id","VisitorId","CheckinTime","CheckoutTime","StaffId","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), v."Id", NOW() - INTERVAL '15 minutes', NULL, s."Id", false, NOW(), NOW()
FROM (SELECT "Id" FROM "Visitors" WHERE "Name"='Zomato Delivery') v CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='security1@example.com') s;

INSERT INTO "VisitorEntries" ("Id","VisitorId","CheckinTime","CheckoutTime","StaffId","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), v."Id", NOW() - INTERVAL '50 minutes', NULL, s."Id", false, NOW(), NOW()
FROM (SELECT "Id" FROM "Visitors" WHERE "Name"='Plumber - Raju') v CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='security2@example.com') s;

-- Entries for checked-out visitors
INSERT INTO "VisitorEntries" ("Id","VisitorId","CheckinTime","CheckoutTime","StaffId","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), v."Id", NOW() - INTERVAL '5 hours', NOW() - INTERVAL '2 hours', s."Id", false, NOW(), NOW()
FROM (SELECT "Id" FROM "Visitors" WHERE "Name"='Priya Nair') v CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='security1@example.com') s;

INSERT INTO "VisitorEntries" ("Id","VisitorId","CheckinTime","CheckoutTime","StaffId","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), v."Id", NOW() - INTERVAL '4 hours', NOW() - INTERVAL '3 hours' - INTERVAL '30 minutes', s."Id", false, NOW(), NOW()
FROM (SELECT "Id" FROM "Visitors" WHERE "Name"='Swiggy Delivery') v CROSS JOIN (SELECT "Id" FROM "Users" WHERE "Email"='security3@example.com') s;

SELECT 'Visitor Entries created' AS status;

-- ===================== COMPLAINTS (15 with AI triage fields) =====================

-- Open complaints with AI triage populated
INSERT INTO "Complaints" ("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","Category","Priority","DraftAdminResponse","PossibleDuplicateIdsCsv","TriageProcessed","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", 'Water Leakage in Kitchen', 'There is a severe water leak near the kitchen sink. Water is pooling on the floor.', 1, NULL, 0, 2, 'We have notified the plumbing team. A technician will inspect your unit within 24 hours.', NULL, true, false, NOW() - INTERVAL '2 hours', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident1@example.com') u CROSS JOIN (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='101') a;

INSERT INTO "Complaints" ("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","Category","Priority","DraftAdminResponse","PossibleDuplicateIdsCsv","TriageProcessed","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", 'No Water Supply Since Morning', 'Complete water supply failure in our apartment since 6 AM. All taps dry.', 1, NULL, 0, 3, 'This has been flagged as urgent. Our maintenance team is investigating the water main. We expect resolution within 4 hours.', NULL, true, false, NOW() - INTERVAL '1 hour', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident2@example.com') u CROSS JOIN (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='201') a;

INSERT INTO "Complaints" ("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","Category","Priority","DraftAdminResponse","PossibleDuplicateIdsCsv","TriageProcessed","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", 'Power Fluctuations Every Evening', 'Frequent voltage drops between 6-9 PM causing appliance trips.', 1, NULL, 1, 2, 'We have logged this with the electrical maintenance team. An electrician will check your unit wiring and the floor distribution board.', NULL, true, false, NOW() - INTERVAL '3 hours', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident3@example.com') u CROSS JOIN (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='301') a;

INSERT INTO "Complaints" ("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","Category","Priority","DraftAdminResponse","PossibleDuplicateIdsCsv","TriageProcessed","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", 'Unauthorized Parking in My Spot', 'An unknown car (MH04XX1234) has been parked in my allotted spot B-101 for 2 days.', 1, NULL, 3, 1, 'We have informed the security team. They will identify the vehicle owner and ensure it is moved. Your allotted spot will be restored.', NULL, true, false, NOW() - INTERVAL '5 hours', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident4@example.com') u CROSS JOIN (SELECT "Id" FROM "Apartments" WHERE "Block"='B' AND "Number"='101') a;

INSERT INTO "Complaints" ("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","Category","Priority","DraftAdminResponse","PossibleDuplicateIdsCsv","TriageProcessed","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", 'Loud Construction Noise at 6 AM', 'Construction work starts at 6 AM every morning in the adjacent plot. Very disruptive on weekends.', 1, NULL, 4, 1, 'We have raised this concern with the construction contractor. Society rules state work hours are 8 AM – 6 PM. We will enforce compliance.', NULL, true, false, NOW() - INTERVAL '1 day', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident5@example.com') u CROSS JOIN (SELECT "Id" FROM "Apartments" WHERE "Block"='B' AND "Number"='201') a;

INSERT INTO "Complaints" ("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","Category","Priority","DraftAdminResponse","PossibleDuplicateIdsCsv","TriageProcessed","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", 'Broken Gym Equipment', 'The treadmill #3 in the gym has a broken belt. Also the cable machine is stuck.', 1, NULL, 5, 1, 'Thank you for reporting. We have notified the gym equipment vendor for inspection and repair. Temporary out-of-order signs will be placed.', NULL, true, false, NOW() - INTERVAL '6 hours', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident6@example.com') u CROSS JOIN (SELECT "Id" FROM "Apartments" WHERE "Block"='B' AND "Number"='301') a;

INSERT INTO "Complaints" ("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","Category","Priority","DraftAdminResponse","PossibleDuplicateIdsCsv","TriageProcessed","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", 'CCTV Camera Not Working in Basement', 'The CCTV camera near the basement parking entrance (B2) appears to be offline for the past 3 days.', 1, NULL, 2, 2, 'This is a security concern. We have escalated to the CCTV maintenance vendor. A temporary patrol has been assigned to the area.', NULL, true, false, NOW() - INTERVAL '4 hours', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident7@example.com') u CROSS JOIN (SELECT "Id" FROM "Apartments" WHERE "Block"='C' AND "Number"='101') a;

-- In-progress complaints
INSERT INTO "Complaints" ("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","Category","Priority","DraftAdminResponse","TriageProcessed","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", 'Lift Making Loud Noise', 'The elevator in Block A makes a grinding sound. Feels unsafe.', 4, 'Technician has been assigned. Expected inspection tomorrow.', 1, 2, 'The elevator maintenance team has been contacted for an urgent inspection.', true, false, NOW() - INTERVAL '2 days', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident2@example.com') u CROSS JOIN (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='201') a;

INSERT INTO "Complaints" ("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","Category","Priority","DraftAdminResponse","TriageProcessed","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", 'Ceiling Seepage After Rain', 'Water seepage from the ceiling near the balcony whenever it rains heavily.', 4, 'Plumber scheduled for inspection on Friday.', 0, 1, 'A waterproofing specialist will inspect your ceiling this week.', true, false, NOW() - INTERVAL '3 days', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident4@example.com') u CROSS JOIN (SELECT "Id" FROM "Apartments" WHERE "Block"='B' AND "Number"='101') a;

-- Resolved complaints
INSERT INTO "Complaints" ("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","Category","Priority","DraftAdminResponse","TriageProcessed","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", 'Gate Barrier Not Opening', 'The main gate barrier is stuck and not responding to the remote.', 2, 'The barrier motor was replaced. Now working fine.', 2, 2, NULL, true, false, NOW() - INTERVAL '5 days', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident1@example.com') u CROSS JOIN (SELECT "Id" FROM "Apartments" WHERE "Block"='A' AND "Number"='101') a;

INSERT INTO "Complaints" ("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","Category","Priority","DraftAdminResponse","TriageProcessed","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", 'Streetlight Not Working', 'The streetlight near Block C entrance has been off for a week.', 2, 'Bulb replaced and timer reconfigured. Issue resolved.', 1, 0, NULL, true, false, NOW() - INTERVAL '7 days', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident8@example.com') u CROSS JOIN (SELECT "Id" FROM "Apartments" WHERE "Block"='C' AND "Number"='201') a;

INSERT INTO "Complaints" ("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","Category","Priority","DraftAdminResponse","TriageProcessed","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", 'Dog Fouling in Garden', 'Pet owners not cleaning up after their dogs in the central garden area.', 2, 'Notice issued to all residents. Pet waste stations installed.', 5, 0, NULL, true, false, NOW() - INTERVAL '10 days', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident6@example.com') u CROSS JOIN (SELECT "Id" FROM "Apartments" WHERE "Block"='B' AND "Number"='301') a;

-- Complaint still being triaged by AI
INSERT INTO "Complaints" ("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","Category","Priority","DraftAdminResponse","TriageProcessed","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", 'Water Pressure Very Low', 'Water pressure in our apartment has been extremely low for the last two days. Barely any water coming from taps.', 1, NULL, NULL, NULL, NULL, false, false, NOW() - INTERVAL '10 minutes', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident8@example.com') u CROSS JOIN (SELECT "Id" FROM "Apartments" WHERE "Block"='C' AND "Number"='201') a;

-- Rejected complaint
INSERT INTO "Complaints" ("Id","UserId","ApartmentId","Title","Description","Status","AdminResponse","Category","Priority","DraftAdminResponse","TriageProcessed","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", a."Id", 'Neighbour Playing Music', 'My neighbour plays music until 10 PM and it is slightly audible.', 5, 'As per society rules, music is permitted until 10 PM. This does not constitute a noise violation.', 4, 0, 'Music until 10 PM falls within permitted hours. We suggest using your window insulation or discussing directly with your neighbour.', true, false, NOW() - INTERVAL '8 days', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident5@example.com') u CROSS JOIN (SELECT "Id" FROM "Apartments" WHERE "Block"='B' AND "Number"='201') a;

SELECT 'Complaints created' AS status;

-- ===================== NOTIFICATIONS (25+) =====================

-- Resident 1 notifications
INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Bill Due Reminder', 'Your maintenance bill for July 2026 is due on the 15th.', false, false, NOW() - INTERVAL '1 hour', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident1@example.com') u;
INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Booking Confirmed', 'Your Club House booking for 2 seats has been confirmed.', true, false, NOW() - INTERVAL '1 day', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident1@example.com') u;
INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Visitor Registered', 'Your visitor Rahul Sharma has been pre-registered.', false, false, NOW() - INTERVAL '2 hours', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident1@example.com') u;
INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Complaint Submitted', 'Your complaint "Water Leakage in Kitchen" has been logged.', true, false, NOW() - INTERVAL '3 hours', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident1@example.com') u;

-- Resident 2 notifications
INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Bill Overdue', 'Your maintenance bill for June 2026 is overdue. Penalty of ₹150 applied.', false, false, NOW() - INTERVAL '5 hours', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident2@example.com') u;
INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Booking Expired', 'Your Party Hall booking hold expired before payment.', true, false, NOW() - INTERVAL '1 day', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident2@example.com') u;
INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Complaint Update', 'Your complaint "Lift Making Loud Noise" is now In Progress.', false, false, NOW() - INTERVAL '2 days', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident2@example.com') u;
INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Visitor Checked In', 'Your visitor Amazon Courier has checked in at the gate.', false, false, NOW() - INTERVAL '3 hours', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident2@example.com') u;
INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'New Announcement', 'Water supply will be interrupted on Sunday 10 AM – 2 PM.', true, false, NOW() - INTERVAL '4 days', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident2@example.com') u;
INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Bill Generated', 'Your maintenance bill for August 2026 has been generated. Amount: ₹3,200.', false, false, NOW() - INTERVAL '6 days', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident2@example.com') u;

-- Resident 3-8 notifications (1-2 each)
INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Complaint Submitted', 'Your complaint "Power Fluctuations Every Evening" has been logged.', true, false, NOW() - INTERVAL '4 hours', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident3@example.com') u;

INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Visitor Arrived', 'Your visitor Zomato Delivery has checked in at the gate.', false, false, NOW() - INTERVAL '20 minutes', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident4@example.com') u;
INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Bill Overdue', 'Your maintenance bill for April 2026 is 3 month(s) overdue. Penalty: ₹300.', false, false, NOW() - INTERVAL '1 day', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident4@example.com') u;

INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Booking Held', 'Your Tennis Court booking has been held. Complete payment within 10 minutes.', false, false, NOW() - INTERVAL '2 minutes', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident4@example.com') u;

INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Booking Held', 'Your Party Hall booking has been held. Complete payment within 10 minutes.', false, false, NOW() - INTERVAL '3 minutes', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident5@example.com') u;

INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Complaint Resolved', 'Your complaint "Dog Fouling in Garden" has been resolved.', true, false, NOW() - INTERVAL '8 days', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident6@example.com') u;

INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Bill Overdue', 'Your maintenance bill for June 2026 is overdue. Penalty: ₹150.', false, false, NOW() - INTERVAL '2 days', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident7@example.com') u;

INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Payment Processing', 'Your payment for July 2026 bill is being processed.', false, false, NOW() - INTERVAL '30 minutes', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident8@example.com') u;
INSERT INTO "Notifications" ("Id","UserId","Title","Message","IsRead","IsDeleted","CreatedAt","UpdatedAt")
SELECT gen_random_uuid(), u."Id", 'Complaint Resolved', 'Your complaint "Streetlight Not Working" has been resolved.', true, false, NOW() - INTERVAL '5 days', NOW()
FROM (SELECT "Id" FROM "Users" WHERE "Email"='resident8@example.com') u;

SELECT 'Notifications created' AS status;

-- ===================== SUMMARY =====================
SELECT 'ENRICHED DEMO DATASET LOADED SUCCESSFULLY' AS message;
SELECT 'Users' AS entity,      COUNT(*) AS count FROM "Users"
UNION ALL SELECT 'Apartments',  COUNT(*) FROM "Apartments"
UNION ALL SELECT 'Residents',   COUNT(*) FROM "Residents"
UNION ALL SELECT 'Facilities',  COUNT(*) FROM "Facilities"
UNION ALL SELECT 'Bookings',    COUNT(*) FROM "Bookings"
UNION ALL SELECT 'Bills',       COUNT(*) FROM "Bills"
UNION ALL SELECT 'Visitors',    COUNT(*) FROM "Visitors"
UNION ALL SELECT 'Entries',     COUNT(*) FROM "VisitorEntries"
UNION ALL SELECT 'Complaints',  COUNT(*) FROM "Complaints"
UNION ALL SELECT 'Announce',    COUNT(*) FROM "Announcements"
UNION ALL SELECT 'Notifs',      COUNT(*) FROM "Notifications";