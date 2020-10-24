using Hl7.Entities;
using Hl7.Helpers;
using Xunit;

namespace Hl7_DotNetCoreTest
{
    public class Segment
    {
        [Fact]
        public void NewSegmentAdded()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12);
            Assert.Equal(10, mess.GetSegmentCollections().Count);
            Hl7.Entities.BaseSegment seg = SegmentHelper.CreateSegment("TST||123|456|789", mess.Encoding);
            var result = mess.AddNewSegment(seg);
            Assert.True(result, result.Error);
            Assert.Equal(11, result.Value.GetSegmentCollections().Count);
            var res = result.Value.TryGetValue("TST.2");
            Assert.True(res, res.Error);
            Assert.Equal("123", res.Value);
        }

        [Fact]
        public void NewSegmentAddedWithCollision()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12);
            Assert.Equal(10, mess.GetSegmentCollections().Count);
            Hl7.Entities.BaseSegment seg = SegmentHelper.CreateSegment("NTE|3||other stuff will move down", mess.Encoding);
            var result = mess.AddNewSegment(seg);
            Assert.True(result, result.Error);
            Assert.Equal(10, result.Value.GetSegmentCollections().Count);//note we are adding to the existing NTE set, so the number remains the same.
            var res = result.Value.TryGetValue("NTE-3");
            Assert.True(res,res.Error);
            Assert.Equal("NTE|3||other stuff will move down", res.Value);
            res = result.Value.TryGetValue("NTE-4");
            Assert.True(res, res.Error);
            Assert.Equal(@"NTE|4||Comment\R\\R\", res.Value);
            res = result.Value.TryGetValue("NTE-5");
            Assert.True(res, res.Error);
            Assert.Equal(@"NTE|5|P|Reason for Request", res.Value);
            res = result.Value.TryGetValue("NTE-6");
            Assert.True(res, res.Error);
            Assert.Equal(@"NTE|6||NEEDS TO VISIT THIS SPECIALIST", res.Value);
        }

        [Fact]
        public void NewSegmentAddedWithCollisionAndSegIndex()
        {
            string iz = Helpers.GetTestFileContent("IZ.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(iz);
            Assert.Equal(12, mess.GetSegmentCollections().Count);
            var res = mess.TryGetValue("OBX:2-8");
            Assert.True(res, res.Error);
            Assert.Equal(@"OBX|8|CE|30956-7^vaccine type^LN|4|45^Hep B, unspecified formulation^CVX||||||F", res.Value);//validate this is there before we move and renumber it
            var newSeg = SegmentHelper.CreateSegment("OBX|8|CE|30956-7^vaccine type^LN|4|45^MOVE IT DOWN^CVX||||||F", mess.Encoding);
            var result = mess.AddNewSegment(newSeg, 12);
            Assert.True(result, result.Error);
            res = result.Value.TryGetValue("OBX:2-8");
            Assert.True(res, res.Error);
            Assert.Equal("OBX|8|CE|30956-7^vaccine type^LN|4|45^MOVE IT DOWN^CVX||||||F", res.Value);
            res = result.Value.TryGetValue("OBX:2-9");
            Assert.True(res, res.Error);
            Assert.Equal(@"OBX|9|CE|30956-7^vaccine type^LN|4|45^Hep B, unspecified formulation^CVX||||||F", res.Value);
        }

        [Fact]
        public void NewSegmentAddedWithSegIndex()
        {
            string iz = Helpers.GetTestFileContent("IZ.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(iz);
            Assert.Equal(12, mess.GetSegmentCollections().Count);
            var newSeg = SegmentHelper.CreateSegment("ORC|RE||IZ-783278^NDA|||||||||57422^new^Thing^^^^^^NDA^L", mess.Encoding);
            var result = mess.AddNewSegment(newSeg, 13);
            Assert.True(result, result.Error);
            var res = result.Value.TryGetValue("ORC:4");
            Assert.True(res, res.Error);
            Assert.Equal("ORC|RE||IZ-783278^NDA|||||||||57422^new^Thing^^^^^^NDA^L", res.Value);
        }

        [Fact]
        public void ExistingSegmentUpdated()
        {
            string ref12 = Helpers.GetTestFileContent("ref12.txt");
            Message mess = Hl7.Helpers.MessageHelper.ParseMessage(ref12);
            Assert.Equal(10, mess.GetSegmentCollections().Count);
            Hl7.Entities.BaseSegment seg = SegmentHelper.CreateSegment("NTE||whatevs", mess.Encoding);
            var result = mess.AddNewSegment(seg);
            Assert.True(result, result.Error);
            Assert.Equal(10, result.Value.GetSegmentCollections().Count);

        }
    }
}
