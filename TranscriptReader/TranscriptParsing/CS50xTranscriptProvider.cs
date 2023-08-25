using System;
using System.Collections.Generic;
using System.Linq;
using TranscriptReader.Models;

namespace TranscriptReader.TranscriptParsing
{
    public class CS50xTranscriptProvider : ITranscriptProvider
    {
        public Transcription Parse(List<string> transcript)
        {
            var transcription = new Transcription();
            var lines = transcript;

            UserMessage currentMessage = null;
            var @default = new User("Default User");

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (currentMessage != null)
                    {
                        AddMessageWithWordLimit(transcription, currentMessage);
                        currentMessage = null;
                    }
                    continue;
                }

                if (line.Contains(':'))
                {
                    var parts = line.Split(new[] { ':' }, 2);
                    var userName = parts[0].Trim();

                    var user = transcription.Users.FirstOrDefault(x => x.Name.Equals(userName, StringComparison.OrdinalIgnoreCase));
                    if (user == null)
                    {
                        user = new User(userName);
                        transcription.Users.Add(user);
                    }

                    var messageText = parts[1].Trim();

                    if (currentMessage != null)
                    {
                        AddMessageWithWordLimit(transcription, currentMessage);
                    }

                    currentMessage = new UserMessage { User = user, Message = messageText };
                }
                else
                {
                    var message = line.Trim();
                    if (currentMessage != null)
                        currentMessage.Message += " " + message;
                    else
                    {
                        // Handle edge cases where transcription starts without a user name.
                        currentMessage = new UserMessage { User = @default, Message = message };
                    }
                }
            }

            if (currentMessage != null)
                AddMessageWithWordLimit(transcription, currentMessage);

            return transcription;
        }

        private static void AddMessageWithWordLimit(Transcription transcription, UserMessage message)
        {
            var words = message.Message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length <= 100)
            {
                transcription.Messages.Add(message);
            }
            else
            {
                var currentWordCount = 0;
                var currentMessage = new UserMessage { User = message.User, Message = "" };

                foreach (var word in words)
                {
                    if (currentWordCount < 100)
                    {
                        currentMessage.Message += word + " ";
                        currentWordCount++;
                    }
                    else
                    {
                        transcription.Messages.Add(currentMessage);
                        currentMessage = new UserMessage { User = message.User, Message = word + " " };
                        currentWordCount = 1;
                    }
                }

                if (!string.IsNullOrWhiteSpace(currentMessage.Message))
                    transcription.Messages.Add(currentMessage);
            }
        }
    }
}
