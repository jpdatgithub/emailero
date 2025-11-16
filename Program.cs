var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins("http://localhost:3001"); // <- contato-express
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors();

app.Use(async (context, next) =>
{
    var configuration = context.RequestServices.GetService<IConfiguration>();
    var apiKey = configuration?["ApiKey"]!;

    if (!context.Request.Headers.TryGetValue("API-KEY", out var extractedApiKey))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("API Key missing");
        return;
    }

    if (!apiKey.Equals(extractedApiKey))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Invalid API Key");
        return;
    }

    await next.Invoke();
});

app.MapControllers();

app.MapGet("/", () => "API rodando");

app.Run();
