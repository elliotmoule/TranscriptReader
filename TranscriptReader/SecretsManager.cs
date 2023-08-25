using Microsoft.Extensions.Configuration;

namespace TranscriptReader
{
    public class SecretsManager
    {
        private static IConfiguration _configuration;

        public static string GetOpenAIKey()
        {
            _configuration ??= new ConfigurationBuilder()
                    .AddUserSecrets<SecretsManager>()
                    .Build();

            return _configuration["OpenAIKey"];
        }
    }
}
