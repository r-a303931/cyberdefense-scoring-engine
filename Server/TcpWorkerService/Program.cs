using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;

using EngineController.Data;
using EngineController.Workers.TcpConnectionService;

namespace EngineController.Workers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            CreateDbIfNotExists(host);

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) => builder
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{Environment.OSVersion.Platform}.json", optional: true)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                    .AddJsonFile($"appsettings.{Environment.OSVersion.Platform}.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables())
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<EngineControllerContext>(options =>
                            options.UseSqlite(hostContext.Configuration.GetConnectionString("EngineControllerContext")));

                    services.AddHostedService<TcpService>();
                });

        private static void CreateDbIfNotExists(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<EngineControllerContext>();
                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred creating the DB.");
            }
        }
    }
}
