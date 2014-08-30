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

        private const string Database = @"C:\Users\Rene\AppData\Roaming\Skype\rene.sackers\main.db";

        public MainWindow()
        {
            Messages = new ObservableCollection<Message>();

            InitializeComponent();

            _connection = new SQLiteConnection(String.Format( @"Data Source={0}", Database));
        }

        private async Task GetLatestMessages()
        {
            try
            {
                if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
                var command = _connection.CreateCommand();
                command.CommandText = "SELECT `id`, `from_dispname`, `timestamp`, `body_xml` FROM `Messages` WHERE `timestamp` > @timestamp ORDER BY `timestamp` DESC LIMIT 100;";
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
            var databaseFileInfo = new FileInfo(Database);
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
            if (!File.Exists(Database))
            {
                MessageBox.Show("Could not find database file \"" + Database + "\".");
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
    }
}
