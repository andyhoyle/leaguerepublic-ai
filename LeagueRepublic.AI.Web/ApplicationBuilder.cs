using LeagueRepublic.AI.Web.Adapters.AzureAI;
using LeagueRepublic.AI.Web.Adapters.LeagueRepublic;
using LeagueRepublic.AI.Web.Service;
using LeagueRepublic.AI.Web.Service.Models;
using LeagueRepublic.AI.Web.Components;

namespace LeagueRepublic.AI.Web;

public static class ApplicationBuilder
{
    public static WebApplication ConfigureApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder.Services, builder.Configuration);
        
        var app = builder.Build();
        ConfigurePipeline(app);
        
        return app;
    }

    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Add services to the container.
        services.AddRazorComponents()
            .AddInteractiveServerComponents();
        services.AddControllers();

        // Configure HTTP clients
        services.AddHttpClient<ILeagueRepublicAdapter, LeagueRepublicAdapter>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "LeagueRepublic.AI/1.0");
        });

        // Configure application services
        services.AddScoped<IMatchProcessingService, MatchProcessingService>();
        services.AddScoped<IAzureAIAdapter, AzureAIAdapter>();
        services.AddScoped<ILeagueRepublicAdapter, LeagueRepublicAdapter>();

        // Configure settings
        var leagueConfig = configuration.GetSection("LeagueRepublic").Get<LeagueRepublicConfig>() 
            ?? new LeagueRepublicConfig { LeagueId = "186006607", BaseUrl = "https://www.leaguerepublic.com" };
        services.AddSingleton(leagueConfig);

        var azureConfig = configuration.GetSection("AzureAI").Get<AzureAIConfig>()
            ?? new AzureAIConfig { Endpoint = "", ApiKey = "", ModelName = "gpt-4o" };
        services.AddSingleton(azureConfig);

        // Add logging
        services.AddLogging();
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();

        app.UseRouting();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        app.MapControllers();
    }
}