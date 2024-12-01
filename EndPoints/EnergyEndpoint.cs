using System.Threading.Tasks;

namespace EnsekApiTests.Endpoints
{
    public class EnergyEndpoint
    {
        private readonly ApiClient _apiClient;

        public EnergyEndpoint(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<string> GetEnergyDetails()
        {
            var response = await _apiClient.GetAsync("/ENSEK/energy");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}