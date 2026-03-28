using CampusApi.Data;
using CampusApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Подключаем MySQL БД
var connectionString = "Server=localhost;Database=campus_test;User=root;Password=;";

builder.Services.AddDbContext<CampusDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрируем наши сервисы
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