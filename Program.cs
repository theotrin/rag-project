using StartApi.Services;
using Microsoft.AspNetCore.Builder;
using ragproject.Controllers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Registra os serviços como scoped (apenas uma vez)
builder.Services.AddScoped<PdfTextExtractorService>();
builder.Services.AddScoped<ProprocessTextService>();
builder.Services.AddScoped<QuestionProcessingService>(); // Certifique-se de que existe
// Registra os serviços como scoped
builder.Services.AddScoped<PdfTextExtractorService>();
builder.Services.AddScoped<ProprocessTextService>();

// Configuração de CORS simples
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()    // Permite qualquer origem
              .AllowAnyMethod()    // Permite qualquer método (POST, GET, etc.)
              .AllowAnyHeader();   // Permite qualquer cabeçalho
    });
});

// Adiciona o HttpClientFactory
builder.Services.AddHttpClient("EmbeddingClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:11434/");
});

builder.Services.AddHttpClient("SurrealDbClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:8001/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("surreal-ns", "default");
    client.DefaultRequestHeaders.Add("surreal-db", "rag");
    client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("root:root")));
});

var app = builder.Build();

// Configuração do pipeline
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();