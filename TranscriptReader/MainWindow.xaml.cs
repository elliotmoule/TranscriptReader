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

        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new()
            {
                Multiselect = false,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = FileFilterX.Library.FileFilterX.FilterBuilder(false, new FileFilterX.Library.Filter("Text Files", new string[] { "TXT", "txt" })),
                Title = "Select Transcript.."
            };

            fileDialog.ShowDialog();

            Path = fileDialog.FileName;

            SortTranscript(Path);
        }

        private void SortTranscript(string filepath)
        {
            if (string.IsNullOrEmpty(filepath)) return;
            if (!File.Exists(filepath)) return;

            UserMessages.Clear();
            Users.Clear();

            var transcript = File.ReadAllLines(filepath);

            Regex regex = new("[0-9]+:[0-9]+:[0-9]+\\.[0-9]+\\s-->\\s(([+-]?(?=\\.\\d|\\d)(?:\\d+)?(?:\\.?\\d*))(?:[Ee]([+-]?\\d+))?(:([+-]?(?=\\.\\d|\\d)(?:\\d+)?(?:\\.?\\d*))(?:[Ee]([+-]?\\d+))?)+) ");
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

                var newUser = _allUsers.FirstOrDefault(x => x.Name == name);
                if (newUser == null)
                {
                    newUser = new User(name);
                    _allUsers.Add(newUser);
                }

                UserMessages.Add(new()
                {
                    Time = time,
                    Message = message,
                    User = newUser,
                });
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
                        if(ColourDictionary.LuminosityList.Count <= iteration)
                        {
                            iteration = 0;
                        }

                        _allUsers[i].Colour = ColourDictionary.LuminosityList[iteration];
                        iteration++;
                    }
                });
            });

            Users = new ObservableCollection<string>(_allUsers.Select(x => x.Name).OrderBy(y => y));
            Users.Insert(0, "< None >");
            SelectedUser = Users[0];
            NotifyChanged(nameof(UserMessages));
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

        private string _path = string.Empty;
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                NotifyChanged(nameof(Path));
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

        private ObservableCollection<UserMessage> _filteredMessages;
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
                    FilteredMessages = new ObservableCollection<UserMessage>(UserMessages);
                }
                else
                {
                    FilteredMessages = new ObservableCollection<UserMessage>(UserMessages.Where(x => x.User.Name == value));
                }
            }
        }
    }
}
