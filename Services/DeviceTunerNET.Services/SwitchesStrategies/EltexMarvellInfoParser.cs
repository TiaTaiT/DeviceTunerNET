using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using System.Text.RegularExpressions;

namespace DeviceTunerNET.Services.SwitchesStrategies
{
    public class EltexMarvellInfoParser : ISwitchInfoParser
    {
        private static readonly Regex InfoRegex = new Regex(
            @"(?<mac>(?:[0-9A-Fa-f]{2}[:\-]){5}[0-9A-Fa-f]{2})\s+" +
            @"(?<hwver>\S+)\s+" +
            @"(?<serial>[A-Za-z0-9]+)",
            RegexOptions.Compiled | RegexOptions.Multiline);

        public EthernetSwitch Parse(string systemInfo, EthernetSwitch ethernetSwitch)
        {
            if (string.IsNullOrWhiteSpace(systemInfo))
            {
                ethernetSwitch.Serial = "Unrecognized";
                return ethernetSwitch;
            }
                 

            var match = InfoRegex.Match(systemInfo);
            if (!match.Success)
            {
                ethernetSwitch.Serial = "Unrecognized";
                return ethernetSwitch;
            }
                

            ethernetSwitch.MACaddress = match.Groups["mac"].Value;
            ethernetSwitch.HardwareVersion = match.Groups["hwver"].Value;
            ethernetSwitch.Serial = match.Groups["serial"].Value;

            return ethernetSwitch;
        }
    }
}
