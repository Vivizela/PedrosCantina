-- Tømmer tabellerne, så scriptet kan køres igen
DELETE FROM ShiftAssignment;
DELETE FROM Shift;
DELETE FROM MonthPlan;
DELETE FROM Employee;

-- Medarbejdere (månedsplanerne bliver lavet af programmet)
INSERT INTO Employee (Name, Phone, Email, CanBeLeader) VALUES ('Pedro Pedroson', '20112233', 'pedro@pedros.dk', 1);
INSERT INTO Employee (Name, Phone, Email, CanBeLeader) VALUES ('Jacoby Jacobyson', '20223344', 'jacoby@pedros.dk', 1);
INSERT INTO Employee (Name, Phone, Email, CanBeLeader) VALUES ('Sofie Nielsen', '20334455', 'sofie@pedros.dk', 0);
INSERT INTO Employee (Name, Phone, Email, CanBeLeader) VALUES ('Jonas Pedersen', '20445566', 'jonas@pedros.dk', 0);
INSERT INTO Employee (Name, Phone, Email, CanBeLeader) VALUES ('Emma Larsen', '20556677', 'emma@pedros.dk', 0);
