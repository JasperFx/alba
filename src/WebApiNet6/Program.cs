using System;
using Microsoft.AspNetCore.Builder;
using Oakton;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();


app.MapGet("/", () => "Hello World!");
app.MapGet("/blowup", context => throw new Exception("Boo!"));

app.Run();
//return app.RunOaktonCommandsSynchronously(args);
