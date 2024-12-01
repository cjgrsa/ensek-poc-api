using System.Threading.Tasks;

namespace EnsekApiTests.Endpoints
{
    public class ResetEndpoint
    {
        private readonly ApiClient _apiClient;

        public ResetEndpoint(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task ResetTestData()
        {
            var response = await _apiClient.PostAsync("/ENSEK/reset", null);
            response.EnsureSuccessStatusCode();
            Console.WriteLine("Test data reset successfully.");
        }
    }
}