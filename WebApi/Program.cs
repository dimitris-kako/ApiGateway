using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
//Add Memory Caching
builder.Services.AddMemoryCache();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

var spotifyApiSettings = builder.Configuration.GetSection("Spotify").Get<SpotifySettings>();
builder.Services.AddHttpClient<ISpotifyService, SpotifyService>((serviceProvider, httpClient) =>
{
    httpClient.BaseAddress = new Uri(spotifyApiSettings.BaseUrl);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    };
});

var newsApiSettings = builder.Configuration.GetSection("NewsApi").Get<NewsApiSettings>();
builder.Services.AddHttpClient<INewsService, NewsService>((serviceProvider, httpClient) =>
{
    httpClient.DefaultRequestHeaders.Add("Authorization", newsApiSettings.ApiKey);
    httpClient.BaseAddress = new Uri(newsApiSettings.BaseUrl);
    httpClient.DefaultRequestHeaders.Add("User-Agent", "ApiAggregationApp/1.0");
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    };
});

var moviesApiSettings = builder.Configuration.GetSection("MoviesApi").Get<MoviesApiSettings>();
builder.Services.AddHttpClient<IMoviesService, MoviesService>((serviceProvider, httpClient) =>
{
    httpClient.BaseAddress = new Uri(moviesApiSettings.BaseUrl);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    };
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

// Register global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();

public class NewsApiSettings
{
    public string ApiKey { get; set; }
    public string BaseUrl { get; set; }
}

public class MoviesApiSettings
{
    public string ApiKey { get; set; }
    public string BaseUrl { get; set; }
}

public class SpotifySettings
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string BaseUrl { get; set; }
}
