
Program.cs is the entry point for the script (You can execute the Main directly)

Endpoint Folder - Hosts all the endpoints from the webpage

ApiClient.cs deals with the types of API calls

execution.csv is where you can setup the test iterations

Results folder stroed the results after executing



Note:  The one thing I couldn't get working on my environment was NUnit with Allure (and for the life of me I can't figure out why) . I created a PipelineTests.cs and imported NUnit and Allure so that I could get some decent looking reports - but NUnit is not picking up any tests in my project just stuck on "Updating Test List"




If i was to continue the framework i would do the following:
- Get NUnit and Allure working to make the pipeline tests easier to implement
- Make the components more resuable - at the moment they are fit for purpose (which is the assignemt)
- Group th etests into structured classes
- connect the framework to the clients test management tool via API (If needed)

      -> This would then be able to take over the input data and the results/dashboards
