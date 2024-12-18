using Microsoft.Extensions.Caching.StackExchangeRedis;
using WeatherAPI.Configuration;
using WeatherAPI.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddStackExchangeRedisCache(options => 
{ 
    options.Configuration = builder.Configuration.GetSection("Redis")["ConnectionString"]; 
    options.InstanceName = "WeatherAPI_"; 
});

builder.Services.Configure<WeatherApiOptions>(builder.Configuration.GetSection("WeatherApi"));

builder.Services.AddHttpClient<IWeatherService, WeatherService>(client =>
{
    var options = builder.Configuration.GetSection("WeatherApi").Get<WeatherApiOptions>();
	client.BaseAddress = new Uri(options.BaseUrl);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

