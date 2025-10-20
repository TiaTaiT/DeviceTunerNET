using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DeviceTunerNET.Core;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using Prism.Events;

namespace DeviceTunerNET.Services.SwitchesStrategies
{
    public class Eltex : ISwitchConfigUploader
    {
        private readonly IEventAggregator _ea;
        private readonly INetworkUtils _networkUtils;
        private readonly ISender _telnetMarvellSender;
        private readonly ISender _telnetBroadcomSender;
        private ISender _currentTelnetSender;
        private readonly ISender _sshSender;
        private readonly ITftpServerManager _tftpServer;
        private readonly IConfigParser _configParser;
        private EthernetSwitch _ethernetSwitch;
        private int repeatNumer = 5;
        private Dictionary<string, string> _sDict;
        private readonly IEnumerable<string> _broadcomModels = 
            [
                "MES24",
                "MES37",
            ];
        private readonly IEnumerable<string> _marvellModels =
            [
                "MES35",
                "MES23",
            ];

        private string _tftpSharedDirectory = @"C:\Temp\";
        private string _resourcePath = "Resources\\Files\\";
        private string marvellTemplateFileName = @"EltexMarvellTemplateConfig.conf";
        private string broadcomTemplateFileName = @"EltexBroadcomTemplateConfig.conf";
        private string configOutputFileName = @"config.txt";

        public string DefaultIpAddress { get; set; } = "192.168.1.239";
        public ushort DefaultTelnetPort { get; set; } = 23;
        public ushort DefaultSshPort { get; set; } = 22;
        public string DefaultUsername { get; set; } = "admin";
        public string DefaultPassword { get; set; } = "admin";
        public string RsaKeyFile { get; set; } = "id_rsa.key";

        
        public Eltex(INetworkUtils networkUtils,
                     IEnumerable<ISender> senders,
                     ITftpServerManager tftpServer,
                     IConfigParser configParser,
                     IEventAggregator eventAggregator)
        {
            _networkUtils = networkUtils;
            _telnetMarvellSender = senders.ElementAt((int)SenderSrvKey.telnetMarvellKey);
            _telnetBroadcomSender = senders.ElementAt((int)SenderSrvKey.telnetBroadcomKey);
            _sshSender = senders.ElementAt((int)SenderSrvKey.sshKey);
            _tftpServer = tftpServer;
            _configParser = configParser;
            _ea = eventAggregator;

            // Start TFTP Server
            _tftpServer.Start(_tftpSharedDirectory);
        }

        public string GetSwitchModel()
        {
            throw new NotImplementedException();
        }

        public EthernetSwitch SendConfig(EthernetSwitch ethernetSwitch, Dictionary<string, string> settingsDict, CancellationToken token)
        {
            _ethernetSwitch = ethernetSwitch;
            MessageToConsole("Waiting device...");
            _sDict = settingsDict;
            SetCurrentTelnetSender(ethernetSwitch.Model);
            string configPath = GetConfigPath(ethernetSwitch.Model);
            var result = _configParser.Parse(settingsDict, configPath, _tftpSharedDirectory + configOutputFileName);
            Debug.WriteLine("Parse result: " + result);
            var State = 0;
            var IsSendComplete = false;

            while (State < 7 && !token.IsCancellationRequested)
            {
                switch (State)
                {
                    case 0:
                        // Пингуем в цикле коммутатор по дефолтному адресу пока коммутатор не ответит на пинг
                        MessageForUser("Ожидание" + "\r\n" + "коммутатора");
                        if (_networkUtils.SendMultiplePing(_sDict["%%DEFAULT_IP_ADDRESS%%"], repeatNumer))
                            State = 1;
                        break;
                    case 1:
                        // Пытаемся в цикле подключиться по Telnet (сервер Telnet загружается через некоторое время после успешного пинга)
                        if (_telnetMarvellSender.CreateConnection(_sDict["%%DEFAULT_IP_ADDRESS%%"],
                                                           DefaultTelnetPort, _sDict["%%DEFAULT_ADMIN_LOGIN%%"],
                                                           _sDict["%%DEFAULT_ADMIN_PASSWORD%%"],
                                                           null))
                            State = 2;
                        break;
                    case 2:
                        // Заливаем первую часть конфига в коммутатор по Telnet
                        // copy tftp://192.168.1.254/config.txt running-config
                        MessageToConsole("Заливаем первую часть конфига в коммутатор по Telnet.");

                        _currentTelnetSender.Send(_ethernetSwitch, _sDict);
                        // Закрываем Telnet соединение
                        _currentTelnetSender.CloseConnection();
                        State = 3;
                        break;
                    case 3:
                        // Пытаемся в цикле подключиться к SSH-серверу
                        if (_sshSender.CreateConnection(_ethernetSwitch.AddressIP,
                                                        DefaultSshPort, _sDict["%%NEW_ADMIN_LOGIN%%"],
                                                        _sDict["%%NEW_ADMIN_PASSWORD%%"],
                                                        _resourcePath + RsaKeyFile))
                            State = 4;
                        break;
                    case 4:
                        // Заливаем вторую часть конфига по SSH-протоколу
                        MessageToConsole("Заливаем вторую часть конфига по SSH-протоколу.");
                        _sshSender.Send(_ethernetSwitch, _sDict);
                        // Закрываем SSH-соединение
                        _sshSender.CloseConnection();

                        MessageToConsole("Заливка конфига в коммутатор завершена.");
                        State = 5;
                        break;
                    case 5:
                        // Пингуем в цикле коммутатор по новому IP-адресу (как только пинг пропал - коммутатор отключили)
                        MessageForUser("Замени" + "\r\n" + "коммутатор!");
                        if (!_networkUtils.SendMultiplePing(_ethernetSwitch.AddressIP, repeatNumer)) State = 6;
                        break;
                    case 6:
                        IsSendComplete = true;
                        State = 7;
                        break;
                }
                Thread.Sleep(100); // Слишком часто коммутатор лучше не долбить (может воспринять как атаку)
                                   // Go to state 0
            }
            if (IsSendComplete)
                return _ethernetSwitch;
            return null;
        }

        private string GetConfigPath(string model)
        {
            foreach (var item in _broadcomModels)
            {
                if (model.StartsWith(item))
                return _resourcePath + broadcomTemplateFileName;

            }
            foreach (var item in _marvellModels)
            {
                if (model.StartsWith(item))
                    return _resourcePath + marvellTemplateFileName;
            }
            return "";
        }

        private void SetCurrentTelnetSender(string model)
        {
            foreach (var item in _broadcomModels)
            {
                if (model.StartsWith(item))
                {
                    _currentTelnetSender = _telnetBroadcomSender;
                    return;
                }
            }
            foreach (var item in _marvellModels)
            {
                if (model.StartsWith(item))
                {
                    _currentTelnetSender = _telnetMarvellSender;
                    return;
                }
            }
            throw new Exception("Unrecognized switch model");
        }

        private void MessageToConsole(string message)
        {
            //Сообщаем об обновлении данных в репозитории
            _ea.GetEvent<MessageSentEvent>().Publish(new Message
            {
                ActionCode = MessageSentEvent.StringToConsole,
                MessageString = message
            });
        }

        private void MessageForUser(string message)
        {
            //Сообщаем об обновлении данных в репозитории
            _ea.GetEvent<MessageSentEvent>().Publish(new Message
            {
                ActionCode = MessageSentEvent.NeedOfUserAction,
                MessageString = message
            });
        }
    }
}
