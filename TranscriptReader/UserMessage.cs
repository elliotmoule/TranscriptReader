using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TranscriptReader
{
    public class UserMessage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _time = string.Empty;
        public string Time
        {
            get { return _time; }
            set
            {
                _time = value;
                NotifyChanged(nameof(Time));
            }
        }

        private string _message = string.Empty;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                NotifyChanged(nameof(Message));
            }
        }

        private User _user = null;
        public User User
        {
            get { return _user; }
            set
            {
                _user = value;
                NotifyChanged(nameof(User));
            }
        }
    }
}
