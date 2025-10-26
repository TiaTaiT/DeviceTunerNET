using DeviceTunerNET.Core.Enums;
using DeviceTunerNET.SharedDataModel;
using System.Collections.Generic;
using System.Threading;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface ISwitchConfigUploader
    {
        //IP адрес коммутатора
        public string DefaultIpAddress { get; set; }

        //Сетевой порт (22 для SSH-соединения , 23 для Telnet-соединения)
        public ushort DefaultTelnetPort { get; set; }

        //
        public ushort DefaultSshPort { get; set; }

        //Имя учетной записи на коммутаторе (например, по умолчанию admin)
        public string DefaultUsername { get; set; }

        //Пароль от учетной записи на коммутаторе (например, по умолчанию admin)
        public string DefaultPassword { get; set; }

        //Путь к файлу RSA ключа шифрования (SSH)
        public string RsaKeyFile { get; set; }

        // Set download protocol (Serial port, Ethernet  port,...)
        public void SetProtocol(SwitchProtocolEnum switchProtocol);

        // Set serial port name for switch console
        public void SetSerialPortName(string serialPortName);

        /// <summary>
        /// Заливка конфига в коммутатор по протоколу Telnet и\или SSH
        /// </summary>
        /// <param name="ethernetDevice">Коммутатор</param>
        /// <param name="SettingsDict">Словарь с настройками коммутатора</param>
        /// <returns>Объект типа EthernetDevices с заполненными полями (которые удалось выцепить из коммутатора)</returns>
        public EthernetSwitch SendConfig(EthernetSwitch ethernetDevice, Dictionary<string, string> SettingsDict, CancellationToken token);
    }
}
