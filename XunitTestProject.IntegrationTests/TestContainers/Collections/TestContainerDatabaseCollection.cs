using XunitTestProject.IntegrationTests.TestContainers.Fixtures;

namespace XunitTestProject.IntegrationTests.TestContainers.Collections
{
    /// <summary>
    /// Collection definition for Test Container database fixture
    /// Ensures that tests within the same collection share the same container instance
    /// but tests in different collections get their own containers
    /// </summary>
    [CollectionDefinition("TestContainer collection")]
    public class TestContainerDatabaseCollection : ICollectionFixture<TestContainerDatabaseFixture>
    {
        // This class is empty. It's just used to define the collection.
        // The ICollectionFixture<TestContainerDatabaseFixture> interface tells xUnit
        // that this collection uses the TestContainerDatabaseFixture.
    }
}