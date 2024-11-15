namespace DeviceTunerNET.Services.Interfaces
{
    public interface ITablesManager
    {
        public int Rows { get; set; }
        public int Columns { get; set; }

        bool SetCurrentDocument(string document);
        bool SetCurrentPageByName(string pageName);
        bool CreateNewPage(string pageName);

        int GetLastRowIndex();
        int GetLastColumnIndex();

        string GetCellValueByIndex(int row, int column);
        void SetCellValueByIndex(string cellValue, int row, int column);

        void SetCellColor(System.Drawing.Color color, int row, int column);

        void Save();
    }
}
