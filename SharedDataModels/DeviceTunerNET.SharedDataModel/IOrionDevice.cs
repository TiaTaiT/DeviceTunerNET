﻿using DeviceTunerNET.SharedDataModel.Devices;
using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace DeviceTunerNET.SharedDataModel
{
    public interface IOrionDevice : Irs485device
    {
        /// <summary>
        /// Код прибора (зашит в каждом приборе Болид'а)
        /// </summary>
        int ModelCode { get; set; }

        /// <summary>
        /// Изменить адрес прибора с текущего на новый. При этом прибор ищется по адресу  в поле Rs485Address.
        /// </summary>
        /// <param name="newDeviceAddress"></param>
        /// <returns>Возвращает true если адрес был успешно изменен</returns>
        bool ChangeDeviceAddress(byte newDeviceAddress);

        /// <summary>
        /// Присвоить адрес прибору с дефолтным адресом 127
        /// </summary>
        /// <returns>Возвращает true если адрес был успешно изменен</returns>
        bool SetAddress();

        /// <summary>
        /// Запрос кода модели прибора
        /// </summary>
        /// <param name="deviceAddress">Адрес прибора</param>
        /// <param name="deviceCode">код модели прибора</param>
        /// <returns>True if success, otherwise false</returns>
        bool GetModelCode(byte deviceAddress, out byte deviceCode);

        /// <summary>
        /// Запись сокращенного конфига (WriteConfig - очень долго)
        /// </summary>
        /// <param name="port"></param>
        /// <param name="progressStatus"></param>
        void WriteBaseConfig(Action<int> progressStatus);

        /// <summary>
        /// Send full packet (with checksum) and get full response from device
        /// </summary>
        /// <param name="address">Device address</param>
        /// <param name="sendArray">Data to send</param>
        /// <param name="timeout">Timeout</param>
        /// <returns></returns>
        byte[] AddressTransaction(byte address,
                                  byte[] sendArray,
                                  IOrionNetTimeouts.Timeouts timeout);

        /// <summary>
        /// Upload settings in device with default address (127)
        /// </summary>
        /// <param name="progress">update progress bar</param>
        /// <returns></returns>
        bool Setup(Action<int> progress, int modelCode = 0);

        /// <summary>
        /// Последний ответ от прибора
        /// </summary>
        byte[] Response { get; }

        public bool CheckDeviceType();

        /// <summary>
        /// Reset all addresses to default (127)
        /// </summary>
        public void ResetAddresses();
    }
}