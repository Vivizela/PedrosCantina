using Microsoft.Data.SqlClient;

namespace PedrosCantina
{
    internal class Program
    {
        static string connectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=PedrosCantina;Trusted_Connection=True;TrustServerCertificate=True;";

        static EmployeeRepository employeeRepository = new EmployeeRepository(connectionString);
        static PlanRepository planRepository = new PlanRepository(connectionString);

        static void Main(string[] args)
        {
            TestCrud();

            bool hasTestData = CreateTestPlans();
            if (hasTestData)
            {
                ShowQueries();
                ChangePlans();
            }

            Console.WriteLine();
            Console.WriteLine("Done. Press a key to close.");
            Console.ReadKey();
        }

        // Afprøver CRUD på medarbejderne gennem repository'et
        static void TestCrud()
        {
            EmployeeRepository repository = new EmployeeRepository(connectionString);

            Console.WriteLine("===== CRUD on Employee =====");

            // Create
            Employee newEmployee = new Employee();
            newEmployee.Name = "Test Testesen";
            newEmployee.Phone = "12345678";
            newEmployee.Email = "test@pedros.dk";
            newEmployee.CanBeLeader = false;

            int newId = repository.Add(newEmployee);
            Console.WriteLine("Created employee with id " + newId);

            // Read (alle)
            Console.WriteLine();
            Console.WriteLine("All employees:");
            List<Employee> employees = repository.GetAll();
            foreach (Employee employee in employees)
            {
                Console.WriteLine(employee);
            }

            // Read (én)
            Console.WriteLine();
            Console.WriteLine("Get employee " + newId + ":");
            Employee found = repository.GetById(newId);
            Console.WriteLine(found);

            // Update
            found.Phone = "87654321";
            found.CanBeLeader = true;
            bool updated = repository.Update(found);
            Console.WriteLine();
            Console.WriteLine("Updated: " + updated);
            Console.WriteLine(repository.GetById(newId));

            // Delete
            bool deleted = repository.Delete(newId);
            Console.WriteLine();
            Console.WriteLine("Deleted: " + deleted);

            Console.WriteLine();
            Console.WriteLine("All employees after delete:");
            foreach (Employee employee in repository.GetAll())
            {
                Console.WriteLine(employee);
            }
        }

