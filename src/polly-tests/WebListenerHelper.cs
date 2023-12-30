namespace polly_tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Creates a web listener to listen for, and
    /// respond to, http requests from tests.
    /// </summary>
    /// <remarks>Mike McMaster, 2/1/2022.</remarks>
    public static class WebListenerHelper
    {
        // Used to limit access to startup and shutdown web service.
        private static readonly SemaphoreSlim ServiceSemaphore = new(1, 1);

        // Used to limit access to adjust the client count.
        private static readonly SemaphoreSlim ClientCountSemaphore = new(1, 1);

        /// <summary>Number of clients using this web listener.</summary>
        private static int ClientCount;

        private static readonly Dictionary<string, Func<HttpContext, Task>> Callbacks = [];

        private static WebApplication? webApplication;

        private static string? TestingUrlValue;

        /// <summary>Gets the URL to use for testing.</summary>
        /// <value>The testing URL.</value>
        /// <remarks>The value is only valid during testing runs.</remarks>
        public static string TestingUrl
        {
            get
            {
                if (TestingUrlValue == null)
                {
                    throw new InvalidOperationException("Call InitializeWebListener before accessing TestingUrl");
                }

                return TestingUrlValue;
            }
        }

        /// <summary>
        /// When testing starts this method is called to
        /// set up a web server that can listen for HTTP
        /// calls from tests.
        /// </summary>
        /// <remarks>Mike McMaster, 2/2/2022.</remarks>
        /// <param name="context">The testing context.</param>
        /// <returns>A Task.</returns>
        public static async Task InitializeWebListener(TestContext context)
        {
            System.Diagnostics.Debug.WriteLine($"InitializeWebListener called by {context.FullyQualifiedTestClassName}.");

            if (webApplication != null)
            {
                return;
            }

            await ServiceSemaphore.WaitAsync();

            if (webApplication == null)
            {
                string port = GetOpenPort();

                TestingUrlValue = $"http://localhost:{port}";

                // Start a web server on a given port
                var builder = WebApplication.CreateBuilder();
                builder.WebHost.UseUrls(TestingUrl);
                webApplication = builder.Build();

                webApplication.MapFallback((string path, HttpContext context) => WebRequestHandler(path, context));

                try
                {
                    System.Diagnostics.Debug.WriteLine("Starting web application.");
                    await webApplication.StartAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error starting web application.");
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            
            ServiceSemaphore.Release();
        }

        /// <summary>
        /// When testing ends this method is called to
        /// shut down the web server and cleanup calls from tests.
        /// </summary>
        /// <remarks>Mike McMaster, 2/2/2022.</remarks>
        /// <returns>A Task.</returns>
        public static async Task CleanupWebListener()
        {
            System.Diagnostics.Debug.WriteLine($"CleanupWebListener called.");

            if (webApplication == null)
            {
                return;
            }

            await ServiceSemaphore.WaitAsync();

            if (webApplication != null)
            {
                System.Diagnostics.Debug.WriteLine("Stopping web application.");
                await webApplication.StopAsync();
                await webApplication.DisposeAsync();
                Callbacks.Clear();
                TestingUrlValue = null;
            }
            
            ServiceSemaphore.Release();
        }

        public static async Task IncrementClientCount()
        {
            await ClientCountSemaphore.WaitAsync();
            ClientCount++;
        }

        /// <summary>Registers a callback for the path.</summary>
        /// <remarks>
        /// Mike McMaster, 2/2/2022.
        /// When a callback path is registered any request made to the
        /// path will cause the callback action to be executed.
        /// </remarks>
        /// <param name="path">Full pathname of the file.</param>
        /// <param name="callback">The callback.</param>
        public static void RegisterWebRequestCallbackPath(string path, Func<HttpContext, Task> callback)
        {
            if (webApplication == null)
            {
                throw new InvalidOperationException("Call InitializeWebListener before accessing Registering a callback path");
            }

            string cleanedPath = path.TrimStart('/').ToLower();

            if (Callbacks.ContainsKey(cleanedPath))
            {
                UnregisterWebRequestCallbackPath(cleanedPath);
            }

            Callbacks.Add(cleanedPath, callback);
        }

        /// <summary>Unregister the callback for this path.</summary>
        /// <remarks>Mike McMaster, 2/2/2022.</remarks>
        /// <param name="path">Full pathname of the file.</param>
        public static void UnregisterWebRequestCallbackPath(string path)
        {
            if (webApplication == null)
            {
                throw new InvalidOperationException("Call InitializeWebListener before accessing Registering a callback path");
            }

            Callbacks.Remove(path.TrimStart('/').ToLower());
        }

        /// <summary>Handles the web request by calling the registered WebRequestCallbackHandler.</summary>
        /// <remarks>Mike McMaster, 2/2/2022.</remarks>
        /// <param name="path">The path of the request.</param>
        /// <param name="context">The context of the Http request.</param>
        /// <returns>A HttpResponseMessage.</returns>
        private static async Task WebRequestHandler(string path, HttpContext context)
        {
            if (!Callbacks.ContainsKey(path.ToLower()))
            {
                System.Diagnostics.Trace.WriteLine($"No callback found for path '{path.ToLower()}'");

                string msg = $"No callback found for path '{path.ToLower()}'";
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.NotImplemented;
                context.Response.ContentType = "application/text";
                context.Response.ContentLength = msg.Length;
                await context.Response.WriteAsync(msg);
                return;
            }

            Func<HttpContext, Task> handlerCallback = Callbacks[path.ToLower()];

            await handlerCallback(context);
        }

        /// <summary>Gets the first open port within the range.</summary>
        /// <remarks>Mike McMaster, 2/2/2022.</remarks>
        /// <returns>The open port number as a string.</returns>
        private static string GetOpenPort()
        {
            int portStartIndex = 5000;
            int portEndIndex = 6000;
            System.Net.NetworkInformation.IPGlobalProperties properties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
            System.Net.IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();

            List<int> usedPorts = tcpEndPoints.Select(p => p.Port).ToList<int>();
            int unusedPort = 0;

            for (int port = portStartIndex; port < portEndIndex; port++)
            {
                if (!usedPorts.Contains(port))
                {
                    unusedPort = port;
                    break;
                }
            }

            return unusedPort.ToString();
        }
    }
}