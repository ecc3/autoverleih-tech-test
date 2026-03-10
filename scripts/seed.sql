-- Seed script for autoverleih database
-- Run: docker compose exec db psql -U postgres -d autoverleih -f /seed.sql
-- Or:  cat scripts/seed.sql | docker compose exec -T db psql -U postgres -d autoverleih

BEGIN;

-- Clear existing data
DELETE FROM "Rentals";
DELETE FROM "Cars";
DELETE FROM "Customers";

-- Customers
INSERT INTO "Customers" ("Id", "FirstName", "LastName", "Email", "PhoneNumber", "CreatedAt") VALUES
('a1b2c3d4-0001-4000-8000-000000000001', 'Anna',    'Müller',    'anna.mueller@example.de',    '+49 170 1234567', NOW() - INTERVAL '90 days'),
('a1b2c3d4-0001-4000-8000-000000000002', 'Thomas',  'Schmidt',   'thomas.schmidt@example.de',  '+49 171 2345678', NOW() - INTERVAL '85 days'),
('a1b2c3d4-0001-4000-8000-000000000003', 'Laura',   'Weber',     'laura.weber@example.de',     '+49 172 3456789', NOW() - INTERVAL '60 days'),
('a1b2c3d4-0001-4000-8000-000000000004', 'Markus',  'Fischer',   'markus.fischer@example.de',  '+49 173 4567890', NOW() - INTERVAL '45 days'),
('a1b2c3d4-0001-4000-8000-000000000005', 'Sophie',  'Wagner',    'sophie.wagner@example.de',   NULL,              NOW() - INTERVAL '30 days'),
('a1b2c3d4-0001-4000-8000-000000000006', 'Jan',     'Becker',    'jan.becker@example.de',      '+49 175 6789012', NOW() - INTERVAL '20 days'),
('a1b2c3d4-0001-4000-8000-000000000007', 'Lisa',    'Hoffmann',  'lisa.hoffmann@example.de',   '+49 176 7890123', NOW() - INTERVAL '10 days'),
('a1b2c3d4-0001-4000-8000-000000000008', 'Felix',   'Braun',     'felix.braun@example.de',     NULL,              NOW() - INTERVAL '5 days');

-- Cars
INSERT INTO "Cars" ("Id", "Make", "Model", "LicensePlate", "Year", "IsAvailable", "CreatedAt") VALUES
('b2c3d4e5-0002-4000-8000-000000000001', 'Volkswagen', 'Golf',       'B-VW 1234',  2023, true,  NOW() - INTERVAL '120 days'),
('b2c3d4e5-0002-4000-8000-000000000002', 'BMW',        '3er',        'B-BM 5678',  2024, false, NOW() - INTERVAL '120 days'),
('b2c3d4e5-0002-4000-8000-000000000003', 'Mercedes',   'C-Klasse',   'B-MB 9012',  2023, true,  NOW() - INTERVAL '100 days'),
('b2c3d4e5-0002-4000-8000-000000000004', 'Audi',       'A4',         'B-AU 3456',  2024, false, NOW() - INTERVAL '100 days'),
('b2c3d4e5-0002-4000-8000-000000000005', 'Opel',       'Corsa',      'B-OP 7890',  2022, true,  NOW() - INTERVAL '80 days'),
('b2c3d4e5-0002-4000-8000-000000000006', 'Ford',       'Focus',      'B-FO 2345',  2023, true,  NOW() - INTERVAL '80 days'),
('b2c3d4e5-0002-4000-8000-000000000007', 'Porsche',    'Cayenne',    'B-PO 6789',  2024, true,  NOW() - INTERVAL '60 days'),
('b2c3d4e5-0002-4000-8000-000000000008', 'Tesla',      'Model 3',    'B-TE 1357',  2024, true,  NOW() - INTERVAL '40 days'),
('b2c3d4e5-0002-4000-8000-000000000009', 'Volkswagen', 'Passat',     'B-VW 2468',  2022, true,  NOW() - INTERVAL '30 days'),
('b2c3d4e5-0002-4000-8000-000000000010', 'BMW',        'X5',         'B-BM 1122',  2025, true,  NOW() - INTERVAL '10 days');

-- Rentals
INSERT INTO "Rentals" ("Id", "CustomerId", "CarId", "StartDate", "EndDate", "ReturnedAt", "Status", "CreatedAt") VALUES
-- Completed rentals
('c3d4e5f6-0003-4000-8000-000000000001', 'a1b2c3d4-0001-4000-8000-000000000001', 'b2c3d4e5-0002-4000-8000-000000000001', NOW() - INTERVAL '80 days', NOW() - INTERVAL '73 days', NOW() - INTERVAL '73 days', 'Completed', NOW() - INTERVAL '80 days'),
('c3d4e5f6-0003-4000-8000-000000000002', 'a1b2c3d4-0001-4000-8000-000000000002', 'b2c3d4e5-0002-4000-8000-000000000003', NOW() - INTERVAL '70 days', NOW() - INTERVAL '65 days', NOW() - INTERVAL '64 days', 'Completed', NOW() - INTERVAL '70 days'),
('c3d4e5f6-0003-4000-8000-000000000003', 'a1b2c3d4-0001-4000-8000-000000000003', 'b2c3d4e5-0002-4000-8000-000000000005', NOW() - INTERVAL '50 days', NOW() - INTERVAL '45 days', NOW() - INTERVAL '45 days', 'Completed', NOW() - INTERVAL '50 days'),
('c3d4e5f6-0003-4000-8000-000000000004', 'a1b2c3d4-0001-4000-8000-000000000004', 'b2c3d4e5-0002-4000-8000-000000000007', NOW() - INTERVAL '40 days', NOW() - INTERVAL '35 days', NOW() - INTERVAL '36 days', 'Completed', NOW() - INTERVAL '40 days'),
('c3d4e5f6-0003-4000-8000-000000000005', 'a1b2c3d4-0001-4000-8000-000000000001', 'b2c3d4e5-0002-4000-8000-000000000006', NOW() - INTERVAL '30 days', NOW() - INTERVAL '25 days', NOW() - INTERVAL '25 days', 'Completed', NOW() - INTERVAL '30 days'),

-- Cancelled rental
('c3d4e5f6-0003-4000-8000-000000000006', 'a1b2c3d4-0001-4000-8000-000000000005', 'b2c3d4e5-0002-4000-8000-000000000008', NOW() - INTERVAL '20 days', NOW() - INTERVAL '15 days', NULL, 'Cancelled', NOW() - INTERVAL '20 days'),

-- Active rentals (BMW 3er and Audi A4 are currently rented out)
('c3d4e5f6-0003-4000-8000-000000000007', 'a1b2c3d4-0001-4000-8000-000000000006', 'b2c3d4e5-0002-4000-8000-000000000002', NOW() - INTERVAL '3 days', NOW() + INTERVAL '4 days', NULL, 'Active', NOW() - INTERVAL '3 days'),
('c3d4e5f6-0003-4000-8000-000000000008', 'a1b2c3d4-0001-4000-8000-000000000007', 'b2c3d4e5-0002-4000-8000-000000000004', NOW() - INTERVAL '1 day',  NOW() + INTERVAL '6 days', NULL, 'Active', NOW() - INTERVAL '1 day');

COMMIT;
