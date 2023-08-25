using System.Collections.Generic;

namespace TranscriptReader.Models
{
    public class Transcription
    {
        public List<User> Users { get; } = new();
        public List<UserMessage> Messages { get; } = new();
    }
}
