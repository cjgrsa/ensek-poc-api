using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnsekApiTests.Endpoints
{
    public class LoginEndpoint
    {
        private readonly ApiClient _apiClient;

        public LoginEndpoint(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<string> GetAccessToken()
        {
            var loginData = new { username = "test", password = "testing" };
            var json = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _apiClient.PostAsync("/ENSEK/login", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseString);

            return tokenResponse.AccessToken;
        }
    }
}