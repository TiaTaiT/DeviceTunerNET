using DeviceTunerNET.SharedDataModel;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface ISwitchInfoParser
    {
        /// <summary>
        /// Parse system information to set Serial Number, MAC address, Hardware Version and so on.
        /// </summary>
        /// /// <param name="systemInfo">Telnet response line with system information</param>
        /// <param name="ethernetSwitch">switch without system information</param>
        /// <returns>switch with system information</returns>
        EthernetSwitch Parse(string systemInfo, EthernetSwitch ethernetSwitch);
    }
}
