namespace SkypePlus.Models
{
    public class Message
    {
        public int Id { get; private set; }
        public string FromDisplayName { get; private set; }
        public int Timestamp { get; private set; }
        public string BodyXml { get; private set; }

        public Message(int id, string fromDisplayName, int timestamp, string bodyXml)
        {
            Id = id;
            FromDisplayName = fromDisplayName;
            Timestamp = timestamp;
            BodyXml = bodyXml;
        }
    }
}
