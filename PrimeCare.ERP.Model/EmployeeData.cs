namespace PrimeCare.ERP.Model
{
    public class EmployeeData
    {
        public EmployeeData()
        {

        }

        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public bool Active { get; set; }
        public string Woreda { get; set; }
        public string Kebele { get; set; }
        public string SubCity { get; set; }
        public string PrivateCity { get; set; }
        public string HouseNumber { get; set; }
        public string PostalCode { get; set; }
        public string PrivatePhone { get; set; }
        public string PrivateEmail { get; set; }
        public string LiscenceNumber { get; set; }
        public string LiscenceIssueDate { get; set; }
        public string EmployeeType { get; set; }
    }
}
