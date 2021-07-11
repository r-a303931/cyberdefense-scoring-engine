using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EngineController.Data
{
    class EngineControllerDataContextFactory : IDesignTimeDbContextFactory<EngineControllerContext>
    {
        public EngineControllerContext CreateDbContext(string[] args)
        {
            var EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../EngineController"))
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.OSVersion.Platform}.json", optional: true)
                .AddJsonFile($"appsettings.{EnvironmentName}.json", optional: true)
                .AddJsonFile($"appsettings.{Environment.OSVersion.Platform}.{EnvironmentName}.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<EngineControllerContext>();
            optionsBuilder.UseSqlite(configuration.GetConnectionString("EngineControllerContext"));

            return new EngineControllerContext(optionsBuilder.Options);
        }
    }
}
