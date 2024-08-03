using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class OdooApiClient
{
    private readonly OdooConfig _config;
    private readonly HttpClient _httpClient;

    public OdooApiClient(OdooConfig config)
    {
        _config = config;
        _httpClient = new HttpClient();
    }

    public async Task<int?> LoginAsync()
    {
        var loginPayload = new
        {
            jsonrpc = "2.0",
            method = "call",
            @params = new
            {
                db = _config.DbName,
                login = _config.UserName,
                password = _config.Password
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

    public async Task<JToken> CreateAsync(string model, Dictionary<string, object> values)
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

    public async Task<JToken> ReadAsync(string model, int id, string[] fields)
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

    public async Task<JToken> UpdateAsync(string model, int id, Dictionary<string, object> values)
    {
        var updatePayload = new
        {
            jsonrpc = "2.0",
            method = "call",
            @params = new
            {
                model = model,
                method = "write",
                args = new object[] { new[] { id }, values },
                kwargs = new { }
            }
        };

        var response = await SendRequestAsync("/web/dataset/call_kw", updatePayload);
        return response["result"];
    }

    public async Task<JToken> DeleteAsync(string model, int id)
    {
        var deletePayload = new
        {
            jsonrpc = "2.0",
            method = "call",
            @params = new
            {
                model = model,
                method = "unlink",
                args = new[] { new[] { id } },
                kwargs = new { }
            }
        };

        var response = await SendRequestAsync("/web/dataset/call_kw", deletePayload);
        return response["result"];
    }

    private async Task<JToken> SendRequestAsync(string endpoint, object payload)
    {
        var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(new Uri(new Uri(_config.ApiUrl), endpoint), content);

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
