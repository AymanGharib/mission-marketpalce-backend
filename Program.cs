using MissionGenerator.Services;

var builder = WebApplication.CreateBuilder(args);

// Ajouter les services nécessaires
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ajouter le service Gemini
builder.Services.AddHttpClient<GeminiService>();
builder.Services.AddScoped<GeminiService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();

// Activer Swagger en développement

    app.UseSwagger();
    app.UseSwaggerUI();

app.UseCors();
// Sécurisation HTTPS et autorisation
app.UseHttpsRedirection();
app.UseAuthorization();

// Mapper les contrôleurs API
app.MapControllers();

app.Run();



