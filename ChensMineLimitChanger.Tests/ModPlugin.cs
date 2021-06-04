using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chen.MineLimitChanger.Tests
{
    [TestClass]
    public class ModPlugin
    {
        [TestMethod]
        public void DebugCheck_Toggled_ReturnsFalse()
        {
            bool result = MineLimitChanger.ModPlugin.DebugCheck();

            Assert.IsFalse(result);
        }
    }
}