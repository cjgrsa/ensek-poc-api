using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnsekApiTests.Endpoints
{
    public class UpdateOrderEndpoint
    {
        private readonly ApiClient _apiClient;

        public UpdateOrderEndpoint(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task UpdateOrder(string orderId, Order order)
        {
            var json = JsonConvert.SerializeObject(order);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _apiClient.PostAsync($"/ENSEK/orders/{orderId}", content);
            response.EnsureSuccessStatusCode();
            Console.WriteLine($"Order {orderId} updated successfully.");
        }
    }
}