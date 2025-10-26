using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IAuthLoader
    {
        string Capsule { get; }
        IEnumerable<string> AvailableServicesNames { get; set; }
        Task<IEnumerable<string>> GetAvailableServices();
    }
}
