namespace WebClient
{
    using System;
    using System.Data;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>The program functionality.</summary>
    internal abstract class Worker(IHost host, IConfiguration configuration, ILogger logger, IHttpClientFactory httpClientFactory) : BackgroundService
    {
        
        string id = Guid.NewGuid().ToString();

        IHost host = host;
        private readonly IConfiguration configuration = configuration;
        private readonly ILogger logger = logger;

        private readonly IHttpClientFactory httpClientFactory = httpClientFactory;

        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" />
        /// starts. The implementation should return a task that represents the lifetime of the long
        /// running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">
        /// Triggered when
        /// <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" />
        /// is called.
        /// </param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task" /> that represents the long running operations.
        /// </returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation($"Starting Worker");

            HttpClient httpClient = this.httpClientFactory.CreateClient(Program.HttpClientName);

            string url = $"/fail?id={id}&count=200";

            await MakeTheCall(httpClient, url);

            // if this call gets awaited it seems to cause an issue
            // with the last few logger messages getting written.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            //this.host.StopAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        protected async Task MakeTheCall(HttpClient httpClient, string url)
        {
            try
            {
                var result = await httpClient.GetAsync(url);
                this.logger.LogWarning($"Http Status Code = {result.StatusCode}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);
            }
        }
    }

    internal class WorkerA(IHost host, IConfiguration configuration, ILogger<WorkerA> logger, IHttpClientFactory httpClientFactory) :
        Worker(host, configuration, logger, httpClientFactory)
    {
    }

    internal class WorkerB(IHost host, IConfiguration configuration, ILogger<WorkerB> logger, IHttpClientFactory httpClientFactory) :
        Worker(host, configuration, logger, httpClientFactory)
    {
    }

    internal class WorkerC(IHost host, IConfiguration configuration, ILogger<WorkerC> logger, IHttpClientFactory httpClientFactory) :
        Worker(host, configuration, logger, httpClientFactory)
    {
    }

    internal class WorkerD(IHost host, IConfiguration configuration, ILogger<WorkerD> logger, IHttpClientFactory httpClientFactory) :
        Worker(host, configuration, logger, httpClientFactory)
    {
    }

    internal class WorkerE(IHost host, IConfiguration configuration, ILogger<WorkerE> logger, IHttpClientFactory httpClientFactory) :
        Worker(host, configuration, logger, httpClientFactory)
    {
    }

    internal class WorkerF(IHost host, IConfiguration configuration, ILogger<WorkerF> logger, IHttpClientFactory httpClientFactory) :
        Worker(host, configuration, logger, httpClientFactory)
    {
    }

    internal class WorkerG(IHost host, IConfiguration configuration, ILogger<WorkerG> logger, IHttpClientFactory httpClientFactory) :
        Worker(host, configuration, logger, httpClientFactory)
    {
    }

    internal class WorkerH(IHost host, IConfiguration configuration, ILogger<WorkerH> logger, IHttpClientFactory httpClientFactory) :
        Worker(host, configuration, logger, httpClientFactory)
    {
    }

    internal class WorkerI(IHost host, IConfiguration configuration, ILogger<WorkerI> logger, IHttpClientFactory httpClientFactory) :
        Worker(host, configuration, logger, httpClientFactory)
    {
    }

    internal class WorkerJ(IHost host, IConfiguration configuration, ILogger<WorkerJ> logger, IHttpClientFactory httpClientFactory) :
        Worker(host, configuration, logger, httpClientFactory)
    {
    }

    internal class WorkerK(IHost host, IConfiguration configuration, ILogger<WorkerK> logger, IHttpClientFactory httpClientFactory) :
        Worker(host, configuration, logger, httpClientFactory)
    {
    }
}
