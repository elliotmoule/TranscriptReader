using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TranscriptReader.OpenAISummarizer
{
    public class Summarizer
    {
        private const string API_ENDPOINT = "https://api.openai.com/v1/chat/completions";
        private const int MAX_TOKENS_PER_REQUEST = 145000; // A buffer under 150000 to account for prompt tokens and other overhead
        private const int MAX_CONCURRENT_REQUESTS = 3;

        private readonly SemaphoreSlim _semaphore = new(MAX_CONCURRENT_REQUESTS);

        public async Task<List<string>> SummarizeSentences(IEnumerable<string> sentences)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {SecretsManager.GetOpenAIKey()}");

            var sentenceBatches = new List<List<string>>();
            var currentBatch = new List<string>();
            var currentTokenCount = 0;

            foreach (var sentence in sentences)
            {
                var tokenCount = sentence.Split(' ').Length; // A simple token estimate
                if ((currentTokenCount + tokenCount) > MAX_TOKENS_PER_REQUEST)
                {
                    sentenceBatches.Add(currentBatch);
                    currentBatch = new List<string>();
                    currentTokenCount = 0;
                }

                currentBatch.Add(sentence);
                currentTokenCount += tokenCount;
            }

            if (currentBatch.Any())
            {
                sentenceBatches.Add(currentBatch);
            }

            var tasks = sentenceBatches.Select(batch => SummarizeBatch(httpClient, batch)).ToList();
            var summariesList = await Task.WhenAll(tasks);

            return summariesList.SelectMany(x => x).ToList();
        }

        private async Task<List<string>> SummarizeBatch(HttpClient client, List<string> sentences)
        {
            await _semaphore.WaitAsync();
            try
            {
                var prompt = $"The following sentences are from a transcript. Summarize each of the sentences in 5 words or less, ignoring sentences which do not add anything useful:" + string.Join(Environment.NewLine, sentences);
                var payload = new
                {
                    model = "text-davinci-003",
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    },
                    temperature = "0.7"
                };

                var response = await client.PostAsync(API_ENDPOINT, new StringContent(JsonSerializer.Serialize(payload)));
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<OpenAIResponse>(responseBody);

                return responseObject.choices.Select(c => c.text.Trim()).ToList();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private class OpenAIResponse
        {
            public List<Choice> choices { get; set; }

            public class Choice
            {
                public string text { get; set; }
            }
        }
    }
}
