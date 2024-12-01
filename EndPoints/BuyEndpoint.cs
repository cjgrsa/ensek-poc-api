using System;
using System.Globalization;
using System.IO;
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

            // Write validation result back to the timestamped CSV file
            WriteValidationResultToCsv(resultsCsvPath, id, validationResult);
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

        private void WriteValidationResultToCsv(string resultsCsvPath, int id, string validationResult)
        {
            var lines = File.ReadAllLines(resultsCsvPath).ToList();
            for (int i = 1; i < lines.Count; i++) // Skip header line
            {
                var fields = lines[i].Split(',');
                if (int.Parse(fields[0]) == id)
                {
                    fields[2] = validationResult; // Update buy_validation_msg
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