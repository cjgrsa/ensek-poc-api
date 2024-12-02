# Ensek API Test Automation Framework

A test automation framework for interacting with the Ensek API, performing operations like login, reset, buy fuel, and retrieving orders.

## Installation

Ensure you have the following installed:

- .NET SDK 9.0
- Visual Studio or Visual Studio Code

Install the required NuGet packages:

- CsvHelper
- Newtonsoft.Json
- NUnit
- NUnit3TestAdapter
- Allure.NUnit

You can install them via the NuGet Package Manager or using the dotnet CLI.

## Usage

To execute the test suite, run the `Main` method in `Program.cs`. This method reads test data from `execution.csv`, performs the API operations, and stores the results in the `results` folder.

### Input Data

- **execution.csv**: Contains the test data including fuel types and quantities to purchase.

### Output Data

- **results folder**: Stores the timestamped CSV files with test results and validation statuses.

## Project Structure

- **Program.cs**: Entry point of the application.
- **Endpoints Folder**: Contains classes for interacting with different API endpoints.
  - **BuyEndpoint.cs**: Handles buying fuel.
  - **LoginEndpoint.cs**: Handles user login.
  - **ResetEndpoint.cs**: Handles resetting test data.
  - **OrdersEndpoint.cs**: Retrieves orders from the API.
- **ApiClient.cs**: Manages HTTP requests to the API.
- **execution.csv**: Input CSV file for test data.
- **Results Folder**: Stores output CSV files with test results.

## Known Issues

- **NUnit Test Discovery Issue**: The NUnit test adapter is not discovering tests, getting stuck on "Updating Test List". This is due to unresolved conflicts or misconfigurations with Allure.NUnit integration.

- **Solution Attempts**:
  - Ensured Allure.NUnit package is correctly installed.
  - Checked test class and method attributes for correctness.
  - Tried running tests from the console to see if the issue is specific to the Visual Studio Test Explorer.


## Future Work

- **Resolve NUnit and Allure Integration**: Investigate and fix the issue preventing tests from being discovered by NUnit.
- **Enhance Reusability**: Refactor code to make components more reusable beyond the current scope.
- **Structured Test Classes**: Organize tests into coherent classes for better maintainability.
- **Integration with Test Management Tool**: Implement API connections to a test management tool for dynamic test data input and result reporting.
