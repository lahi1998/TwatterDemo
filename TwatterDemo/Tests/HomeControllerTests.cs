using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TwatterDemo;
using TwatterDemo.Controllers;
using TwatterDemo.Models;
using Xunit;

namespace TwatterDemo.Tests
{
    public class HomeControllerIntegrationTests
    {
        private readonly IServiceProvider _serviceProvider;

        public HomeControllerIntegrationTests()
        {
            // Set up the DI container with the same configuration as your application.
            var services = new ServiceCollection();

            // Configuration setup for testing (minimal)
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection() // Add an empty configuration for testing
                .Build();

            // Register the configuration service
            services.AddSingleton<IConfiguration>(configuration);

            // Configure your database connection here, matching your application configuration.
            string connectionString = "Server=localhost;Database=twatterdb;User=Remote;Password=Kode1234!;";

            services.AddDbContext<DBConnector>(options =>
                options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 26)))
            );

            services.AddLogging();
            services.AddControllersWithViews();
            services.AddSession();

            // Add any other services that your HomeController may require.

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task Login_ValidUser_RedirectsToIndex_Integration()
        {
            // Arrange
            var logger = _serviceProvider.GetRequiredService<ILogger<HomeController>>();
            var dbContext = _serviceProvider.GetRequiredService<DBConnector>();

            // Ensure that the database is created and seeded as needed.
            // You can use a separate method to seed data if necessary.

            // Create an instance of HomeController with the actual database context.
            var controller = new HomeController(logger, dbContext, _serviceProvider.GetRequiredService<IConfiguration>());

            // Set up user data for the test
            var userDto = new UserDTO { userName = "TestUser", password = "TestPassword" };

            // Act
            var result = await controller.Login(userDto);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);

        }
    }
}
