using S5TcpChat.Services;
using System.Net;

namespace S5TcpChat
{
    class Program
    {
        public static void Main(string[] args)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55555);
            ChatServer tcpServer = new ChatServer(endPoint);

            AutoResetEvent starter = new AutoResetEvent(true);
            RegisteredWaitHandle handle = ThreadPool.RegisterWaitForSingleObject(starter, tcpServer.Run, null, Timeout.Infinite, true);

            Console.WriteLine("Сервер\n\nQ - остановка сервера\n" + new string('-', 21));

            while (true)
            {
                string operation = Console.ReadKey(true).KeyChar.ToString().ToUpper();

                if (operation == "Q" || operation == "Й")
                {
                    handle.Unregister(starter);
                    break;
                }
            }
        }
    }
}
