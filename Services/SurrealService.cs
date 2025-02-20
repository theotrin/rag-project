using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class SurrealService
{
    private static readonly HttpClient client = new HttpClient();

    static async Task Main(string[] args)
    {
        // Configurar a base URL do SurrealDB
        string baseUrl = "http://localhost:8000"; // Substitua pelo seu endpoint
        string database = "rag"; // Substitua pelo nome do seu banco
        string namespaceName = "test"; // Substitua pelo namespace
        string token = "your_auth_token"; // Token de autenticação, se necessário

        // Autenticar (se necessário)
        await Authenticate(client, baseUrl, namespaceName, database, token);

        // Dados a serem inseridos (exemplo: uma pessoa)
        var data = new
        {
            name = "João Silva",
            age = 30,
            email = "joao@example.com"
        };

        // Endpoint para inserir (substitua "pessoas" pelo nome da sua tabela)
        string tableName = "test";
        string url = $"{baseUrl}/sql";

        // Query SQL para INSERT
        string query = $"INSERT INTO {tableName} (name, age, email) VALUES ({data.name}, {data.age}, {data.email});";

        // Enviar a requisição
        var response = await InsertData(client, url, query);

        Console.WriteLine("Dados inseridos com sucesso: " + response);
    }

    static async Task Authenticate(HttpClient client, string baseUrl, string namespaceName, string database, string token)
    {
        // Configurar cabeçalhos para autenticação
        client.DefaultRequestHeaders.Add("NS", namespaceName);
        client.DefaultRequestHeaders.Add("DB", database);
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }
    }

    static async Task<string> InsertData(HttpClient client, string url, string query)
    {
        // Configurar o conteúdo da requisição (query SQL)
        var content = new StringContent(
            $"{{ \"query\": \"{query}\" }}",
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            // Enviar requisição POST
            HttpResponseMessage response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            else
            {
                throw new Exception($"Erro ao inserir: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro na requisição: {ex.Message}");
        }
    }
    
    static async Task<string> GetAuthToken(HttpClient client, string baseUrl, string namespaceName, string database, string username, string password)
    {
        // Endpoint de autenticação
        string url = $"{baseUrl}/signin";

        // Dados de autenticação (no formato JSON)
        var credentials = new
        {
            user = username,
            pass = password,
            NS = namespaceName,
            DB = database
        };

        // Serializar para JSON
        string jsonContent = JsonConvert.SerializeObject(credentials);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        try
        {
            // Enviar requisição POST
            HttpResponseMessage response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                // Ler a resposta (o token está no corpo da resposta)
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject jsonResponse = JObject.Parse(responseBody);

                // Extrair o token (pode estar em um campo como "token" ou "jwt")
                string token = jsonResponse["token"]?.ToString();

                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("Token não encontrado na resposta.");
                }

                return token;
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro ao autenticar: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro na requisição: {ex.Message}");
        }
    }
}