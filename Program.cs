var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//app.MapGet("/", () => "Hello World Map!");

app.Run(async context => await context.Response.WriteAsync("Hello World!"));
app.Run(); // Application won't display without that final empty Run().
