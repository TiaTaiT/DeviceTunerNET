namespace DeviceTunerNET.Services.Interfaces
{
    public interface INetmaskConverter
    {
        public string CidrToSubnetMask(int cidr);

        // Helper method to validate and convert string input
        public string ConvertToSubnetMask(string cidrString);
    }
}
