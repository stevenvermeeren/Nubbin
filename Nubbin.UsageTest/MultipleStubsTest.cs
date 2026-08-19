using Nubbin;
using Nubbin.UsageTest.Bases;

namespace Nubbin.UsageTest
{
    public class MultipleStubsTest
    {
        [Fact]
        public void CanDistinguishStubsWithSameName()
        {
            var stub1 = new Namespace1.InterfaceStub();
            Assert.Equal("Namespace1", stub1.Name);

            var stub2 = new Namespace2.InterfaceStub();
            Assert.Equal("Namespace2", stub2.Name);
        }
    }
}
namespace Namespace1
{
    [Stub]
    public partial class InterfaceStub : InterfaceSample
    {
        public string? Name { get; set; } = "Namespace1";
    }
}
namespace Namespace2
{
    [Stub]
    public partial class InterfaceStub : InterfaceSample
    {
        public string? Name { get; set; } = "Namespace2";
    }
}