using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using System;
using System.Drawing;
using System.Collections.Generic;
using static System.Int32;
using DeviceTunerNET.SharedDataModel.Devices;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services
{
    public class DataDecoder : IDataDecoder
    {
        #region Constants
        private const char transparent = 'T';
        private const char master = 'M';
        private const char slave = 'S';
        #endregion Constants

        private int IPaddressCol = 0; // Index of the column containing device addresses
        private int RS485addressCol = 0; // Index of the column containing device addresses
        private int RS232addressCol = 0; // Index of the column containing device addresses
        private int nameCol = 0;    // Index of the column containing device names
        private int serialCol = 0;  // Index of the column containing device serial number
        private int modelCol = 0;   // Index of the column containing device model
        private int parentCol = 0;   // Index of the column containing parent cabinet
        private int CaptionRow = 1; // Table caption row index
        private int rangCol = 0; // Index of the column containing networkRelationship (master, slave, Transparent)
        private int qcCol = 0; // Index of the column contaning quality control passed mark
        private int projectCol = 0; // Project column (Площадка)

        private const string ColProjectCaption = "Площадка"; //Заголовок столбца с наименованием проекта
        private const string ColIPAddressCaption = "IP"; //Заголовок столбца с IP-адресами
        private const string ColRS485AddressCaption = "RS485"; //Заголовок столбца с адресами RS485
        private const string ColRS232AddressCaption = "RS232"; //Заголовок столбца с адресами RS232
        private const string ColNamesCaption = "Обозначение"; //Заголовок столбца с обозначениями приборов
        private const string ColSerialCaption = "Серийный номер"; //Заголовок столбца с обозначениями приборов
        private const string ColModelCaption = "Модель"; //Заголовок столбца с наименованием модели прибора
        private const string ColParentCaption = "Шкаф"; //Заголовок столбца с наименованием шкафа в котором находится дивайс
        private const string ColNetRelationship = "Rang"; //Заголовок столбца с мастерами и слевами C2000-Ethernet
        private const string ColQualityControl = "QC"; //Заголовок столбца о прохождении шкафом ОТК

        private const string qcPassed = "Passed";
        private const string qcDidntPass = "Failed!";


        //private ExcelPackage package;
        //private FileInfo sourceFile;
        //private ExcelWorksheet worksheet;
        //int rows; // number of rows in the sheet
        //int columns;//number of columns in the sheet

        //Dictionary with all found C2000-Ethernet
        private Dictionary<C2000Ethernet, Tuple<char, int>> dictC2000Ethernet = [];

        private readonly IDeviceGenerator _devicesGenerator;
        private readonly IDialogCaller _dialogCaller;

        public ITablesManager Driver
        {
            get;
            set;
        }

        public DataDecoder(IDeviceGenerator deviceGenerator, IDialogCaller dialogCaller)
        {
            _devicesGenerator = deviceGenerator;
            _dialogCaller = dialogCaller;
        }

        public List<Cabinet> GetCabinets(string excelFileFullPath)
        {
            //ExcelInit(excelFileFullPath);
            Driver.SetCurrentDocument(excelFileFullPath);
            IPaddressCol = 0;
            RS485addressCol = 0;
            RS232addressCol = 0;
            nameCol = 0;
            serialCol = 0;
            modelCol = 0;
            parentCol = 0;
            rangCol = 0;
            qcCol = 0;
            projectCol = 0;
            //Определяем в каких столбцах находятся обозначения приборов и их адреса
            FindColumnIndexesByHeader();
            if (IsTableCaptionValid())
                return GetCabinetContent();
            return [];
        }

        private bool IsTableCaptionValid()
        {
            if (IPaddressCol == 0
                || RS485addressCol == 0
                || RS232addressCol == 0
                || nameCol == 0
                || serialCol == 0
                || modelCol == 0
                || parentCol == 0
                || rangCol == 0
                || qcCol == 0
                || projectCol == 0)
            {
                _dialogCaller.ShowMessage($"Error! There is no valid caption in the table!");
                return false;
            }
            return true;
        }

        private List<Cabinet> GetCabinetContent()
        {
            var cabinetsLst = new List<Cabinet>();
            var cabinet = new Cabinet();
            var lastDevCabinet = "";
            var lastDevProject = "";
            for (var rowIndex = CaptionRow + 1; rowIndex <= Driver.Rows; rowIndex++)
            {
                TryParse(Driver.GetCellValueByIndex(rowIndex, RS232addressCol), out int devRS232Addr);
                TryParse(Driver.GetCellValueByIndex(rowIndex, RS485addressCol), out int devRS485Addr);

                var deviceDataSet = new DeviceDataSet
                {
                    Id = rowIndex,
                    DevProject = DefaultValue(Driver.GetCellValueByIndex(rowIndex, projectCol), lastDevProject), 
                    DevCabinet = DefaultValue(Driver.GetCellValueByIndex(rowIndex, parentCol), lastDevCabinet), 
                    DevName = Driver.GetCellValueByIndex(rowIndex, nameCol),
                    DevModel = Driver.GetCellValueByIndex(rowIndex, modelCol),
                    DevIPAddr = Driver.GetCellValueByIndex(rowIndex, IPaddressCol),
                    DevSerial = Driver.GetCellValueByIndex(rowIndex, serialCol),
                    DevRang = Driver.GetCellValueByIndex(rowIndex, rangCol),
                    DevRS232Addr = devRS232Addr,
                    DevRS485Addr = devRS485Addr,
                    DevQcPassed = GetQcStatus(Driver.GetCellValueByIndex(rowIndex, qcCol)),
                };

                if (!string.Equals(deviceDataSet.DevCabinet, lastDevCabinet)) // Если новый шкаф - сохранить старый в список шкафов
                {
                    if (rowIndex != CaptionRow + 1) 
                        cabinetsLst.Add(cabinet); // первый шкаф надо сначала наполнить а потом добавлять в cabinetsLst
                    
                    cabinet = new Cabinet
                    {
                        Id = rowIndex,
                        Designation = deviceDataSet.DevCabinet,
                        ParentName = deviceDataSet.DevProject,
                    };
                }

                if(_devicesGenerator.TryGetDevice(deviceDataSet.DevModel, out var device))
                {
                    var deviceWithSettings = GetDeviceWithSettings(device, deviceDataSet);
                    deviceWithSettings.Cabinet = cabinet.Designation;
                    
                    if(device is C2000Ethernet c2000Ethernet)
                    {
                        //Add to dict for master/slave/translate sort
                        dictC2000Ethernet.Add(c2000Ethernet, GetRangTuple(deviceDataSet.DevRang));
                    }
                    
                    cabinet.AddItem(deviceWithSettings);
                }
                               
                if (rowIndex == Driver.Rows) // В последней строчке таблицы надо добавить последний шкаф в список шкафов, иначе (исходя из условия) он туда не попадёт
                {
                    cabinetsLst.Add(cabinet);
                }
                lastDevCabinet = deviceDataSet.DevCabinet;
                lastDevProject = deviceDataSet.DevProject;
            }
            FillDevicesDependencies(dictC2000Ethernet, master, slave);
            FillDevicesDependencies(dictC2000Ethernet, slave, master);
            return cabinetsLst;
        }

        private bool GetQcStatus(string qcStatus)
        {
            if(qcStatus != null && qcStatus.Equals(qcPassed))
            {
                return true;
            }
            return false;
        }

        private Tuple<char, int> GetRangTuple(string rang)
        {
            var _rang = rang[0];
            var lineStr = rang.Substring(1); //right part of rang

            if (_rang != master && _rang != slave && _rang != transparent)
                return null;

            if (!TryParse(lineStr, out var lineNumb))
                return null;

            return new Tuple<char, int>(_rang, lineNumb);
        }

        // связываем все C2000-Ethernet в общую сеть, добавляя ссылки мастеров на слейв и прописывая мастеров в слейвы
        private void FillDevicesDependencies(Dictionary<C2000Ethernet, Tuple<char, int>> ethDevices, char dep1, char dep2)
        {
            foreach (var device in ethDevices)
            {
                switch (device.Value.Item1)
                {
                    case transparent:
                        device.Key.NetworkMode = C2000Ethernet.Mode.transparent; // Transparent
                        break;
                    case master:
                        device.Key.NetworkMode = C2000Ethernet.Mode.master; // master
                        break;
                    case slave:
                        device.Key.NetworkMode = C2000Ethernet.Mode.slave; // slave
                        break;
                }

                if (device.Value.Item1 != dep1)
                    continue;

                foreach (var item in ethDevices)
                {
                    if (item.Value.Item1 != dep2 || device.Value.Item2 != item.Value.Item2)
                        continue;

                    device.Key.RemoteDevicesList.Add(item.Key);
                    //Debug.WriteLine(item.Key.AddressIP + " добавлен в " + device.Key.AddressIP + " (" + item.Value.Item2 + ")");
                }
            }
            //Debug.WriteLine("----------------");
        }

        private static ICommunicationDevice GetDeviceWithSettings(ICommunicationDevice device, DeviceDataSet settings)
        {
            if (device is EthernetSwitch ethernetSwitch)
            {
                FillEthernetSwitchSettings(settings, ethernetSwitch);

                return ethernetSwitch;
            }

            if (device is C2000Ethernet C2000Ethernet)
            {
                FillC2000EthernetSettings(settings, C2000Ethernet);

                return C2000Ethernet;
            }

            if (device is RS485device rS485Device)
            {
                device = FillRS485Settings(rS485Device, settings);
            }

            return device;
        }

        private static ICommunicationDevice FillRS485Settings(RS485device device, DeviceDataSet settings)
        {
            device.Id = settings.Id;
            device.Designation = settings.DevName;
            device.Model = settings.DevModel;
            device.Serial = settings.DevSerial;
            device.AddressRS485 = (uint)settings.DevRS485Addr;
            device.QualityControlPassed = settings.DevQcPassed;
            return device;
        }

        private static void FillC2000EthernetSettings(DeviceDataSet settings, C2000Ethernet C2000Ethernet)
        {
            C2000Ethernet.Id = settings.Id;
            C2000Ethernet.Designation = settings.DevName;
            C2000Ethernet.Model = settings.DevModel;
            C2000Ethernet.Serial = settings.DevSerial;
            C2000Ethernet.AddressRS485 = (uint)settings.DevRS485Addr;
            C2000Ethernet.AddressRS232 = settings.DevRS232Addr;
            C2000Ethernet.AddressIP = settings.DevIPAddr;
            C2000Ethernet.NetName = settings.DevName;
            C2000Ethernet.QualityControlPassed = settings.DevQcPassed;
        }

        private static void FillEthernetSwitchSettings(DeviceDataSet settings, EthernetSwitch ethernetSwitch)
        {
            ethernetSwitch.Id = settings.Id;
            ethernetSwitch.Designation = settings.DevName;
            ethernetSwitch.Model = settings.DevModel;
            ethernetSwitch.Serial = settings.DevSerial;
            ethernetSwitch.AddressIP = settings.DevIPAddr;
            ethernetSwitch.QualityControlPassed = settings.DevQcPassed;
        }

        private void FindColumnIndexesByHeader()
        {
            for (var colIndex = 1; colIndex <= Driver.Columns; colIndex++)
            {
                var content = Driver.GetCellValueByIndex(CaptionRow, colIndex);// worksheet.Cells[CaptionRow, colIndex].Value?.ToString();

                if (content == ColNamesCaption) { nameCol = colIndex; }
                if (content == ColIPAddressCaption) { IPaddressCol = colIndex; }
                if (content == ColRS485AddressCaption) { RS485addressCol = colIndex; }
                if (content == ColRS232AddressCaption) { RS232addressCol = colIndex; }
                if (content == ColSerialCaption) { serialCol = colIndex; }
                if (content == ColModelCaption) { modelCol = colIndex; }
                if (content == ColParentCaption) { parentCol = colIndex; }
                if (content == ColNetRelationship) { rangCol = colIndex; }
                if (content == ColQualityControl) { qcCol = colIndex; }
                if (content == ColProjectCaption) { projectCol = colIndex; }
            }
        }

        public async Task<bool> SaveSerialNumberAsync(int id, string serialNumber)
        {
            // записываем серийник коммутатора в графу "Серийный номер" напротив номера строки указанного в id
            Driver.SetCellValueByIndex(serialNumber, id, serialCol); // worksheet.Cells[id, serialCol].Value = serialNumber;

            return await SaveCurrentPackageAsync();
        }

        public async Task<bool> SaveQualityControlPassedAsync(int id, bool qualityControlPassed)
        {
            // записываем метку прохождения прохождения контроля качества в графу "QC" напротив номера строки указанного в id
            if (qualityControlPassed)
            {
                Driver.SetCellColor(Color.Black, id, qcCol); // worksheet.Cells[id, qcCol].Style.Font.Color.SetColor(Color.Black);
                Driver.SetCellValueByIndex(qcPassed, id, qcCol); // worksheet.Cells[id, qcCol].Value = qcPassed;
            }
            else
            {
                Driver.SetCellColor(Color.Red, id, qcCol); // worksheet.Cells[id, qcCol].Style.Font.Color.SetColor(Color.Red);
                Driver.SetCellValueByIndex(qcDidntPass, id, qcCol); // worksheet.Cells[id, qcCol].Value = qcDidntPass;
            }

            return await SaveCurrentPackageAsync();
        }

        private async Task<bool> SaveCurrentPackageAsync()
        {
            return await Driver.Save();
        }

        private static string DefaultValue(string value, string defaultValue)
        {
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }
    }
    
    internal class DeviceDataSet
    {
        internal int Id { get; set; }
        internal string DevProject { get; set; } = "";
        internal string DevCabinet { get; set; } = "";
        internal string DevName { get; set; } = "";
        internal string DevModel { get; set; } = "";
        internal string DevIPAddr { get; set; } = "";
        internal string DevSerial { get; set; } = "";
        internal string DevRang { get; set; } = "";
        internal int DevRS232Addr { get; set; }
        internal int DevRS485Addr { get; set; }
        internal bool DevQcPassed { get; set; }
    }
}
