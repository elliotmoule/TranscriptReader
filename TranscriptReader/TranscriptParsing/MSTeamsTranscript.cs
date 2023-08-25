using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TranscriptReader.Models;

namespace TranscriptReader.TranscriptParsing
{
    public class MSTeamsTranscript : ITranscriptProvider
    {
        public Transcription Parse(List<string> transcript)
        {
            var transcription = new Transcription();
            var transcriptArray = transcript;

            UserMessage last = null;

            Regex regex = new("[0-9]+:[0-9]+:[0-9]+\\.[0-9]+\\s+-->\\s+[0-9]+:[0-9]+:[0-9]+\\.[0-9]+");
            for (int i = 0; i < transcriptArray.Count; i++)
            {
                var maxLoop = 10000;
                var time = string.Empty;

                while (maxLoop > 0)
                {
                    time = transcriptArray[i];
                    if (regex.IsMatch(time))
                    {
                        break;
                    }
                    i++;
                    maxLoop--;
                }

                if (maxLoop <= 0)
                {
                    throw new Exception("Failed to get time!");
                }

                i++;
                var name = transcriptArray[i];

                i++;
                var message = transcriptArray[i];

                maxLoop = 10000;
                while (maxLoop > 0)
                {
                    i++;
                    if (i < transcriptArray.Count && !regex.IsMatch(transcriptArray[i]))
                    {
                        message += $" {transcriptArray[i]}";
                    }
                    else
                    {
                        i--;
                        break;
                    }
                    maxLoop--;
                }

                var speaker = transcription.Users.FirstOrDefault(x => x.Name == name);
                if (speaker == null)
                {
                    speaker = new User(name);
                    transcription.Users.Add(speaker);
                }

                UserMessage newMessage = new()
                {
                    Time = time,
                    Message = message,
                    User = speaker,
                };

                if (transcription.Messages.Count > 0 && last.User.Id == speaker.Id)
                {
                    last.Message += $"{Environment.NewLine + Environment.NewLine}{message}";
                }
                else
                {
                    transcription.Messages.Add(newMessage);
                    last = newMessage;
                }
            }

            return transcription;
        }
    }
}
