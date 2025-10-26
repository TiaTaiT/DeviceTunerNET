using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using MinimalisticTelnet;
using Prism.Events;
using System;
using System.Threading;

namespace DeviceTunerNET.Services.SwitchesStrategies
{
    public class EltexMarvellTelnet(EventAggregator ea, TelnetConnection tc, ISwitchInfoParser infoParser) : TelnetAbstract(ea, tc)
    {
        private string _response = "";
        private ISwitchInfoParser _infoParser = infoParser;
        public override EthernetSwitch GetSwitchInfo(EthernetSwitch ethernetSwitch)
        {
            _response = SendMessage("sh system id");
            return _infoParser.Parse(_response, ethernetSwitch);
        }

        public override bool SendPacket()
        {
            var tftpServerIp = NetUtils.GetLocalIpAddress();
            _response = SendMessage("boot config tftp://" + tftpServerIp + "/config.txt");
            _response = SendMessage("Y");
            Thread.Sleep(5000);
            _response = SendMessage(Environment.NewLine);
            _response = SendMessage("reload");
            _response = SendMessage("Y");
            _response = SendMessage("Y");

            return true;
        }
    }
}
