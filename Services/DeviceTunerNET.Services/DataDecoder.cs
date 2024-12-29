using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using System;
using System.Drawing;
using System.Collections.Generic;
using static System.Int32;
using DeviceTunerNET.SharedDataModel.Devices;
using System.Threading.Tasks;
using System.Linq;

namespace DeviceTunerNET.Services
{
    public enum TableColumns
    {
        IPAddress,
        RS485Address,
        RS232Address,
        Name,
        Serial,
        Model,
        Parent,
        Rang,
        QC,
        Project
    }

    public enum NetworkMode
    {
        Transparent = 'T',
        Master = 'M',
        Slave = 'S'
    }

    public class DataDecoder(IDeviceGenerator deviceGenerator, IDialogCaller dialogCaller) : IDataDecoder
    {
        private readonly Dictionary<TableColumns, int> columnIndices = [];
        private readonly Dictionary<TableColumns, string> columnCaptions = new()
        {
            { TableColumns.Project, "Площадка" },
            { TableColumns.IPAddress, "IP" },
            { TableColumns.RS485Address, "RS485" },
            { TableColumns.RS232Address, "RS232" },
            { TableColumns.Name, "Обозначение" },
            { TableColumns.Serial, "Серийный номер" },
            { TableColumns.Model, "Модель" },
            { TableColumns.Parent, "Шкаф" },
            { TableColumns.Rang, "Rang" },
            { TableColumns.QC, "QC" }
        };

        private const int CaptionRow = 1;
        private const string QcPassed = "Passed";
        private const string QcDidntPass = "Failed!";

        private readonly Dictionary<C2000Ethernet, Tuple<char, int>> dictC2000Ethernet = [];
        private readonly IDeviceGenerator _devicesGenerator = deviceGenerator;
        private readonly IDialogCaller _dialogCaller = dialogCaller;

        public ITablesManager Driver { get; set; }

        public IEnumerable<Cabinet> GetCabinets(string excelFileFullPath)
        {
            Driver.SetCurrentDocument(excelFileFullPath);
            ResetColumnIndices();
            FindColumnIndexesByHeader();

            if (!IsTableCaptionValid())
            {
                return [];
            }

            var cabinets = GetCabinetContent();
            return GetCabinetsWithoutDashName(cabinets);
        }

        private void ResetColumnIndices()
        {
            columnIndices.Clear();
            foreach (TableColumns column in Enum.GetValues(typeof(TableColumns)))
            {
                columnIndices[column] = 0;
            }
        }

        private static IEnumerable<Cabinet> GetCabinetsWithoutDashName(List<Cabinet> cabinets)
        {
            foreach (var cabinet in cabinets)
            {
                if (!cabinet.Designation.Equals("-"))
                {
                    yield return cabinet;
                }
            }
        }

        private bool IsTableCaptionValid()
        {
            var isValid = columnIndices.Values.All(index => index != 0);
            if (!isValid)
            {
                _dialogCaller.ShowMessage("Error! There is no valid caption in the table!");
            }
            return isValid;
        }

        private List<Cabinet> GetCabinetContent()
        {
            var cabinetsLst = new List<Cabinet>();
            var cabinet = new Cabinet();
            var lastDevCabinet = "";
            var lastDevProject = "";

            for (var rowIndex = CaptionRow + 1; rowIndex <= Driver.Rows; rowIndex++)
            {
                var deviceDataSet = CreateDeviceDataSet(rowIndex, lastDevProject, lastDevCabinet);

                if (!string.Equals(deviceDataSet.DevCabinet, lastDevCabinet))
                {
                    if (rowIndex != CaptionRow + 1)
                    {
                        cabinetsLst.Add(cabinet);
                    }

                    cabinet = CreateNewCabinet(rowIndex, deviceDataSet);
                }

                ProcessDevice(deviceDataSet, cabinet);

                if (rowIndex == Driver.Rows)
                {
                    cabinetsLst.Add(cabinet);
                }

                lastDevCabinet = deviceDataSet.DevCabinet;
                lastDevProject = deviceDataSet.DevProject;
            }

            ProcessC2000EthernetDependencies();
            return cabinetsLst;
        }

