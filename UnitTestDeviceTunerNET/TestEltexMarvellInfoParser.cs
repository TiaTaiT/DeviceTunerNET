using DeviceTunerNET.Services.SwitchesStrategies;
using DeviceTunerNET.SharedDataModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestDeviceTunerNET
{
    [TestClass]
    public class TestEltexMarvellInfoParser
    {
        [TestMethod]
        public void Parse_ShouldExtract_ValidLine()
        {
            string input = "sh system id\r\n\r\nUnit    MAC address    Hardware version Serial number \r\n---- ----------------- ---------------- ------------- \r\n 1   ec:b1:e0:4b:03:d0     02.00.02      ESE2002736   \r\n\r\n\r\nconsole#";
            var parser = new EltexMarvellInfoParser();
            var ethernetSwitch = new EthernetSwitch(null);
            var result = parser.Parse(input, ethernetSwitch);

            Assert.IsNotNull(result);
            Assert.AreEqual("ec:b1:e0:4b:03:d0", result.MACaddress);
            Assert.AreEqual("02.00.02", result.HardwareVersion);
            Assert.AreEqual("ESE2002736", result.Serial);
        }

        [TestMethod]
        public void Parse_ShouldExtractBroadcom_ValidLine()
        {
            string input = "sh system id\r\r\n\r\r\nUnit Id       MAC Address Hardware version Serial Number \r\r\n------- ----------------- ---------------- ------------- \r\r\n      1 cc:9d:a2:b4:6f:80              3v1    ESD1014229 \r\r\n\r\r\n\rconsole#";
            var parser = new EltexMarvellInfoParser();
            var ethernetSwitch = new EthernetSwitch(null);
            var result = parser.Parse(input, ethernetSwitch);

            Assert.IsNotNull(result);
            Assert.AreEqual("cc:9d:a2:b4:6f:80", result.MACaddress);
            Assert.AreEqual("3v1", result.HardwareVersion);
            Assert.AreEqual("ESD1014229", result.Serial);
        }

        [TestMethod]
        public void Parse_ShouldHandle_ExtraSpaces()
        {
            string input = "   1   ec:b1:e0:4b:03:d0        02.00.02      ESE2002736   ";
            var parser = new EltexMarvellInfoParser();
            var ethernetSwitch = new EthernetSwitch(null);
            var result = parser.Parse(input, ethernetSwitch);

            Assert.IsNotNull(result);
            Assert.AreEqual("ec:b1:e0:4b:03:d0", result.MACaddress);
            Assert.AreEqual("02.00.02", result.HardwareVersion);
            Assert.AreEqual("ESE2002736", result.Serial);
        }

        [TestMethod]
        public void Parse_ShouldHandle_DifferentSeparator_AndLowercaseSerial()
        {
            string input = "unit 1 ec-b1-e0-4b-03-d0  1.2.3  ese2002736";
            var parser = new EltexMarvellInfoParser();
            var ethernetSwitch = new EthernetSwitch(null);
            var result = parser.Parse(input, ethernetSwitch);

            Assert.IsNotNull(result);
            Assert.AreEqual("ec-b1-e0-4b-03-d0", result.MACaddress);
            Assert.AreEqual("1.2.3", result.HardwareVersion);
            Assert.AreEqual("ese2002736", result.Serial);
        }

        [TestMethod]
        public void Parse_ShouldHandle_MixedCase()
        {
            string input = "1 EC:B1:E0:4B:03:D0 02.00.02 EsE123456";
            var parser = new EltexMarvellInfoParser();
            var ethernetSwitch = new EthernetSwitch(null);
            var result = parser.Parse(input, ethernetSwitch);

            Assert.IsNotNull(result);
            Assert.AreEqual("EC:B1:E0:4B:03:D0", result.MACaddress);
            Assert.AreEqual("02.00.02", result.HardwareVersion);
            Assert.AreEqual("EsE123456", result.Serial);
        }

        [TestMethod]
        public void Parse_ShouldHandle_TextAround()
        {
            string input = "random text 1 ec:b1:e0:4b:03:d0 02.00.02 ESE2002736 more text";
            var parser = new EltexMarvellInfoParser();
            var ethernetSwitch = new EthernetSwitch(null);
            var result = parser.Parse(input, ethernetSwitch);

            Assert.IsNotNull(result);
            Assert.AreEqual("ec:b1:e0:4b:03:d0", result.MACaddress);
            Assert.AreEqual("02.00.02", result.HardwareVersion);
            Assert.AreEqual("ESE2002736", result.Serial);
        }

        [TestMethod]
        public void Parse_ShouldReturnNull_WhenNoMatch()
        {
            string input = "invalid data line without any MAC info";
            var parser = new EltexMarvellInfoParser();
            var ethernetSwitch = new EthernetSwitch(null);
            var result = parser.Parse(input, ethernetSwitch);

            Assert.IsNotNull(result);
            Assert.AreEqual("", result.MACaddress);
            Assert.AreEqual("", result.HardwareVersion);
            Assert.AreEqual("Unrecognized", result.Serial);
        }
    }
}
