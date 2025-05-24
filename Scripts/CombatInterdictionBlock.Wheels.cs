using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;


namespace Khjin.CombatInterdiction
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MotorSuspension), false)]
    public class CombatInterdictionWheels : MyGameLogicComponent
    {
        IMyMotorSuspension suspension;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateBeforeSimulation100()
        {
            suspension = Entity as IMyMotorSuspension;
            if (suspension == null) { return; }
            if (suspension.TopGrid?.Physics != null)
            {
                if (suspension.IsWorking && !suspension.TopGrid.Physics.IsActive)
                {
                    suspension.TopGrid.Physics.Activate();
                }
                else if (!suspension.IsWorking && suspension.TopGrid.Physics.IsActive)
                {
                    suspension.TopGrid.Physics.Deactivate();
                }
            }
        }
    }
}
