using Hl7.Entities;
using Hl7.Helpers;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;

namespace Hl7Generator
{
    internal class Program
    {
        private static readonly char Splitter = '*';
        static int Main(string[] args)
        {
            int res = 0;
            var app = new CommandLineApplication() { Name = "Hl7Generator" };
            app.HelpOption("-?|-h|--help");
            app.OnExecute(() =>
            {
                Console.WriteLine("Please specify arguments, use -h for help");
                return res;
            });

            app.Command("Process", (command) =>
            {
                command.Description = "Send a specified HL7 message, optionally updating content before sending";
                command.HelpOption("-?|-h|--help");

                var server = command.Argument("server", "The server to which to push the Hl7 message.", false);
                var port = command.Argument("port", "The port on the server to which to push the message.", false);
                var filePath = command.Argument("filePath", "The file location containing the HL7 message to send.", false);
                var SetConsultId = command.Option("-c|--ConsultId <ConsultID>", "Set the consult ID / UCID for the referral to the specified value", CommandOptionType.SingleValue);
                var diagnosis = command.Option("-d|--Diagnosis <Diagnosis>", "Set the consult diagnosis to the specified value, expected in \"ICD-10^Title\" format", CommandOptionType.SingleValue);
                var division = command.Option("-di|--Division <Division>", "Set the Division / location of the consult to the specified value", CommandOptionType.SingleValue);
                var veteranIcn = command.Option("-i|--Icn <VeteranICN>", "Set the ICN of the veteran to the specified value", CommandOptionType.SingleValue);
                var InsuredName = command.Option("-in|--InsuredName <Name>", "Set the name of the primarty insured to the specified value, \"lastname,firstname\" format", CommandOptionType.SingleValue);
                var veteranName = command.Option("-n|--Name <VeteranName>", "Set the name of the veteran to the specified value, \"lastname,firstname\" format", CommandOptionType.SingleValue);
                var setNteTime = command.Option("-nt|--SetNteTime", "Set the send time in NTE-2 to now", CommandOptionType.NoValue);
                var primaryProvider = command.Option("-pp|--PrimaryProvider <ProviderData>", "Set the primary provider to the specified value in format \"lastname,firstname,duz,npi\"", CommandOptionType.SingleValue);
                var referringProvider = command.Option("-rp|--ReferringProvider <ProviderData>", "Set the referring provider to the specified value in format \"lastname,firstname,duz,npi\"", CommandOptionType.SingleValue);
                var setReferralTime = command.Option("-rt|--SetReferralTime", "Set the send time of the referral to now", CommandOptionType.NoValue);
                var setSendTime = command.Option("-st|--SetSendTime", "Set the send time in the header to now", CommandOptionType.NoValue);
                var setSeoc = command.Option("-se|--Seoc <Seoc>", "Set the SEOC to the specified value ", CommandOptionType.SingleValue);
                var updates = command.Option("-u|--Update <Updates>",
                    $"Changes to make to the message to be sent in \"target{Splitter}value\" format e.g. \"MST.1.1.1{Splitter} 123\" where MST.1.1.1 is the location of the value to change, and 123 is the value to which to change it.",
                    CommandOptionType.MultipleValue);
                var verbose = command.Option("-v|--verbose", "provide verbose output", CommandOptionType.NoValue);
                var waitTime = command.Option("-w|--waitTime <waittime>", "The amount of time in seconds to wait for a response", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    res = ExecuteProcess(server.Value, port.Value, filePath.Value, updates.Values, verbose.HasValue(), waitTime.Value(), setSendTime.HasValue(), setNteTime.HasValue(),
                        setReferralTime.HasValue(), veteranIcn.Value(), SetConsultId.Value(), veteranName.Value(), setSeoc.Value(), diagnosis.Value(), primaryProvider.Value(), referringProvider.Value(),
                        InsuredName.Value(), division.Value());
#if DEBUG
                    //Console.WriteLine("Press any key to continue...");
                    //Console.ReadKey(true);
#endif
                    return res;
                });
            });
            app.Execute(args);
            return res;
        }

