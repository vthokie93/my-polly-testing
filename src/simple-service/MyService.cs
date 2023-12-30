namespace simple_service;

public class MyService (HttpClient httpClient)
{
    private readonly HttpClient httpClient = httpClient;

    public async Task<HttpResponseMessage?> MakeServiceCall(string endPoint)
    {
        try
        {
            return await httpClient.GetAsync(endPoint);
        }
        catch (    Exception ex)
        {
            Exception? inner = ex;
            
            while (inner != null)
            {
                System.Diagnostics.Debug.WriteLine(inner.Message);
                inner = inner.InnerException;
            }

            return null;
        }
    }
}
