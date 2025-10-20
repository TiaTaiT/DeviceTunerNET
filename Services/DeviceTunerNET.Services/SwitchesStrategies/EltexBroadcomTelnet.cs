using MinimalisticTelnet;
using Prism.Events;

namespace DeviceTunerNET.Services.SwitchesStrategies
{
    public class EltexBroadcomTelnet(EventAggregator ea, TelnetConnection tc) : TelnetAbstract(ea, tc)
    {
        public override bool SendPacket()
        {
            var tftpServerIp = NetUtils.GetLocalIpAddress();
            SendMessage("boot config tftp://" + tftpServerIp + "/config.txt");

            return true;
        }
    }
}
