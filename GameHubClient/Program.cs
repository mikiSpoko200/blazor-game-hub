using System.Net.Http.Headers;
using GameHubClient;
using GameHubShared.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddSingleton(new AppState());
builder.Services.AddTransient(sp =>
{
    var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    var appState = sp.GetRequiredService<AppState>(); // Assuming AppState is registered in DI

    if (!string.IsNullOrEmpty(appState.JWT))
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", appState.JWT);
        // httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + appState.JWT);
    }

    return httpClient;
});



var app = builder.Build();

await app.RunAsync();

public class AppState {
    public UserModel? User { get; set; } = null;
    public string? JWT { get; set; } = null;
}
