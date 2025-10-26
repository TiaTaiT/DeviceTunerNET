using System.Collections.Generic;
using System.Net;

namespace DeviceTunerNET.SharedDataModel
{
    public class EthernetSwitch : CommunicationDevice, IEthernetDevice
    {
        public EthernetSwitch(IPort port)
        {
            Port = port;
        }

        /// <summary>
        /// LogIn user name
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// LogIn user password
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Control interface (Telnet, SSH, WEB) IP address
        /// </summary>
        public string AddressIP { get; set; } = string.Empty;
        public string Netmask { get; set; } = string.Empty;
        public string MACaddress { get; set; } = string.Empty;
        public string DefaultGateway { get; set; } = string.Empty;
        public int CIDR
        {
            get => EthernetUtils.ConvertToCidr(Netmask);
            set => Netmask = EthernetUtils.CidrToString(value);
        }

    }
}
