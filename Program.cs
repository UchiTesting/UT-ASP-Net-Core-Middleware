using SimpleWebApp;

using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<ConsoleLoggerMiddleware>();

var app = builder.Build();

//app.MapGet("/", () => "Hello World Map!");

app.Use((Func<HttpContext, Func<Task>, Task>)(async (httpContext, nextMiddleware) =>
{
    Console.WriteLine($"\n\n\n======== {DateTime.Now.ToString("f", CultureInfo.InvariantCulture)} ========");
    await AddBasicMenu(httpContext);
    await nextMiddleware();
}));

app.Map("/favicon.ico", (app) => { });

app.MapWhen(context => context.Request.Query.ContainsKey("q"), HandleQuery);

app.Map("/map/anything/yet", HandleMapYet);
app.Map("/map/anything", HandleMapAnything);
app.Map("/map", HandleMap);


app.UseMiddleware<ConsoleLoggerMiddleware>();

app.Run(async context => await context.Response.WriteAsync("Hello World!"));
app.Run(); // Application won't display without that final empty Run().

#region Handlers and more

static async Task AddBasicMenu(HttpContext httpContext)
{
    await httpContext.Response.WriteAsync("<a href=\"/\">Root</a><br/>\n");
    await httpContext.Response.WriteAsync("<a href=\"/map\">Map</a><br/>\n");
    await httpContext.Response.WriteAsync("<a href=\"/map/anything\">Map Anything</a><br/>\n");
    await httpContext.Response.WriteAsync("<a href=\"/map/anything/yet\">Map Yet</a><br/>\n");
    await httpContext.Response.WriteAsync("<a href=\"/map/anything/yet2\">Map Yet 2</a><br/>\n");
    await httpContext.Response.WriteAsync("<a href=\"/map/anything2/yet\">Map Anything 2</a><br/>\n");
    await httpContext.Response.WriteAsync("<a href=\"/map/anything2/yet?r=r\">Map Anything 2 with r query string</a><br/>\n");
    await httpContext.Response.WriteAsync("<a href=\"/map/anything2/yet?r=r&q=q\">Map Anything 2 with 'r' and 'q' query string</a><br/>\n");
    await httpContext.Response.WriteAsync("<div style=\"height:0;border:solid 1px red; margin: 15px 0;\"/>\n");
    await httpContext.Response.WriteAsync("<div style=\"margin:15px 0;background:lightgrey;padding:20px\">");
}

void HandleMap(IApplicationBuilder app)
{
    app.Run(async (context) =>
    {
        Console.WriteLine("Mapped!");
        await context.Response.WriteAsync("Mapped!");
    });
}

void HandleMapAnything(IApplicationBuilder app)
{
    app.Run(async (context) =>
    {
        Console.WriteLine("Mapped anything!");
        await context.Response.WriteAsync("Mapped Anything!");
    });
}

void HandleMapYet(IApplicationBuilder app)
{
    app.Run(async (context) =>
    {
        Console.WriteLine("Mapped yet!");
        await context.Response.WriteAsync("Mapped Yet!");
    });
}

void HandleQuery(IApplicationBuilder app)
{
    app.Run(async (context) =>
    {
        string message = "Contains a \"q\" query string!";
        await context.Response.WriteAsync(message + "<br/>\n");
        Console.WriteLine(message);
    });
}
#endregion
