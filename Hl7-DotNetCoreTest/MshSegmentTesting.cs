using Hl7.Entities;
using System;
using Xunit;
using static Hl7.Helpers.MshMessageHelper;

namespace Hl7_DotNetCoreTest
{
    public class MshSegmentTesting
    {
        [Fact]
        public void ValidateMshPropertyGetters()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12);
            Assert.Equal(@"|^~\&", mess.GetDelimiters());
            Assert.Equal("GMRC CCRA SEND", mess.GetSendingApplication());
            Assert.Equal("534", mess.GetSendingFacility());
            Assert.Equal("GMRC CCRA RECEIVE", mess.GetReceivingApplication());
            Assert.Equal("200", mess.GetReceivingFacility());
            //Note, adding 6 hours to the expected value below (the current offset in the file) and using UTC to support running this in any locale
            Assert.Equal(new DateTime(2019, 3, 28, 20, 22, 31, DateTimeKind.Utc), mess.GetMessageDateTime()!.Value.ToUniversalTime());//20190328142231-0600
            Assert.Equal(string.Empty, mess.GetSecurity());
            Assert.Equal("REF^I12", mess.GetMessageType());
            Assert.Equal("5001181024", mess.GetMessageControlId());
            Assert.Equal("T", mess.GetProcessingId());
            Assert.Equal("2.5", mess.GetVersionId());
            Assert.Equal(string.Empty, mess.GetSequenceNumber());
            Assert.Equal(string.Empty, mess.GetContinuationPointer());
            Assert.Equal("AL", mess.GetAcceptAckType());
            Assert.Equal("AL", mess.GetAppAckType());
            Assert.Equal("US", mess.GetCountryCode());//Note that this is a 2 character field, even thought the value in the file is 3 characters
            Assert.Equal(string.Empty, mess.GetCharacterSet());
            Assert.Equal(string.Empty, mess.GetPrincipalLanguage());
        }
    }
}
