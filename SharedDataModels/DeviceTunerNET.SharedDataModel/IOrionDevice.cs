﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using static DeviceTunerNET.SharedDataModel.RS485device;

namespace DeviceTunerNET.SharedDataModel
{
    public interface IOrionDevice : IRS485device
    {
        /// <summary>
        /// Код прибора (зашит в каждом приборе Болид'а)
        /// </summary>
        int ModelCode { get; set; }

        /// <summary>
        /// Модели приборов на которые распространяются настройки. Например Сигнал-20П и Сигнал-20П исп.01 или С2000-СП1 и С2000-СП1 исп.01
        /// </summary>
        IEnumerable<string> SupportedModels { get; set; }

        /// <summary>
        /// Серийный порт
        /// </summary>
        SerialPort ComPort { get; set; }

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
        /// <param name="address">RS485 адрес прибора</param>
        /// <returns>Код прибора</returns>
        byte GetModelCode(byte address);

        /// <summary>
        /// Запись сокращенного конфига (WriteConfig - очень долго)
        /// </summary>
        /// <param name="serialPort"></param>
        /// <param name="progressStatus"></param>
        void WriteBaseConfig(SerialPort serialPort, Action<int> progressStatus);
    }
}