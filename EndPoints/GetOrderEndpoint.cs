using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnsekApiTests.Endpoints
{
    public class GetOrderEndpoint
    {
        private readonly ApiClient _apiClient;

        public GetOrderEndpoint(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<Order> GetOrder(string orderId)
        {
            var response = await _apiClient.GetAsync($"/ENSEK/orders/{orderId}");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Order>(responseString);
        }
    }
}