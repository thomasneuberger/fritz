using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FritzApp;
using FritzApp.Services;
using Microsoft.Extensions.Localization;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<CultureService>();
builder.Services.AddLocalization();

var host = builder.Build();

// Initialize culture
var cultureService = host.Services.GetRequiredService<CultureService>();
await cultureService.InitializeCultureAsync();

await host.RunAsync();
