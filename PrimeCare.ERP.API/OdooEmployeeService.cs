//using PrimeCare.ERP.Model;

//public class OdooEmployeeService
//{
//    private readonly OdooApiClient _odooApiClient;

//    public OdooEmployeeService(OdooApiClient odooApiClient)
//    {
//        _odooApiClient = odooApiClient;
//    }

//    public async Task<bool> CreateEmployeeAsync(EmployeeData employee)
//    {
//        try
//        {
//            var createValues = new Dictionary<string, object>
//            {
//                { "name", employee.FirstName },
//                { "middle_name", employee.MiddleName },
//                { "last_name", employee.LastName },
//                { "employee_type", "employee" },
//            };

//            var result = await _odooApiClient.CreateAsync("hr.employee", createValues);

//            return result != null;
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Error creating employee: {ex.Message}");
//            return false;
//        }
//    }
//}
