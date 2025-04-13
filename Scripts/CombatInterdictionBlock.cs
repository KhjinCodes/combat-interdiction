using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace Khjin.CombatInterdiction
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Cockpit), false)]
    public class CombatInterdictionBlock : MyGameLogicComponent
    {
        IMyCockpit cockpit;
        public static Guid SuperCruiseKey = new Guid("2d14d3e8a962424db0114056c53bbb01");

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            CombatInterdictionBlockUI.DoOnce(ModContext);
            cockpit = (IMyCockpit)Entity;
            if (cockpit.CubeGrid?.Physics == null)
                return;
        }

        public bool SuperCruise
        {
            get
            {
                if (cockpit == null) { return false; }
                if (cockpit.Storage == null)
                { cockpit.Storage = new MyModStorageComponent(); }
                if (cockpit.Storage.ContainsKey(SuperCruiseKey))
                { return bool.Parse(cockpit.Storage[SuperCruiseKey]); }
                else
                { cockpit.Storage.Add(SuperCruiseKey, "false"); return false; }
            }
            set
            {
                if (cockpit == null) { return; }
                if (cockpit.Storage == null)
                { cockpit.Storage = new MyModStorageComponent(); }
                if (cockpit.Storage.ContainsKey(SuperCruiseKey))
                { cockpit.Storage[SuperCruiseKey] = value.ToString(); }
                else
                { cockpit.Storage.Add(SuperCruiseKey, value.ToString()); }
            }
        }
    }
}
