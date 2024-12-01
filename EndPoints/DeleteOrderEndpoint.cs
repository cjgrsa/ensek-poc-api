using System.Threading.Tasks;

namespace EnsekApiTests.Endpoints
{
    public class DeleteOrderEndpoint
    {
        private readonly ApiClient _apiClient;

        public DeleteOrderEndpoint(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task DeleteOrder(string orderId)
        {
            var response = await _apiClient.DeleteAsync($"/ENSEK/orders/{orderId}");
            response.EnsureSuccessStatusCode();
            Console.WriteLine($"Order {orderId} deleted successfully.");
        }
    }
}