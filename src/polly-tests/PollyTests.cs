namespace polly_tests;

using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using simple_service;

[TestClass]
public class PollyTests
{
   public TestContext? TestContext { get; set; }

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        WebListenerHelper.InitializeWebListener(context).Wait();
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
        WebListenerHelper.CleanupWebListener().Wait();
    }

    [TestMethod]
    public async Task NoResilience_Success()
    {
        string endPoint = "/NoResilience_Success";

        WebListenerHelper.RegisterWebRequestCallbackPath(endPoint, context => 
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            return Task.CompletedTask;
        });

        HttpClient httpClient = BuildConfiguredHttpClient(builder =>
        {
        }); 

        MyService target = new MyService(httpClient);

        HttpResponseMessage? result = await target.MakeServiceCall(endPoint);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccessStatusCode);
    }

    [TestMethod]
    public async Task NoResilience_Failure()
    {
        string endPoint = "/NoResilience_Failure";

        WebListenerHelper.RegisterWebRequestCallbackPath(endPoint, context => 
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return Task.CompletedTask;
        });

        HttpClient httpClient = BuildConfiguredHttpClient(builder =>
        {
        }); 

        MyService target = new MyService(httpClient);

        HttpResponseMessage? result = await target.MakeServiceCall(endPoint);

        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsSuccessStatusCode);
    }

    [TestMethod]
    public async Task StandardResilienceWith2Retry()
    {
        string name = "StandardResilience";
        string endPoint = $"/{name}";

        int callCount = 0;

        WebListenerHelper.RegisterWebRequestCallbackPath(endPoint, context => 
        {
            callCount++;

            if (callCount == 1)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }

            return Task.CompletedTask;
        });

        HttpClient httpClient = BuildConfiguredHttpClient(builder =>
        {
            builder.AddStandardResilienceHandler();
        }); 

        MyService target = new MyService(httpClient);

        HttpResponseMessage? result = await target.MakeServiceCall(endPoint);

        Assert.AreEqual(2, callCount);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccessStatusCode);
    }

    [TestMethod]
    public async Task StandardResilienceSameError()
    {
        string name = "StandardResilienceAllRetry";
        string endPoint = $"/{name}";

        int callCount = 0;

        WebListenerHelper.RegisterWebRequestCallbackPath(endPoint, context => 
        {
            callCount++;


            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;


            return Task.CompletedTask;
        });

        HttpClient httpClient = BuildConfiguredHttpClient(builder =>
        {
            builder.AddStandardResilienceHandler();
        }); 

        MyService target = new MyService(httpClient);

        HttpResponseMessage? result = await target.MakeServiceCall(endPoint);

        Assert.AreEqual(4, callCount);

        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task StandardResilienceDifferentError()
    {
        string name = "StandardResilienceDifferentError";
        string endPoint = $"/{name}";

        int callCount = 0;

        WebListenerHelper.RegisterWebRequestCallbackPath(endPoint, context => 
        {
            callCount++;

            if (callCount == 1)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            }


            return Task.CompletedTask;
        });

        HttpClient httpClient = BuildConfiguredHttpClient(builder =>
        {
            builder.AddStandardResilienceHandler();
        }); 

        MyService target = new MyService(httpClient);

        HttpResponseMessage? result = await target.MakeServiceCall(endPoint);

        Assert.AreEqual(4, callCount);

        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, result.StatusCode);
    }

    [TestMethod]
    public async Task StandardResilienceMultipleFailures()
    {
        string name = "StandardResilienceAllRetry";
        string endPoint = $"/{name}";

        int callCount = 0;

        WebListenerHelper.RegisterWebRequestCallbackPath(endPoint, context => 
        {
            callCount++;


            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;


            return Task.CompletedTask;
        });

        HttpClient httpClient = BuildConfiguredHttpClient(builder =>
        {
            builder.AddStandardResilienceHandler();
        }); 

        MyService target = new MyService(httpClient);

        HttpResponseMessage? result = await target.MakeServiceCall(endPoint);

        Assert.AreEqual(4, callCount);

        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);

        HttpResponseMessage? result2 = await target.MakeServiceCall(endPoint);

        Assert.AreEqual(8, callCount);

        Assert.IsNotNull(result2);
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);


        HttpResponseMessage? result3 = await target.MakeServiceCall(endPoint);

        Assert.AreEqual(12, callCount);

        Assert.IsNotNull(result3);
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);

        HttpResponseMessage? result4 = await target.MakeServiceCall(endPoint);

        Assert.AreEqual(16, callCount);

        Assert.IsNotNull(result4);
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    private static HttpClient BuildConfiguredHttpClient(Action<IHttpClientBuilder> action)
    {
        string clientName = "client";

        IServiceCollection servicesCollection = new ServiceCollection();
        IHttpClientBuilder httpClientBuilder = servicesCollection.AddHttpClient(clientName, client => 
        {
            client.BaseAddress = new Uri(WebListenerHelper.TestingUrl);
        });

        action(httpClientBuilder);
        
        IServiceProvider services = servicesCollection.BuildServiceProvider();
        IHttpClientFactory httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
        return httpClientFactory.CreateClient(clientName);
    }
}