using DeviceTunerNET.SharedModels;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IGoogleSpreadsheetCache
    {
        Cell[,] Cache { get; }
        void PopulateCache(string spreadsheetId, string sheetName);
    }    
}
