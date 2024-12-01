using System;
using System.Threading.Tasks;
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

        public async Task BuyFuel(int id, int quantity)
        {
            var response = await _apiClient.PutAsync($"/ENSEK/buy/{id}/{quantity}");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Bought {quantity} units of fuel {id}: {responseString}");

            // Parse the response string
            var buyResponse = JsonConvert.DeserializeObject<BuyResponse>(responseString);

            // Validate the response dynamically
            ValidateBuyResponse(buyResponse, id, quantity);
        }

        private void ValidateBuyResponse(BuyResponse response, int id, int quantity)
        {
            if (response.Message.Contains("There is no nuclear fuel to purchase!"))
            {
                Console.WriteLine($"Skipped validation for fuel {id} with quantity {quantity}.");
            }
            else if (response.Message.Contains($"You have purchased {quantity}"))
            {
                Console.WriteLine($"Validation passed for fuel {id} with quantity {quantity}.");
            }
            else
            {
                Console.WriteLine($"Validation failed for fuel {id} with quantity {quantity}.");
            }
        }
    }

    public class BuyResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}