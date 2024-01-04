namespace WebClient
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Http.Resilience;

        /// <summary>The application.</summary>
    internal class Program
    {
        public const string HttpClientName = "MyClient";

        /// <summary>Main entry-point for this application.</summary>
        /// <param name="args">An array of command-line argument strings.</param>
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigureAppConfigurationDelegate)
                .ConfigureServices(ConfigureServicesDelegate)
                .Build();

            host.Run();
        }

        /// <summary>Configure services delegate.</summary>
        /// <param name="context">The context.</param>
        /// <param name="services">The services.</param>
        public static void ConfigureServicesDelegate(HostBuilderContext context, IServiceCollection services)
        {
            // This is the worker service that will orchestrate the processing
            services.AddHostedService<WorkerA>();
            services.AddHostedService<WorkerB>();
            services.AddHostedService<WorkerC>();
            services.AddHostedService<WorkerD>();
            services.AddHostedService<WorkerE>();
            services.AddHostedService<WorkerF>();
            services.AddHostedService<WorkerG>();
            services.AddHostedService<WorkerH>();
            services.AddHostedService<WorkerI>();
            services.AddHostedService<WorkerJ>();
            services.AddHostedService<WorkerK>();

            IHttpClientBuilder httpClientBuilder = services.AddHttpClient(HttpClientName);
            httpClientBuilder.ConfigureHttpClient(client => {
                client.BaseAddress = new Uri("http://localhost:5189");
            });

            var b = httpClientBuilder.AddStandardResilienceHandler(options => {
                options.CircuitBreaker.MinimumThroughput = 9;
            });
        }

        /// <summary>Set up application configuration.</summary>
        /// <param name="builder">The configuration builder.</param>
        public static void ConfigureAppConfigurationDelegate(IConfigurationBuilder builder)
        {
            // When running in docker container (outside of VS)
            // Environment.GetEnvironmentVariable returning null
            // so, instead we build the configuration and then it's
            // there.
            IConfigurationRoot config = builder.Build();
            builder.AddUserSecrets<Program>();
        }
    }
}