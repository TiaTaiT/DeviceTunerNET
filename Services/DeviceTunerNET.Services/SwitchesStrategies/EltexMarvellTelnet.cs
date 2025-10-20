using MinimalisticTelnet;
using Prism.Events;

namespace DeviceTunerNET.Services.SwitchesStrategies
{
    public class EltexMarvellTelnet(EventAggregator ea, TelnetConnection tc) : TelnetAbstract(ea, tc)
    {
        public override bool SendPacket()
        {
            var tftpServerIp = NetUtils.GetLocalIpAddress();
            SendMessage("copy tftp://" + tftpServerIp + "/config.txt running-config");
            
            return true;
        }
    }
}
