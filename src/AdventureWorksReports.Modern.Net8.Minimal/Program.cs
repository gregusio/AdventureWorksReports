using AdventureWorksReports.Modern.Net8.Minimal.Data;
using AdventureWorksReports.Modern.Net8.Minimal.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AdventureWorksContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapReportsEndpoints();

app.Run();
