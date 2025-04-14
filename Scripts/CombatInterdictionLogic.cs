using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using Jakaria.API;

namespace Khjin.CombatInterdiction
{
    public class CombatInterdictionLogic
    {
        private CombatInterdictionSession session;
        private CombatInterdictionSettings settings;

        private List<long> activeContacts = new List<long>();
        private ConcurrentQueue<CombatMessage> combatMessages = new ConcurrentQueue<CombatMessage>();

        private const float GRAVITY = 9.81f;                        // Earth gravtiy in m/s²
        private const float AIR_DENSITY = 1.225f;                   // Earth air density at sea level in kg/m³
        private const float WATER_DENSITY = 1026.0f;                // Earth sea water density at sea level in kg/m³
        private const float LARGE_GRID_DRAG_COEFFICIENT = 0.105f;   // Drag coefficient of a large grid cube
        private const float SMALL_GRID_DRAG_COEFFICIENT = 0.047f;   // Drag coefficient of a small grid sphere
        private const float FRONT_AREA_FACTOR = 0.05f;              // Fake front area drag from mass

        private class CombatMessage
        {
            public long Recipient { get; private set; }
            public string Message { get; private set; }
            public string Color { get; private set; }

            public CombatMessage(long recipient, string message, string color)
            {
                Recipient = recipient;
                Message = message;
                Color = color;
            }
        }

        public CombatInterdictionLogic() { }

        public void LoadData()
        {
            session = CombatInterdictionSession.Instance;
            settings = session.Settings;
        }

        public void UnloadData()
        {
            activeContacts.Clear();
        }

        public void UpdateCombatZones(object target, ref MyDamageInformation info)
        {
            // Only process blocks and valid damage
            IMySlimBlock targetBlock = target as IMySlimBlock;
            if (targetBlock == null || IsNonCombatDamage(info.Type)) { return; }

            // Try to get the parent grid
            IMyCubeGrid targetGrid = Utilities.GetBaseGrid(targetBlock.CubeGrid);
            if (targetGrid == null) { return; }

            long targetId = targetGrid.EntityId;
            long attackerId = info.AttackerId;

            // Try to get the entities
            IMyEntity targetEntity = null;
            IMyEntity attackerEntity = null;
            if (!MyAPIGateway.Entities.TryGetEntityById(targetId, out targetEntity)
            || !MyAPIGateway.Entities.TryGetEntityById(attackerId, out attackerEntity))
            { return; }

            // Filter out deformation damages not caused by grids
            if (info.Type == MyDamageType.Deformation
            && !(attackerEntity is MyCubeGrid))
            { return; }

            // Don't process null values
            if (targetEntity.MarkedForClose || attackerEntity.MarkedForClose) { return; }

            // Get the center location between combatants
            Vector3D center = (targetEntity.GetPosition() + attackerEntity.GetPosition()) / 2;
            BoundingSphereD combatArea = new BoundingSphereD(center, settings.combatZoneRadius);

            List<MyEntity> entitiesInSpehere = new List<MyEntity>();
            MyGamePruningStructure.GetAllEntitiesInSphere(ref combatArea, entitiesInSpehere);

            // Register or update each combatant
            for (int i = (entitiesInSpehere.Count - 1); i >= 0; i--)
            {
                MyEntity entity = entitiesInSpehere[i];

                // Only handle grids not due for clean-up
                IMyCubeGrid grid = entity as IMyCubeGrid;
                if (grid == null) { continue; }

                // Only process registered ships
                grid = Utilities.GetBaseGrid(grid);
                if (grid == null
                || grid.MarkedForClose
                || !session.ContainsShip(grid.EntityId))
                { continue; }

                // Ship is not yet in combat, inform the current pilot
                Ship ship = session.GetShip(grid.EntityId);
                if (!ship.InCombat)
                {
                    long identiyId = GetControllingPlayerIdentiyId(ship);
                    if (identiyId > 0)
                    { SendCombatMessage(identiyId, "Entered COMBAT MODE", "Red"); }
                }

                // Refresh interdiction duration, 1 sec = 60 ticks
                ship.InterdictionDuration = settings.interdictionDuration * 60;
            }
        }

        private void SendCombatMessage(long identiyId, string message, string color)
        {
            combatMessages.Enqueue(new CombatMessage(identiyId, message, color));
        }

