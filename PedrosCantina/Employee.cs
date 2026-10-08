namespace PedrosCantina
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool CanBeLeader { get; set; }

        public override string ToString()
        {
            return EmployeeId + ": " + Name + ", " + Phone + ", " + Email + ", leader: " + CanBeLeader;
        }
    }
}
