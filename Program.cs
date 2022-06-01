using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//app.MapGet("/", () => "Hello World Map!");

app.Use(async (httpContext, nextMiddleware) =>
{
    Console.WriteLine($"\n\n\n======== {DateTime.Now.ToString("f", CultureInfo.InvariantCulture)} ========");
    await nextMiddleware();
});

app.Use(async (httpContext, nextMiddleware) =>
{
    Console.WriteLine("Before Request 1");
    await httpContext.Response.WriteAsync("Use 1.\n");
    await nextMiddleware();
    Console.WriteLine("After Request 1");
});

app.Use(async (httpContext, nextMiddleware) =>
{
    Console.WriteLine("Before Request 2");
    await httpContext.Response.WriteAsync("Use 2.\n");
    await nextMiddleware();
    Console.WriteLine("After Request 2");
});

app.Use(async (httpContext, nextMiddleware) =>
{
    Console.WriteLine("Before Request 3");
    await httpContext.Response.WriteAsync("Use 3.\n");
    await nextMiddleware();
    Console.WriteLine("After Request 3");
});

app.Run(async context => await context.Response.WriteAsync("Hello World!"));
app.Run(); // Application won't display without that final empty Run().
