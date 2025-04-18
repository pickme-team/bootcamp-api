using Bootcamp.Api;
using Bootcamp.Api.Models.Requests;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BootcampContext>(o => o.UseInMemoryDatabase("Bootcamp"));

builder.Services.AddScoped<IMlService, MlService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapGet("/ping", () => "pong")
    .WithName("Ping");

app.MapPost("/generate-text", async (GenerateTextRequest request, IMlService mlService) 
        => await mlService.GenerateText(request))
    .WithName("GenerateText");

app.Run();