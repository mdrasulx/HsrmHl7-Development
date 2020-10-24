using Hl7.Entities;
using Xunit;

namespace Hl7_DotNetCoreTest
{
    public class FileAndSerializeAreEqual
    {
        [Fact]
        public void FileReadIZProcessResultsAreTheSame()
        {
            string iz = Helpers.GetTestFileContent("IZ.txt");//note that IZ has linux line endings instead of windows
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(iz);
            string serialized = Hl7.Helpers.MessageHelper.SerializeMessage(mess);
            Xunit.Assert.Equal(iz.Trim(), serialized.Trim());
        }
        [Fact]
        public void FileReadRef14ProcessResultsAreTheSame()
        {
            string iz = Helpers.GetTestFileContent("ref14.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(iz);
            string serialized = Hl7.Helpers.MessageHelper.SerializeMessage(mess);
            Xunit.Assert.Equal(iz.Trim(), serialized.Trim());
        }
        [Fact]
        public void FileReadRef12AndProcessResultsAreTheSame()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12);
            string serialized = Hl7.Helpers.MessageHelper.SerializeMessage(mess);
            Xunit.Assert.Equal(ref12.Trim(), serialized.Trim());
        }
        [Fact]
        public void FileReadRefI2AndProcessResultsAreTheSame()
        {
            string refI13 = Helpers.GetTestFileContent("refI13.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(refI13);
            string serialized = Hl7.Helpers.MessageHelper.SerializeMessage(mess);
            Xunit.Assert.Equal(refI13.Trim(), serialized.Trim());
        }
        [Fact]
        public void FileReadSius12AndProcessResultsAreTheSame()
        {
            string siUs12 = Helpers.GetTestFileContent("SIUS12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(siUs12);
            string serialized = Hl7.Helpers.MessageHelper.SerializeMessage(mess);
            Xunit.Assert.Equal(siUs12.Trim(), serialized.Trim());
        }
        [Fact]
        public void SerializeWithValidationCheck()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12,validateOutMatchesIn:true);//note that the validateOutMatchesIn ensures that the raw message matches the serialization, so the validaiton below is redundant
            string serialized = Hl7.Helpers.MessageHelper.SerializeMessage(mess);
            Xunit.Assert.Equal(ref12.Trim(), serialized.Trim());
        }
        [Fact]
        public void FileReadCernerI12ProcessResultsAreTheSame()
        {
            string cI12 = Helpers.GetTestFileContent("CernerI12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(cI12);
            string serialized = Hl7.Helpers.MessageHelper.SerializeMessage(mess);
            Xunit.Assert.Equal(cI12.Trim(), serialized.Trim());
        }
        [Fact]
        public void FileReadCernerI13ProcessResultsAreTheSame()
        {
            string cI13 = Helpers.GetTestFileContent("CernerI13.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(cI13);
            string serialized = Hl7.Helpers.MessageHelper.SerializeMessage(mess);
            Xunit.Assert.Equal(cI13.Trim(), serialized.Trim());
        }
        [Fact]
        public void FileReadCernerI14ProcessResultsAreTheSame()
        {
            string cI14 = Helpers.GetTestFileContent("CernerI14.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(cI14);
            string serialized = Hl7.Helpers.MessageHelper.SerializeMessage(mess);
            Xunit.Assert.Equal(cI14.Trim(), serialized.Trim());
        }

    }
}
