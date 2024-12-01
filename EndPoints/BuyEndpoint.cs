using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using Newtonsoft.Json;

namespace EnsekApiTests.Endpoints
{
    public class BuyEndpoint
    {
        private readonly ApiClient _apiClient;

        public BuyEndpoint(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task BuyFuel(int id, int quantity, string resultsCsvPath)
        {
            var response = await _apiClient.PutAsync($"/ENSEK/buy/{id}/{quantity}");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Bought {quantity} units of fuel {id}: {responseString}");

            // Parse the response string
            var buyResponse = JsonConvert.DeserializeObject<BuyResponse>(responseString);

            // Validate the response dynamically
            var validationResult = ValidateBuyResponse(buyResponse, id, quantity);

            // Extract the order ID from the response message
            var orderId = ExtractOrderId(buyResponse.Message);

            // Write validation result and order ID back to the timestamped CSV file
            WriteValidationResultAndOrderIdToCsv(resultsCsvPath, id, validationResult, orderId);
        }

        private string ValidateBuyResponse(BuyResponse response, int id, int quantity)
        {
            if (response.Message.Contains("There is no nuclear fuel to purchase!"))
            {
                return "skipped";
            }
            else if (response.Message.Contains($"You have purchased {quantity}"))
            {
                return "pass";
            }
            else
            {
                return "fail";
            }
        }

        private string ExtractOrderId(string message)
        {
            // Use a regular expression to extract the order ID
            var match = Regex.Match(message, @"id is (\w{8}-\w{4}-\w{4}-\w{4}-\w{12})\.");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private void WriteValidationResultAndOrderIdToCsv(string resultsCsvPath, int id, string validationResult, string orderId)
        {
            var lines = File.ReadAllLines(resultsCsvPath).ToList();
            for (int i = 1; i < lines.Count; i++) // Skip header line
            {
                var fields = lines[i].Split(',');
                if (int.Parse(fields[0]) == id)
                {
                    fields[2] = validationResult; // Update buy_validation_msg
                    fields[3] = orderId; // Update order_id
                    lines[i] = string.Join(',', fields);
                    break;
                }
            }
            File.WriteAllLines(resultsCsvPath, lines);
        }
    }

    public class BuyResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}