using System;

namespace SkypePlus.Models
{
    public class Message
    {
        public int Id { get; private set; }

        public string FromDisplayName { get; private set; }

        private int timestamp;
        public DateTime Timestamp
        {
            get
            {
                var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds(timestamp).ToLocalTime();

                return dateTime;
            }
        }

        public string BodyXml { get; private set; }

        public Message(int id, string fromDisplayName, int timestamp, string bodyXml)
        {
            this.timestamp = timestamp;

            Id = id;
            FromDisplayName = fromDisplayName;
            BodyXml = bodyXml;
        }
    }
}
