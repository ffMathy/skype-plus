using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using SkypePlus.Models;

namespace SkypePlus
{
    public partial class MainWindow
    {
        public ObservableCollection<Message> Messages { get; private set; }

        private readonly SQLiteConnection _connection;
        private int _lastMessageTimestamp;

        public MainWindow()
        {
            InitializeComponent();

            Messages = new ObservableCollection<Message>();

            _connection = new SQLiteConnection(@"Data Source=C:\Users\Rene\AppData\Roaming\Skype\rene.sackers\main.db");

            GetLatestMessages();
        }

        private void GetLatestMessages()
        {
            _connection.Open();
            var command = _connection.CreateCommand();
            command.CommandText = "SELECT `id`, `from_dispname`, `timestamp`, `body_xml` FROM `Messages` WHERE `timestamp` > @timestamp ORDER BY `timestamp` DESC LIMIT 100;";
            command.Parameters.Add(new SQLiteParameter("@timestamp", _lastMessageTimestamp));
            var reader = command.ExecuteReader();

            var messages = new List<Message>();
            while (reader.NextResult())
            {
                messages.Add(new Message(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetString(3)));
            }

            if (messages.Count <= 0) return;

            _lastMessageTimestamp = messages.Max(m => m.Timestamp);
            messages.OrderBy(m => m.Timestamp).ToList().ForEach(Messages.Add);
        }
    }
}
