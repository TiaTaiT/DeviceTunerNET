using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using MinimalisticTelnet;
using Prism.Events;
using System.Threading;

namespace DeviceTunerNET.Services.SwitchesStrategies
{
    public class EltexBroadcomTelnet(EventAggregator ea, TelnetConnection tc, ISwitchInfoParser infoParser) : TelnetAbstract(ea, tc)
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
            SendMessage("copy tftp://" + tftpServerIp + "/config.conf" + " startup-config");
            Thread.Sleep(5000);
            SendMessage("copy startup-config running-config");
            Thread.Sleep(1000);
            _response = SendMessage("write startup-config");
            _response = SendMessage("Y");
            Thread.Sleep(1000);

            return true;
        }
    }
}
