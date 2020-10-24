using Hl7.Entities;
using Hl7.Helpers;
using Xunit;

namespace Hl7_DotNetCoreTest
{
    public class GettersAndSetters
    {

        [Fact]
        public void GetSegment()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12);
            var result = mess.TryGetValue("DG1");
            Assert.True(result);
            Assert.Equal("DG1|1||I48.91^Unspecified Atrial Fibrillation|||W", result.Value);
        }

        [Fact]
        public void GetSegmentByIndex()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12);
            var result = mess.TryGetValue("11");
            Assert.True(result, result.Error);
            Assert.Equal(@"NTE|2||Datetime\R\\R\20190328142231-0600", result.Value);
        }


        [Fact]
        public void SetSegment()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12);
            var messResult = mess.TrySetValue("DG1", "DG1|123");
            Assert.True(messResult);
            var result = messResult.Value.TryGetValue("DG1");
            Assert.True(result);
            Assert.Equal("DG1|123", result.Value);
        }

        [Fact]
        public void GetNumberedSegment()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12);
            var result = mess.TryGetValue("NTE-3");
            Assert.True(result);
            Assert.Equal("NTE|3||Comment\\R\\\\R\\", result.Value);
        }

        [Fact]
        public void SetNumberedSegment()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12);
            var messResult = mess.TrySetValue("NTE-3", "NTE|3||Comments\\R\\\\R\\");
            Assert.True(messResult);
            var result = messResult.Value.TryGetValue("NTE-3");
            Assert.True(result);
            Assert.Equal("NTE|3||Comments\\R\\\\R\\", result.Value);
        }

        [Fact]
        public void GetField()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12);
            var result = mess.TryGetValue("DG1.1");
            Assert.True(result);
            Assert.Equal("1", result.Value);
        }

        [Fact]
        public void SetField()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12);
            var messResult = mess.TrySetValue("DG1.1", "123");//fields are 1 based indexes.
            Assert.True(messResult);
            var result = messResult.Value.TryGetValue("DG1.1");
            Assert.True(result);
            Assert.Equal("123", result.Value);
        }

        [Fact]
        public void GetComponent()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12);
            var result = mess.TryGetValue("RF1.1.1");// make sure just the one value works too 
            Assert.True(result);
            Assert.Equal("NW", result.Value);
        }

        [Fact]
        public void GetComponentLookAhead()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12);
            var result = mess.TryGetValue("PID.3-[5:NI].1");
            Assert.True(result);
            Assert.Equal("1012866997V751520", result.Value);
        }

        [Fact]
        public void GetLookAhead()
        {
            string ref14 = Helpers.GetTestFileContent("ref14.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref14);
            var result = mess.TryGetValue("PRD-PP.2.1");
            Assert.True(result);
            Assert.Equal("PROVIDER", result.Value);
        }

        [Fact]
        public void SetComponentLookAhead()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12);
            var result = mess.TryGetValue("PID.3-[5:NI].1");
            Assert.True(result);
            Assert.Equal("1012866997V751520", result.Value);
            var messResult = mess.TrySetValue("PID.3-[5:NI].1", "123V123");//fields are 1 based indexes.
            Assert.True(messResult);
            result = messResult.Value.TryGetValue("PID.3-[5:NI].1");
            Assert.True(result);
            Assert.Equal("123V123", result.Value);
        }

        [Fact]
        public void SetComponent()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12);
            var messResult = mess.TrySetValue("RF1.1.1", "SW");//fields are 1 based indexes.
            Assert.True(messResult);
            var valueResult = messResult.Value.TryGetValue("RF1.1");// get the whole field to compare
            Assert.True(valueResult);
            Assert.Equal("SW^CPRS RELEASED ORDER", valueResult.Value);
            valueResult = messResult.Value.TryGetValue("RF1.1.1");
            Assert.True(valueResult);// make sure just the one value works too
            Assert.Equal("SW", valueResult.Value);
        }

        [Fact]
        public void GetSubComponent()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12);
            var result = mess.TryGetValue("PID.3-1.6.2");
            Assert.True(result);
            Assert.Equal("200M", result.Value);
        }

        [Fact]
        public void SetSubComponent()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12);
            var messResult = mess.TrySetValue("PID.3-1.6.2", "500M");//fields are 1 based indexes.
            Assert.True(messResult);
            var result = messResult.Value.TryGetValue("PID.3-1");
            Assert.True(result);// get the whole field to compare
            Assert.Equal("1012866997V751520^^^USVHA&&0363^NI^VA FACILITY ID&500M&L", result.Value);
            result = messResult.Value.TryGetValue( "PID.3-1.6");// make sure the component looks good
            Assert.True(result);
            Assert.Equal("VA FACILITY ID&500M&L", result.Value);
            result = messResult.Value.TryGetValue("PID.3-1.6.2");// validate the sub component
            Assert.True(result);
            Assert.Equal("500M", result.Value);
        }
    }
}
