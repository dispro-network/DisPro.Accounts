using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DisPro.Accounts.Migrations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace DisPro.Accounts
{
    public class Program
    {
        private static readonly string _env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        public static void Main(string[] args)
        {

            if (args == null || args.Length == 0)
            {
                Console.WriteLine("No command was provided. Valid commands are \"run\", \"migrate\" and \"destroy\"");
                return;
            }

            var cmd = args[0];

            switch (cmd)
            {
                case "run":
                    handleRun();
                    break;
                case "migrate":
                    handleMigrate();
                    break;
                case "destroy":
                    handleDestroy();
                    break;
                default:
                    Console.WriteLine("Invalid command was provided. Valid commands are \"run\", \"migrate\" and \"destroy\"");
                    return;
            }
        }

        private static void handleRun()
        {
            IHost host;
            switch (_env)
            {
                case "Development":
                    host = CreateLocalHostBuilder().Build();
                    host.Run();
                    break;
                case "DevelopmentServer":
                    host = CreateServerHostBuilder().Build();
                    host.Run();
                    break;
                case "Staging":
                    break;
                case "Production":
                    break;
                default:
                    Console.WriteLine("Invalid ASPNETCORE_ENVIRONMENT provided.");
                    Environment.Exit(1);
                    break;
            }
        }

        private static void handleMigrate()
        {
            switch (_env)
            {
                case "Development":
                case "DevelopmentServer":
                    var host = CreateMigratorHostBuilder().Build();
                    using (var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
                    {
                        var migrator = scope.ServiceProvider.GetRequiredService<Migrator>();
                        migrator.Migrate();
                        migrator.SeedIdentityServer();
                        migrator.SeedUsers();
                    }
                    break;
                case "Staging":
                    break;
                case "Production":
                    break;
                default:
                    Console.WriteLine("Invalid ASPNETCORE_ENVIRONMENT provided.");
                    Environment.Exit(1);
                    return;
            }
        }

        private static void handleDestroy()
        {
            switch (_env)
            {
                case "Development":
                case "DevelopmentServer":
                    var host = CreateMigratorHostBuilder().Build();
                    using (var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
                    {
                        var migrator = scope.ServiceProvider.GetRequiredService<Migrator>();
                        migrator.Destroy();
                    }
                    break;
                default:
                    Console.WriteLine("Invalid ASPNETCORE_ENVIRONMENT provided for the destroy command. Data can only be destroyed on Development or DevelopmentServer");
                    return;
            }
        }


        private static void HandleLocalDevelopment()
        {

            //var host = CreateLocalDevelopmentHostBuilder().Build();

            //if (clean)
            //{
            //    using var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            //    Console.WriteLine("Cleaning!");
            //    SeedData.EnsureCleanData(scope.ServiceProvider);
            //}

            //if (seed)
            //{
            //    using var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            //    Console.WriteLine("Seeding!");
            //    SeedData.EnsureSeedData(scope.ServiceProvider);
            //}

            //if (dontRun) return;

            //host.Run();
        }

        private static IHostBuilder CreateLocalHostBuilder()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.Development.json", optional: false, reloadOnChange: true)
                .AddJsonFile("secrets/appsettings.secrets.json", optional: false)
                .Build();

            var certificateSettings = config.GetSection("CertificateSettings");
            string certificateFile = certificateSettings.GetValue<string>("Filename");
            string certificatePassword = certificateSettings.GetValue<string>("Password");

            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostingContext, configBuilder) =>
                {
                    configBuilder.AddJsonFile("secrets/appsettings.secrets.json", optional: false, reloadOnChange: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(options =>
                    {
                        options.Listen(IPAddress.Loopback, 5000); // This port is required for https redirection
                        options.Listen(IPAddress.Loopback, 5001, listenOptions =>
                        {
#pragma warning disable IDE0067 // Dispose objects before losing scope. If disposed here, application will fail
                            var certificate = new X509Certificate2(certificateFile, certificatePassword);
#pragma warning restore IDE0067 // Dispose objects before losing scope. If disposed here, application will fail
                            listenOptions.UseHttps(certificate);
                        });
                    });

                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseSerilog((context, configuration) =>
                    {
                        configuration
                            .MinimumLevel.Debug()
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                            .MinimumLevel.Override("System", LogEventLevel.Information)
                            .Enrich.FromLogContext()
                            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate);
                    });
                });
        }

        private static IHostBuilder CreateServerHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostingContext, configBuilder) =>
                {
                    configBuilder.AddJsonFile("secrets/appsettings.secrets.json", optional: false, reloadOnChange: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseSerilog((context, configuration) =>
                    {
                        configuration
                            .MinimumLevel.Debug()
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                            .MinimumLevel.Override("System", LogEventLevel.Information)
                            .Enrich.FromLogContext()
                            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate);
                    });
                });
        }

        private static IHostBuilder CreateMigratorHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("secrets/appsettings.secrets.json", optional: false, reloadOnChange: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<MigratorStartup>();
                    webBuilder.UseSerilog((context, configuration) =>
                    {
                        configuration
                            .MinimumLevel.Debug()
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                            .MinimumLevel.Override("System", LogEventLevel.Information)
                            .Enrich.FromLogContext()
                            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate);
                    });
                });
        }
    }
}
