namespace WebApplicationAPI.IntegrationTests
{
    public class DepartmentsServiceTests : IClassFixture<WebApplicationAPIFactory>
    {
        private readonly HttpClient _client;

        public DepartmentsServiceTests(WebApplicationAPIFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_All_Departments_Return_Success()
        {
            // Arrange

            // Act
            var response = await _client.GetAsync("/api/departments");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
        }
    }
}
