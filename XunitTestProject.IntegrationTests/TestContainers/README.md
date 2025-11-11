# Test Containers Integration Testing

This directory demonstrates integration testing using **Testcontainers for .NET** with SQL Server containers as an alternative to LocalDB.

## Overview

Testcontainers provides lightweight, throwaway instances of databases running in Docker containers. This approach offers several advantages over traditional database testing methods.

## Benefits of Test Containers

### ✅ Advantages
- **Production Parity**: Uses same SQL Server version as production
- **Isolation**: Each test gets fresh container instance
- **Portability**: Works on any machine with Docker (Windows, macOS, Linux)
- **CI/CD Friendly**: Perfect for build pipelines and GitHub Actions
- **Version Control**: Exact database version specified in code
- **No Setup Required**: No need to install SQL Server locally

### 🔄 Considerations
- **Docker Dependency**: Requires Docker to be running
- **Startup Time**: Container initialization adds ~2-5 seconds
- **Resource Usage**: Slightly higher memory usage than LocalDB
- **Learning Curve**: Developers need basic Docker knowledge

## Architecture

```
TestContainers/
├── Fixtures/
│   └── TestContainerDatabaseFixture.cs     # SQL Server container setup
├── Collections/
│   └── TestContainerDatabaseCollection.cs  # xUnit collection definition
├── Base/
│   └── TestContainerIntegrationTestBase.cs # Base class for container tests
└── TestContainerDepartmentsServiceIntegrationTests.cs # Integration tests
```

## Container Configuration

- **Image**: `mcr.microsoft.com/mssql/server:2022-latest`
- **Memory**: 2GB allocated to container
- **Port**: Dynamically assigned by Testcontainers
- **Database**: Fresh `master` database per test run
- **Credentials**: `sa` user with auto-generated password

## Test Categories

### 🐳 Container Health Tests
- Container startup verification
- Connection string validation
- Performance monitoring

### 📊 Data Operations Tests
- CRUD operations with containerized SQL Server
- Transaction rollback testing
- Bulk operation performance

### 🔒 Isolation Tests
- Container-specific data verification
- Complete test isolation
- Concurrent test support

## Usage Example

```csharp
public class MyServiceIntegrationTests : TestContainerIntegrationTestBase
{
    public MyServiceIntegrationTests(TestContainerDatabaseFixture databaseFixture) 
        : base(databaseFixture)
    {
    }

    [Fact]
    public async Task MyTest_ShouldWorkWithContainer()
    {
        // Arrange
        await AssertContainerHealthyAsync();
        
        // Act & Assert
        // Your test code here
    }
}
```

## Running Tests

### Prerequisites
1. **Docker Desktop** must be installed and running
2. **Docker daemon** must be accessible

### Command Line
```powershell
# Run all container tests
dotnet test --filter "TestContainerDepartmentsServiceIntegrationTests"

# Run specific container test
dotnet test --filter "Container_ShouldBeHealthyAndResponsive"
```

### Visual Studio
- Open Test Explorer
- Look for tests under `TestContainerDepartmentsServiceIntegrationTests`
- Right-click and "Run Tests"

## Troubleshooting

### Common Issues

1. **Docker Not Running**
   ```
   Error: Docker daemon is not running
   Solution: Start Docker Desktop
   ```

2. **Port Already in Use**
   ```
   Error: Port binding failed
   Solution: Testcontainers automatically handles port assignment
   ```

3. **Container Startup Timeout**
   ```
   Error: Container failed to start within timeout
   Solution: Increase timeout in TestContainerDatabaseFixture.cs
   ```

### Debugging Container Issues

```csharp
// Add to your test to debug container details
[Fact]
public void Debug_ContainerInfo()
{
    var (hostname, port, image) = GetContainerInfo();
    _output.WriteLine($"Container: {hostname}:{port} using {image}");
    
    var connectionString = _databaseFixture.ConnectionString;
    _output.WriteLine($"Connection: {connectionString}");
}
```

## Comparison: LocalDB vs Test Containers

| Feature | LocalDB | Test Containers |
|---------|---------|----------------|
| **Setup** | Windows only | Cross-platform |
| **Performance** | Faster startup | Slight overhead |
| **CI/CD** | Windows runners only | Any Docker-enabled runner |
| **Isolation** | Database per test | Container per test |
| **Production Parity** | SQL Server Express | Full SQL Server |
| **Dependencies** | SQL Server LocalDB | Docker |

## Performance Benchmarks

Based on typical test runs:
- **Container Startup**: ~2-3 seconds
- **Test Execution**: Comparable to LocalDB
- **Memory Usage**: ~400MB per container
- **Cleanup**: Automatic on test completion

## Best Practices

### ✅ Do
- Use container health checks
- Implement proper cleanup in fixtures
- Cache container instances when possible
- Monitor container resource usage

### ❌ Don't
- Share containers between test classes
- Skip Docker availability checks
- Use containers for unit tests
- Hardcode connection details

## Migration Guide

To migrate existing LocalDB tests to containers:

1. **Copy test class** from main integration tests
2. **Extend TestContainerIntegrationTestBase** instead of IntegrationTestBase
3. **Use TestContainerDatabaseCollection** attribute
4. **Update constructor** to accept TestContainerDatabaseFixture
5. **Add container health checks** as needed

## Further Reading

- [Testcontainers for .NET Documentation](https://dotnet.testcontainers.org/)
- [SQL Server Container Guide](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-docker-container-deployment)
- [xUnit Collection Fixtures](https://xunit.net/docs/shared-context)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)