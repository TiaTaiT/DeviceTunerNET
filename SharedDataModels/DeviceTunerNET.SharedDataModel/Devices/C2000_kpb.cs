﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_kpb : OrionDevice
    {
        public new const int ModelCode = 15;
        public new const int Code = 15;
        public C2000_kpb(IPort port) : base(port)
        {
            Model = "С2000-КПБ";
            SupportedModels = new List<string>
            {
                Model
            };
        }

        public override bool Setup(Action<int> updateProgressBar, int modelCode = 0)
        {
            return base.Setup(updateProgressBar, Code);
        }
    }
}
