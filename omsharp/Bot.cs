using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace omsharp
{
    class Bot
    {
        string Nick { get; set; }
        string Server { get; set; }
        StreamReader Input { get; set; }
        StreamWriter Output { get; set; }
        List<string> Channels { get; set; }

        private List<Action<string>> Reactions;

        public Bot()
        {
            Reactions = new List<Action<string>>
                            {
                                (line => PingPong(line))
                            };
        }
        static void Main(string[] args)
        {
            if (args.Length < 3)
                throw new ArgumentException("usage: ./bot nick server port [channel+]", "args");

            var argQueue = new Queue<string>(args);
            var nick = argQueue.Dequeue();
            var server = argQueue.Dequeue();
            var port = int.Parse(argQueue.Dequeue());
            var chans = argQueue.ToList();

            var client = new TcpClient(server, port);
            var stream = client.GetStream();
            var buffered = new BufferedStream(stream);
            var input = new StreamReader(buffered, Encoding.UTF8);
            var output = new StreamWriter(buffered, Encoding.UTF8);
            var bot = new Bot
                          {
                              Nick = nick,
                              Server = server,
                              Input = input,
                              Output = output,
                              Channels = chans
                          };

            bot.Run();
        }

        private void Run()
        {
            SendNick();
            SendUserInfo();

            while (true)
            {
                string line = Input.ReadLine();
                ReactTo(line);
            }
        }

        private void ReactTo(string line)
        {
            Console.WriteLine("<- " + line);
            Reactions.ForEach(reaction => reaction.Invoke(line));
        }

        private void PingPong(string line)
        {
            var regex = new Regex("^PING");
            if (regex.IsMatch(line))
                WriteToServer("PONG " + Server);
        }

        private void WriteToServer(string message)
        {
            Console.WriteLine("-> " + message);
            Output.Write(message);
            Output.Flush();
        }
        private void SendNick()
        {
            WriteToServer("NICK " + Nick);
        }
        private void SendUserInfo()
        {
            WriteToServer(string.Format("USER {0} {0} :{0}", Nick));
        }
    }
}