        public void ProcessCombatMessages()
        {
            CombatMessage message;
            if (combatMessages.Count > 0 && combatMessages.TryDequeue(out message))
            {
                Utilities.MessagePlayer(message.Message, message.Recipient, message.Color);
            }
        }

        public void UpdateShips()
        {
            Ship[] ships = session.Ships;
            for (int i = (ships.Length-1); i >= 0; i--)
            {
                Ship ship = ships[i];
                if (ship.Grid == null || ship.Grid.Physics == null || ship.MarkedForClose) { continue; }
                UpdateCombatStatus(ship);
                ApplySpeedLimits(ship);
            }
        }

        private void UpdateCombatStatus(Ship ship)
        {
            if (ship.InCombat)
            {
                ship.InterdictionDuration--;
                if (!ship.InCombat)
                {
                    long identiyId = GetControllingPlayerIdentiyId(ship);
                    if (identiyId > 0)
                    { SendCombatMessage(identiyId, "Entered PEACE MODE", "Blue"); }
                }
                else
                {
                    // Less than 5 seconds
                    if (ship.InterdictionDuration <= 60 * 5)
                    {
                        if (ship.InterdictionDuration % 60 == 0)
                        {
                            long identiyId = GetControllingPlayerIdentiyId(ship);
                            if (identiyId > 0)
                            { SendCombatMessage(identiyId, $"Exiting COMBAT MODE in...{(int)(ship.InterdictionDuration / 60)} secs", "Blue"); }
                        }
                    }
                }
            }
        }

        private void ApplySpeedLimits(Ship ship)
        {
            // Check grid thrusters
            RefreshThrusters(ship);

            if (ship.Thrusters.Count == 0
            || (GetControllingPlayerIdentiyId(ship) == -1 && ship.BoundingBox.Volume <= settings.minimumGridVolume)
            || (Utilities.IsNpcOwned(ship.Grid) && !ship.InCombat))
            { return; }

            RefreshAtmosphereStatus(ship);
            RefreshWaterStatus(ship);

            float shipMaxSpeed = 100; // SE Default Max Speed
            if (ship.Grid.GridSizeEnum == MyCubeSize.Small)
            {
                if (HasGasBasedThrusters(ship))
                { shipMaxSpeed = GetSmallGridJetBaseSpeed(ship); }
                else
                { shipMaxSpeed = GetSmallGridBaseSpeed(ship); }

                if (settings.allowSuperCruise && !ship.InCombat && IsOnSuperCruise(ship))
                {
                    shipMaxSpeed *= settings.smallGridBoostSpeedMultiplier;
                }

                shipMaxSpeed = MathHelper.Clamp(shipMaxSpeed, 0, settings.smallGridMaxSpeed);
            }
            else if (ship.Grid.GridSizeEnum == MyCubeSize.Large)
            {
                if (HasGasBasedThrusters(ship))
                { shipMaxSpeed = GetLargeGridJetBaseSpeed(ship); }
                else
                { shipMaxSpeed = GetLargeGridBaseSpeed(ship); }

                if (settings.allowSuperCruise && !ship.InCombat && IsOnSuperCruise(ship))
                {
                    shipMaxSpeed *= settings.largeGridBoostSpeedMultiplier;
                    ApplyThrustBoost(ship);
                }

                shipMaxSpeed = MathHelper.Clamp(shipMaxSpeed, 0, settings.largeGridMaxSpeed);
            }
            else
            { /* A NEW SIZE?!?! DO NOTHING. */ }


            // Apply Speed Limit to Grid
            float shipMaxSpeedSq = shipMaxSpeed * shipMaxSpeed;
            float shipSpeedSq = ship.LinearVelocity.LengthSquared();
            if (shipSpeedSq > shipMaxSpeedSq)
            {
                // Create speed buffer to prevent sudden slow down
                if (ship.SpeedBuffer == 0)
                {
                    ship.SpeedBuffer = ship.LinearVelocity.Length() - shipMaxSpeed;
                    ship.SpeedBufferDecayRate = ship.SpeedBuffer / (60 * 3);
                }
                else
                {
                    ship.SpeedBuffer -= ship.SpeedBufferDecayRate;
                }

                // Calculate grid speed buffer and unified direction
                ship.SpeedBuffer = ship.SpeedBuffer < 0 ? 0 : ship.SpeedBuffer;
                Vector3 direction = ship.Grid.Physics.LinearVelocity.Normalized();
                Vector3 maxLinearVelocity = direction * (shipMaxSpeed + ship.SpeedBuffer);
                ship.Grid.Physics.SetSpeeds(maxLinearVelocity, ship.AngularVelocity);
            }
            else
            {
                if (ship.SpeedBuffer > 0)
                {
                    ship.SpeedBuffer = 0;
                    ship.SpeedBufferDecayRate = 0;
                }
            }
        }

