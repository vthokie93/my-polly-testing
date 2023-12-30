using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

Dictionary<string, int> failCount = new();

app.MapGet("/fail", (HttpRequest req, HttpResponse response) => 
{
    string id = req.Query["id"];
    int count = int.Parse(req.Query["count"]);

    if (failCount.ContainsKey(id))
    {
        failCount[id] += 1;
    }
    else
    {
        failCount.Add(id, 1);
    }

    response.StatusCode = failCount[id] >= count ?  (int)HttpStatusCode.OK : (int)HttpStatusCode.InternalServerError;

    //string returnVal = failCount[id] >= count ?  "Ok" : "InternalServerError";

    return;
});


app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
