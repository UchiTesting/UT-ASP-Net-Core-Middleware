namespace SimpleWebApp;

public class ConsoleLoggerMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        Console.WriteLine("Before Logger Middleware");
        await context.Response.WriteAsync("Use Logger Middleware.\n");
        await next(context);
        Console.WriteLine("After Logger Middleware");
    }
}
