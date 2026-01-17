using ControlOverWeb.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Veritabanı Ayarları (PostgreSQL)
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Swagger Kurulumu
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Statik Dosya Servisi
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Ana Sayfa Yönlendirmesi
app.MapGet("/", async context =>
{
    context.Response.Redirect("/index.html");
});

app.Run();
