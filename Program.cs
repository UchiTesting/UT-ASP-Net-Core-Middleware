using SimpleWebApp;

using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<ConsoleLoggerMiddleware>();

var app = builder.Build();

//app.MapGet("/", () => "Hello World Map!");

app.Use(async (httpContext, nextMiddleware) =>
{
    Console.WriteLine($"\n\n\n======== {DateTime.Now.ToString("f", CultureInfo.InvariantCulture)} ========");
    await nextMiddleware();
});

app.UseMiddleware<ConsoleLoggerMiddleware>();

app.Run(async context => await context.Response.WriteAsync("Hello World!"));
app.Run(); // Application won't display without that final empty Run().
