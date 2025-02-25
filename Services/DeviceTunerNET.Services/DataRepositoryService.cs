﻿using DeviceTunerNET.Core;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using DryIoc;
using Prism.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace DeviceTunerNET.Services
{
    public class DataRepositoryService : IDataRepositoryService
    {
        private List<Cabinet> _cabinetsLst = new();

        private readonly IEventAggregator _ea;
        private readonly IResolverContext _resolver;
        private IDataDecoder _decoder;
        private Dispatcher _dispatcher;
        private int _dataProviderType = 1;

        public DataRepositoryService(IEventAggregator ea, IDataDecoder dataDecoder, IResolverContext resolverContext)
        {
            _ea = ea;
            _resolver = resolverContext;
            _decoder = dataDecoder;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public void SetDevices(int DataProviderType, string FullPathToData)
        {
            _dataProviderType = DataProviderType;
            var _fullPathToData = FullPathToData;
            _dispatcher.BeginInvoke(() =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
            });
            _cabinetsLst.Clear();
            switch (_dataProviderType)
            {
                case 1:
                    _decoder.Driver = _resolver.Resolve<ITablesManager>(serviceKey: DataSrvKey.excelKey);
                    _cabinetsLst = _decoder.GetCabinets(_fullPathToData).ToList();
                    break;
                case 2:
                    _decoder.Driver = _resolver.Resolve<ITablesManager>(serviceKey: DataSrvKey.googleKey);
                    _cabinetsLst = _decoder.GetCabinets(_fullPathToData).ToList();
                    break;
            }
            //Сообщаем всем об обновлении данных в репозитории
            _dispatcher.BeginInvoke(() => _ea.GetEvent<MessageSentEvent>().Publish(new Message
            {
                ActionCode = MessageSentEvent.RepositoryUpdated
            }));
            _dispatcher.BeginInvoke(() =>
            {
                Mouse.OverrideCursor = null;
            });
        }

        public async Task<bool> SaveSerialNumberAsync(int id, string serialNumber)
        {
            /*
            if (_dataProviderType != 1)
                return false;
            */
            return await _decoder.SaveSerialNumberAsync(id, serialNumber);
        }

        public async Task<bool> SaveQualityControlPassedAsync(int id, bool qualityControlPassed)
        {
            /*
            if (_dataProviderType != 1)
                return false;
            */
            return await _decoder.SaveQualityControlPassedAsync(id, qualityControlPassed);
        }

        public IList<Cabinet> GetCabinetsWithTwoTypeDevices<T1, T2>()
            where T1 : ISimplestComponent
            where T2 : ISimplestComponent
        {
            var cabinetsWithdevs = new List<Cabinet>();
            foreach (var cabinet in _cabinetsLst)
            {
                var devicesListT1 = (List<T1>)cabinet.GetDevicesList<T1>();
                var devicesListT2 = (List<T2>)cabinet.GetDevicesList<T2>();

                // Будем работать только с теми шкафами в которых есть приборы типа T1 или T2
                if (devicesListT1.Count <= 0 && devicesListT2.Count <= 0) 
                    continue;

                // В возвращаемом из метода списке будем создавать новые шкафы
                var newCabinet = new Cabinet
                {
                    Id = cabinet.Id,
                    ParentName = cabinet.ParentName,
                    Designation = cabinet.Designation,
                    DeviceType = cabinet.DeviceType
                };

                foreach (var item in devicesListT1)
                {
                    newCabinet.AddItem(item);
                }

                foreach (var item in devicesListT2)
                {
                    newCabinet.AddItem(item);
                }
                cabinetsWithdevs.Add(newCabinet);
            }
            return cabinetsWithdevs;
        }

        public IList<Cabinet> GetCabinetsWithDevices<T>() where T : ISimplestComponent
        {
            var cabinetsWithdevs = new List<Cabinet>();
            foreach (var cabinet in _cabinetsLst)
            {
                var devicesList = (List<T>)cabinet.GetDevicesList<T>();
                if (devicesList.Count <= 0)
                    continue;

                var newCabinet = new Cabinet
                {
                    Id = cabinet.Id,
                    ParentName = cabinet.ParentName,
                    Designation = cabinet.Designation,
                    DeviceType = cabinet.DeviceType
                };

                foreach (var item in devicesList)
                {
                    newCabinet.AddItem(item);
                }
                cabinetsWithdevs.Add(newCabinet);
            }
            return cabinetsWithdevs;
        }

        public IList<Cabinet> GetFullCabinets()
        {
            return _cabinetsLst;
        }

        public IList<T> GetAllDevices<T>() where T : ISimplestComponent
        {
            var cabinets = GetCabinetsWithDevices<T>();
            var resultDevices = new List<T>();
            foreach (var cabinet in cabinets)
            {
                foreach (var device in cabinet.GetDevicesList<T>())
                {
                    resultDevices.Add(device);
                }
            }
            return resultDevices;
        }

        public IList<Cabinet> AddTwoListsOfCabinets(IList<Cabinet> list1, IList<Cabinet> list2)
        {
            var cabOut = new List<Cabinet>();

            foreach (var cab485 in list1)
            {
                var newCab = new Cabinet
                {
                    Id = cab485.Id,
                    ParentName = cab485.ParentName,
                    Designation = cab485.Designation
                };

                foreach (IOrionDevice device485 in cab485.GetAllDevicesList)
                {

                    newCab.AddItem(device485);
                }
                cabOut.Add(newCab);
            }

            foreach (var cab232 in list2)
            {
                foreach (var cab485 in cabOut)
                {
                    if (!cab232.Designation.Equals(cab485.Designation))
                        continue;

                    foreach (EthernetOrionDevice device232 in cab232.GetAllDevicesList)
                    {
                        cab485.AddItem(device232);
                    }
                }
            }

            foreach (var cab232 in list2)
            {
                var compare = false;
                foreach (var cab485 in list1)
                {
                    if (!cab232.Designation.Equals(cab485.Designation))
                        continue;

                    compare = true;
                    break;

                }
                if (!compare)
                    cabOut.Add(cab232);
            }
            return cabOut;
        }

        public IEnumerable<Cabinet> GetCabinetsWithoutExcludeDevices<T>() where T : ISimplestComponent
        {

            foreach(var cabinet in GetFullCabinets())
            {
                var newCabinet = new Cabinet()
                {
                    Id = cabinet.Id,
                    Designation = cabinet.Designation,
                    DeviceType = cabinet.DeviceType,
                    ParentName = cabinet.ParentName,
                };

                var devices = cabinet.GetAllDevicesList.Where(c => c is not T).Cast<SimplestСomponent>();
                newCabinet.AddItems(devices);

                yield return newCabinet;
            }
        }

    }
}
