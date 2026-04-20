using CampusApi.Data;
using CampusApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CampusDb")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=campus_test;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddDbContext<CampusDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ��������� ����� ������ ��� Dependency Injection
builder.Services.AddScoped<IGoogleClassroomService, GoogleClassroomService>();
builder.Services.AddScoped<GradeSyncService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();