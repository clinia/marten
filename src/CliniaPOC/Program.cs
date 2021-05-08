using System;
using System.Threading.Tasks;
using CliniaPOC.Extensions;
using CliniaPOC.Infrastructure;
using CliniaPOC.Models;
using Marten;
using Marten.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace CliniaPOC
{
    class Program
    {
        public static IConfiguration Configuration { get; private set; }

        public static string Env { get; private set; }

        public static async Task Main(string[] args)
        {
            Env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            try
            {
                Configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", false, true)
                    .AddJsonFile($"appsettings.{Env}.json", true, true)
                    .Build();

                Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                    .CreateLogger();
                Log.Information("Starting up...");

                var services = ConfigureServices();
                var serviceProvider = services.BuildServiceProvider();

                await serviceProvider.GetRequiredService<App>().Run();

                Log.Information("Shutting down...");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton(Configuration);
            services.AddOptions();

            services.AddMarten(opts =>
            {
                opts.Connection(Configuration.GetConnectionString("Default"));
                opts.CreateDatabasesForTenants(c =>
                {
                    c.ForTenant()
                        .CheckAgainstPgDatabase()
                        .CreatePLV8()
                        .WithOwner("postgres");
                });

                if (Env.Equals("development", StringComparison.InvariantCultureIgnoreCase))
                {
                    opts.AutoCreateSchemaObjects = AutoCreate.All;
                }

                opts.Linq.MethodCallParsers.Add(new HealthServiceBySpecialityExtension());
                opts.Linq.MethodCallParsers.Add(new EntityEqualsStringExtension());

                opts.Policies.AllDocumentsAreMultiTenanted();

                opts.Schema.For<HealthFacility>();

                opts.Schema.For<Department>()
                    .ForeignKey<HealthFacility>(x => x.HealthFacility, definition => definition.CascadeDeletes = true);

                opts.Schema.For<Practice>()
                    .ForeignKey<HealthFacility>(x => x.HealthFacility, definition => definition.CascadeDeletes = true)
                    .ForeignKey<Practitioner>(x => x.Practitioner, definition => definition.CascadeDeletes = true);

                opts.Schema.For<Practitioner>();

                opts.Schema.For<HealthService>()
                    .ForeignKey<HealthFacility>(x => x.HealthFacilityId)
                    .ForeignKey<Practitioner>(x => x.PractitionerId)
                    .ForeignKey<Practice>(x => x.PracticeId)
                    .ForeignKey<Department>(x => x.DepartmentId)
                    .Index(x => x.HealthServiceId);

                if (Configuration.GetValue<bool>("Seed"))
                {
                    opts.InitialData.Add(new InitialData());
                }
            });

            // Register the actual application entry point
            services.AddTransient<App>();

            return services;
        }
    }
}
