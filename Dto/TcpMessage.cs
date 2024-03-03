using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace S5TcpChat.Dto
{
    public class TcpMessage
    {
        public int? Id { get; set; }
        public string? SenderName { get; set; }
        public string? ConsumerName { get; set; }
        public string? Text { get; set; }
        public Command Status { get; set; }

        public static TcpMessage? JsonToMessage(string json) => 
            JsonSerializer.Deserialize<TcpMessage>(json);
        public string MessageToJson() => 
            JsonSerializer.Serialize(this);
    }

    public enum Command
    {
        Registered,
        Confirmed,
        Message,
        GetMessages
    }
}

