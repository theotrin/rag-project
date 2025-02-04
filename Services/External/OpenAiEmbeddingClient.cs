using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

public class OpenAiEmbeddingClient {
  private readonly string _apiKey;
  private readonly HttpClient _httpClient;

  public OpenAiEmbeddingClient(string apiKey) {
    _apiKey = apiKey;
    _httpClient = new HttpClient();
    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
  }

  public async Task<float[]> GetEmbeddingAsync(string text) {
    var payload = new {
      input = text,
      model = "text-embedding-3-small" // Use OpenAI's embeddings model
    };

    var content = new StringContent(
      JsonConvert.SerializeObject(payload),
      Encoding.UTF8,
      "application/json"
    );

    var response = await _httpClient.PostAsync(
      "https://api.openai.com/v1/embeddings",
      content
    );

    if (!response.IsSuccessStatusCode) {
      throw new Exception($"API Error: {await response.Content.ReadAsStringAsync()}");
    }

    var responseJson = await response.Content.ReadAsStringAsync();
    dynamic responseData = JsonConvert.DeserializeObject(responseJson);
    return responseData.data[0].embedding.ToObject<float[]>();
  }
}