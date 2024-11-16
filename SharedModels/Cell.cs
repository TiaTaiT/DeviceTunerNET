using Google.Apis.Sheets.v4.Data;

namespace DeviceTunerNET.SharedModels
{
    public class Cell
    {
        private string? _value;
        public string? Value
        {
            get => _value;
            set
            {
                _value = value;
                WasChanged = true;
            }
        }

        public bool WasChanged { get; set; }

        private Cell() { }

        public Cell(string cellValue)
        {
            _value = cellValue;
        }
    }
}
