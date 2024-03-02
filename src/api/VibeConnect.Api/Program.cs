using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using VibeConnect.Api.Extensions;
using VibeConnect.Auth.Module.Options;
using VibeConnect.Shared;
using VibeConnect.Storage;

var builder = WebApplication.CreateBuilder(args);

const string corsPolicyName = "VibeConnect.Api";


var services = builder.Services;
var config = builder.Configuration;

// Add services to the container.
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? config.GetConnectionString("DbConnection");

services.AddDbContextPool<ApplicationDbContext>(options => 
    options.UseNpgsql(connectionString));

services.AddBaseRepositories();
services.AddAuthModuleServiceCollection();

services.Configure<JwtConfig>(c=> config.GetSection(nameof(JwtConfig)).Bind(c));


services.AddHealthChecks();
services.AddCors(options => options
    .AddPolicy(corsPolicyName, policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()));

services.AddApiControllers();

services.AddApiVersioning(1);

services.AddScoped(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));

services.AddBearerAuthentication(config);

services.AddSwaggerGen(config, $"{CommonConstants.AuthScheme.Bearer}");

services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.All;
    options.RequestBodyLogLimit = 4096;
    options.ResponseBodyLogLimit = 4096;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwaggerDoc();

await app.RunMigrationsAsync();

app.UseExceptionHandler(!app.Environment.IsProduction());


app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(corsPolicyName);

app.UseAuthentication();

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

await app.RunAsync();
