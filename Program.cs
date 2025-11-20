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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors();

// Middleware de API KEY usando variável de ambiente
app.Use(async (context, next) =>
{
    var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
    var apiKeyFromEnv = configuration["API_KEY"];

    if (string.IsNullOrEmpty(apiKeyFromEnv))
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("API_KEY not configured on server");
        return;
    }

    if (!context.Request.Headers.TryGetValue("API-KEY", out var extractedApiKey))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("API Key missing");
        return;
    }

    if (!apiKeyFromEnv.Equals(extractedApiKey))
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
