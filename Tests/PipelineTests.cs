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
using NUnit.Allure.Attributes;
using NUnit.Allure.Core;
using NUnit.Framework;

namespace EnsekApiTests.Tests
{
    [TestFixture]
    [AllureNUnit]
    public class PipelineTests
    {
        private static readonly string baseUrl = "https://qacandidatetest.ensek.io";
        private static readonly string inputCsvPath = @"C:\workspace\EnsekApiTests\execution.csv";
        private static readonly string resultsFolder = @"C:\workspace\EnsekApiTests\results";

        private ApiClient apiClient;
        private LoginEndpoint loginEndpoint;
        private ResetEndpoint resetEndpoint;
        private BuyEndpoint buyEndpoint;
        private OrdersEndpoint ordersEndpoint;

        [SetUp]
        public async Task SetUp()
        {
            apiClient = new ApiClient(baseUrl);
            loginEndpoint = new LoginEndpoint(apiClient);
            resetEndpoint = new ResetEndpoint(apiClient);
            buyEndpoint = new BuyEndpoint(apiClient);
            ordersEndpoint = new OrdersEndpoint(apiClient);

            // Obtain an access token
            var token = await loginEndpoint.GetAccessToken();
            apiClient.SetAuthorizationHeader(token);
        }

        [Test]
        [AllureStory("Reset Test Data")]
        public async Task TestResetTestData()
        {
            await resetEndpoint.ResetTestData();
        }

        [Test]
        [AllureStory("Buy Fuel")]
        public async Task TestBuyFuel()
        {
            // Read the input CSV file
            var fuelQuantities = ReadCsvFile(inputCsvPath);

            // Create a timestamped copy of the CSV file for results
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var resultsCsvPath = Path.Combine(resultsFolder, $"execution_{timestamp}.csv");
            File.Copy(inputCsvPath, resultsCsvPath, overwrite: true);

            // Buy a quantity of each fuel
            foreach (var fuel in fuelQuantities)
            {
                await buyEndpoint.BuyFuel(fuel.Key, fuel.Value.buyAmount, resultsCsvPath);
            }
        }

        [Test]
        [AllureStory("Verify Orders")]
        public async Task TestVerifyOrders()
        {
            // Reload fuelQuantities with the updated CSV file
            var updatedFuelQuantities = ReadCsvFile(Path.Combine(resultsFolder, $"execution_{DateTime.Now.ToString("yyyyMMddHHmmss")}.csv"));

            // Verify that each order from the previous step is returned in the /orders list with the expected details
            var orders = await ordersEndpoint.GetOrders();
            foreach (var fuel in updatedFuelQuantities)
            {
                var orderId = fuel.Value.orderId;
                if (string.IsNullOrEmpty(orderId))
                {
                    UpdateCsvField(Path.Combine(resultsFolder, $"execution_{DateTime.Now.ToString("yyyyMMddHHmmss")}.csv"), fuel.Key, "buy_validation_order", "Skip - No ID");
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

                        UpdateCsvField(Path.Combine(resultsFolder, $"execution_{DateTime.Now.ToString("yyyyMMddHHmmss")}.csv"), fuel.Key, "buy_validation_order", "pass");
                        UpdateCsvField(Path.Combine(resultsFolder, $"execution_{DateTime.Now.ToString("yyyyMMddHHmmss")}.csv"), fuel.Key, "buy_validation_fuel_type", fuelTypeValidationResult);
                        UpdateCsvField(Path.Combine(resultsFolder, $"execution_{DateTime.Now.ToString("yyyyMMddHHmmss")}.csv"), fuel.Key, "buy_validation_fuel_amount", quantityValidationResult);

                        if (fuelTypeValidationResult == "fail" || quantityValidationResult == "fail")
                        {
                            Assert.Fail($"Order ID {orderId} does not match expected fuel or quantity. Expected: {expectedFuel}, {expectedQuantity}. Actual: {order.Fuel}, {order.Quantity}.");
                        }
                    }
                    else
                    {
                        UpdateCsvField(Path.Combine(resultsFolder, $"execution_{DateTime.Now.ToString("yyyyMMddHHmmss")}.csv"), fuel.Key, "buy_validation_order", "fail");
                        Assert.Fail($"Order ID {orderId} not found in the returned orders.");
                    }
                }
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
}