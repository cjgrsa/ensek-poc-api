using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using EnsekApiTests;
using EnsekApiTests.Endpoints;
using Newtonsoft.Json;

class Program
{
    private static readonly string baseUrl = "https://qacandidatetest.ensek.io";
    private static readonly string inputCsvPath = @"C:\workspace\EnsekApiTests\execution.csv";
    private static readonly string resultsFolder = @"C:\workspace\EnsekApiTests\results";

    static async Task Main(string[] args)
    {
        var apiClient = new ApiClient(baseUrl);
        var loginEndpoint = new LoginEndpoint(apiClient);
        var resetEndpoint = new ResetEndpoint(apiClient);
        var buyEndpoint = new BuyEndpoint(apiClient);
        var ordersEndpoint = new OrdersEndpoint(apiClient);

        try
        {
            // Read the input CSV file
            var fuelQuantities = ReadCsvFile(inputCsvPath);

            // Create a timestamped copy of the CSV file for results
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var resultsCsvPath = Path.Combine(resultsFolder, $"execution_{timestamp}.csv");
            File.Copy(inputCsvPath, resultsCsvPath, overwrite: true);

            // Obtain an access token
            var token = await loginEndpoint.GetAccessToken();
            apiClient.SetAuthorizationHeader(token);

            // Reset the test data
            await resetEndpoint.ResetTestData();

            // Buy a quantity of each fuel
            foreach (var fuel in fuelQuantities)
            {
                await buyEndpoint.BuyFuel(fuel.Key, fuel.Value, resultsCsvPath);
            }

            // Verify that each order from the previous step is returned in the /orders list with the expected details
            var orders = await ordersEndpoint.GetOrders();
            foreach (var fuel in fuelQuantities)
            {
                var order = orders.Find(o => o.Fuel == GetFuelName(fuel.Key) && o.Quantity == fuel.Value);
                if (order == null)
                {
                    Console.WriteLine($"Order for {GetFuelName(fuel.Key)} with quantity {fuel.Value} not found.");
                }
                else
                {
                    Console.WriteLine($"Order for {GetFuelName(fuel.Key)} with quantity {fuel.Value} found.");
                }
            }

            // Confirm how many orders were created before the current date
            var currentDate = DateTime.UtcNow.Date;
            var ordersBeforeCurrentDate = orders.FindAll(o => DateTime.TryParse(o.Time, out var orderTime) && orderTime.Date < currentDate);
            Console.WriteLine($"Orders created before the current date: {ordersBeforeCurrentDate.Count}");

            // Automate any other validation scenarios that you would consider writing for this API
            // For example, check if the reset endpoint actually resets the data
            await resetEndpoint.ResetTestData();
            var ordersAfterReset = await ordersEndpoint.GetOrders();
            if (ordersAfterReset.Count == 0)
            {
                Console.WriteLine("Reset test data successful.");
            }
            else
            {
                Console.WriteLine("Reset test data failed.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

  static Dictionary<int, int> ReadCsvFile(string filePath)
{
    var fuelQuantities = new Dictionary<int, int>();

    var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);
    using (var reader = new StreamReader(filePath))
    using (var csv = new CsvReader(reader, csvConfig))
    {
        // Read the header row
        csv.Read();
        csv.ReadHeader();

        while (csv.Read())
        {
            var fuelType = csv.GetField<int>("fuel_type");
            var buyAmount = csv.GetField<int>("buy_amount");
            fuelQuantities[fuelType] = buyAmount;
        }
    }

    return fuelQuantities;
}


    static string GetFuelName(int id)
    {
        switch (id)
        {
            case 1: return "gas";
            case 2: return "nuclear";
            case 3: return "electric";
            case 4: return "oil";
            default: return "unknown";
        }
    }
}

public class TokenResponse
{
    public TokenResponse()
    {
        AccessToken = string.Empty;
        Message = string.Empty;
    }

    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }
}

public class Order
{
    public Order()
    {
        Fuel = string.Empty;
        Id = string.Empty;
        Time = string.Empty;
    }

    public string Fuel { get; set; }
    public string Id { get; set; }
    public int Quantity { get; set; }
    public string Time { get; set; }
}