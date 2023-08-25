using System.Collections.Generic;
using TranscriptReader.Models;
using TranscriptReader.TranscriptParsing;

namespace TranscriptReader
{
    internal class TranscriptionParser
    {
        MainWindow _parent;
        public TranscriptionParser(MainWindow parent)
        {
            _parent = parent;
        }

        internal static Transcription Parse(List<string> transcript, TranscriptType transcriptType)
        {
            ITranscriptProvider transcriptProvider = transcriptType switch
            {
                TranscriptType.MSTeams => new MSTeamsTranscript(),
                TranscriptType.CS50x => new CS50xTranscriptProvider(),
                TranscriptType.None => new DefaultTranscriptProvider(),
                _ => new DefaultTranscriptProvider(),
            };

            var transcription = transcriptProvider.Parse(transcript);
            return transcription;
        }
    }
}
