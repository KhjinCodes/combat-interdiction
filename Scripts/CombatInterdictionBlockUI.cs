using Sandbox.Game.Localization;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Text;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace Khjin.CombatInterdiction
{
    public static class CombatInterdictionBlockUI
    {
        const string MOD_PREFIX = "KJN_CI_";
        static bool Done = false;

        public static void DoOnce(IMyModContext context)
        {
            if (Done) { return; }
            Done = true;
            CreateControls();
            CreateActions(context);
        }

        static bool CustomVisibleCondition(IMyTerminalBlock b)
        {
            // Only applicable to cockpits that can actually control ship
            var logic = b?.GameLogic?.GetAs<CombatInterdictionBlock>();
            var cockpit = b as IMyCockpit;
            return logic != null && cockpit != null && cockpit.CanControlShip;
        }

        static bool CustomEnabledCondition(IMyTerminalBlock b)
        {
            // Only applicable to cockpits that can actually control ship
            var cockpit = b as IMyCockpit;
            return cockpit != null && cockpit.CanControlShip;
        }

        static void CreateControls()
        {
            { // Add a separator
                var c = MyAPIGateway
                    .TerminalControls.CreateControl<IMyTerminalControlSeparator, 
                    IMyCockpit>("");
                c.SupportsMultipleBlocks = false;
                c.Visible = CustomVisibleCondition;

                MyAPIGateway.TerminalControls.AddControl<IMyCockpit>(c);
            }
            { // Add supercruise button
                var c = MyAPIGateway.TerminalControls
                    .CreateControl<IMyTerminalControlOnOffSwitch, 
                    IMyCockpit>(MOD_PREFIX + "SuperCruise_OnOff");
                c.Title = MyStringId.GetOrCompute("Super Cruise");
                c.Tooltip = MyStringId.GetOrCompute("Adds a massive off-combat speed boost.");
                c.SupportsMultipleBlocks = false;
                c.Enabled = CustomEnabledCondition;
                c.Visible = CustomVisibleCondition;

                c.OnText = MySpaceTexts.SwitchText_On;
                c.OffText = MyStringId.GetOrCompute("OFF");

                // setters and getters should both be assigned on all controls that have them, to avoid errors in mods or PB scripts getting exceptions from them.
                c.Getter = (b) => b?.GameLogic?.GetAs<CombatInterdictionBlock>()?.SuperCruise ?? false;
                c.Setter = (b, v) =>
                {
                    var logic = b?.GameLogic?.GetAs<CombatInterdictionBlock>();
                    if (logic != null) { logic.SuperCruise = v; }
                };

                MyAPIGateway.TerminalControls.AddControl<IMyCockpit>(c);
            }
        }

        static void CreateActions(IMyModContext context)
        {
            { // ON
                var superCruiseAction = MyAPIGateway.TerminalControls.CreateAction<IMyCockpit>(MOD_PREFIX + "Action_SuperCruise_On");
                superCruiseAction.Name = new StringBuilder("Super Cruise On");
                superCruiseAction.ValidForGroups = false;
                superCruiseAction.Icon = @"Textures\GUI\Icons\Actions\MissileSwitchOn.dds";
                // superCruiseAction.Icon = Path.Combine(context.ModPath, @"Textures\YourIcon.dds");
                superCruiseAction.Action = (b) =>
                {
                    var logic = b?.GameLogic?.GetAs<CombatInterdictionBlock>();
                    if(logic != null) { logic.SuperCruise = true; }
                };
                superCruiseAction.Writer = (b, sb) =>
                {
                    sb.AppendLine(" ");
                    sb.AppendLine("Boost");
                    sb.AppendLine("On");
                };
                superCruiseAction.Enabled = CustomEnabledCondition;
                MyAPIGateway.TerminalControls.AddAction<IMyCockpit>(superCruiseAction);
            }

            { // OFF
                var superCruiseAction = MyAPIGateway.TerminalControls.CreateAction<IMyCockpit>(MOD_PREFIX + "Action_SuperCruise_Off");
                superCruiseAction.Name = new StringBuilder("Super Cruise Off");
                superCruiseAction.ValidForGroups = false;
                superCruiseAction.Icon = @"Textures\GUI\Icons\Actions\MissileSwitchOff.dds";
                // superCruiseAction.Icon = Path.Combine(context.ModPath, @"Textures\YourIcon.dds");
                superCruiseAction.Action = (b) =>
                {
                    var logic = b?.GameLogic?.GetAs<CombatInterdictionBlock>();
                    if (logic != null) { logic.SuperCruise = false; }
                };
                superCruiseAction.Writer = (b, sb) =>
                {
                    sb.AppendLine(" ");
                    sb.AppendLine("Boost");
                    sb.AppendLine("Off");
                };
                superCruiseAction.Enabled = CustomEnabledCondition;
                MyAPIGateway.TerminalControls.AddAction<IMyCockpit>(superCruiseAction);
            }

            { // ON/OFF
                var superCruiseAction = MyAPIGateway.TerminalControls.CreateAction<IMyCockpit>(MOD_PREFIX + "Action_SuperCruise_OnOff");
                superCruiseAction.Name = new StringBuilder("Super Cruise On/Off");
                superCruiseAction.ValidForGroups = false;
                superCruiseAction.Icon = @"Textures\GUI\Icons\Actions\MissileToggle.dds";
                // superCruiseAction.Icon = Path.Combine(context.ModPath, @"Textures\YourIcon.dds");

                superCruiseAction.Action = (b) =>
                {
                    var logic = b?.GameLogic?.GetAs<CombatInterdictionBlock>();
                    if (logic != null) { logic.SuperCruise = (!logic.SuperCruise); }
                };
                superCruiseAction.Writer = (b, sb) =>
                {
                    var logic = b?.GameLogic?.GetAs<CombatInterdictionBlock>();
                    if (logic != null)
                    {
                        sb.AppendLine(" ");
                        sb.AppendLine("Boost");
                        sb.AppendLine(logic.SuperCruise ? "On" : "Off");
                    }
                };
                superCruiseAction.Enabled = CustomEnabledCondition;
                MyAPIGateway.TerminalControls.AddAction<IMyCockpit>(superCruiseAction);
            }
        }
    }
}