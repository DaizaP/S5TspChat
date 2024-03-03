using S5TcpChat.Dto;
using S5TcpChat.Models;
using System.Net;
using System.Net.Sockets;

namespace S5TcpChat.Services
{
    public class ChatServer : IDisposable
    {
        TcpListener? _listener;

        public ChatServer(IPEndPoint? endPoint)
        {
            if (endPoint != null)
            {
                _listener = new TcpListener(endPoint);
            }
        }

        public void Run(object? state, bool timeOut)
        {
            try
            {
                if (_listener != null)
                    _listener.Start();

                Console.Out.WriteLineAsync("Запущен");

                if (_listener != null)
                    while (true)
                    {
                        TcpClient? tcpClient = _listener.AcceptTcpClient();

                        Task entry = ProcessClient(tcpClient);

                        Console.WriteLine($"Клиент {tcpClient.GetHashCode()} Успешно подключен");
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Dispose();
            }
        }

        async Task ProcessClient(TcpClient client)
        {
            try
            {
                using var reader = new StreamReader(client.GetStream());

                string? json;

                while (true)
                {
                    json = await reader.ReadToEndAsync();

                    TcpMessage? message = TcpMessage.JsonToMessage(json);

                    switch (message?.Status)
                    {
                        case Command.Registered:
                            RegisterClient(message.SenderName);
                            break;
                        case Command.Confirmed:
                            Confirmed(message.Id);
                            break;
                        case Command.Message:
                            SaveMessage(message);
                            break;
                        case Command.GetMessages:
                            Task s = new Task(() => SendMessage(message.SenderName, client));
                            s.Start();
                            break;
                        case null:
                            break;
                    }
                }
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.Message);
            }
            finally 
            { 
                lock (client)
                {
                    client.Dispose();
                }
            }
        }

        private void RegisterClient(string? name)
        {
            using var context = new ChatContext();

            context.Users.Add(new User { UserName = name });
            context.SaveChanges();
        }

        private void Confirmed(int? id)
        {
            using var context = new ChatContext();

            var message = context.Messages.FirstOrDefault(m => m.Id == id);
            if (message != null)
            {
                message.IsReceived = true;
            }
            context.SaveChanges();
        }

        private void SaveMessage(TcpMessage message)
        {
            using var context = new ChatContext();

            User? autor = context.Users.FirstOrDefault(m => m.UserName == message.SenderName);
            User? consumer = context.Users.FirstOrDefault(m => m.UserName == message.ConsumerName);

            context.Messages.Add(new Message
            {
                AuthorId = autor?.Id,
                ConsumerId = consumer?.Id,
                Content = message.Text,
                IsReceived = false
            });
        }
        private async Task SendMessage(string? consumerName, TcpClient client)
        {
            using var writer = new StreamWriter(client.GetStream());
            using var context = new ChatContext();

            User? consumer = context.Users.FirstOrDefault(m => m.UserName == consumerName);

            if (consumer != null)
            {
                List<Message> messages = context.Messages
                    .Where(m => m.ConsumerId == consumer.Id && m.IsReceived == false).ToList();

                foreach (Message message in messages)
                {
                    User? autor = context.Users.FirstOrDefault(m => m.Id == message.AuthorId);
                    string tcpMessage = new TcpMessage
                    {
                        Id = message.Id,
                        SenderName = autor?.UserName,
                        ConsumerName = consumer.UserName,
                        Text = message.Content,
                        Status = Command.Message
                    }.MessageToJson();
                    await writer.WriteLineAsync(tcpMessage);
                    await writer.FlushAsync();
                }
            }
        }

        public void Dispose()
        {
            if (_listener != null)
            {
                _listener.Stop();
                _listener.Server?.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