        public float GetLargeGridBaseSpeed(Ship ship)
        {
            float weightKg = ship.Grid.Physics.Mass;
            float maxThrust = settings.largeGridSpeedFactor * (float)Math.Pow(weightKg, settings.largeGridWeightFactor + 1) * GRAVITY;
            float frontalArea = weightKg * FRONT_AREA_FACTOR;
            float fluidDrag = GetFluidDensity(ship) * LARGE_GRID_DRAG_COEFFICIENT * frontalArea;
            fluidDrag = fluidDrag <= 0 ? 1 : fluidDrag;
            return (float)Math.Sqrt((2 * maxThrust) / fluidDrag);
        }

        public float GetLargeGridJetBaseSpeed(Ship ship)
        {
            float weightKg = ship.Grid.Physics.Mass;
            float maxThrust = settings.largeGridJetSpeedFactor * (float)Math.Pow(weightKg, settings.largeGridJetWeightFactor + 1) * GRAVITY;
            float frontalArea = weightKg * FRONT_AREA_FACTOR;
            float fluidDrag = GetFluidDensity(ship) * LARGE_GRID_DRAG_COEFFICIENT * frontalArea;
            fluidDrag = fluidDrag <= 0 ? 1 : fluidDrag;
            return (float)Math.Sqrt((2 * maxThrust) / fluidDrag);
        }

        public float GetSmallGridBaseSpeed(Ship ship)
        {
            float weightKg = ship.Grid.Physics.Mass;
            float thrust = settings.smallGridSpeedFactor * (float)Math.Pow(weightKg, settings.smallGridWeightFactor + 1) * GRAVITY;
            float frontalArea = weightKg * FRONT_AREA_FACTOR;
            float fluidDrag = GetFluidDensity(ship) * SMALL_GRID_DRAG_COEFFICIENT * frontalArea;
            fluidDrag = fluidDrag <= 0 ? 1 : fluidDrag;
            return (float)Math.Sqrt((2 * thrust) / fluidDrag);
        }

        public float GetSmallGridJetBaseSpeed(Ship ship)
        {
            float weightKg = ship.Mass;
            float thrust = settings.smallGridJetSpeedFactor * (float)Math.Pow(weightKg, settings.smallGridJetWeightFactor + 1) * GRAVITY;
            float frontalArea = weightKg * FRONT_AREA_FACTOR;
            float fluidDrag = GetFluidDensity(ship) * SMALL_GRID_DRAG_COEFFICIENT * frontalArea;
            fluidDrag = fluidDrag <= 0 ? 1 : fluidDrag;
            return (float)Math.Sqrt((2 * thrust) / fluidDrag);
        }

        private float GetFluidDensity(Ship ship)
        {
            if (ship.InWater)
            {
                return WATER_DENSITY * (ship.IsSubmerged ? 1.1f : 1.0f);
            }
            else if (ship.InAtmosphere)
            {
                return AIR_DENSITY * ship.AirDensity;
            }
            else
            {
                return 0;
            }
        }

        private long GetControllingPlayerIdentiyId(Ship ship)
        {
            IMyPlayer player = MyAPIGateway.Players.GetPlayerControllingEntity(ship.Grid);
            return player == null ? -1 : player.IdentityId;
        }

