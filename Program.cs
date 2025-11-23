using ControlOverWeb.Data; 
using Microsoft.EntityFrameworkCore; // AddDbContext'i bulmak için


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// --- POSTGRE ---
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Learn more about configuring Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
