using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeCare.ERP.Model;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PrimeCare.ERP.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly OdooConfig _odooConfig;
        private readonly HttpClient _httpClient;

        public EmployeeController()
        {
            _odooConfig = new OdooConfig
            {
                ApiUrl = "http://localhost:8069",
                DbName = "dsp",
                UserName = "admin@example.com",
                Password = "admin1234"
            };
            _httpClient = new HttpClient();
        }

        private async Task<int?> LoginAsync()
        {
            var loginPayload = new
            {
                jsonrpc = "2.0",
                method = "call",
                @params = new
                {
                    db = _odooConfig.DbName,
                    login = _odooConfig.UserName,
                    password = _odooConfig.Password
                }
            };

            var response = await SendRequestAsync("/web/session/authenticate", loginPayload);

            if (response == null || response["result"] == null || response["result"]["uid"] == null)
            {
                Console.WriteLine("Failed to log in. Response: " + response);
                return null;
            }

            return response["result"]["uid"].Value<int?>();
        }


        [HttpPost]
        [SwaggerOperation(Summary = "Save a new employee", Description = "Saves a new employee to the system.")]
        [SwaggerResponse(200, "Employee saved successfully.", typeof(Response))]
        [SwaggerResponse(400, "Bad request. Invalid employee data.")]
        public async Task<Response> SaveEmployee([FromBody] EmployeeData employee)
        {
            try
            {
                // Odoo login
                var userId = await LoginAsync();
                if (userId == null)
                {
                    return new Response() { Success = false, Message = "Login failed." };
                }

                // Prepare employee data for Odoo
                var createValues = new Dictionary<string, object>
                    {
                        { "name", employee.FirstName },
                        { "middle_name", employee.MiddleName },
                        { "last_name", employee.LastName },
                        { "active", employee.Active },
                        { "woreda", employee.Woreda },
                        { "kebele", employee.Kebele },
                        { "sub_city", employee.SubCity },
                        { "private_city", employee.PrivateCity },
                        { "house_number", employee.HouseNumber },
                        { "postal_code", employee.PostalCode },
                        { "private_phone", employee.PrivatePhone },
                        { "private_email", employee.PrivateEmail },
                        { "liscence_number", employee.LiscenceNumber },
                        { "liscence_issue_date", employee.LiscenceIssueDate },
                        { "employee_type", employee.EmployeeType }
                    };


                // Post employee data to Odoo
                var createResult = await CreateAsync("hr.employee", createValues);
                if (createResult != null)
                {
                    return new Response() { Success = true, Message = "Posting Successful." };
                }
                else
                {
                    return new Response() { Success = false, Message = "Posting failed." };
                }
            }
            catch (Exception ex)
            {
                return new Response() { Success = false, Message = $"Error saving employee: {ex.Message}" };
            }
        }

        private async Task<JToken> CreateAsync(string model, Dictionary<string, object> values)
        {
            var createPayload = new
            {
                jsonrpc = "2.0",
                method = "call",
                @params = new
                {
                    model,
                    method = "create",
                    args = new[] { values },
                    kwargs = new { }
                }
            };

            var response = await SendRequestAsync("/web/dataset/call_kw", createPayload);
            return response["result"];
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Check employee status", Description = "Checks whether an employee is active or not.")]
        [SwaggerResponse(200, "Employee status retrieved successfully.", typeof(Response))]
        [SwaggerResponse(404, "Employee not found.")]
        public async Task<Response> CheckEmployeeStatus(int id)
        {
            try
            {

                var userId = await LoginAsync();
                if (userId == null)
                {
                    return new Response() { Success = false, Message = "Login failed." };
                }

                var readFields = new[] { "active" };
                var readResult = await ReadAsync("hr.employee", id, readFields);

                if (readResult != null && readResult is JArray jArray && jArray.Count > 0)
                {
                    var employeeData = jArray[0] as JObject;
                    if (employeeData != null && employeeData["active"] != null)
                    {
                        bool isActive = employeeData["active"].Value<bool>();
                        return new Response() { Success = true, Message = isActive ? "Employee is active." : "Employee is not active." };
                    }
                    else
                    {
                        return new Response() { Success = false, Message = "Employee data not found or does not have 'active' field." };
                    }
                }
                else
                {
                    return new Response() { Success = false, Message = "Employee not found." };
                }
            }
            catch (Exception ex)
            {
                return new Response() { Success = false, Message = $"Error checking employee status: {ex.Message}" };
            }
        }


        private async Task<JToken> ReadAsync(string model, int id, string[] fields)
        {
            var readPayload = new
            {
                jsonrpc = "2.0",
                method = "call",
                @params = new
                {
                    model = model,
                    method = "read",
                    args = new object[] { new[] { id }, fields },
                    kwargs = new { }
                }
            };

            var response = await SendRequestAsync("/web/dataset/call_kw", readPayload);
            return response["result"];
        }

       
      

        private async Task<JToken> SendRequestAsync(string endpoint, object payload)
        {
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(new Uri(new Uri(_odooConfig.ApiUrl), endpoint), content);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("HTTP request failed. Status code: " + response.StatusCode);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Response Content: " + responseContent);

            try
            {
                return JsonConvert.DeserializeObject<JToken>(responseContent);
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine("Failed to parse JSON response: " + ex.Message);
                return null;
            }
        }
    }
}
