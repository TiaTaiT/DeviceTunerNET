using System.Collections.Generic;

namespace DeviceTunerNET.SharedDataModel
{
    public class Device : SimplestСomponent, IDevice
    {
        /// <summary>
        /// Серийный номер прибора ("456426")
        /// </summary>
        public string Serial { get; set; } = string.Empty;

        /// <summary>
        /// Версия железяки
        /// </summary>
        public string HardwareVersion { get; set; } = string.Empty;

        /// <summary>
        /// Версия прошивки
        /// </summary>
        public string FirmwareVersion { get; set; } = string.Empty;

        /// <summary>
        /// Прибор прошёл проверку в собранном шкафу
        /// </summary>
        public bool QualityControlPassed { get; set; }

        /// <summary>
        /// Название площадки на которой находится шкаф с этим прибором ("КС "Невинномысская"")
        /// </summary>
        public string Area { get; set; } = string.Empty;

        /// <summary>
        /// Наименование шкафа в котором находится этот прибор ("ШКО1")
        /// </summary>
        public string Cabinet { get; set; } = string.Empty;

        /// <summary>
        /// Список всех наименований приборов с этим конфигом
        /// </summary>
        public IEnumerable<string> SupportedModels { get; set; }
    }
}
