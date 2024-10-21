using DeviceTunerNET.Core.Mvvm;
using DeviceTunerNET.SharedDataModel;
using System.Collections.ObjectModel;


namespace DeviceTunerNET.Modules.ModuleSwitch.ViewModels
{
    public partial class ViewSwitchViewModel : RegionViewModelBase
    {
        #region Properties

        private ObservableCollection<string> _availableStrategies;
        public ObservableCollection<string> AvailableStrategies
        {
            get => _availableStrategies;
            set => SetProperty(ref _availableStrategies, value);
        }

        private string _selectedStrategy;
        public string SelectedStrategy
        {
            get => _selectedStrategy;
            set
            {
                SetProperty(ref _selectedStrategy, value);
            }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private string _defaultLogin = "admin";
        public string DefaultLogin
        {
            get => _defaultLogin;
            set => SetProperty(ref _defaultLogin, value);
        }

        private string _newLogin = "admin";
        public string NewLogin
        {
            get => _newLogin;
            set => SetProperty(ref _newLogin, value);
        }

        private string _defaultPassword = "admin";
        public string DefaultPassword
        {
            get => _defaultPassword;
            set => SetProperty(ref _defaultPassword, value);
        }

        private string _newPassword = "admin123";
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        private string _defaultIP = "192.168.1.239";
        public string DefaultIP
        {
            get => _defaultIP;
            set => SetProperty(ref _defaultIP, value);
        }

        private int _ipMask = 23;
        public int IPMask
        {
            get => _ipMask;
            set => SetProperty(ref _ipMask, value);
        }

        private EthernetSwitch _selectedDevice;
        public EthernetSwitch SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                // Если пошкафная настройка коммутаторов и выделен коммутатор => можно активировать кнопку старт
                if (IsCheckedByCabinets)
                {
                    IsCanDoStart = true;
                    IsSelectedSwitchCanBePrinted = !string.IsNullOrEmpty(value.Serial);
                }
                SetProperty(ref _selectedDevice, value);
            }
        }

        private string _selectedPrinter;
        public string SelectedPrinter
        {
            get => _selectedPrinter;
            set
            {
                if (value != null)
                {
                    IsCanBePrinted = true;
                }
                SetProperty(ref _selectedPrinter, value);
            }
        }

        private string _printLabelPath = "Resources\\Files\\label25x25switch.dymo";
        public string PrintLabelPath
        {
            get => _printLabelPath;
            set => SetProperty(ref _printLabelPath, value);
        }

        private string _currentItemTextBox = "0";
        public string CurrentItemTextBox
        {
            get => _currentItemTextBox;
            set => SetProperty(ref _currentItemTextBox, value);
        }

        private string _messageForUser = "Подключи \r\n коммутатор";
        public string MessageForUser
        {
            get => _messageForUser;
            set => SetProperty(ref _messageForUser, value);
        }

        private bool _sliderIsChecked = false;
        public bool SliderIsChecked
        {
            get => _sliderIsChecked;
            set => SetProperty(ref _sliderIsChecked, value);
        }

        private string _observeConsole;
        public string ObserveConsole
        {
            get => _observeConsole;
            set => SetProperty(ref _observeConsole, value);
        }

        private ObservableCollection<EthernetSwitch> _switchList;
        public ObservableCollection<EthernetSwitch> SwitchList //Список коммутаторов
        {
            get => _switchList;
            set => SetProperty(ref _switchList, value);
        }

        private ObservableCollection<string> _printers = new ObservableCollection<string>();
        public ObservableCollection<string> Printers
        {
            get => _printers;
            set => SetProperty(ref _printers, value);
        }

        private bool _isCheckedByCabinets;
        public bool IsCheckedByCabinets
        {
            get => _isCheckedByCabinets;
            set => SetProperty(ref _isCheckedByCabinets, value);
        }

        private bool _isCheckedByArea;
        public bool IsCheckedByArea
        {
            get => _isCheckedByArea;
            set
            {
                if(SwitchList.Count > 0)
                    IsCanDoStart = value;
                SetProperty(ref _isCheckedByArea, value);
            }
        }

        private bool _isCanDoStart;
        public bool IsCanDoStart
        {
            get => _isCanDoStart;
            set
            {
                CheckedCommand.RaiseCanExecuteChanged();
                SetProperty(ref _isCanDoStart, value);
            }
        }

        private bool _isCanBePrinted;
        public bool IsCanBePrinted
        {
            get => _isCanBePrinted;
            set
            {
                PrintTestLabel.RaiseCanExecuteChanged();
                SetProperty(ref _isCanBePrinted, value);
            }
        }

        private bool _allowPrintLabel;
        public bool AllowPrintLabel
        {
            get => _allowPrintLabel;
            set => SetProperty(ref _allowPrintLabel, value);
        }

        private bool _isSelectedSwitchCanBePrinted;
        public bool IsSelectedSwitchCanBePrinted
        {
            get => _isSelectedSwitchCanBePrinted;
            set
            {
                SetProperty(ref _isSelectedSwitchCanBePrinted, value);
            }
        }
        #endregion
    }
}
