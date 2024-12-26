using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_Kdl2i_isp1 : C2000_Kdl2i
    {
        public const int Code = 81;
        public C2000_Kdl2i_isp1(IPort port) : base(port)
        {
            ModelCode = Code;
            Model = "С2000-КДЛ-2И исп.01";
            SupportedModels = new List<string>
            {
                Model,
            };
        }

        public override bool Setup(Action<int> updateProgressBar, int modelCode = 0)
        {
            return base.Setup(updateProgressBar, Code);
        }
    }
}
