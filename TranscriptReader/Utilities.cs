using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TranscriptReader
{
    public static class Utilities
    {
        public static bool AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> input)
        {
            foreach (var item in input)
            {
                collection.Add(item);
            }

            return true;
        }
    }
}
