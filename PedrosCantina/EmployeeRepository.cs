using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace PedrosCantina
{
    public class EmployeeRepository
    {
        private string connectionString;

        public EmployeeRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        // Create
        public int Add(Employee employee)
        {
            string sql = "INSERT INTO Employee (Name, Phone, Email, CanBeLeader) " +
                         "OUTPUT INSERTED.EmployeeId " +
                         "VALUES (@Name, @Phone, @Email, @CanBeLeader)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Name", employee.Name);
                command.Parameters.AddWithValue("@Phone", employee.Phone);
                command.Parameters.AddWithValue("@Email", employee.Email);
                command.Parameters.AddWithValue("@CanBeLeader", employee.CanBeLeader);

                int newId = (int)command.ExecuteScalar();
                employee.EmployeeId = newId;
                return newId;
            }
        }

        // Read (alle medarbejdere)
        public List<Employee> GetAll()
        {
            List<Employee> employees = new List<Employee>();
            string sql = "SELECT EmployeeId, Name, Phone, Email, CanBeLeader FROM Employee";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    employees.Add(ReadEmployee(reader));
                }
            }
            return employees;
        }

        // Read (én medarbejder)
        public Employee GetById(int id)
        {
            string sql = "SELECT EmployeeId, Name, Phone, Email, CanBeLeader FROM Employee " +
                         "WHERE EmployeeId = @Id";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Id", id);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return ReadEmployee(reader);
                }
            }
            return null;
        }

        // Update
        public bool Update(Employee employee)
        {
            string sql = "UPDATE Employee SET Name = @Name, Phone = @Phone, Email = @Email, " +
                         "CanBeLeader = @CanBeLeader WHERE EmployeeId = @Id";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Name", employee.Name);
                command.Parameters.AddWithValue("@Phone", employee.Phone);
                command.Parameters.AddWithValue("@Email", employee.Email);
                command.Parameters.AddWithValue("@CanBeLeader", employee.CanBeLeader);
                command.Parameters.AddWithValue("@Id", employee.EmployeeId);

                int rows = command.ExecuteNonQuery();
                return rows > 0;
            }
        }

        // Delete
        public bool Delete(int id)
        {
            string sql = "DELETE FROM Employee WHERE EmployeeId = @Id";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Id", id);

                int rows = command.ExecuteNonQuery();
                return rows > 0;
            }
        }

        // Laver den aktuelle række i reader'en om til et Employee-objekt
        private Employee ReadEmployee(SqlDataReader reader)
        {
            Employee employee = new Employee();
            employee.EmployeeId = (int)reader["EmployeeId"];
            employee.Name = (string)reader["Name"];
            employee.Phone = (string)reader["Phone"];
            employee.Email = (string)reader["Email"];
            employee.CanBeLeader = (bool)reader["CanBeLeader"];
            return employee;
        }
    }
}
