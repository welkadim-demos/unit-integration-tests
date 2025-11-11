using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplicationAPI.Model;

namespace WebApplicationAPI.IntegrationTests
{
    public class WebApplicationAPIFactory : WebApplicationFactory<Program>
    {
        public WebApplicationAPIFactory()
        {
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(IDbContextOptionsConfiguration<AppDbContext>));

                services.Remove(dbContextDescriptor);

                var dbConnectionDescriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbConnection));

                services.Remove(dbConnectionDescriptor);

                // Create open SqliteConnection so EF won't automatically close it.
                //services.AddSingleton<DbConnection>(container =>
                //{
                //    var connection = new SqliteConnection("DataSource=:memory:");
                //    connection.Open();

                //    return connection;
                //});

                services.AddDbContext<AppDbContext>((container, options) =>
                {
                    // use in memory database 
                    options.UseInMemoryDatabase("webapitest");

                    //var connection = container.GetRequiredService<DbConnection>();
                    //options.UseSqlite(connection);
                });
            });

            builder.UseEnvironment("Development");
        }
    }
}

