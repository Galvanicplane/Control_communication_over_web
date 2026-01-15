using ControlOverWeb.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Veritabani baglantisi - PostgreSQL
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Swagger kurulumu
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Statik dosyalari (html, js, css) sunmak icin
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Ana dizine girildiginde index.html'e yonlendir
app.MapGet("/", async context =>
{
    context.Response.Redirect("/index.html");
});

app.Run();
