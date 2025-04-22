# 🧠 RAG Project
Este projeto implementa um sistema de Geração Aumentada por Recuperação (RAG) utilizando .NET 8, SurrealDB como banco de dados vetorial e Ollama para execução de modelos de linguagem

## 📌 Sobre o Projeto

Este projeto é uma implementação de um sistema **RAG (Retrieval-Augmented Generation)** que permite a um **modelo de linguagem local (LLM)** responder perguntas com base em documentos PDF fornecidos pelo usuário.

Funciona da seguinte forma:

1. **Ingestão de PDFs**: o sistema lê e processa arquivos PDF, extraindo o conteúdo textual.
2. **Indexação Vetorial**: o texto extraído é dividido em chunks e vetorizado, sendo armazenado no **SurrealDB**.
3. **Recuperação Semântica**: ao receber uma pergunta, o sistema busca os chunks mais relevantes via similaridade vetorial.
4. **Geração com Contexto**: esses trechos recuperados são enviados junto à pergunta para um modelo LLM rodando localmente via **Ollama**, que então gera uma resposta contextualizada — como um chatbot especialista no conteúdo dos PDFs.

- Essa abordagem permite criar assistentes personalizados e privados que "aprendem" com seus próprios documentos. Ideal para automação de atendimento, suporte técnico baseado em documentação, e estudos com material específico.

## ⚙️ Tecnologias Utilizadas

- **.NET 8**:Framework principal para desenvolvimento da API
- **SurrealDB**:Banco de dados multimodelo utilizado para armazenamento e busca vetorial
- **Ollama**:Plataforma para execução local de modelos de linguagem (LLMs)
- **Docker Compose**:Orquestração dos serviços necessários para o funcionamento do sistema
- **HTML/CSS/JS**:Frontend simples para interação com a API
  

## 🚀 Como Iniciar o Projeto

1. **Clone o repositório:**

   ```bash
   git clone https://github.com/theotrin/rag-project.git
   cd rag-project
   ```

2. **Inicie o SurrealDB:**

   ```bash
   docker compose -f surreal-compose.yaml up
   ```

3. **Inicie o Ollama:**

   ```bash
   docker compose -f ollama-compose.yaml up
   ```

4. **Execute a aplicação .NET:**

   ```bash
   dotnet run
   ```

5. **Acesse o frontend:**

   Abra o arquivo `index.html` localizado na pasta `front_end` em seu navegador para interagir com a aplicação.

## 📁 Estrutura do Projeto

- `Controllers/`: Controladores da AI. 
- `Models/`: Modelos de dados utilizados na aplicaço. 
- `Services/`: Serviços que contêm a lógica de negóco. 
- `Utilities/`: Funções utilitárias auxiliars. 
- `Properties/`: Configurações do projeo. 
- `front_end/`: Arquivos do frontend (HTML, CSS, J). 
- `Program.cs`: Ponto de entrada da aplicaço. 
- `appsettings.json`: Configurações da aplicaço. 
- `docker-compose.yaml`: Orquestração dos serviços com Dockr.  

## 🧪 Tests

Atualmente, não há testes automatizados implementados. Contribuições são bem-vinas!

## 🤝 Contribuiçes

Sinta-se à vontade para abrir issues ou pull requests. Toda contribuição é bem-vnda! 
