using System.Text;
using CORE.Configuration;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository.Abstraction;
using Repository.Implementation;
using ServiceLayer.Abstraction;
using ServiceLayer.Implementation;
using UserRegistration;

var builder = WebApplication.CreateBuilder(args);

builder.Services
        .AddPresentation(builder.Configuration)
        .AddAuthenticationServices(builder.Configuration);







var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseCors(builder =>
    builder.WithOrigins("https://localhost:7000")
           .AllowAnyMethod()
           .AllowAnyHeader());


app.Run();
