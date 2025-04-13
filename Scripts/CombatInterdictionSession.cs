using Sandbox.Definitions;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;

namespace Khjin.CombatInterdiction
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public partial class CombatInterdictionSession : MySessionComponentBase
    {
        public static CombatInterdictionSession Instance { get; private set; }

        public const string MOD_VERSION = "1.0";
        public const string MOD_NAME = "Combat Interdiction";

        // Handles messages through chat input and from other sessions (players, server)
        public CombatInterdictionMessaging Messaging { get; private set; } = null;

        // Handles the loading, saving, and updating of settings
        public CombatInterdictionSettings Settings { get; private set; } = null;

        // Handles the commands parsing and execution
        public CombatInterdictionCommands Commands { get; private set; } = null;

        // Handles the main logic of the mod
        private CombatInterdictionLogic Logic;

        // Tracks ship instances
        private Dictionary<long, Ship> _ships;

        // Take note of original speed settings
        private float defaultSmallGridMaxSpeed;
        private float defaultLargeGridMaxSpeed;
        private const float MODDED_SMALL_GRID_MAX_SPEED = 500f;
        private const float MODDED_LARGE_GRID_MAX_SPEED = 500f;

        public CombatInterdictionSession()
        {
            Messaging = new CombatInterdictionMessaging();
            Settings = new CombatInterdictionSettings();
            Commands = new CombatInterdictionCommands();
            Logic = new CombatInterdictionLogic();
            _ships = new Dictionary<long, Ship>();
        }

        public override void LoadData()
        {
            Instance = this;

            // Load the Managers
            Messaging.LoadData();
            Settings.LoadData();
            Commands.LoadData();
            Logic.LoadData();

            if (Utilities.IsServer())
            {
                Instance = this;
                MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
            }

            // Update the world speed
            defaultSmallGridMaxSpeed = MyDefinitionManager.Static.EnvironmentDefinition.SmallShipMaxSpeed;
            defaultLargeGridMaxSpeed = MyDefinitionManager.Static.EnvironmentDefinition.LargeShipMaxSpeed;
            MyDefinitionManager.Static.EnvironmentDefinition.SmallShipMaxSpeed = MODDED_SMALL_GRID_MAX_SPEED;
            MyDefinitionManager.Static.EnvironmentDefinition.LargeShipMaxSpeed = MODDED_LARGE_GRID_MAX_SPEED;

            // Listen to messages entered via chat
            MyAPIGateway.Utilities.MessageEntered += MessageEntered;
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= MessageEntered;

            // Clear references to ships
            if (Utilities.IsServer())
            {
                MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdd;
                _ships.Clear();
            }

            // Reset Speed
            MyDefinitionManager.Static.EnvironmentDefinition.SmallShipMaxSpeed = defaultSmallGridMaxSpeed;
            MyDefinitionManager.Static.EnvironmentDefinition.LargeShipMaxSpeed = defaultLargeGridMaxSpeed;

            // Unload the managers
            Logic.UnloadData();
            Commands.UnloadData();
            Settings.UnloadData(); // Saves settings before unload
            Messaging.UnloadData();

            Instance = null;
        }

        public override void BeforeStart()
        {
            if (Utilities.IsServer())
            {
                MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(int.MaxValue, AfterDamageHandler);
            }
        }

        private void OnEntityAdd(IMyEntity entity)
        {
            IMyCubeGrid grid = entity as IMyCubeGrid;
            if (grid == null) { return; }

            grid = Utilities.GetBaseGrid(grid);
            if (grid.Physics != null)
            {
                if(!_ships.ContainsKey(entity.EntityId))
                {
                    grid.OnMarkForClose += OnMarkForClose;
                    Ship ship = new Ship(grid);
                    _ships.Add(ship.EntityId, ship);
                }
            }
            grid.OnIsStaticChanged += OnIsStaticChanged;
        }

        private void OnIsStaticChanged(IMyCubeGrid grid, bool isStatic)
        {
            var baseEntity = Utilities.GetBaseGrid(grid);
            if (isStatic)
            {
                if (_ships.ContainsKey(baseEntity.EntityId))
                {
                    _ships[baseEntity.EntityId].Grid.OnMarkForClose -= OnMarkForClose;
                    _ships.Remove(baseEntity.EntityId);
                }
            }
            else
            {
                if (baseEntity.Physics != null
                &&  !_ships.ContainsKey(baseEntity.EntityId))
                {
                    grid.OnMarkForClose += OnMarkForClose;
                    Ship ship = new Ship(grid);
                    _ships.Add(ship.EntityId, ship);
                }
            }
        }

        private void OnMarkForClose(IMyEntity entity)
        {
            if (_ships.ContainsKey(entity.EntityId))
            {
                _ships[entity.EntityId].Grid.OnMarkForClose -= OnMarkForClose;
                _ships.Remove(entity.EntityId);
            }
        }

        private void AfterDamageHandler(object target, MyDamageInformation info)
        {
            try
            {
                Logic.UpdateCombatZones(target, ref info);
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");
                Messaging.NotifyPlayer($"[ ERROR: {GetType().FullName}: {e.Message} | Send SpaceEngineers.Log to mod author ]", "Red");
            }
        }

        public override void UpdateAfterSimulation()
        {
            // Show welcome message
            Messaging.WelcomePlayer();
            if (!Utilities.IsServer() || _ships.Count == 0) { return; }

            try
            {
                if (Utilities.IsServer())
                {
                    MyAPIGateway.Parallel.Start(Logic.UpdateShips);
                    MyAPIGateway.Parallel.Start(Logic.ProcessCombatMessages);
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");
                Messaging.NotifyPlayer($"[ ERROR: {GetType().FullName}: {e.Message} | Send SpaceEngineers.Log to mod author ]", "Red");
            }
        }

        private void MessageEntered(string messageText, ref bool sendToOthers)
        {
            if (messageText.StartsWith(CombatInterdictionCommands.COMMAND_PREFIX))
            {
                Commands.HandleCommand(messageText, MyAPIGateway.Multiplayer.MyId, true);
                sendToOthers = false;
            }
        }

        // Utility Functions
        public bool ContainsShip(long entityId)
        {
            return _ships.ContainsKey(entityId);
        }

        public Ship GetShip(long entityId)
        {
            return _ships[entityId];
        }

        public Ship[] Ships
        { get { return _ships.Values.ToArray(); } }
    }
}
