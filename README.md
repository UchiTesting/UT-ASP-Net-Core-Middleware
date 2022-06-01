Using Middleware as of .NET 6
=============================

Based on the video from [Rahul Nath](https://youtu.be/5eifH7LEnGo) about Middleware in ASP .NET Core 5.

**Table of Content**

  - [WebApp Creation](#webapp-creation)
  - [Removing Everything](#removing-everything)
  - [Adding a single Run method](#adding-a-single-run-method)
  - [Request Delegates](#request-delegates)
    - [`Run())`](#run)
    - [`Use()`](#use)
    - [`Map()`](#map)
    - [`MapWhen()`](#mapwhen)
    - [`UseWhen()`](#usewhen)
  - [Externalize inline middleware](#externalize-inline-middleware)
  - [Extra info](#extra-info)

These notes are done in the context of following the video on Middleware from Rahul Nath which covers .NET 5.
Differences with .NET 6 being the new LTS version will be reported here.

## WebApp Creation

Upon creating a new project from CLI with command `dotnet new web -o SimpleWebApp`, the content of `Program.cs` looks like this:

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```

On .NET 6 there is no more `ConfigureServices()` or `Configure()` methods. Also there is no more `Startup.cs` file neither.
All this have been moved to `Program.cs` in a top level code. In there the aforementioned methods have respectively been replaced by `builder` and `app` identifiers.

## Removing Everything

A first edition of the application was to remove everything from withing the `Startup` class.

In .NET 5 this would mean removing `Configure()` and `ConfigureServices()` methods from `Startup.cs`.
It throws an `System.InvalidOperationException` at runtime. The message states *A public method named 'ConfigureDevelopement' or 'Configure' could not be found in 'SimpleWebApp.Startup' type.*

To have the application start again we can restore an empty `Configure()` method. The page will launch but response a 404 error.

In .NET 6 removing everything won't compile. The message is there is no static `Main()` method available.


This is made available by the 1<sup>st</sup> line:

```csharp
var builder = WebApplication.CreateBuilder(args);
```

In such case the program exits with code 0.

## Adding a single Run method

A second edition after `Configure()` is put back, is add a simple `Run()` method in it:

```csharp
app.Run(async context => await context.Response.WriteAsync("Hello World!"));
```

> The above line is reffered to as *request delegate* or *ASP .NET middleware*. In particular *inline middleware* because it is written in the `Startup` class (for .NET 5).

On .NET 5 this is enough to be able to start the application again and have a browser displayed.

On .NET 6 the program exits with code 0. It seems that no matter what, the code needs an empty `Run()` at its end.

So the equivalent code to this in .NET 6 is:

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(async context => await context.Response.WriteAsync("Hello World!"));
app.Run(); // Application won't display without that final empty Run().
```

## Request Delegates

As mentioned in a note above they are also called **Middleware** and are inline when they are written directly in the `Startup` class.

There are 3 main ways to write them :

-`Run()`
-`Use()`
-`Map()`

There are also other options:

- `MapWhen()`
- `UseWhen()`

### `Run())`

It received a delegate taking only a `context` parameter as shown above. Its type is `HttpContext`. They don't know about any following middleware and are also called *Terminal delegates*. 
Indeed it is a convention to put them last and .NET 6 with the remark above stating it cannot run without a final empty `app.Run();` statement
reinforce this.

### `Use()`

`Use()` takes a delegate which in turn takes both a `context` but also a `next` param. Their respective types are `HttpContext` and `Func<Task>`.

Let's add a few middlewares before our `Run()` displaying *Hello World!*.

> To use `CultureInfo` we imported `System.Globalization`.

```csharp
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
```
> Output

```
======== Wednesday, 01 June 2022 14:00 ========
Before Request 1
Before Request 2
Before Request 3
After Request 3
After Request 2
After Request 1
```

We can observe the request are nested and go one way and back.
Hence we can write code to be executed prior or post calling a further middleware.
The next middleware is simply executed with `await nextMiddleware();` statement.

### `Map()`

It is analog to `Run()` in that it is terminal. Nothing after it will be executed. It's specificity is that it takes a routing pattern as 1<sup>st</sup> parameter.
Every matching URL will trigger it.

Say our routing pattern is `/map`, every other routing path having it will be triggering it also. So `/map/anything` qualifies.

That said, should another map be defined before, it will take precedence.

```csharp
app.Map("/map/anything/yet", HandleMapYet);
app.Map("/map/anything", HandleMapAnything);
app.Map("/map", HandleMap);
```

### `MapWhen()`

Branches the pipeline based on the result of a given `Predicate`.

Here we cant to branch is there is a 'q' query string.

```csharp
app.MapWhen(context => context.Request.Query.ContainsKey("q"), HandleQuery);
```

The related handler is as is

```csharp
void HandleQuery(IApplicationBuilder app)
{
    app.Use(async (context, next) =>
    {
        string message = "Contains a \"q\" query string!";
        await context.Response.WriteAsync(message + "<br/>\n");
        Console.WriteLine(message);
        await next();
    });
}
```

When we meet our condition and `invalidOperationException` is thrown though.

```
fail: Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware[1]
      An unhandled exception has occurred while executing the request.
      System.InvalidOperationException: StatusCode cannot be set because the response has already started.
         at Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpProtocol.ThrowResponseAlreadyStartedException(String value)
         at Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpProtocol.set_StatusCode(Int32 value)
         at Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpProtocol.Microsoft.AspNetCore.Http.Features.IHttpResponseFeature.set_StatusCode(Int32 value)
         at Microsoft.AspNetCore.Http.DefaultHttpResponse.set_StatusCode(Int32 value)
         at Microsoft.AspNetCore.Builder.ApplicationBuilder.<>c.<Build>b__18_0(HttpContext context)
         at Microsoft.AspNetCore.Builder.UseExtensions.<>c__DisplayClass0_2.<Use>b__2()
         at Program.<>c.<<<Main>$>b__0_12>d.MoveNext() in D:\Users\UchiTesting\Desktop\repos\C#\Tuts\SimpleWebApp\Program.cs:line 84
```

This is because `MapWhen()` does not provide a terminating middleware. This may cause a code 404 response.

To fix this either remove the call to `next()` or better call `Run()` instead of `Use()`.

```csharp
void HandleQuery(IApplicationBuilder app)
{
    app.Run(async (context) =>
    {
        string message = "Contains a \"q\" query string!";
        await context.Response.WriteAsync(message + "<br/>\n");
        Console.WriteLine(message);
    });
}
```

### `UseWhen()`

`UseWhen()` works in analog way to `MapWhen()` but merges back in the pipeline.


```csharp
app.UseWhen(context => context.Request.Query.ContainsKey("q"), HandleQuery);
```

In the previous scenario where we called the `next()` middleware, the code failed because `MapWhen()` is meant to be terminal.
This is no more a problem with `UseMap()`.

```csharp
void HandleQuery(IApplicationBuilder app)
{
    app.Use(async (context, next) =>
    {
        string message = "Contains a \"q\" query string!";
        await context.Response.WriteAsync(message + "<br/>\n");
        Console.WriteLine(message);
        await next();
    });
}
```

As a result, further middlewares would be executed and the response completed accordingly.

## Externalize inline middleware

We can move inline middlewares to their own type.

```csharp
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
```

We can then use our externalised middleware with the `UseMiddleware<T>()` method.

```csharp
app.UseMiddleware<ConsoleLoggerMiddleware>();
```

We also need to register our service. If not there will be a `System.InvalidOperationException` thrown.

```
fail: Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware[1]
      An unhandled exception has occurred while executing the request.
      System.InvalidOperationException: No service for type 'SimpleWebApp.ConsoleLoggerMiddleware' has been registered.
```

So the `Services` property to `builder` provides an `AddTransient<T>()` method for that purpose.

```csharp
builder.Services.AddTransient<ConsoleLoggerMiddleware>();
```

## Extra info

ASP .NET Core comes with a set of ready to use middlewares which are introduced in the documentation bellow. Their order is to be respected.

- [ASP .NET Core Middleware @MSDN](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0)
  - [Concept of Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0#built-in-middleware)
  - [Built-in Middleware @MSDN](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0#built-in-middleware)
  - [Middleware Order](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0#built-in-middleware)
- [Dealing with status code](https://docs.microsoft.com/en-us/aspnet/core/performance/performance-best-practices?view=aspnetcore-6.0#do-not-modify-the-status-code-or-headers-after-the-response-body-has-started)