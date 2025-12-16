using Fritz.Api.Endpoints;
using Fritz.Api.Hubs;
using Fritz.Api.Middleware;
using Fritz.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddSingleton<GameService>();
builder.Services.AddSignalR();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    options.AddPolicy("AllowSignalR", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "https://localhost:5001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Use API Key authentication
app.UseMiddleware<ApiKeyMiddleware>();

app.UseCors("AllowAll");

// Map API endpoints
app.MapGamesEndpoints();

// Map SignalR hub
app.MapHub<GameHub>("/gamehub");

app.Run();
