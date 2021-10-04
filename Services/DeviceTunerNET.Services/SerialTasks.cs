﻿using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace DeviceTunerNET.Services
{
    public class SerialTasks : ISerialTasks
    {
        

        private ISerialSender _serialSender;

        private byte _rsAddress = 127;
        private string _comPort = "";
        private const int LAST_ADDRESS = 127;
        private const int FIRST_ADDRESS = 1;

        public SerialTasks(ISerialSender serialSender)
        {
            _serialSender = serialSender;
        }

        public ISerialTasks.ResultCode SendConfig<T>(T device, string comPort, int rsAddress)
        {
            _comPort = comPort;
            _rsAddress = Convert.ToByte(rsAddress);
            object dev = device;
            if (device.GetType() == typeof(RS485device))
                return SendConfigRS485((RS485device)dev);
            if (device.GetType() == typeof(C2000Ethernet))
                return SendConfigC2000Ethernet((C2000Ethernet)dev);
            return 0;
        }

        private ISerialTasks.ResultCode SendConfigRS485(RS485device device)
        {
            var result = CheckOnlineDevice(device);
            if (result != ISerialTasks.ResultCode.ok)
            {
                return result;
            }

            var newAddress = Convert.ToByte(device.AddressRS485);
            if (_serialSender.SetDeviceRS485Address(_comPort, _rsAddress, newAddress))
            {
                return ISerialTasks.ResultCode.ok;
            }

            return ISerialTasks.ResultCode.undefinedError;
        }

        private ISerialTasks.ResultCode CheckOnlineDevice(RS485device checkDevice)
        {
            if (checkDevice.AddressRS485 == null)
            {
                return ISerialTasks.ResultCode.addressFieldNotValid;
            }

            var deviceModel = _serialSender.GetDeviceModel(_comPort, _rsAddress);
            if (deviceModel.Length == 0)
            {
                return ISerialTasks.ResultCode.deviceNotRespond;
            }

            if (!IsModelRight(deviceModel, checkDevice.Model))
            {
                return ISerialTasks.ResultCode.deviceTypeMismatch;
            }

            return ISerialTasks.ResultCode.ok;
        }

        private bool IsModelRight(string expectedModel, string receivedModel)
        {
            return receivedModel.Contains(expectedModel);
        }

        private ISerialTasks.ResultCode SendConfigC2000Ethernet(C2000Ethernet device)
        {
            if (device.AddressRS485 == null)
            {
                return ISerialTasks.ResultCode.addressFieldNotValid;
            }

            var deviceModel = _serialSender.GetDeviceModel(_comPort, _rsAddress);
            if (deviceModel.Length == 0)
            {
                return ISerialTasks.ResultCode.deviceNotRespond;
            }

            if (!device.Model.Contains(deviceModel))
            {
                return ISerialTasks.ResultCode.deviceTypeMismatch;
            }

            if (_serialSender.SetC2000EthernetConfig(_comPort, _rsAddress, device))
            {
                return ISerialTasks.ResultCode.ok;
            }

            return (int)ISerialTasks.ResultCode.undefinedError;
        }

        public ObservableCollection<string> GetAvailableCOMPorts()
        {
            return _serialSender.GetAvailableCOMPorts();
        }

        public int ShiftDevicesAddresses(string ComPort, int StartAddress, int TargetAddress, int Range)
        {
            if (TargetAddress + Range >= LAST_ADDRESS || 
                TargetAddress <= 0 ||
                TargetAddress >= StartAddress &&
                TargetAddress <= StartAddress + Range)
                return (int)ISerialTasks.ResultCode.ok;

            var _comPort = ComPort;
            var _startAddress = Convert.ToByte(StartAddress);
            var _targetAddress = Convert.ToByte(TargetAddress);
            var _range = Convert.ToByte(Range);
            //byte oldEndAddress = (byte)(_startAddress + _range);
            //для сдвига адресов вправо
            for (var counter = _range; counter >= 0; counter--)
            {
                var currentAddress = (byte)(counter + _startAddress);
                var deviceModel = _serialSender.GetDeviceModel(_comPort, currentAddress);
                if (deviceModel.Length == 0)
                {
                    return (int)ISerialTasks.ResultCode.deviceNotRespond;
                }

                var newAddr = (byte)(_targetAddress + counter);
                if (!_serialSender.SetDeviceRS485Address(_comPort, currentAddress, newAddr))
                {
                    return (int)ISerialTasks.ResultCode.deviceNotRespond;
                }
            }
            //для сдвига адресов вправо
            return (int)ISerialTasks.ResultCode.ok;
        }

        public IEnumerable<RS485device> GetOnlineDevices(string ComPort)
        {
            var _comPort = ComPort;

            for (byte currAddr = FIRST_ADDRESS; currAddr <= LAST_ADDRESS; currAddr++)
            {
                var devType = _serialSender.GetDeviceModel(_comPort, currAddr);
                if (devType.Length > 0)
                {
                    yield return new RS485device()
                    {
                        AddressRS485 = currAddr,
                        DeviceType = devType
                    };
                }
            }
        }
    }
}
