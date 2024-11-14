using DeviceTunerNET.SharedDataModel;
using System.Collections.Generic;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IDataDecoder
    {
        List<Cabinet> GetCabinetsAsync(string ExcelFileFullPath);
        //bool SaveDevice<T>(T arg) where T : SimplestСomponent;
        bool SaveSerialNumber(int id, string serialNumber);

        bool SaveQualityControlPassed(int id, bool qualityControlPassed);
    }
}