        private DeviceDataSet CreateDeviceDataSet(int rowIndex, string lastDevProject, string lastDevCabinet)
        {
            TryParse(Driver.GetCellValueByIndex(rowIndex, columnIndices[TableColumns.RS232Address]), out int devRS232Addr);
            TryParse(Driver.GetCellValueByIndex(rowIndex, columnIndices[TableColumns.RS485Address]), out int devRS485Addr);

            return new DeviceDataSet
            {
                Id = rowIndex,
                DevProject = DefaultValue(Driver.GetCellValueByIndex(rowIndex, columnIndices[TableColumns.Project]), lastDevProject),
                DevCabinet = DefaultValue(Driver.GetCellValueByIndex(rowIndex, columnIndices[TableColumns.Parent]), lastDevCabinet),
                DevName = Driver.GetCellValueByIndex(rowIndex, columnIndices[TableColumns.Name]),
                DevModel = Driver.GetCellValueByIndex(rowIndex, columnIndices[TableColumns.Model]),
                DevIPAddr = Driver.GetCellValueByIndex(rowIndex, columnIndices[TableColumns.IPAddress]),
                DevSerial = Driver.GetCellValueByIndex(rowIndex, columnIndices[TableColumns.Serial]),
                DevRang = Driver.GetCellValueByIndex(rowIndex, columnIndices[TableColumns.Rang]),
                DevRS232Addr = devRS232Addr,
                DevRS485Addr = devRS485Addr,
                DevQcPassed = GetQcStatus(Driver.GetCellValueByIndex(rowIndex, columnIndices[TableColumns.QC]))
            };
        }

        private static Cabinet CreateNewCabinet(int rowIndex, DeviceDataSet deviceDataSet)
        {
            return new Cabinet
            {
                Id = rowIndex,
                Designation = deviceDataSet.DevCabinet,
                ParentName = deviceDataSet.DevProject
            };
        }

        private void ProcessDevice(DeviceDataSet deviceDataSet, Cabinet cabinet)
        {
            if (_devicesGenerator.TryGetDevice(deviceDataSet.DevModel, out var device))
            {
                var deviceWithSettings = GetDeviceWithSettings(device, deviceDataSet);
                deviceWithSettings.Cabinet = cabinet.Designation;

                if (device is C2000Ethernet c2000Ethernet)
                {
                    dictC2000Ethernet.Add(c2000Ethernet, GetRangTuple(deviceDataSet.DevRang));
                }

                cabinet.AddItem(deviceWithSettings);
            }
        }

        private void ProcessC2000EthernetDependencies()
        {
            FillDevicesDependencies(dictC2000Ethernet, (char)NetworkMode.Master, (char)NetworkMode.Slave);
            FillDevicesDependencies(dictC2000Ethernet, (char)NetworkMode.Slave, (char)NetworkMode.Master);
        }

        private bool GetQcStatus(string qcStatus)
        {
            return qcStatus != null && qcStatus.Equals(QcPassed);
        }

        private static Tuple<char, int> GetRangTuple(string rang)
        {
            if (string.IsNullOrEmpty(rang) || rang.Length < 2)
            {
                return null;
            }

            var mode = rang[0];
            var lineStr = rang[1..];

            if (!Enum.GetValues<NetworkMode>().Any(m => (char)m == mode))
            {
                return null;
            }

            if (!TryParse(lineStr, out var lineNumb))
            {
                return null;
            }

            return new Tuple<char, int>(mode, lineNumb);
        }

