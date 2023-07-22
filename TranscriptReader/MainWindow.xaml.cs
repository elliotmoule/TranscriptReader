using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TranscriptReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            IsFormEnabled = true;
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new()
            {
                Multiselect = false,
                InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                Filter = FileFilterX.Library.FileFilterX.FilterBuilder(false, new FileFilterX.Library.Filter("Text Files", new string[] { "TXT", "txt" })),
                Title = "Select Transcript.."
            };

            fileDialog.ShowDialog();

            FilePath = fileDialog.FileName;

            SortTranscript(FilePath);
        }

        private void SortTranscript(string filepath)
        {
            if (string.IsNullOrEmpty(filepath)) return;
            if (!File.Exists(filepath)) return;
            IsFormEnabled = false;

            UserMessages.Clear();
            _allUsers.Clear();
            Users.Clear();

            var transcript = File.ReadAllLines(filepath);
            UserMessage last = null;

            Regex regex = new("[0-9]+:[0-9]+:[0-9]+\\.[0-9]+\\s+-->\\s+[0-9]+:[0-9]+:[0-9]+\\.[0-9]+");
            for (int i = 0; i < transcript.Length; i++)
            {
                var maxLoop = 10000;
                var time = string.Empty;

                while (maxLoop > 0)
                {
                    time = transcript[i];
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
                var name = transcript[i];

                i++;
                var message = transcript[i];

                maxLoop = 10000;
                while (maxLoop > 0)
                {
                    i++;
                    if (i < transcript.Length && !regex.IsMatch(transcript[i]))
                    {
                        message += $" {transcript[i]}";
                    }
                    else
                    {
                        i--;
                        break;
                    }
                    maxLoop--;
                }

                if (FilterSpeechFillers && IsFillerWord(message, 2))
                {
                    continue;
                }

                var speaker = _allUsers.FirstOrDefault(x => x.Name == name);
                if (speaker == null)
                {
                    speaker = new User(name);
                    _allUsers.Add(speaker);
                }

                UserMessage newMessage = new()
                {
                    Time = time,
                    Message = message,
                    User = speaker,
                };

                if (UserMessages.Count > 0 && last.User.Id == speaker.Id)
                {
                    last.Message += $"{Environment.NewLine + Environment.NewLine}{message}";
                }
                else
                {
                    UserMessages.Add(newMessage);
                    last = newMessage;
                }
            }

            Task.Factory.StartNew(() =>
            {
                // Filter colour list for colours which can be used.
                ColourDictionary.AllColours.Remove(Brushes.DodgerBlue); // Removing this due to it being the main message background colour.
                foreach (var colour in ColourDictionary.AllColours)
                {
                    if (ColourDictionary.IsReadableForeground(Colors.WhiteSmoke, colour.Color))
                    {
                        ColourDictionary.LuminosityList.Add(colour);
                    }
                }

                Shuffle(ColourDictionary.LuminosityList);

                App.Current.Dispatcher.Invoke(() =>
                {
                    var iteration = 0;
                    for (int i = 0; i < _allUsers.Count; i++)
                    {
                        if (ColourDictionary.LuminosityList.Count <= iteration)
                        {
                            iteration = 0;
                        }

                        _allUsers[i].Colour = ColourDictionary.LuminosityList[iteration];
                        iteration++;
                    }
                });
            });

            Users.AddRange(_allUsers.Select(x => x.Name).OrderBy(y => y));
            Users.Insert(0, "< None >");
            SelectedUser = Users[0];
            NotifyChanged(nameof(UserMessages));
            NotifyChanged(nameof(Users));

            IsFormEnabled = true;
        }

        private static Random _random = new();
        private static void Shuffle(List<SolidColorBrush> colors)
        {
            int n = colors.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                var value = colors[k];
                colors[k] = colors[n];
                colors[n] = value;
            }
        }

        private static bool IsFillerWord(string input, int tolerance)
        {
            string[] words = input.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            int fillerWordCount = words.Length;

            return fillerWordCount <= tolerance;
        }

        private string _path = string.Empty;
        public string FilePath
        {
            get { return _path; }
            set
            {
                _path = value;
                NotifyChanged(nameof(FilePath));
            }
        }

        private ObservableCollection<UserMessage> userMessages = new();
        public ObservableCollection<UserMessage> UserMessages
        {
            get { return userMessages; }
            set
            {
                userMessages = value;
                NotifyChanged(nameof(UserMessages));
            }
        }

        private ObservableCollection<UserMessage> _filteredMessages = new();
        public ObservableCollection<UserMessage> FilteredMessages
        {
            get { return _filteredMessages; }
            set
            {
                _filteredMessages = value;
                NotifyChanged(nameof(FilteredMessages));
            }
        }

        private UserMessage _selectedMessage = null;
        public UserMessage SelectedMessage
        {
            get { return _selectedMessage; }
            set
            {
                _selectedMessage = value;
                NotifyChanged(nameof(SelectedMessage));
            }
        }

        public List<User> _allUsers = new();

        private ObservableCollection<string> _users = new();
        public ObservableCollection<string> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                NotifyChanged(nameof(Users));
            }
        }

        private string _selectedUser = string.Empty;
        public string SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;
                NotifyChanged(nameof(SelectedUser));

                if (value == null || value.Contains("None"))
                {
                    FilteredMessages.Clear();
                    FilteredMessages.AddRange(UserMessages);
                }
                else
                {
                    FilteredMessages.Clear();
                    FilteredMessages.AddRange(UserMessages.Where(x => x.User.Name == value));
                }
                SelectedMessage = FilteredMessages.FirstOrDefault();
            }
        }

        private string _messageFilter = string.Empty;
        public string MessageFilter
        {
            get { return _messageFilter; }
            set
            {
                _messageFilter = value;
                NotifyChanged(nameof(MessageFilter));

                if (string.IsNullOrWhiteSpace(value) || value.Length < 3)
                {
                    FilteredMessages.Clear();
                    FilteredMessages.AddRange(UserMessages);
                }
                else
                {
                    FilteredMessages.Clear();
                    FilteredMessages.AddRange(UserMessages.Where(x => x.Message.Contains(value, StringComparison.OrdinalIgnoreCase)));
                }
                SelectedMessage = FilteredMessages.FirstOrDefault();
            }
        }

        private bool _filterSpeechFillers = false;
        public bool FilterSpeechFillers
        {
            get { return _filterSpeechFillers; }
            set
            {
                _filterSpeechFillers = value;
                NotifyChanged(nameof(FilterSpeechFillers));

                SortTranscript(FilePath);
            }
        }

        private bool _isFormEnabled = false;
        public bool IsFormEnabled
        {
            get { return _isFormEnabled; }
            set
            {
                _isFormEnabled = value;
                NotifyChanged(nameof(IsFormEnabled));
            }
        }
    }
}
