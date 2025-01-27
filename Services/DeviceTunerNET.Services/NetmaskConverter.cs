using DeviceTunerNET.Services.Interfaces;
using System;
using System.Net;

namespace DeviceTunerNET.Services
{
    public class NetmaskConverter : INetmaskConverter
    {
        public string CidrToSubnetMask(int cidr)
        {
            if (cidr < 0 || cidr > 32)
            {
                throw new ArgumentException("CIDR value must be between 0 and 32", nameof(cidr));
            }

            // Create a 32-bit mask with the specified number of 1's from left to right
            uint mask = cidr == 0 ? 0 : 0xffffffff << (32 - cidr);

            // Convert to network byte order (big-endian)
            byte[] bytes = BitConverter.GetBytes(mask);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            // Convert to IP address format
            return new IPAddress(bytes).ToString();
        }

        public string ConvertToSubnetMask(string cidrString)
        {
            if (!int.TryParse(cidrString, out int cidr))
            {
                throw new ArgumentException("Invalid CIDR format", nameof(cidrString));
            }

            return CidrToSubnetMask(cidr);
        }
    }
}
