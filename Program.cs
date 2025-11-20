using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var frontOrigin = builder.Configuration["PSA_FRONT"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins(frontOrigin ?? "http://localhost:5173");
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("formLimiter", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;         // 5 requisições
        limiterOptions.Window = TimeSpan.FromMinutes(1); // por minuto
        limiterOptions.QueueLimit = 0;          // não faz fila
        limiterOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.MapGet("/", () => "API rodando");

app.Run();