        private static void FillDevicesDependencies(Dictionary<C2000Ethernet, Tuple<char, int>> ethDevices, char dep1, char dep2)
        {
            foreach (var device in ethDevices)
            {
                SetDeviceNetworkMode(device.Key, device.Value.Item1);

                if (device.Value.Item1 != dep1)
                {
                    continue;
                }

                foreach (var item in ethDevices.Where(x =>
                    x.Value.Item1 == dep2 && device.Value.Item2 == x.Value.Item2))
                {
                    device.Key.RemoteDevicesList.Add(item.Key);
                }
            }
        }

        private static void SetDeviceNetworkMode(C2000Ethernet device, char mode)
        {
            device.NetworkMode = mode switch
            {
                (char)NetworkMode.Transparent => C2000Ethernet.Mode.transparent,
                (char)NetworkMode.Master => C2000Ethernet.Mode.master,
                (char)NetworkMode.Slave => C2000Ethernet.Mode.slave,
                _ => throw new ArgumentException($"Invalid network mode: {mode}")
            };
        }

        private static ICommunicationDevice GetDeviceWithSettings(ICommunicationDevice device, DeviceDataSet settings) =>
            device switch
            {
                EthernetSwitch ethernetSwitch => FillEthernetSwitchSettings(settings, ethernetSwitch),
                C2000Ethernet c2000Ethernet => FillC2000EthernetSettings(settings, c2000Ethernet),
                RS485device rs485Device => FillRS485Settings(rs485Device, settings),
                _ => device
            };

        private static ICommunicationDevice FillCommonDeviceSettings(ICommunicationDevice device, DeviceDataSet settings)
        {
            device.Id = settings.Id;
            device.Designation = settings.DevName;
            device.Model = settings.DevModel;
            device.Serial = settings.DevSerial;
            device.QualityControlPassed = settings.DevQcPassed;
            return device;
        }

        private static RS485device FillRS485Settings(RS485device device, DeviceDataSet settings)
        {
            FillCommonDeviceSettings(device, settings);
            device.AddressRS485 = (uint)settings.DevRS485Addr;
            return device;
        }

        private static C2000Ethernet FillC2000EthernetSettings(DeviceDataSet settings, C2000Ethernet device)
        {
            FillCommonDeviceSettings(device, settings);
            device.AddressRS485 = (uint)settings.DevRS485Addr;
            device.AddressRS232 = settings.DevRS232Addr;
            device.AddressIP = settings.DevIPAddr;
            device.NetName = settings.DevName;
            return device;
        }

        private static EthernetSwitch FillEthernetSwitchSettings(DeviceDataSet settings, EthernetSwitch device)
        {
            FillCommonDeviceSettings(device, settings);
            device.AddressIP = settings.DevIPAddr;
            return device;
        }

        private void FindColumnIndexesByHeader()
        {
            for (var colIndex = 1; colIndex <= Driver.Columns; colIndex++)
            {
                var content = Driver.GetCellValueByIndex(CaptionRow, colIndex);
                foreach (var caption in columnCaptions)
                {
                    if (content == caption.Value)
                    {
                        columnIndices[caption.Key] = colIndex;
                    }
                }
            }
        }

        public async Task<bool> SaveSerialNumberAsync(int id, string serialNumber)
        {
            Driver.SetCellValueByIndex(serialNumber, id, columnIndices[TableColumns.Serial]);
            return await SaveCurrentPackageAsync();
        }

        public async Task<bool> SaveQualityControlPassedAsync(int id, bool qualityControlPassed)
        {
            var qcColumnIndex = columnIndices[TableColumns.QC];
            if (qualityControlPassed)
            {
                Driver.SetCellColor(Color.Black, id, qcColumnIndex);
                Driver.SetCellValueByIndex(QcPassed, id, qcColumnIndex);
            }
            else
            {
                Driver.SetCellColor(Color.Red, id, qcColumnIndex);
                Driver.SetCellValueByIndex(QcDidntPass, id, qcColumnIndex);
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