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
        Task<bool> SaveSerialNumberAsync(int id, string serialNumber);

        Task<bool> SaveQualityControlPassedAsync(int id, bool qualityControlPassed);
    }
}
