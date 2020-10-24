using Hl7.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Hl7Sink
{
    public class AsynchronousSocketListener
    {
        // State object for reading client data asynchronously  
        internal class StateObject
        {
            public SocketAndStream WorkSocket;// Client socket.
            public byte[] Buffer = new byte[1024];// Receive buffer.
            public StringBuilder ReceivedData = new StringBuilder();// Received data string.

            public StateObject(SocketAndStream workSocket) => WorkSocket = workSocket;
        }

        internal class SocketAndStream
        {
            public Socket Socket { get; set; }
            public TextWriter Output { get; set; }

            public SocketAndStream(Socket socket, TextWriter output) => (Socket, Output) = (socket, output);
        }

        private const byte VERTICAL_TAB = (Byte)11;//< ASCII VERTICAL TAB - $C(11) / 0X0B >
        private const byte FILE_SEPARATOR = (Byte)28;//< ASCII FILE SEPARATOR - $C(28) / 0X1C >
        private const byte CARRIAGE_RETURN = (Byte)13;// < ASCII CARRIAGE RETURN - $C(13) / 0X0D >
        //HL7 Message structure
        private readonly static string START_MESSAGE = Encoding.ASCII.GetString(new byte[] { VERTICAL_TAB });
        private readonly static string END_MESSAGE = Encoding.ASCII.GetString(new byte[] { FILE_SEPARATOR, CARRIAGE_RETURN });

        private readonly static ManualResetEvent AllDone = new ManualResetEvent(false);// Thread signal.  

        public static void StartListening(string server, int port, AddressFamily IpAddressType = AddressFamily.InterNetworkV6, TextWriter? output = null)
        {
            output ??= Console.Out;
            var ip = Dns.GetHostAddresses(server).FirstOrDefault(p => p.AddressFamily == IpAddressType);
            using (Socket socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                var listener = new SocketAndStream(socket, output);
                try
                {// Bind the socket and listen for incoming connections
                    listener.Socket.Bind(new IPEndPoint(ip, port));
                    listener.Socket.Listen(100);
                    while (true)
                    {
                        AllDone.Reset();// Set the event to nonsignaled state.  
                        output.WriteLine("Waiting for a connection...");
                        listener.Socket.BeginAccept(new AsyncCallback(AcceptCallback), listener);// Start an asynchronous socket to listen for connections
                        AllDone.WaitOne();// Wait until a connection is made before continuing
                    }
                }
                catch (Exception e)
                {
                    output.WriteLine(e.ToString());
                }
            }
            output.WriteLine("Exiting");
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            AllDone.Set();// Signal the main thread to continue.
            SocketAndStream listener = (SocketAndStream)ar.AsyncState!;// Get the socket that handles the client request.  
            StateObject state = new StateObject(new SocketAndStream(listener.Socket.EndAccept(ar), listener.Output));
            state.WorkSocket.Socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState!;
            int bytesRead = state.WorkSocket.Socket.EndReceive(ar);
            if (bytesRead > 0)
            {
                state.ReceivedData.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));
                String content = state.ReceivedData.ToString();
                if (content.IndexOf(END_MESSAGE) > -1)
                {//end of message received 
                    state.WorkSocket.Output.WriteLine($"Read {content.Length} bytes from socket. Data:{Environment.NewLine}{content}");
                    Respond(state, content);
                }
                else
                {// Not all data received. Get more.  
                    state.WorkSocket.Socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private static string RemoveWrapper(string message)
        {
            message = message.Remove(0, START_MESSAGE.Length);
            message = message.Remove(message.Length - END_MESSAGE.Length, END_MESSAGE.Length);
            return message;
        }

        private static void Respond(StateObject state, string content)
        {
            var message = MessageHelper.ParseMessage(RemoveWrapper(content));
            if (!MessageHelper.GetAck(message).DecomposeResult(out var resp, out string? error))
                throw new Exception(error ?? "unknown error getting ACK");
            byte[] byteData = Encoding.ASCII.GetBytes($"{START_MESSAGE}{resp.SerializeMessage()}{END_MESSAGE}");
            state.WorkSocket.Socket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), state.WorkSocket);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            if (ar.AsyncState == null)
                throw new Exception("Null Sync State");
            SocketAndStream handler = (SocketAndStream)ar.AsyncState;
            try
            {
                int bytesSent = handler.Socket.EndSend(ar);
                handler.Output.WriteLine($"Sent {bytesSent} bytes to client.");
                handler.Socket.Shutdown(SocketShutdown.Both);
                handler.Socket.Close();
            }
            catch (Exception e)
            {
                handler.Output.WriteLine(e.ToString());
            }
        }
    }
}