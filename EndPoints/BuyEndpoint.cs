using System.Threading.Tasks;

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
        }
    }
}