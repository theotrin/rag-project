namespace StartApi.Models;
    public class ChunkEmbedding
    {
        public string Chunk { get; set; }
        public float Embedding { get; set; }

        public ChunkEmbedding(string chunk, float embedding)
        {
            Chunk = chunk;
            Embedding = embedding;
        }
    }
