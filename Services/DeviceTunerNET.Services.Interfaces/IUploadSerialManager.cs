using DeviceTunerNET.SharedDataModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IUploadSerialManager: IDisposable
    {
        string Protocol { get; set; }
        string PortName { get; set; }
        Action<int> UpdateProgressBar { get; set; }
        Task<bool> QualityControlAsync(IOrionDevice device);
        bool Upload(IOrionDevice device, string serialNumb);
    }
}
