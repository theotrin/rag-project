using StartApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();





EmbeddingService embeddingService = new EmbeddingService();
//  var embedding = await embeddingService.GenerateEmbeddingAsync("Hello, world!");
//  Console.WriteLine($"Embedding: {string.Join(", ", embedding)}");

PdfTextExtractorService pdfTextExtractorService = new PdfTextExtractorService();
ProprocessTextService proprocessTextService = new ProprocessTextService();

var text = pdfTextExtractorService.ExtractTextFromPdf("Services/Profile.pdf");

List<string> chunks = proprocessTextService.TextSplit(text);

// foreach (var chunk in chunks)
// {
//     Console.WriteLine(chunk);
// }

  List<float[]> embeddings = new List<float[]>();
  foreach (var chunk in chunks) 
  {
    var embedding = await embeddingService.GenerateEmbeddingAsync(chunk);
    
    float[] embeddingArray = embedding.ToArray();
    
    embeddings.Add(embeddingArray);
    Console.WriteLine($"Generated embedding for chunk of length {chunk.Length}");
    Console.WriteLine($"Embedding: {string.Join(", ", embeddingArray)}");
    Console.WriteLine($"Chunk: {chunk}");
  }

app.Run();
