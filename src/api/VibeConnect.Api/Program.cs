using Microsoft.EntityFrameworkCore;
using VibeConnect.Api.Extensions;
using VibeConnect.Storage;

var builder = WebApplication.CreateBuilder(args);

const string corsPolicyName = "VibeConnect.Api";


var services = builder.Services;
var config = builder.Configuration;

// Add services to the container.
services.AddDbContextPool<ApplicationDbContext>(options => 
    options.UseNpgsql(config.GetConnectionString("DbConnection")));

services.AddHealthChecks();
services.AddCors(options => options
    .AddPolicy(corsPolicyName, policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()));

services.AddApiControllers();

services.AddApiVersioning(1);

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
