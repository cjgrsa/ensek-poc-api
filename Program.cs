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
    private static readonly string inputCsvPath = @"C:\workspace\ensek-poc-api\execution.csv";
    private static readonly string resultsFolder = @"C:\workspace\ensek-poc-api\results";

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

            // TEST 0 Obtain an access token
            var token = await loginEndpoint.GetAccessToken();
            apiClient.SetAuthorizationHeader(token);

            // TEST 1 Reset the test data
            await resetEndpoint.ResetTestData();

            // TEST 2 Buy a quantity of each fuel
            foreach (var fuel in fuelQuantities)
            {
                await buyEndpoint.BuyFuel(fuel.Key, fuel.Value.buyAmount, resultsCsvPath);
            }

            // Reload fuelQuantities with the updated CSV file
            var updatedFuelQuantities = ReadCsvFile(resultsCsvPath);

            // TEST 3 Verify that each order from the previous step is returned in the /orders list with the expected details
            var orders = await ordersEndpoint.GetOrders();
            foreach (var fuel in updatedFuelQuantities)
            {
                var orderId = fuel.Value.orderId;
                if (string.IsNullOrEmpty(orderId))
                {
                    UpdateCsvField(resultsCsvPath, fuel.Key, "buy_validation_order", "Skip - No ID");
                }
                else
                {
                    var order = orders.FirstOrDefault(o => o.Id == orderId);
                    if (order != null)
                    {
                        // Validate the fuel type and quantity
                        var expectedFuel = GetFuelName(fuel.Key);
                        var expectedQuantity = fuel.Value.buyAmount;

                        var fuelTypeValidationResult = order.Fuel == expectedFuel ? "pass" : "fail";
                        var quantityValidationResult = order.Quantity == expectedQuantity ? "pass" : "fail";

                        UpdateCsvField(resultsCsvPath, fuel.Key, "buy_validation_order", "pass");
                        UpdateCsvField(resultsCsvPath, fuel.Key, "buy_validation_fuel_type", fuelTypeValidationResult);
                        UpdateCsvField(resultsCsvPath, fuel.Key, "buy_validation_fuel_amount", quantityValidationResult);

                        if (fuelTypeValidationResult == "fail" || quantityValidationResult == "fail")
                        {
                            Console.WriteLine($"Order ID {orderId} does not match expected fuel or quantity. Expected: {expectedFuel}, {expectedQuantity}. Actual: {order.Fuel}, {order.Quantity}.");
                        }
                    }
                    else
                    {
                        UpdateCsvField(resultsCsvPath, fuel.Key, "buy_validation_order", "fail");
                        Console.WriteLine($"Order ID {orderId} not found in the returned orders.");
                    }
                }
            }

            // Confirm how many orders were created before the current date
            var currentDate = DateTime.UtcNow.Date;
            var ordersBeforeCurrentDate = orders.FindAll(o => DateTime.TryParse(o.Time, out var orderTime) && orderTime.Date < currentDate);
            Console.WriteLine($"Orders created before the current date: {ordersBeforeCurrentDate.Count}");

  
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static Dictionary<int, (int buyAmount, string orderId)> ReadCsvFile(string filePath)
    {
        var fuelQuantities = new Dictionary<int, (int buyAmount, string orderId)>();

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
                var orderId = csv.GetField<string>("order_id");
                fuelQuantities[fuelType] = (buyAmount, orderId);
            }
        }

        return fuelQuantities;
    }

    static void UpdateCsvField(string filePath, int fuelType, string fieldName, string newValue)
    {
        var lines = File.ReadAllLines(filePath).ToList();
        for (int i = 1; i < lines.Count; i++) // Skip header line
        {
            var fields = lines[i].Split(',');
            if (int.Parse(fields[0]) == fuelType)
            {
                fields[Array.IndexOf(lines[0].Split(','), fieldName)] = newValue;
                lines[i] = string.Join(',', fields);
                break;
            }
        }
        File.WriteAllLines(filePath, lines);
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