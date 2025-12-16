using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Fritz.Client;
using Fritz.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add configuration from appsettings.json
var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
using var response = await http.GetAsync("appsettings.json");
using var stream = await response.Content.ReadAsStreamAsync();
builder.Configuration.AddJsonStream(stream);

// Register services
builder.Services.AddScoped<GameApiService>();
builder.Services.AddScoped<GameHubService>();
builder.Services.AddScoped<HttpClient>();

await builder.Build().RunAsync();