        internal static int ExecuteProcess(string server, string portString, string filePath, List<string> updatesRaw, bool verbose, string waitTimeString, bool setSendTime, bool setReferralTime, bool setNteTime,
            string vetIcn, string consultId, string name, string seoc, string diagnosis, string primaryProvider, string referringProvider, string insuredName, string division)
        {
            Console.WriteLine("validating inputs, and preparing the message");
            var result = TryPrepareMessageToSend(server, portString, filePath, updatesRaw, waitTimeString, setSendTime, setReferralTime, setNteTime, vetIcn, consultId, name, seoc, diagnosis,
                primaryProvider, referringProvider, insuredName, division);
            if (!result.Success)
                return ErrorOutput(result.Error ?? "unknown error attempting to validate message and process input variables", -1);
            Console.WriteLine("Validation and prep successful");
            string serializedMessage = result.Value.Message.SerializeMessage();
            if (verbose)
                Console.WriteLine($"HL7 to be sent: {Environment.NewLine}{serializedMessage}");
            Console.WriteLine($"Sending message to server {server}:{result.Value.Port}");
            if (!MessageSender.SendMessage(server, result.Value.Port, serializedMessage, out string? response, out string? error, waitTime: new TimeSpan(0, 0, result.Value.WaitTime)))
                return ErrorOutput(error ?? "unknown error sending message", -4);
            var responseMessage = MessageHelper.TryParseMessage(response);
            if (!responseMessage)
                return ErrorOutput(responseMessage.Error ?? "unknown error parsing response", -5);
            //TODO validate the response?
            //output the reponse to a file optionally?
            Console.WriteLine("Processed requested message, response:");
            Console.WriteLine(responseMessage.Value.SerializeMessage());
            return 0;
        }

