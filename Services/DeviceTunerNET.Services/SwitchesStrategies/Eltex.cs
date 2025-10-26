using DeviceTunerNET.Core;
using DeviceTunerNET.Core.Enums;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using DeviceTunerNET.SharedDataModel.Ports;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace DeviceTunerNET.Services.SwitchesStrategies
{
    public class Eltex : ISwitchConfigUploader
    {
        private const int LastEthernetState = 5;
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
        private string _serialPortName = "";
        private int _currentConfigLineIndex = 0;
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
        private string configOutputFileName = @"config.conf";
        private SwitchProtocolEnum _protocol;
        private SimpleComPort _comPort;

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

        public void SetProtocol(SwitchProtocolEnum switchProtocol)
        {
            _protocol = switchProtocol;
        }

        public void SetSerialPortName(string serialPortName)
        {
            _serialPortName = serialPortName;
        }

        public EthernetSwitch SendConfig(EthernetSwitch ethernetSwitch, Dictionary<string, string> settingsDict, CancellationToken token)
        {
            _ethernetSwitch = ethernetSwitch;
            MessageToConsole("Waiting device...");
            _sDict = settingsDict;
            SetCurrentTelnetSender(ethernetSwitch.Model);
            string configPath = GetConfigPath(ethernetSwitch.Model);
            var result = _configParser.Parse(settingsDict, configPath, _tftpSharedDirectory + configOutputFileName);
            MessageToConsole("Parse result: " + result);

            var IsSendComplete = false;
            if (_protocol == SwitchProtocolEnum.Serial)
            {
                IsSendComplete = SendingSerialStateMachine(IsSendComplete, _configParser.GetConfig(), token);
            }
            else
            {
                IsSendComplete = SendingEthernetStateMachine(IsSendComplete, token);
            }
            if (IsSendComplete)
                return _ethernetSwitch;
            return null;
        }

        private bool SendingSerialStateMachine(bool isSendComplete, IEnumerable<string> config, CancellationToken token)
        {
            _comPort = new SimpleComPort
            {
                SerialPort = new SerialPort
                {
                    PortName = _serialPortName,
                    BaudRate = 115200
                }
            };

            var State = 0;

            while (State < 7 && !token.IsCancellationRequested)
            {
                switch (State)
                {
                    case 0:
                        MessageForUser("Ожидание" + "\r\n" + "коммутатора");
                        try
                        {
                            _comPort.Timeout = 1000;
                            var response = UtfSend("");
                            Debug.WriteLine(response);
                            if (response?.Length > 0 && !response.Contains("press ENTER"))
                            {
                                State = 1;
                                break;
                            }
                            
                        }
                        catch 
                        {
                            break;
                        }
                        break;

                    case 1:
                        try
                        {
                            var response = UtfSend(DefaultUsername);
                            Debug.WriteLine(response);
                            if (response?.Length == 0)
                            {
                                State = 0;
                                break;
                            }
                            response = UtfSend(DefaultPassword);
                            Debug.WriteLine(response);
                            if (response?.Length == 0)
                            {
                                State = 0;
                                break;
                            }
                            State = 2;
                        }
                        catch
                        {
                            break;
                        }
                        break;
                    case 2:
                        try
                        {
                            if (!_comPort.SerialPort.IsOpen)
                            {
                                _comPort.SerialPort.Open();
                                State = 3;
                            }
                            break;
                        }
                        catch 
                        {
                            State = 0;
                        }
                        break;
                    case 3:
                        if (_currentConfigLineIndex >= config.Count())
                        {
                            State = 4;
                            break;
                        }
                        SendSerialConfig(config);
                        break;
                    case 4:
                        break;
                }
            }

            return isSendComplete;
        }

        private void SendSerialConfig(IEnumerable<string> config)
        {
            UtfSendWithoutConfirmation(config.ElementAt(_currentConfigLineIndex) + '\n');
            _currentConfigLineIndex++;
        }

        private static bool NeedRepeat(string response) => response?.Length == 0 || response.Contains("press ENTER");

        private bool SendingEthernetStateMachine(bool isSendComplete, CancellationToken token)
        {
            var State = 0;

            while (State < LastEthernetState && !token.IsCancellationRequested)
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
                        if (_currentTelnetSender.CreateConnection(_sDict["%%DEFAULT_IP_ADDRESS%%"],
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
                        // Пингуем в цикле коммутатор по новому IP-адресу (как только пинг пропал - коммутатор отключили)
                        MessageForUser("Замени" + "\r\n" + "коммутатор!");
                        if (!_networkUtils.SendMultiplePing(_ethernetSwitch.AddressIP, repeatNumer)) State = 4;
                        break;
                    case 4:
                        isSendComplete = true;
                        State = LastEthernetState;
                        break;
                }
                Thread.Sleep(100); // Слишком часто коммутатор лучше не долбить (может воспринять как атаку)
                                   // Go to state 0
            }

            return isSendComplete;
        }

        private string UtfSend(string s)
        {
            return Encoding.UTF8.GetString(_comPort.Send(Encoding.UTF8.GetBytes(s + '\n')));
        }

        private void UtfSendWithoutConfirmation(string s)
        {
            try
            {
                _comPort.SendWithoutСonfirmation(Encoding.UTF8.GetBytes(s + '\n'));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
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
