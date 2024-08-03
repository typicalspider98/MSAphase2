using backend.Models;
using backend.Repositories;
using backend.Services;
using backend.Data;
using backend.Controllers;
using backend.Hubs; 
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    //options.UseSqlerver(builder.Configuration.GetConnectionString("DefaultConnection")));
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    options.User.RequireUniqueEmail = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

//SignalR
builder.Services.AddSignalR();
builder.Services.AddScoped<IFileRepository, FileRepository>();


// Configure Repositories
builder.Services.AddScoped<IFileRepository, FileRepository>();

// Configure JWT Authentication
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>(); // Implement JwtTokenService

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Enable CORS
app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication(); // Add this line
app.UseAuthorization();

app.MapControllers();
app.MapHub<FileHub>("/fileHub");

app.Run();
