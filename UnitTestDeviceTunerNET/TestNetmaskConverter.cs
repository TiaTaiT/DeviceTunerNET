using DeviceTunerNET.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTestDeviceTunerNET
{
    [TestClass]
    public class TestNetmaskConverter
    {
        [TestMethod]
        public void CidrToSubnetMask_CommonValues_ReturnsCorrectMask()
        {
            var converter = new NetmaskConverter();
            // Arrange & Act & Assert
            Assert.AreEqual("255.255.255.0", converter.CidrToSubnetMask(24));
            Assert.AreEqual("255.255.252.0", converter.CidrToSubnetMask(22));
            Assert.AreEqual("255.255.0.0", converter.CidrToSubnetMask(16));
            Assert.AreEqual("255.0.0.0", converter.CidrToSubnetMask(8));
        }

        [TestMethod]
        public void CidrToSubnetMask_EdgeValues_ReturnsCorrectMask()
        {
            var converter = new NetmaskConverter();
            // Test minimum value (0)
            Assert.AreEqual("0.0.0.0", converter.CidrToSubnetMask(0));

            // Test maximum value (32)
            Assert.AreEqual("255.255.255.255", converter.CidrToSubnetMask(32));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CidrToSubnetMask_NegativeValue_ThrowsArgumentException()
        {
            var converter = new NetmaskConverter();
            converter.CidrToSubnetMask(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CidrToSubnetMask_ValueTooLarge_ThrowsArgumentException()
        {
            var converter = new NetmaskConverter();
            converter.CidrToSubnetMask(33);
        }

        [TestMethod]
        public void ConvertToSubnetMask_ValidStringInput_ReturnsCorrectMask()
        {
            var converter = new NetmaskConverter();
            // Arrange & Act & Assert
            Assert.AreEqual("255.255.255.0", converter.ConvertToSubnetMask("24"));
            Assert.AreEqual("255.255.252.0", converter.ConvertToSubnetMask("22"));
            Assert.AreEqual("255.255.0.0", converter.ConvertToSubnetMask("16"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConvertToSubnetMask_InvalidString_ThrowsArgumentException()
        {
            var converter = new NetmaskConverter();
            converter.ConvertToSubnetMask("not a number");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConvertToSubnetMask_EmptyString_ThrowsArgumentException()
        {
            var converter = new NetmaskConverter();
            converter.ConvertToSubnetMask("");
        }

        [TestMethod]
        [DataRow("255.255.255.0", 24)]
        [DataRow("255.255.252.0", 22)]
        [DataRow("255.255.0.0", 16)]
        [DataRow("255.0.0.0", 8)]
        [DataRow("0.0.0.0", 0)]
        [DataRow("255.255.255.255", 32)]
        public void CidrToSubnetMask_DataDrivenTests(string expected, int cidr)
        {
            var converter = new NetmaskConverter();
            // Arrange & Act
            string result = converter.CidrToSubnetMask(cidr);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}
