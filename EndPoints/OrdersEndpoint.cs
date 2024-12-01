using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnsekApiTests.Endpoints
{
    public class OrdersEndpoint
    {
        private readonly ApiClient _apiClient;

        public OrdersEndpoint(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<Order>> GetOrders()
        {
            var response = await _apiClient.GetAsync("/ENSEK/orders");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Order>>(responseString);
        }
    }
}