        public bool HasGasBasedThrusters(Ship ship)
        {
            try
            {
                for (int i = (ship.Thrusters.Count - 1); i >= 0; i--)
                {
                    IMyThrust thruster = ship.Thrusters[i];
                    if (thruster == null || !thruster.IsFunctional) { continue; }

                    // Check its definition
                    MyThrustDefinition thrustDef = (thruster as MyThrust)?.BlockDefinition;
                    if (thrustDef != null)
                    {
                        if ($"{thrustDef.FuelConverter.FuelId.TypeId}" == "MyObjectBuilder_GasProperties")
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch
            {
                // WTH happened? :D
                return false;
            }
        }

        private bool IsOnSuperCruise(Ship ship)
        {
            List<IMyCockpit> cockpits = new List<IMyCockpit>(ship.Grid.GetFatBlocks<IMyCockpit>());
            for (int i = (cockpits.Count-1); i >= 0; i--)
            {
                IMyCockpit cockpit = cockpits[i];
                var logic = cockpit.GameLogic?.GetAs<CombatInterdictionBlock>();
                if (logic != null && cockpit.IsFunctional && cockpit.CanControlShip && cockpit.IsUnderControl)
                {
                    if (logic.SuperCruise) { return true; }
                    else { continue; }
                }
            }
            return false;
        }

        private void ApplyThrustBoost(Ship ship)
        {
            // Get the total thrust count
            float totalThrust = 0;
            int activeThrusterCount = 0;
            for (int i = (ship.Thrusters.Count - 1); i >= 0; i--)
            {
                IMyThrust thruster = ship.Thrusters[i];
                float currentThrust = thruster.CurrentThrust;
                if (Math.Abs(currentThrust) < 0.0001f) { continue; }
                totalThrust += currentThrust;
                activeThrusterCount++;
            }

            // Apply thruster based boost
            float maxThrust = ship.Grid.Physics.Mass * GRAVITY * settings.largeGridBoostTwr;
            for (int i = (ship.Thrusters.Count - 1); i >= 0; i--)
            {
                IMyThrust thruster = ship.Thrusters[i];
                float currentThrust = thruster.CurrentThrust;
                if (Math.Abs(currentThrust) < 0.0001f) { continue; }

                float ratio = currentThrust / totalThrust;
                float boostForce = (maxThrust * ratio) - currentThrust;
                Vector3D force = thruster.WorldMatrix.Backward * boostForce;

                var groupProperties = MyGridPhysicalGroupData.GetGroupSharedProperties((MyCubeGrid)ship.Grid);
                ship.Grid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE,
                    force, groupProperties.CoMWorld, null);
            }
        }

        private bool IsNonCombatDamage(MyStringHash type)
        {
            if (type == MyDamageType.Bullet
            || type == MyDamageType.Explosion
            || type == MyDamageType.Rocket
            || type == MyDamageType.Mine
            || type == MyDamageType.Weapon
            || type == MyDamageType.Deformation
            || type == MyDamageType.Destruction
            || type == MyDamageType.Spider
            || type == MyDamageType.Wolf
            || type == MyDamageType.Unknown)
            { return false; }
            return true;
        }
    
        private void RefreshThrusters(Ship ship)
        {
            ship.Grids.Clear();
            ship.Thrusters.Clear();
            var group = ship.Grid.GetGridGroup(GridLinkTypeEnum.Mechanical);
            group.GetGrids(ship.Grids);
            for (int i = (ship.Grids.Count - 1); i >= 0; i--)
            {
                IMyCubeGrid grid = ship.Grids[i];
                ship.Thrusters.AddRange(grid.GetFatBlocks<IMyThrust>());
            }
        }

        private void RefreshAtmosphereStatus(Ship ship)
        {
            ship.NaturalGravity = ship.Grid.NaturalGravity.Length();
            if (ship.InGravity)
            {
                var planet = MyGamePruningStructure.GetClosestPlanet(ship.Grid.WorldMatrix.Translation);
                ship.AirDensity = planet.GetAirDensity(ship.Grid.WorldMatrix.Translation);
            }
        }

        private void RefreshWaterStatus(Ship ship)
        {
            ship.InWater = false;
            ship.IsSubmerged = false;
            try
            {
                int submergedPoints = 0;
                for (int i = 0; i < 8; i++)
                {
                    var point = ship.BoundingBox.GetCorner(i);
                    if (WaterModAPI.IsUnderwater(point))
                    {
                        submergedPoints++;
                        ship.InWater = submergedPoints >= 4;
                        ship.IsSubmerged = submergedPoints >= 8;
                    }
                }
            }
            catch
            { /* Failed to call WaterMod. Table flip! */ }
        }
    
        private IMyShipController GetControlSeat(Ship ship)
        {
            List<IMyShipController> controllers = 
                new List<IMyShipController>(ship.Grid.GetFatBlocks<IMyShipController>());
            for (int i = (controllers.Count-1); i >= 0; i--)
            {
                IMyShipController controller = controllers[i];
                if (controller.CanControlShip && controller.IsUnderControl)
                { return controller; }
            }
            return null;
        }
    }
}
