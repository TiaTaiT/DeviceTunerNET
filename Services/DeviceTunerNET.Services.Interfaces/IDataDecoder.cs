using DeviceTunerNET.SharedDataModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IDataDecoder
    {
        ITablesManager Driver { get; set; }
        List<Cabinet> GetCabinets(string ExcelFileFullPath);
        //bool SaveDevice<T>(T arg) where T : SimplestСomponent;
        bool SaveSerialNumber(int id, string serialNumber);

        bool SaveQualityControlPassed(int id, bool qualityControlPassed);
    }
}
