using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
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
        public static void Main(string[] args)
        {
            var clean = args.Any(x => x == "--clean");
            if (clean) args = args.Except(new[] { "--clean" }).ToArray();

            var seed = args.Any(x => x == "--seed");
            if (seed) args = args.Except(new[] { "--seed" }).ToArray();

            var dontRun = args.Any(x => x == "--dont-run");
            if (dontRun) args = args.Except(new[] { "--dont-run" }).ToArray();

            var host = CreateHostBuilder(args).Build();

            if (clean)
            {
                using var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
                Console.WriteLine("Cleaning!");
                SeedData.EnsureCleanData(scope.ServiceProvider);
            }

            if (seed)
            {
                using var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
                Console.WriteLine("Seeding!");
                SeedData.EnsureSeedData(scope.ServiceProvider);
            }

            if (dontRun) return;

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                .Build();

            var certificateSettings = config.GetSection("CertificateSettings");
            string certificateFile = certificateSettings.GetValue<string>("Filename");
            string certificatePassword = certificateSettings.GetValue<string>("Password");
            var connectionStrings = config.GetSection("ConnectionStrings");
            string serilogConnectionString = connectionStrings.GetValue<string>("SerilogConnection");

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(options =>
                    {
                        options.Listen(IPAddress.Loopback, 5000); // This port is required for https redirection
                        options.Listen(IPAddress.Loopback, 5001, listenOptions =>
                        {
#if DEBUG
#pragma warning disable IDE0067 // Dispose objects before losing scope. If disposed here, application will fail
                            var certificate = new X509Certificate2(certificateFile, certificatePassword);
#pragma warning restore IDE0067 // Dispose objects before losing scope. If disposed here, application will fail
                            listenOptions.UseHttps(certificate);
#endif

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
    }
}
