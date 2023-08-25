using System.Collections.Generic;
using TranscriptReader.Models;

namespace TranscriptReader.TranscriptParsing
{
    internal interface ITranscriptProvider
    {
        Transcription Parse(List<string> transcript);
    }
}