        internal static Result<(Message Message, int WaitTime, int Port)> TryPrepareMessageToSend(string server, string portString, string filePath, List<string> updatesRaw, string waitTimeString, bool setSendTime, bool setReferralTime, bool setNteTime,
            string vetIcn, string consultId, string name, string seoc, string diagnosis, string primaryProvider, string referringProvider, string insuredName, string division)
        {
            if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(portString) || string.IsNullOrWhiteSpace(filePath))
                return ErrorReturn<(Message, int, int)>("Server, port and filepath are required");
            if (!int.TryParse(portString, out int port))
                return ErrorReturn<(Message, int, int)>("Specified port was not parsable as an integer");
            if (!int.TryParse(waitTimeString, out int waitTime))
                return ErrorReturn<(Message, int, int)>("specified wait time was not parsable as an integer");
            if (!TryParseArguments(updatesRaw, out List<(string Target, string Update)> updates, out string? error))
                return ErrorReturn<(Message, int, int)>(error ?? "unknown error with parsing arguments");
            if (setReferralTime)
                updates.Add(("RF1.7", NowAsString().Replace(":", "")));
            if (setSendTime)
                updates.Add(("MSH.6", NowAsString().Replace(":", "")));
            if (setNteTime)
                updates.Add(("NTE-2.3", $"Datetime\\R\\\\R\\{NowAsString().Replace(":","")}"));
            if (!string.IsNullOrWhiteSpace(vetIcn))
            {
                updates.Add(("PID.2", vetIcn));
                updates.Add(("PID.3-[5:NI].1", vetIcn));
            }
            if (!string.IsNullOrWhiteSpace(consultId))
                updates.Add(("RF1.6", consultId));
            if (!string.IsNullOrWhiteSpace(name))
            {
                string[] nameSplit = name.Split(',');
                if (nameSplit.Length != 2)
                    return ErrorReturn<(Message, int, int)>($"Name was specified, but not in lastname,firstname format, found: {name}");
                updates.Add(("PID.5-1.1", nameSplit[0].Trim()));
                updates.Add(("PID.5-1.2", nameSplit[1].Trim()));
            }
            if (!string.IsNullOrWhiteSpace(seoc))
                updates.Add(("NTE-8.3", $"SEOC ID: {seoc}"));
            if (!string.IsNullOrWhiteSpace(diagnosis))
                updates.Add(("DG1.3", diagnosis));
            if (!string.IsNullOrWhiteSpace(insuredName))
                updates.Add(("IN1.16", $"{insuredName}"));
            if (!string.IsNullOrWhiteSpace(primaryProvider))
            {
                if (!TrySetProvider("PP", primaryProvider, out var pUpdates, out error))
                    return ErrorReturn<(Message, int, int)>(error ?? "unknown error adding primary provider");
                updates.AddRange(pUpdates);
            }
            if (!string.IsNullOrWhiteSpace(referringProvider))
            {
                if (!TrySetProvider("RP", referringProvider, out var pUpdates, out error))
                    return ErrorReturn<(Message, int, int)>(error ?? "unknown error adding referring provider");
                updates.AddRange(pUpdates);
            }
            if (!string.IsNullOrWhiteSpace(division))
                updates.Add(("PV1.3.4.2", division));
            if (updates.Count > 0)
            {
                updates = updates.ConvertAll(u => (u.Target, u.Update.Replace("**CURRENT_DATE_TIME**", NowAsString())));
                updates.ForEach(update => Console.WriteLine($"key:{update.Target}, value:{update.Update}"));
            }
            if (!TryProcessFileUpdates(filePath, updates, out Message? message, out error))
                return ErrorReturn<(Message, int, int)>(error ?? "unknown error with updating");
            if (message == null)
                return ErrorReturn<(Message, int, int)>(error ?? "updating resulted in null message");
            return new Result<(Message, int, int)>((message, waitTime, port));
        }

        private static bool TrySetProvider(string providerType, string providerValues, out List<(string Target, string Update)> updates, out string? error)
        {
            error = null;
            updates = new List<(string, string)>();
            var values = providerValues.Split(',');
            if (values.Length != 4)
                ErrorReturn($"exactly 4 comma seperated values expected for a provider value, but found {values.Length} on {providerValues} for provider type {providerType}", out error);
            updates.Add(($"PRD-{providerType}.2.1", values[0].Trim()));//last name
            updates.Add(($"PRD-{providerType}.2.2", values[1].Trim()));//first name
            updates.Add(($"PRD-{providerType}.2.9", values[2].Trim()));//duz
            updates.Add(($"PRD-{providerType}.7", values[3].Trim()));//NPI
            return true;
        }

        private static string FormatDateTime(DateTime dt) => dt.ToString("yyyyMMddHHmmsszzz");
        private static string NowAsString() => FormatDateTime(DateTime.Now);

        internal static bool TryProcessFileUpdates(string fileName, IEnumerable<(string, string)> updates, out Message? message, out string? error)
        {
            if (!TryGetMessageFromFile(fileName, out message, out error))
                return ErrorReturn(error ?? "unknown error getting and parsing file content", out error);
            if (message == null)
                return ErrorReturn("unknown error with null message found", out error);
            if (!MessageHelper.TrySetValues(message, updates).DecomposeResult(out message, out error))
                return ErrorReturn(error ?? "unknown error setting value", out error);
            return true;
        }

        internal static bool TryGetMessageFromFile(string fileName, out Message? message, out string? error)
        {
            message = null;
            if (!TryGetFileContent(fileName, out string? content, out error))
                return ErrorReturn(error ?? "unknown error getting file content", out error);
            var result = MessageHelper.TryParseMessage(content);
            if (!result)
                return ErrorReturn(error ?? "unknown error parsing file content", out error);
            message = result.Value;
            return true;
        }

        internal static bool TryGetFileContent(string fileName, out string? content, out string? error)
        {
            error = null; content = null;
            if (!File.Exists(fileName))
                return ErrorReturn($"No such file: {fileName}", out error);
            content = System.IO.File.ReadAllText(fileName);
            return true;
        }

        internal static bool TryParseArguments(IEnumerable<string> args, out List<(string Target, string Update)> updates, out string? error)
        {
            if (!TryParseArgumentPairs(args, out updates, out error))
                return ErrorReturn(error ?? "unknown error parsing update pairs arguments", out error);
            return true;
        }

        private static bool TryParseArgumentPairs(IEnumerable<string> args, out List<(string Target, string Update)> updates, out string? error)
        {
            error = null;
            updates = new List<(string, string)>();
            foreach (string arg in args)
            {
                if (!TryParseArgumentPair(arg, out var parsedPair, out error))
                    ErrorReturn(error ?? "unknown error parsing an argument pair", out error);
                updates.Add(parsedPair);
            }
            return true;
        }

        private static bool TryParseArgumentPair(string arg, out (string Target, string Update) result, out string? error)
        {
            error = null;
            string[] args = arg.Split(Splitter);
            if (args.Length != 2)
                ErrorReturn($"Expected exactly two arguments in argument parser, found: {arg}", out error);
            result = (args[0].Trim(), args[1].Trim());
            return true;
        }

        private static int ErrorOutput(string error, int errorId)
        {
            Console.Error.WriteLine(error);
            return errorId;
        }

        internal static Result<T> ErrorReturn<T>(string error) => new Result<T>(error);

        private static bool ErrorReturn(string error, out string errorReturn)
        {
            errorReturn = error;
            return false;
        }
    }
}
