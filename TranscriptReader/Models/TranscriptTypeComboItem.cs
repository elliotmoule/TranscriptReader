using System;

namespace TranscriptReader.Models
{
    public class TranscriptTypeComboItem
    {
        public TranscriptTypeComboItem(object value)
        {
            Name = value.ToString();
            Type = (TranscriptType)Enum.Parse(typeof(TranscriptType), Name);
        }
        public string Name { get; set; }
        public TranscriptType Type { get; set; }
    }
}
