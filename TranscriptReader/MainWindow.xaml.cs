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
using TranscriptReader.Models;
using TranscriptReader.OpenAISummarizer;

namespace TranscriptReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private List<string> _summarisedSentences = new();
        private Summarizer _summarizer;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            AddTranscriptTypes();
            IsFormEnabled = true;
        }

        private void AddTranscriptTypes()
        {
            foreach (var value in Enum.GetValues(typeof(TranscriptType)))
            {
                TranscriptTypes.Add(new TranscriptTypeComboItem(value));
            }

            SelectedTranscriptType = TranscriptTypes.FirstOrDefault();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            IsFormEnabled = false;
            OpenFileDialog fileDialog = new()
            {
                Multiselect = false,
                InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                Filter = FileFilterX.Library.FileFilterX.FilterBuilder(false, new FileFilterX.Library.Filter("Text Files", new string[] { "TXT", "txt" })),
                Title = "Select Transcript.."
            };

            fileDialog.ShowDialog();

            DoTranscription(fileDialog.FileName);
            IsFormEnabled = true;
        }

        private void DoTranscription(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            if (!File.Exists(filePath)) return;

            IsFormEnabled = false;
            if (FilePath != filePath)
            {
                // New document.
                _summarisedSentences.Clear();
                FilePath = filePath;
            }

            UserMessages.Clear();
            _allUsers.Clear();
            Users.Clear();

            _summarizer ??= new Summarizer();

            Task.Factory.StartNew(async () =>
            {
                List<string> sentences = GetSentences(filePath).ToList();
                if (sentences.Count > 0 && _summarisedSentences.Count == 0)
                {
                    // Only sending for summarisation if there is a list of sentences.
                    sentences = await _summarizer.SummarizeSentences(sentences);
                    _summarisedSentences = sentences;
                }

                var transcription = TranscriptionParser.Parse(sentences, SelectedTranscriptType.Type);
                App.Current.Dispatcher.Invoke(() =>
                {
                    SortTranscript(transcription);
                });
            });
        }

        private void SortTranscript(Transcription transcription)
        {
            UserMessages.AddRange(transcription.Messages);
            _allUsers.AddRange(transcription.Users);

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

        private List<string> GetSentences(string filepath)
        {
            var sentences = new List<string>();
            if (string.IsNullOrWhiteSpace(filepath)) return sentences;
            if (new FileInfo(filepath) is not FileInfo fi || !fi.Exists) return sentences;

            var lines = File.ReadAllLines(filepath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)
                    || line.Trim() is not string trimmed
                    || IsFillerWord(trimmed, 2))
                {
                    continue;
                }

                sentences.Add(trimmed);
            }

            return sentences;
        }

        private static readonly Random _random = new();
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

        internal static bool IsFillerWord(string input, int tolerance)
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

                DoTranscription(FilePath);
            }
        }

        private TranscriptTypeComboItem _selectedTranscriptType = null;
        public TranscriptTypeComboItem SelectedTranscriptType
        {
            get { return _selectedTranscriptType; }
            set
            {
                _selectedTranscriptType = value;
                NotifyChanged(nameof(SelectedTranscriptType));

                DoTranscription(FilePath);
            }
        }

        private ObservableCollection<TranscriptTypeComboItem> _transcriptTypes = new();
        public ObservableCollection<TranscriptTypeComboItem> TranscriptTypes
        {
            get { return _transcriptTypes; }
            set
            {
                _transcriptTypes = value;
                NotifyChanged(nameof(TranscriptTypes));
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
