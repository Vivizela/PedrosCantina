DROP TABLE IF EXISTS ShiftAssignment;
DROP TABLE IF EXISTS Shift;
DROP TABLE IF EXISTS MonthPlan;
DROP TABLE IF EXISTS Employee;

CREATE TABLE Employee (
    EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    CanBeLeader BIT NOT NULL DEFAULT 0
);

CREATE TABLE MonthPlan (
    MonthPlanId INT IDENTITY(1,1) PRIMARY KEY,
    Year INT NOT NULL,
    Month INT NOT NULL CHECK (Month BETWEEN 1 AND 12),
    UNIQUE (Year, Month)
);

CREATE TABLE Shift (
    ShiftId INT IDENTITY(1,1) PRIMARY KEY,
    MonthPlanId INT NOT NULL REFERENCES MonthPlan(MonthPlanId) ON DELETE CASCADE,
    Day INT NOT NULL CHECK (Day BETWEEN 1 AND 31),
    Period NVARCHAR(10) NOT NULL CHECK (Period IN ('Morning', 'Afternoon')),
    UNIQUE (MonthPlanId, Day, Period)
);

CREATE TABLE ShiftAssignment (
    ShiftId INT NOT NULL REFERENCES Shift(ShiftId) ON DELETE CASCADE,
    EmployeeId INT NOT NULL REFERENCES Employee(EmployeeId),
    IsLeader BIT NOT NULL DEFAULT 0,
    PRIMARY KEY (ShiftId, EmployeeId)
);
