using DeviceTunerNET.SharedDataModel;
using MinimalisticTelnet;
using Prism.Events;

namespace DeviceTunerNET.Services.SwitchesStrategies
{
    public class EltexTelnet : TelnetAbstract
    {
        public EltexTelnet(EventAggregator ea, TelnetConnection tc) : base(ea, tc)
        {
        }

        public override bool SendPacket()
        {
            var tftpServerIp = NetUtils.GetLocalIpAddress();
            SendMessage("copy tftp://" + tftpServerIp + "/config.txt running-config");
            
            return true;
        }
    }
}
