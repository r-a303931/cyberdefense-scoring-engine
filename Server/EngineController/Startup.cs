using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.EntityFrameworkCore;
using EngineController.Data;

using EngineController.Workers.TcpConnectionService;

namespace EngineController
{
    public class Startup
    {
        public Startup(IHostEnvironment env)
        {
            Configuration = ConfigureConfiguration(env);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddRazorPages();

            services.AddDbContext<EngineControllerContext>(options =>
                    options.UseSqlite(Configuration.GetConnectionString("EngineControllerContext")));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "EngineControllerAPI", Version = "v1" });
            });

            services.AddHostedService<TcpService>();
        }

        public IConfigurationRoot ConfigureConfiguration(IHostEnvironment hostingEnvironment)
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.OSVersion.Platform}.json", optional: true)
                .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", optional: true)
                .AddJsonFile($"appsettings.{Environment.OSVersion.Platform}.{hostingEnvironment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EngineController API v1");
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
