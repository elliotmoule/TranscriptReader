using System;
using System.Collections.Generic;
using System.Linq;
using TranscriptReader.Models;

namespace TranscriptReader.TranscriptParsing
{
    /// <summary>
    /// A basic transcription provider. Simply sets one user speaking all provided text, and approximates the time for each sentence.
    /// </summary>
    public class DefaultTranscriptProvider : ITranscriptProvider
    {
        public Transcription Parse(List<string> transcript)
        {
            var transcription = new Transcription();
            var user = new User("Default User");
            transcription.Users.Add(user);
            var lines = transcript;
            var time = DateTime.Now;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var words = line.Count(x => x == ' ');
                var seconds = Math.Round(words / 2.333f);
                time = time.AddSeconds(seconds);
                transcription.Messages.Add(new UserMessage
                {
                    Message = line,
                    User = user,
                    Time = time.ToString("hh:mm:ss")
                });
            }

            return transcription;
        }
    }
}
