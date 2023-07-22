using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System;

namespace TranscriptReader
{
    public class User : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public User(string name)
        {
            if (name == null || name.Length < 1)
                throw new System.Exception("Invalid provided User name.");

            var names = name.Split(' ');

            if (names.Length >= 2)
            {
                _initials = string.Concat(names[0].AsSpan(0, 1), names[1].AsSpan(0, 1));
            }
            else if (names.Length == 1)
            {
                _initials = string.Concat(names[0].AsSpan(0, 1), "?");
            }
            else
            {
                throw new System.Exception("Provided user has no proper name.");
            }

            _name = name;
            _initials = _initials.Trim();
            Id = Guid.NewGuid();
        }

        private readonly string _name = string.Empty;
        public string Name
        {
            get { return _name; }
        }

        private readonly string _initials = string.Empty;
        public string Initials
        {
            get { return _initials; }
        }

        private Brush _colour = Brushes.Magenta;
        public Brush Colour
        {
            get { return _colour; }
            set
            {
                _colour = value;
                NotifyChanged(nameof(Colour));
            }
        }

        private Guid _id;
        public Guid Id
        {
            get { return _id; }
            set
            {
                _id = value;
                NotifyChanged(nameof(Id));
            }
        }
    }
}
