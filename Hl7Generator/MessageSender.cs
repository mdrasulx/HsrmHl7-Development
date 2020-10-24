using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Hl7Generator
{
    public static class MessageSender
    {
        //< ASCII VERTICAL TAB - $C(11) / 0X0B >
        private const byte VERTICAL_TAB = (Byte)11;
        private readonly static byte[] START_MESSAGE = new byte[] { VERTICAL_TAB };
        private readonly static string START_MESSAGE_STRING = Encoding.ASCII.GetString(START_MESSAGE);
        //HL7 MESSAGE
        //< ASCII FILE SEPARATOR - $C(28) / 0X1C >
        private const byte FILE_SEPARATOR = (Byte)28;
        // < ASCII CARRIAGE RETURN - $C(13) / 0X0D >
        private const byte CARRIAGE_RETURN = (Byte)13;
        private readonly static byte[] END_MESSAGE = new byte[] { FILE_SEPARATOR, CARRIAGE_RETURN };
        private readonly static string END_MESSAGE_STRING = Encoding.ASCII.GetString(END_MESSAGE);


        private readonly static ConcurrentDictionary<(string Server, int Port), Lazy<(AutoResetEvent ResetEvent, Stream Stream)>> Clients
            = new ConcurrentDictionary<(string, int), Lazy<(AutoResetEvent, Stream)>>();

        private static int IsDeconstructingBackValue = 0;
        private static bool IsDeconstructing
        {
            get { return (Interlocked.CompareExchange(ref IsDeconstructingBackValue, 1, 1) == 1); }
            set
            {
                if (value) Interlocked.CompareExchange(ref IsDeconstructingBackValue, 1, 0);
                else Interlocked.CompareExchange(ref IsDeconstructingBackValue, 0, 1);
            }
        }

        static MessageSender()
        {
            AppDomain.CurrentDomain.ProcessExit += TcpSocket_Dtor;//raise an event to close all the streams
        }

        static void TcpSocket_Dtor(object? sender, EventArgs e)
        {//close all the streams
            IsDeconstructing = true;
            foreach (var key in Clients.Keys)
            {
                var client = Clients[key].Value;
                client.ResetEvent.WaitOne(new TimeSpan(0, 1, 0));//wait up to a minute for someone to be done with this mutex
                client.ResetEvent.Dispose();
                client.Stream.Dispose();
                Clients.Remove(key, out var val);
            }
        }

        public static bool SendMessage(string server, int port, string message, out string? response, out string? error, Func<string, int, Stream>? streamConstructor = null, Encoding? encoding = null, TimeSpan? waitTime = null)
        {
            error = response = null;
            streamConstructor ??= new Func<string, int, Stream>((s, p) => new TcpClient(s, p).GetStream());
            encoding ??= new ASCIIEncoding();
            byte[] bytes = encoding.GetBytes(message);

            var client = Clients.GetOrAdd((server, port), k => new Lazy<(AutoResetEvent, Stream)>(
                 () => (new AutoResetEvent(true), streamConstructor(server, port)), LazyThreadSafetyMode.ExecutionAndPublication));

            if (IsDeconstructing)
            {
                error = "Message not sent, pipe is being destroyed.";
                return false;
            }
            try
            {
                client.Value.ResetEvent.WaitOne();
                client.Value.Stream.Write(START_MESSAGE, 0, START_MESSAGE.Length);
                client.Value.Stream.Write(bytes, 0, bytes.Length);
                client.Value.Stream.Write(END_MESSAGE, 0, END_MESSAGE.Length);
                Thread.Sleep(waitTime ?? TimeSpan.Zero);
                response = GetResponse(client.Value.Stream);
            }
            catch (Exception ex)
            {
                error = $"exception encountered when sending message: {ex}";
                return false;
            }
            finally
            {
                client.Value.ResetEvent.Set();
            }
            return true;
        }

        private static string GetResponse(Stream stream)
        {
            byte[] buffer = new byte[1024];
            string returnValue = string.Empty;

            while (returnValue.IndexOf(END_MESSAGE_STRING) < 0)
            {
                int read = stream.Read(buffer, 0, 1024);
                returnValue += Encoding.ASCII.GetString(buffer, 0, read);
                if (read < 1024)//possible?
                    break;
            }
            return RemoveWrapperString(returnValue);
        }

        private static string RemoveWrapperString(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                message = message.Remove(0, START_MESSAGE_STRING.Length);
                message = message.Remove(message.Length - END_MESSAGE_STRING.Length, END_MESSAGE_STRING.Length);
            }
            return message;
        }
    }
}
