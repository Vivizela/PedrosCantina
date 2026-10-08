using Microsoft.Data.SqlClient;

namespace PedrosCantina
{
    public class PlanRepository
    {
        private string connectionString;

        public PlanRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        // Laver en månedsplan med to vagter for hver dag i måneden
        public void CreateMonthPlan(int year, int month)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand insertPlan = new SqlCommand(
                    "INSERT INTO MonthPlan (Year, Month) VALUES (@Year, @Month)", connection);
                insertPlan.Parameters.AddWithValue("@Year", year);
                insertPlan.Parameters.AddWithValue("@Month", month);
                insertPlan.ExecuteNonQuery();

                // Finder id'et på den plan der lige er lavet
                SqlCommand getPlanId = new SqlCommand(
                    "SELECT MonthPlanId FROM MonthPlan WHERE Year = @Year AND Month = @Month", connection);
                getPlanId.Parameters.AddWithValue("@Year", year);
                getPlanId.Parameters.AddWithValue("@Month", month);
                int planId = (int)getPlanId.ExecuteScalar();

                int days = DateTime.DaysInMonth(year, month);
                for (int day = 1; day <= days; day++)
                {
                    AddShift(connection, planId, day, "Morning");
                    AddShift(connection, planId, day, "Afternoon");
                }
            }
        }

        private void AddShift(SqlConnection connection, int planId, int day, string period)
        {
            SqlCommand command = new SqlCommand(
                "INSERT INTO Shift (MonthPlanId, Day, Period) VALUES (@MonthPlanId, @Day, @Period)", connection);
            command.Parameters.AddWithValue("@MonthPlanId", planId);
            command.Parameters.AddWithValue("@Day", day);
            command.Parameters.AddWithValue("@Period", period);
            command.ExecuteNonQuery();
        }

        // Finder id'et på en vagt ud fra dato og periode
        public int GetShiftId(int year, int month, int day, string period)
        {
            string sql = "SELECT s.ShiftId FROM Shift s " +
                         "JOIN MonthPlan mp ON mp.MonthPlanId = s.MonthPlanId " +
                         "WHERE mp.Year = @Year AND mp.Month = @Month AND s.Day = @Day AND s.Period = @Period";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Year", year);
                command.Parameters.AddWithValue("@Month", month);
                command.Parameters.AddWithValue("@Day", day);
                command.Parameters.AddWithValue("@Period", period);
                return (int)command.ExecuteScalar();
            }
        }

        // Sætter en medarbejder på en vagt
        public void AddToShift(int shiftId, int employeeId, bool isLeader)
        {
            string sql = "INSERT INTO ShiftAssignment (ShiftId, EmployeeId, IsLeader) " +
                         "VALUES (@ShiftId, @EmployeeId, @IsLeader)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@ShiftId", shiftId);
                command.Parameters.AddWithValue("@EmployeeId", employeeId);
                command.Parameters.AddWithValue("@IsLeader", isLeader);
                command.ExecuteNonQuery();
            }
        }

        // Skifter en medarbejder ud med en anden på en vagt
        public void ReplaceOnShift(int shiftId, int oldEmployeeId, int newEmployeeId)
        {
            string sql = "UPDATE ShiftAssignment SET EmployeeId = @NewEmployeeId " +
                         "WHERE ShiftId = @ShiftId AND EmployeeId = @OldEmployeeId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@NewEmployeeId", newEmployeeId);
                command.Parameters.AddWithValue("@ShiftId", shiftId);
                command.Parameters.AddWithValue("@OldEmployeeId", oldEmployeeId);
                command.ExecuteNonQuery();
            }
        }

        // Tager en medarbejder af en vagt
        public void RemoveFromShift(int shiftId, int employeeId)
        {
            string sql = "DELETE FROM ShiftAssignment WHERE ShiftId = @ShiftId AND EmployeeId = @EmployeeId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@ShiftId", shiftId);
                command.Parameters.AddWithValue("@EmployeeId", employeeId);
                command.ExecuteNonQuery();
            }
        }

        // Sletter alle planer, så programmet kan køres igen
        public void DeleteAllPlans()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                new SqlCommand("DELETE FROM ShiftAssignment", connection).ExecuteNonQuery();
                new SqlCommand("DELETE FROM Shift", connection).ExecuteNonQuery();
                new SqlCommand("DELETE FROM MonthPlan", connection).ExecuteNonQuery();
            }
        }
    }
}
