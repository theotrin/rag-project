namespace StartApi.Services;
    public class QuestionProcessingService
    {
        public string ProcessQuestion(string question, string productLabel)
        {
            if (string.IsNullOrEmpty(question))
                throw new ArgumentException("A pergunta não pode ser vazia.");
            
            if (string.IsNullOrEmpty(productLabel))
                throw new ArgumentException("A identificação do produto não pode ser vazia.");

            // Placeholder: Retorna uma resposta mockada por enquanto
            // Mais tarde, aqui você pode integrar com o RAG para buscar nos chunks com o productLabel
            return $"Pergunta recebida: '{question}' para o produto com label '{productLabel}'. Resposta em construção!";
        }
    }
