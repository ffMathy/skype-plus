using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SkypePlus.Models;

namespace SkypePlus
{
    public partial class MainWindow
    {
        public ObservableCollection<Message> Messages { get; set; }

        private readonly SQLiteConnection _connection;
        private int _lastMessageTimestamp;

        private readonly string _database;

        public MainWindow()
        {
            Messages = new ObservableCollection<Message>();

            InitializeComponent();

            _database = FindSkypeDatabase();

            _connection = new SQLiteConnection(String.Format( @"Data Source={0};Read Only=True", _database));
        }

        private async Task GetLatestMessages()
        {
            try
            {
                if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
                var command = _connection.CreateCommand();
                command.CommandText = "SELECT `id`, `from_dispname`, `timestamp`, `body_xml` FROM `Messages` WHERE `timestamp` > @timestamp ORDER BY `timestamp` DESC, `id` ASC LIMIT 100;";
                command.Parameters.Add(new SQLiteParameter("@timestamp", _lastMessageTimestamp));
                var reader = await command.ExecuteReaderAsync();

                var messages = new List<Message>();
                while (await reader.ReadAsync())
                {
                    messages.Add(new Message(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetString(3)));
                }

                TextBlockLatestGet.Text = DateTime.Now.ToString("T");

                if (messages.Count <= 0) return;

                _lastMessageTimestamp = messages.Max(m => m.Timestamp);
                messages.OrderBy(m => m.Timestamp).ToList().ForEach(Messages.Add);

                ListBoxMessages.ScrollIntoView(ListBoxMessages.Items[ListBoxMessages.Items.Count - 1]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception when getting latest messages: " + ex);
            }
        }

        private void WatchDatabase()
        {
            var databaseFileInfo = new FileInfo(_database);
            if (!databaseFileInfo.Exists || databaseFileInfo.DirectoryName == null) return;

            var watcher = new FileSystemWatcher(databaseFileInfo.DirectoryName, databaseFileInfo.Name)
            {
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.LastWrite
            };
            watcher.Changed += (sender, args) =>
            {
                Console.WriteLine(DateTime.Now.ToString("T") + " - " + args.Name);
                Dispatcher.Invoke(() => GetLatestMessages());
            };
            watcher.EnableRaisingEvents = true;
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(_database))
            {
                MessageBox.Show("Could not find database file \"" + _database + "\".");
                Application.Current.Shutdown();
                return;
            }

            await GetLatestMessages();
            WatchDatabase();
        }

        private async void ButtonRefreshClicked(object sender, RoutedEventArgs e)
        {
            await GetLatestMessages();
        }

        private static string FindSkypeDatabase()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Skype";

            var directoryInfo = new DirectoryInfo(appDataPath);

            if (!directoryInfo.Exists)
            {
                MessageBox.Show("No Skype folder found in path \"" + appDataPath + "\".");
                Application.Current.Shutdown();
                return null;
            }

            Debug.WriteLine("Found dbs:\n" + directoryInfo.EnumerateDirectories()
                .Select(d => d.EnumerateFiles().Where(f => f.Name == "main.db"))
                .SelectMany(sublist => sublist.ToList())
                .OrderByDescending(f => f.LastWriteTime)
                .Select(f => f.FullName + "\n\t" + f.LastWriteTime)
                .Aggregate((x, y) => x + "\n" + y));

            //var userDirectory = directoryInfo.EnumerateDirectories().Where(d => d.EnumerateFiles().Any(f => f.Name == "main.db")).OrderByDescending(d => d.LastWriteTime).FirstOrDefault();
            var databaseFile = directoryInfo.EnumerateDirectories()
                .Select(d => d.EnumerateFiles().Where(f => f.Name == "main.db"))
                .SelectMany(sublist => sublist.ToList())
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault();

            if (databaseFile == null)
            {
                MessageBox.Show("Could not find any main.db files in the Skype appdata folder.");
                Application.Current.Shutdown();
                return null;
            }

            var path = databaseFile.FullName;

            Debug.WriteLine("Chosen database: " + path);

            return path;
        }
    }
}
