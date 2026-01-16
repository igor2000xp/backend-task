using BlogPlatform.Api.Extensions;
using BlogPlatform.Application;
using BlogPlatform.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Service Registration
builder.Services
    .AddInfrastructureServices(builder.Configuration, builder.Environment)
    .AddApplicationServices()
    .AddWebUIServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Database Initialization
await app.InitializeDatabaseAsync();

// Pipeline Configuration
app.UsePipelineConfiguration();

// Endpoint Mapping
app.MapEndpoints();

app.Run();

// Make the Program class public for integration testing
public partial class Program { }
