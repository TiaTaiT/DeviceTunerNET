using DeviceTunerNET.Core;
using DeviceTunerNET.DymoModules;
using DeviceTunerNET.Modules.ModuleRS485;
using DeviceTunerNET.Modules.ModuleSwitch;
using DeviceTunerNET.Services;
using DeviceTunerNET.Services.SwitchesStrategies;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.Views;
using DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using System.IO.Ports;
using System.Windows;
using DeviceTunerNET.ViewModels;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using DeviceTunerNET.Modules.ModulePnr;
using Serilog;
using System.Linq;
using System.Threading.Tasks;
using Prism.Container.DryIoc;

namespace DeviceTunerNET
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private IEventAggregator _ea;
        private SerialPort _sp;

        protected override Window CreateShell()
        {
            _sp = new SerialPort();
            _ea = Container.Resolve<IEventAggregator>();
            _ea.GetEvent<MessageSentEvent>().Subscribe(MessageReceived);
            return Container.Resolve<MainWindow>();
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void SwitchToThisWindow(IntPtr hWnd, bool turnOn);
        
        protected override async void OnStartup(StartupEventArgs e)
        {
            var appName = Process.GetCurrentProcess().ProcessName;
            var sameProcesses = Process.GetProcessesByName(appName);
            
            if(sameProcesses != null && sameProcesses.Length > 1)
            {
                SwitchToThisWindow(sameProcesses[1].MainWindowHandle, true);
                Application.Current.Shutdown();
            }

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("log.txt")
                .CreateLogger();
            Log.Information($"Application startup.");

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information($"The application is closed.");
            Log.CloseAndFlush();

            base.OnExit(e);
        }

        private void MessageReceived(Message message)
        {   // A new SerialPort type object needs to be created.
            if (message.ActionCode == MessageSentEvent.UpdateRS485ComPort)
            {
                _sp.PortName = message.MessageString;
            }
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var container = containerRegistry.GetContainer();

            container.Register<IAuthLoader, AuthLoader>(Reuse.Singleton);
            container.Register<IMessageService, MessageService>(Reuse.Singleton);
            container.Register<IDataRepositoryService, DataRepositoryService>(Reuse.Singleton);

            container.Register<IFileDialogService, FileDialogService>(Reuse.Transient);
            container.Register<IDataDecoder, DataDecoder>(Reuse.Transient);
            container.Register<IPrintService, DymoModule>(Reuse.Transient);
            container.Register<ISwitchConfigUploader, Eltex>(Reuse.Transient);
            container.Register<ISerialSender, SerialSender>(Reuse.Transient);
            container.Register<ISerialTasks, SerialTasks>(Reuse.Transient);
            container.Register<INetworkUtils, NetworkUtils>(Reuse.Transient);
            container.Register<IDeviceGenerator, DeviceGenerator>(Reuse.Transient);
            container.Register<IDeviceSearcher, BolidDeviceSearcher>(Reuse.Transient);
            container.Register<IAddressChanger, BolidAddressChanger>(Reuse.Transient);
            container.Register<IDialogCaller, DialogCaller>(Reuse.Transient);
            container.Register<IUploadSerialManager, UploadSerialManager>(Reuse.Transient);
            container.Register<ITftpServerManager, TftpServerManager>(Reuse.Transient);
            container.Register<IConfigParser, ConfigParser>(Reuse.Transient);
            container.Register<IGoogleDriveSheetsLister, GoogleDriveSheetsLister>(Reuse.Transient);
            container.Register<IGoogleSpreadsheetCache, GoogleSpreadsheetCache>(Reuse.Transient);
            container.Register<INetmaskConverter, NetmaskConverter>(Reuse.Transient);

            containerRegistry.RegisterDialog<SerialDialog, SerialDialogViewModel>("SerialDialog");
            containerRegistry.RegisterDialog<OpenUrlDialog, OpenUrlDialogViewModel>("OpenUrlDialog");

            container.Register<ITablesManager, ExcelDriver>(serviceKey: DataSrvKey.excelKey);
            container.Register<ITablesManager, GoogleTablesDriver>(serviceKey: DataSrvKey.googleKey);

            container.Register<ISender, EltexMarvellTelnet>(serviceKey: SenderSrvKey.telnetMarvellKey);
            container.Register<ISender, EltexBroadcomTelnet>(serviceKey: SenderSrvKey.telnetBroadcomKey);
            container.Register<ISender, EltexSsh>(serviceKey: SenderSrvKey.sshKey);
        }

        protected override async void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            var authLoader = Container.Resolve<IAuthLoader>();
            var availableServices = Task.Run(authLoader.GetAvailableServices).Result;
            
            if(availableServices.Contains("RS485PAGE"))
                moduleCatalog.AddModule<ModuleRS485Module>();
            if (availableServices.Contains("SWITCHPAGE"))
                moduleCatalog.AddModule<ModuleSwitchModule>();
            if (availableServices.Contains("PNRPAGE"))
                moduleCatalog.AddModule<ModulePnrModule>();
        }
    }
}
