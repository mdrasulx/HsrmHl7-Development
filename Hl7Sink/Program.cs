using Microsoft.Extensions.CommandLineUtils;
using System;
using System.IO;

namespace Hl7Sink
{
    class Program
    {
        static int Main(string[] args)
        {
            int res = 0;
            var app = new CommandLineApplication() { Name = "Hl7Sink" };
            app.HelpOption("-?|-h|--help");
            app.OnExecute(() =>
            {
                Console.WriteLine("Please specify arguments, use -h for help");
                return res;
            });

            app.Command("StartSink", (command) =>
            {
                command.Description = "Wait for TCP connections at specified port";
                command.HelpOption("-?|-h|--help");

                //var server = command.Argument("server", "The server to push the Hl7 message.", false);
                var port = command.Argument("port", "The port to receive messages upon.", false);

                command.OnExecute(() =>
                {
                    res = OpenSocket("localhost", port.Value, Console.Out);
#if DEBUG
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(true);
#endif
                    return res;
                });
            });
            app.Execute(args);
            return res;
        }

        private static int OpenSocket(string server, string portString, TextWriter? outputStream = null)
        {
            if (!int.TryParse(portString, out int port))
                throw new Exception($"invalid port {portString}");
            AsynchronousSocketListener.StartListening(server, port, output:outputStream);
            return 0;
        }
    }
}
