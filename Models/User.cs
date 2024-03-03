using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S5TcpChat.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? UserName { get; set; }

        public virtual ICollection<Message>? SendedMessage { get; set; }
        public virtual ICollection<Message>? RecievedMessage { get; set; }
    }
}
