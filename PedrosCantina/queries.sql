-- Vis en månedsplan (oktober 2026)
SELECT s.Day, s.Period, e.Name, sa.IsLeader
FROM MonthPlan mp
JOIN Shift s ON s.MonthPlanId = mp.MonthPlanId
JOIN ShiftAssignment sa ON sa.ShiftId = s.ShiftId
JOIN Employee e ON e.EmployeeId = sa.EmployeeId
WHERE mp.Year = 2026 AND mp.Month = 10
ORDER BY s.Day, s.Period DESC, sa.IsLeader DESC;

-- Vis belastningen af de enkelte medarbejdere for en måned (oktober 2026)
-- LEFT JOIN så medarbejdere med 0 vagter også bliver vist
SELECT e.Name, COUNT(mp.MonthPlanId) AS NumberOfShifts
FROM Employee e
LEFT JOIN ShiftAssignment sa ON sa.EmployeeId = e.EmployeeId
LEFT JOIN Shift s ON s.ShiftId = sa.ShiftId
LEFT JOIN MonthPlan mp ON mp.MonthPlanId = s.MonthPlanId AND mp.Year = 2026 AND mp.Month = 10
GROUP BY e.EmployeeId, e.Name
ORDER BY NumberOfShifts DESC;

-- Vis kontaktinformation for en medarbejder, i fald en anden er syg
-- Eksempel: en medarbejder er syg på formiddagsvagten 3. oktober 2026.
-- Viser de medarbejdere der ikke allerede er på den vagt, så Pedro kan ringe til en af dem.
SELECT e.Name, e.Phone, e.Email, e.CanBeLeader
FROM Employee e
WHERE e.EmployeeId NOT IN (
    SELECT sa.EmployeeId
    FROM ShiftAssignment sa
    JOIN Shift s ON s.ShiftId = sa.ShiftId
    JOIN MonthPlan mp ON mp.MonthPlanId = s.MonthPlanId
    WHERE mp.Year = 2026 AND mp.Month = 10 AND s.Day = 3 AND s.Period = 'Morning'
);

-- Vis belastningen af medarbejderne hen over året (2026), måned for måned
SELECT e.Name, mp.Month, COUNT(*) AS NumberOfShifts
FROM Employee e
JOIN ShiftAssignment sa ON sa.EmployeeId = e.EmployeeId
JOIN Shift s ON s.ShiftId = sa.ShiftId
JOIN MonthPlan mp ON mp.MonthPlanId = s.MonthPlanId
WHERE mp.Year = 2026
GROUP BY e.EmployeeId, e.Name, mp.Month
ORDER BY e.Name, mp.Month;

-- Vis belastningen af medarbejderne som total for hele året (2026)
SELECT e.Name, COUNT(*) AS NumberOfShifts
FROM Employee e
JOIN ShiftAssignment sa ON sa.EmployeeId = e.EmployeeId
JOIN Shift s ON s.ShiftId = sa.ShiftId
JOIN MonthPlan mp ON mp.MonthPlanId = s.MonthPlanId
WHERE mp.Year = 2026
GROUP BY e.EmployeeId, e.Name
ORDER BY NumberOfShifts DESC;
