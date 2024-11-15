using DeviceTunerNET.SharedModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IGoogleDriveSheetsLister
    {
        Task<IEnumerable<UrlItem>> ListAllSpreadsheetsAsync();
    }
}