        // Kører forespørgslerne mod databasen og viser resultaterne
        static void ShowQueries()
        {
            RunQuery("Month plan for October 2026", @"
                SELECT s.Day, s.Period, e.Name, sa.IsLeader
                FROM MonthPlan mp
                JOIN Shift s ON s.MonthPlanId = mp.MonthPlanId
                JOIN ShiftAssignment sa ON sa.ShiftId = s.ShiftId
                JOIN Employee e ON e.EmployeeId = sa.EmployeeId
                WHERE mp.Year = 2026 AND mp.Month = 10
                ORDER BY s.Day, s.Period DESC, sa.IsLeader DESC");

            RunQuery("Load of each employee in October 2026", @"
                SELECT e.Name, COUNT(mp.MonthPlanId) AS NumberOfShifts
                FROM Employee e
                LEFT JOIN ShiftAssignment sa ON sa.EmployeeId = e.EmployeeId
                LEFT JOIN Shift s ON s.ShiftId = sa.ShiftId
                LEFT JOIN MonthPlan mp ON mp.MonthPlanId = s.MonthPlanId AND mp.Year = 2026 AND mp.Month = 10
                GROUP BY e.EmployeeId, e.Name
                ORDER BY NumberOfShifts DESC");

            RunQuery("Who can be called if someone is sick on the morning shift 3 October 2026", @"
                SELECT e.Name, e.Phone, e.Email, e.CanBeLeader
                FROM Employee e
                WHERE e.EmployeeId NOT IN (
                    SELECT sa.EmployeeId
                    FROM ShiftAssignment sa
                    JOIN Shift s ON s.ShiftId = sa.ShiftId
                    JOIN MonthPlan mp ON mp.MonthPlanId = s.MonthPlanId
                    WHERE mp.Year = 2026 AND mp.Month = 10 AND s.Day = 3 AND s.Period = 'Morning')");

            RunQuery("Load of the employees over 2026, month by month", @"
                SELECT e.Name, mp.Month, COUNT(*) AS NumberOfShifts
                FROM Employee e
                JOIN ShiftAssignment sa ON sa.EmployeeId = e.EmployeeId
                JOIN Shift s ON s.ShiftId = sa.ShiftId
                JOIN MonthPlan mp ON mp.MonthPlanId = s.MonthPlanId
                WHERE mp.Year = 2026
                GROUP BY e.EmployeeId, e.Name, mp.Month
                ORDER BY e.Name, mp.Month");

            RunQuery("Load of the employees for the whole of 2026", @"
                SELECT e.Name, COUNT(*) AS NumberOfShifts
                FROM Employee e
                JOIN ShiftAssignment sa ON sa.EmployeeId = e.EmployeeId
                JOIN Shift s ON s.ShiftId = sa.ShiftId
                JOIN MonthPlan mp ON mp.MonthPlanId = s.MonthPlanId
                WHERE mp.Year = 2026
                GROUP BY e.EmployeeId, e.Name
                ORDER BY NumberOfShifts DESC");
        }

        // Laver planer for oktober og november og sætter medarbejdere på vagterne
        static bool CreateTestPlans()
        {
            Console.WriteLine();
            Console.WriteLine("===== Creating month plans for October and November 2026 =====");

            List<Employee> employees = employeeRepository.GetAll();
            int pedro = FindEmployeeId(employees, "Pedro Pedroson");
            int jacoby = FindEmployeeId(employees, "Jacoby Jacobyson");
            int sofie = FindEmployeeId(employees, "Sofie Nielsen");
            int jonas = FindEmployeeId(employees, "Jonas Pedersen");
            int emma = FindEmployeeId(employees, "Emma Larsen");

            if (pedro == 0 || jacoby == 0 || sofie == 0 || jonas == 0 || emma == 0)
            {
                Console.WriteLine("The employees are missing. Run test_data.sql first.");
                return false;
            }

            planRepository.DeleteAllPlans();
            planRepository.CreateMonthPlan(2026, 10);
            planRepository.CreateMonthPlan(2026, 11);

            // Oktober: Pedro og Sofie om formiddagen, Jacoby og Jonas om eftermiddagen
            for (int day = 1; day <= 31; day++)
            {
                int morning = planRepository.GetShiftId(2026, 10, day, "Morning");
                int afternoon = planRepository.GetShiftId(2026, 10, day, "Afternoon");

                planRepository.AddToShift(morning, pedro, true);
                planRepository.AddToShift(morning, sofie, false);
                planRepository.AddToShift(afternoon, jacoby, true);
                planRepository.AddToShift(afternoon, jonas, false);

                // Emma er også med de to første formiddage
                if (day <= 2)
                {
                    planRepository.AddToShift(morning, emma, false);
                }
            }

            // November: Jacoby og Emma om formiddagen, Pedro og Jonas om eftermiddagen
            for (int day = 1; day <= 30; day++)
            {
                int morning = planRepository.GetShiftId(2026, 11, day, "Morning");
                int afternoon = planRepository.GetShiftId(2026, 11, day, "Afternoon");

                planRepository.AddToShift(morning, jacoby, true);
                planRepository.AddToShift(morning, emma, false);
                planRepository.AddToShift(afternoon, pedro, true);
                planRepository.AddToShift(afternoon, jonas, false);
            }

            Console.WriteLine("Created 2 month plans with employees on every shift");
            return true;
        }

        // Laver en ny månedsplan og justerer en eksisterende plan
        static void ChangePlans()
        {
            List<Employee> employees = employeeRepository.GetAll();
            int pedro = FindEmployeeId(employees, "Pedro Pedroson");
            int sofie = FindEmployeeId(employees, "Sofie Nielsen");
            int emma = FindEmployeeId(employees, "Emma Larsen");

            Console.WriteLine();
            Console.WriteLine("===== New month plan for December 2026 =====");
            planRepository.CreateMonthPlan(2026, 12);
            Console.WriteLine("Created the plan with two shifts for every day");

            Console.WriteLine();
            Console.WriteLine("===== Pedro (leader) and Sofie are put on the morning shift 1 December 2026 =====");
            int decemberShift = planRepository.GetShiftId(2026, 12, 1, "Morning");
            planRepository.AddToShift(decemberShift, pedro, true);
            planRepository.AddToShift(decemberShift, sofie, false);
            Console.WriteLine("Added 2 employees to the shift");

            Console.WriteLine();
            Console.WriteLine("===== Sofie is sick on the morning shift 3 October 2026, Emma takes the shift =====");
            int sickShift = planRepository.GetShiftId(2026, 10, 3, "Morning");
            planRepository.ReplaceOnShift(sickShift, sofie, emma);
            Console.WriteLine("Replaced Sofie with Emma");

            Console.WriteLine();
            Console.WriteLine("===== Emma is taken off the morning shift 1 October 2026 =====");
            int firstShift = planRepository.GetShiftId(2026, 10, 1, "Morning");
            planRepository.RemoveFromShift(firstShift, emma);
            Console.WriteLine("Removed Emma from the shift");

            RunQuery("The morning shifts 1 and 3 October after the changes", @"
                SELECT s.Day, s.Period, e.Name, sa.IsLeader
                FROM MonthPlan mp
                JOIN Shift s ON s.MonthPlanId = mp.MonthPlanId
                JOIN ShiftAssignment sa ON sa.ShiftId = s.ShiftId
                JOIN Employee e ON e.EmployeeId = sa.EmployeeId
                WHERE mp.Year = 2026 AND mp.Month = 10 AND s.Period = 'Morning' AND (s.Day = 1 OR s.Day = 3)
                ORDER BY s.Day, sa.IsLeader DESC");

            RunQuery("The month plans after the changes", @"
                SELECT mp.Year, mp.Month, COUNT(s.ShiftId) AS NumberOfShifts
                FROM MonthPlan mp
                LEFT JOIN Shift s ON s.MonthPlanId = mp.MonthPlanId
                GROUP BY mp.Year, mp.Month
                ORDER BY mp.Year, mp.Month");
        }

        // Finder id'et på en medarbejder ud fra navnet (0 hvis den ikke findes)
        static int FindEmployeeId(List<Employee> employees, string name)
        {
            foreach (Employee employee in employees)
            {
                if (employee.Name == name)
                {
                    return employee.EmployeeId;
                }
            }
            return 0;
        }

        // Kører en SELECT og skriver alle rækkerne ud i konsollen
        static void RunQuery(string title, string sql)
        {
            Console.WriteLine();
            Console.WriteLine("===== " + title + " =====");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                SqlDataReader reader = command.ExecuteReader();

                string header = "";
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    header = header + reader.GetName(i) + "\t";
                }
                Console.WriteLine(header);

                while (reader.Read())
                {
                    string line = "";
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        line = line + reader[i] + "\t";
                    }
                    Console.WriteLine(line);
                }
            }
        }
    }
}
