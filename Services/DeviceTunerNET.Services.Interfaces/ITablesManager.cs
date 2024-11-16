using System.Threading.Tasks;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface ITablesManager
    {
        public int Rows { get; set; }
        public int Columns { get; set; }

        bool SetCurrentDocument(string document);
        bool SetCurrentPageByName(string pageName);

        void GetLastRowAndColumnIndex();

        string GetCellValueByIndex(int row, int column);
        void SetCellValueByIndex(string cellValue, int row, int column);

        void SetCellColor(System.Drawing.Color color, int row, int column);

        Task SaveAsync();
    }
}
