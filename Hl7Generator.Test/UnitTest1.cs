using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;
using static Hl7.Helpers.MshMessageHelper;
using static Hl7Generator.Test.Helpers;

namespace Hl7Generator.Test
{
    public class GeneratorConsoleTests
    {
        [Fact]
        public void ParseOnePair()
        {
            string[] args = { "MST.1.1* 123" };
            bool result = Hl7Generator.Program.TryParseArguments(args,  out var arguments, out _);
            Assert.True(result);
            Assert.Equal("MST.1.1", arguments[0].Target);
            Assert.Equal("123", arguments[0].Update);
        }

        [Fact]
        public void ParseThreePairs()
        {
            string[] args = {"MST.1.1* 123", "IN1.1* awdawd", "IN2.1-1.1.1* something" };
            bool result = Hl7Generator.Program.TryParseArguments(args, out var updates, out _);
            Assert.True(result);
            Assert.Equal(3, updates.Count);
            Assert.Equal("MST.1.1", updates[0].Target);
            Assert.Equal("123", updates[0].Update);
            Assert.Equal("IN1.1", updates[1].Target);
            Assert.Equal("awdawd", updates[1].Update);
            Assert.Equal("IN2.1-1.1.1", updates[2].Target);
            Assert.Equal("something", updates[2].Update);
        }

        [Fact]
        public void ParseFile()
        {
            string path = Helpers.GetTestFilePath("ref12.txt");
            Assert.True(Hl7Generator.Program.TryGetMessageFromFile(path, out _, out string? error), error);
        }

        [Fact]
        public void UpdateFile()
        {
            //Validate old value isn't 500M
            string content = Helpers.GetTestFileContent("ref12.txt");
            var messIn = Hl7.Helpers.MessageHelper.ParseMessage(content);
            var res = Hl7.Helpers.MessageHelper.TryGetValue(messIn, "PID.3-1.6.2");
            Assert.True(res);
            Assert.NotEqual("500M", res.Value);
            //change the value
            string path = Helpers.GetTestFilePath("ref12.txt");
            string[] args = { "PID.3-1.6.2* 500M" };
            Assert.True(Hl7Generator.Program.TryParseArguments(args, out var updates, out _));
            Assert.True(Hl7Generator.Program.TryProcessFileUpdates(path, updates, out var message, out _));
            //assert the change
            Assert.NotNull(message);
            res = Hl7.Helpers.MessageHelper.TryGetValue(message!, "PID.3-1.6.2");
            Assert.True(res);
            Assert.Equal("500M", res.Value);
        }

        [Fact]
        public void ValidatePrepareProcess()
        {
            var result = Program.TryPrepareMessageToSend("test", "123", Helpers.GetTestFilePath("ref14.txt"), 
                new List<string> { "MSH.2* Test App" }, "5", true, true, true, "newIcn", "500_123456","test,guy","stupid Seoc","Q123.2^bad diagnosis"
                ,"lastname,firstname,druz,npi", "lastname2,firstname2,druz2,npi2", "other,guy","division");
            Assert.True(result,result.Error);
            Assert.NotNull(result);
            Assert.NotNull(result.Value.Message);
            Assert.Equal("Test App", result.Value.Message.GetSendingApplication());
            var messDateTime = result.Value.Message.GetMessageDateTime();
            Assert.NotNull(messDateTime);
            ValidateTimeValueIsRecent(messDateTime!.Value);
            Assert.Equal(123, result.Value.Port);
            Assert.Equal(5, result.Value.WaitTime);
            var result2 = Hl7.Helpers.MessageHelper.TryGetValue(result.Value.Message, "PID.3-[5:NI].1");
            Assert.True(result2, result2.Error);
            Assert.Equal("newIcn", result2.Value);
            result2 = Hl7.Helpers.MessageHelper.TryGetValue(result.Value.Message, "RF1.6");
            Assert.True(result2, result2.Error);
            Assert.Equal("500_123456", result2.Value);
            ValidateTimeValueIsRecent(result.Value.Message, "RF1.7");
            result2 = Hl7.Helpers.MessageHelper.TryGetValue(result.Value.Message, "PID.5-1.1");
            Assert.True(result2, result2.Error);
            Assert.Equal("test", result2.Value);
            result2 = Hl7.Helpers.MessageHelper.TryGetValue(result.Value.Message, "PID.5-1.2");
            Assert.True(result2, result2.Error);
            Assert.NotNull(result2.Value);
            Assert.Equal("guy", result2.Value);
            result2 = Hl7.Helpers.MessageHelper.TryGetValue(result.Value.Message, "NTE-2.3");
            Assert.True(result2, result2.Error);
            ValidateTimeValueIsRecent(result2.Value!.Split('\\').Last());//NTE-2.3 has some wierd stuff in front of the date, but it ends with a \, so split on that and grab the last piece
        }

        private static void ValidateTimeValueIsRecent(Hl7.Entities.Message message, string target)
        {
            var result = Hl7.Helpers.MessageHelper.TryGetValue(message, target);
            Assert.True(result);
            Assert.NotNull(result.Value);
            ValidateTimeValueIsRecent(result.Value!);
        }

        private static void ValidateTimeValueIsRecent(string dateTimeString) =>
            ValidateTimeValueIsRecent(DateTime.ParseExact(dateTimeString, "yyyyMMddHHmmsszzz", CultureInfo.InvariantCulture));

        private static void ValidateTimeValueIsRecent(DateTime datetime) =>
            Assert.True((DateTime.Now - datetime) - new TimeSpan(0, 0, 2) < TimeSpan.Zero);


        //NOTE: when working with the MessageSender, be sure to give your "server" a different name for each test to avoid impacts to the memory stream being disposed in one test and used in another.
        [Fact]
        public void SendMessageToStream()
        {
            using MemoryStream ms = new MemoryStream();
            SendMessage("SendMessageToStream", "Do Something", ms);
            Assert.Equal(WrapMessage("Do Something"), Encoding.ASCII.GetString(ms.ToArray()));
        }


        [Fact]
        public void SendMessagesToStream()
        {
            string workingStream = string.Empty;
            using MemoryStream ms = new MemoryStream();
            for (int i = 0; i <= 3; i++)
            {
                string toSend = $"Do Something {i}";
                workingStream += WrapMessage(toSend);
                SendMessage("SendMessagesToStream", toSend, ms, new TimeSpan(50000));
                Assert.Equal(workingStream, Encoding.ASCII.GetString(ms.ToArray()));
            }
        }

        [Fact]
        public void SendMessagesToStreamThreaded()
        {
            using MemoryStream ms = new MemoryStream();
            List<string> strings = new List<string>();
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i <= 2; i++)
            {
                string toSend = $"Do Something {i}";
                Thread t = new Thread(new ThreadStart(() => SendMessage("SendMessagesToStreamThreaded", toSend, ms, new TimeSpan(0, 0, 0, 0, 500))));
                threads.Add(t);
                strings.Add(WrapMessage(toSend));
                t.Start();
            }
            threads.ForEach(t => t.Join());
            string result = Encoding.ASCII.GetString(ms.ToArray());
            strings.ForEach(s => Assert.Contains(s, result));//results aren't deterministic, so just ensure all 3 are in the results.
        }

        private static void SendMessage(string testId, string message, MemoryStream stream, TimeSpan? waitTime = null)
        {
            Assert.True(Hl7Generator.MessageSender.SendMessage(testId, 123, message, out string? response, out string? error, (s, p) => stream, Encoding.ASCII, waitTime));
            Assert.Equal("", response);
        }
    }
}
