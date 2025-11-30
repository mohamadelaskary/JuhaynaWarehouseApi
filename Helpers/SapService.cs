using GBSWarehouse.Helpers;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

public class SapService
{
    private static readonly string CreateOrderUrl = "http://vhjfiprdai01.sap.juhayna.com:8000/sap/zgbs_create_ord";
    private static readonly string CloseBatchUrl = "http://vhjfiprdai01.sap.juhayna.com:8000/sap/zgbs_closebatch";
    private static readonly string Username = "WM.GBS";
    private static readonly string Password = "WM.2023";

    // ===================== CREATE ORDER =====================
    public static async Task<CreateSapOrderResponse> CreateSapOrderAsync(CreateSapOrderParameters obj)
    {
        using var client = new HttpClient();

        // Basic Auth
        var byteArray = Encoding.ASCII.GetBytes($"{Username}:{Password}");
        var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        client.DefaultRequestHeaders.Authorization = authHeader;

        // Serialize Body
        var jsonBody = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
        {
            DateFormatString = "dd.MM.yyyy",
            Culture = System.Globalization.CultureInfo.InvariantCulture
        });

        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        // 🔎 Log Request
        Console.WriteLine("---- CREATE ORDER REQUEST ----");
        Console.WriteLine("URL: " + CreateOrderUrl);
        Console.WriteLine("Body: " + jsonBody);

        try
        {
            var response = await client.PostAsync(CreateOrderUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine("---- CREATE ORDER RESPONSE ----");
            Console.WriteLine("Status: " + response.StatusCode);
            Console.WriteLine("Body: " + responseContent);

            if (!response.IsSuccessStatusCode)
            {
                return new CreateSapOrderResponse
                {
                    messageText = $"HTTP Error {(int)response.StatusCode}",
                    message = responseContent
                };
            }

            var responseMessage = JsonConvert.DeserializeObject<CreateSapOrderResponse>(responseContent);

            return responseMessage ?? new CreateSapOrderResponse
            {
                messageText = "Empty Response",
                message = "No data returned from SAP"
            };
        }
        catch (Exception ex)
        {
            return new CreateSapOrderResponse
            {
                messageText = "Exception Error",
                message = ex.Message
            };
        }
    }

    // ===================== CLOSE BATCH =====================
    public static async Task<CloseBatchResponse> CloseBatchAsync(CloseBatchParameters obj, string csrfToken)
    {
        using var client = new HttpClient();

        // Basic Auth
        var byteArray = Encoding.ASCII.GetBytes($"{Username}:{Password}");
        var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        client.DefaultRequestHeaders.Authorization = authHeader;

        // CSRF Token
        if (!string.IsNullOrEmpty(csrfToken))
        {
            client.DefaultRequestHeaders.Add("x-csrf-token", csrfToken);
        }

        // Serialize Body
        var jsonBody = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
        {
            DateFormatString = "dd.MM.yyyy",
            Culture = System.Globalization.CultureInfo.InvariantCulture
        });

        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        // 🔎 Log Request
        Console.WriteLine("---- CLOSE BATCH REQUEST ----");
        Console.WriteLine("URL: " + CloseBatchUrl);
        Console.WriteLine("Body: " + jsonBody);

        try
        {
            var response = await client.PostAsync(CloseBatchUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine("---- CLOSE BATCH RESPONSE ----");
            Console.WriteLine("Status: " + response.StatusCode);
            Console.WriteLine("Body: " + responseContent);

            if (!response.IsSuccessStatusCode)
            {
                return new CloseBatchResponse
                {
                    messageText = $"HTTP Error {(int)response.StatusCode}",
                    message = responseContent
                };
            }

            var responseMessage = JsonConvert.DeserializeObject<CloseBatchResponse>(responseContent);

            return responseMessage ?? new CloseBatchResponse
            {
                messageText = "Empty Response",
                message = "No data returned from SAP"
            };
        }
        catch (Exception ex)
        {
            return new CloseBatchResponse
            {
                messageText = "Exception Error",
                message = ex.Message
            };
        }
    }
}
