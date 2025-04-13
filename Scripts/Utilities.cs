using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;

namespace Khjin.CombatInterdiction
{
    public class Utilities
    {
        public static bool IsServer()
        {
            return (MyAPIGateway.Multiplayer.IsServer || MyAPIGateway.Utilities.IsDedicated);
        }

        public static void Log(string message)
        {
            MyLog.Default.WriteLineAndConsole(message);
        }

        public static void ChatMessage(string senderName, string message)
        {
            if (MyAPIGateway.Session?.Player != null)
            {
                MyAPIGateway.Utilities.ShowMessage(senderName, message);
            }
        }

        public static void NotifyMessage(string message, string fontColor = "White", int durationMs = 1500)
        {
            if (MyAPIGateway.Session?.Player != null)
            {
                MyAPIGateway.Utilities.ShowNotification(message, durationMs, fontColor);
            }
        }

        public static void MessagePlayer(string message, long playerId, string font="Blue")
        {
            MyVisualScriptLogicProvider.SendChatMessage(message, playerId: playerId, font: font);
        }

        public static IMyCubeGrid GetBaseGrid(IMyCubeGrid cubeGrid)
        {
            return (IMyCubeGrid)cubeGrid.GetTopMostParent(typeof(IMyCubeGrid));
        }

        public static bool IsNpcOwned(IMyCubeGrid cubeGrid)
        {
            // No owners, consider non-npc to guard against exploits
            if (cubeGrid.SmallOwners == null || cubeGrid.SmallOwners.Count == 0)
            { return false; }
            if (cubeGrid.BigOwners == null || cubeGrid.BigOwners.Count == 0)
            { return false; }

            // Check the faction
            long ownerId = cubeGrid.BigOwners[0];
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(ownerId);
            if (faction == null)
            { return false; }
            else
            { return !faction.AcceptHumans || faction.IsEveryoneNpc();}
        }
    }
}
