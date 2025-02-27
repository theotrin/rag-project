using StartApi.Services;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

var app = builder.Build();

// Configuração do pipeline
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles(); // Para servir arquivos estáticos, se necessário
app.UseCors("AllowAll"); // Aplica a política de CORS antes das rotas
app.UseAuthorization();
app.MapControllers();

app.Run();