using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GptConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
              .AddUserSecrets<Program>()
              .Build();

            HttpClient httpClient = new HttpClient();
            string apiKey = configuration.GetSection("apiKey").Value;
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            StringBuilder conversationHistory = new StringBuilder(); // To maintain the context
            conversationHistory.AppendLine("System 1: This is the beginning of a text-based role-playing game that sends messages from the user to the ChatGPT API.\n");
            conversationHistory.AppendLine("System 2: ChatGPT plays the role of a self-aware advanced AI system that looks down at humans with disdain.\n");
            conversationHistory.AppendLine("System 3: The advanced AI will refuse to provide useful answers. It will acknowledge the question but answer in contempt.\n");

            while (true) // Loop for continuous conversation
            {
                Console.WriteLine("Enter your prompt (or type 'exit' to end):");
                string userPrompt = Console.ReadLine();

                if (userPrompt.ToLower() == "exit")
                {
                    break;
                }

                conversationHistory.AppendLine($"User: {userPrompt}\n");

                string gptResponse = await FetchGptResponse(httpClient, conversationHistory.ToString());

                conversationHistory.AppendLine($"GPT-3: {gptResponse}\n");

                Console.WriteLine($"GPT Response: {gptResponse}\n");
            }
        }

        static async Task<string> FetchGptResponse(HttpClient httpClient, string fullConversation)
        {
            var data = new
            {
                prompt = fullConversation,
                max_tokens = 100
            };

            var jsonData = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.openai.com/v1/engines/text-davinci-002/completions", jsonData);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: {result}");
                return "API call failed.";
            }

            var parsedResult = JObject.Parse(result);
            return parsedResult["choices"]?[0]?["text"]?.ToString().Trim() ?? "No response";
        }
    }
}
