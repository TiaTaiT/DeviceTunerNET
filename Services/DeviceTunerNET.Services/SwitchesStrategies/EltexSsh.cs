﻿using Prism.Events;

namespace DeviceTunerNET.Services.SwitchesStrategies
{
    public class EltexSsh : SshAbstract
    {
        public EltexSsh(EventAggregator ea) : base(ea)
        {
        }

        protected override bool GetIdOverSsh()
        {
            var answer = GetDeviceResponse();

            string MACaddress;
            string HardwareVersion;
            string SerialNumber;
            try
            {
                if (answer.Contains("MAC address :"))
                {
                    answer = answer.Remove(0, answer.IndexOf("MAC address :"));
                    answer = answer.Replace(" ", "");
                    //"MACaddress:e0:d9:e3:3d:ca:80Hardwareversion:01.03.01Serialnumber:ES50004388"
                    MACaddress = answer.Substring(answer.IndexOf(":") + 1, 17);
                    answer = answer.Remove(0, answer.IndexOf("Hardwareversion:"));
                    //"Hardwareversion:01.03.01Serialnumber:ES50004388"
                    HardwareVersion = answer.Substring(answer.IndexOf(":") + 1, answer.IndexOf("Serialnumber:") - (answer.IndexOf(":") + 1));
                    answer = answer.Remove(0, answer.IndexOf("Serialnumber:"));
                    //"Serialnumber:ES50004388"
                    SerialNumber = answer.Remove(0, answer.IndexOf(":") + 1);
                }
                else
                {
                    answer = answer.Trim();
                    //"\rSWITCH_1_2>sh system id\rUnit    MAC address    Hardware version Serial number ---- ----------------- ---------------- -------------  1   e8:28:c1:5d:5f:60     01.02.01      ES5E004602"
                    var LastWordIndex = answer.LastIndexOf(' ') + 1;
                    SerialNumber = answer.Substring(LastWordIndex, answer.Length - LastWordIndex);
                    answer = answer.Remove(LastWordIndex);
                    answer = answer.Trim();
                    // //"\rSWITCH_1_2>sh system id\rUnit    MAC address    Hardware version Serial number ---- ----------------- ---------------- -------------  1   e8:28:c1:5d:5f:60     01.02.01"
                    LastWordIndex = answer.LastIndexOf(' ') + 1;
                    HardwareVersion = answer.Substring(LastWordIndex, answer.Length - LastWordIndex);
                    answer = answer.Remove(LastWordIndex);
                    answer = answer.Trim();
                    // //"\rSWITCH_1_2>sh system id\rUnit    MAC address    Hardware version Serial number ---- ----------------- ---------------- -------------  1   e8:28:c1:5d:5f:60"
                    LastWordIndex = answer.LastIndexOf(' ') + 1;
                    MACaddress = answer.Substring(LastWordIndex, answer.Length - LastWordIndex);
                }
            }
            catch
            {
                return false;
            }
            NetworkSwitch.MACaddress = MACaddress;
            NetworkSwitch.HardwareVersion = HardwareVersion;
            NetworkSwitch.Serial = SerialNumber;
            NetworkSwitch.Username = SettingsDict["%%NEW_ADMIN_LOGIN%%"];
            NetworkSwitch.Password = SettingsDict["%%NEW_ADMIN_PASSWORD%%"];
            return true;
        }

        protected override void SendPacket()
        {
            Stream.WriteLine("sh system id");

            GetIdOverSsh();

            Stream.WriteLine("en");
            Stream.WriteLine(SettingsDict["%%NEW_ADMIN_PASSWORD%%"]);
            Stream.WriteLine("conf t");
/*
            Stream.WriteLine("interface vlan 1");
            Stream.WriteLine("no ip address");
            Stream.WriteLine("shutdown");

            // IP головного коммутатора
            Stream.WriteLine("ip default-gateway 192.168.3.1");
            Stream.WriteLine("ip routing");

            Stream.WriteLine("clock source sntp");
            Stream.WriteLine("no clock timezone");
            //Stream.WriteLine("clock dhcp timezone");
            Stream.WriteLine("sntp client poll timer 60");
            Stream.WriteLine("sntp unicast client enable");
            Stream.WriteLine("sntp unicast client poll");
            Stream.WriteLine("sntp server 192.168.0.1 poll");
*/
            Stream.WriteLine("no ip telnet server");
            Stream.WriteLine("exit");
            Stream.WriteLine("write memory");
            Stream.WriteLine("Y");
            var response = GetDeviceResponse(); // This line is important for memory saving!!!
        }        
    }
}
