using SkypePlus.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkypePlus.DataAccess
{
    public class SkypeDataAccess
    {
        private static SkypeDataAccess instance;

        public event Action<IEnumerable<Message>> NewMessagesArrived;

        private SQLiteConnection _connection;
        private int _lastMessageTimestamp;

        public static SkypeDataAccess Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SkypeDataAccess();
                }
                return instance;
            }
        }

        private ICollection<Message> messages;
        public IEnumerable<Message> Messages
        {
            get
            {
                return messages;
            }
        }

        private SkypeDataAccess()
        {
            messages = new LinkedList<Message>();

            var watcher = new FileSystemWatcher();
            watcher.IncludeSubdirectories = true;
            watcher.Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Skype");
            watcher.EnableRaisingEvents = true;

            watcher.Changed += Watcher_Changed;
        }

        private async void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            var filename = e.Name;
            if (string.Equals(filename, "main.db", StringComparison.OrdinalIgnoreCase))
            {
                //this is the file we want. let's query new entries since last time.
                if (_connection == null)
                {
                    _connection = new SQLiteConnection(string.Format(@"Data Source={0};Read Only=True", filename));
                }

                if (_connection.State != ConnectionState.Open)
                {
                    await _connection.OpenAsync();
                }

                var command = _connection.CreateCommand();
                command.CommandText = "SELECT `id`, `from_dispname`, `timestamp`, `body_xml` FROM `Messages` WHERE `timestamp` > @timestamp ORDER BY `timestamp` DESC, `id` ASC LIMIT 100;";
                command.Parameters.Add(new SQLiteParameter("@timestamp", _lastMessageTimestamp));

                var reader = await command.ExecuteReaderAsync();

                var newMessages = new List<Message>();
                while (await reader.ReadAsync())
                {
                    var timestamp = reader.GetInt32(2);
                    var message = new Message(reader.GetInt32(0), reader.GetString(1), timestamp, reader.GetString(3));
                    newMessages.Add(message);
                    messages.Add(message);

                    if (timestamp > _lastMessageTimestamp)
                    {
                        timestamp = _lastMessageTimestamp;
                    }
                }

                if (NewMessagesArrived != null)
                {
                    NewMessagesArrived(newMessages);
                }
            }
        }
    }
}
