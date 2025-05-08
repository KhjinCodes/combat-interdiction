using Jakaria.API;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

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

        private const float MINIMUM_TURN_RATE = (float)(5.0f * (Math.PI / 180));
        private const float MAXIMUM_TURN_RATE = (float)(25.0f * (Math.PI / 180));
        private const float MAXIMUM_ROLL_RATE = (float)(300.0f * (Math.PI / 180));

        private const float BASE_TURN_RATE = 20.0f;                 // Reference turn rate (strike fighter)
        private const float BASE_TURN_RATE_WEIGHT = 25000.0f;       // Reference weight (strike fighter)
        private const float BASE_TURN_RATE_SPEED = 680.0f;          // Reference speed (strike fighter)
        private const float SPEED_RAMPDOWN_FACTOR = 0.01f;          // Smoothen slow down
        private const float TURN_RATE_RAMPDOWN_FACTOR = 0.05f;      // Smoothen turn rate clamping
        private const float MIN_PARENT_GRID_VOLUME = 400.0f;

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

            // Register or update each combatant
            List<MyEntity> entitiesInSpehere = new List<MyEntity>();
            MyGamePruningStructure.GetAllEntitiesInSphere(ref combatArea, entitiesInSpehere);
            foreach (var entity in entitiesInSpehere)
            {
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
            foreach (var ship in session.Ships)
            {
                if (ship.Grid == null 
                ||  ship.Grid.Physics == null   // Projections
                ||  ship.Grid.IsStatic          // Static Grids (Buildings etc.)
                ||  ship.MarkedForClose) 
                { continue; }

                ship.InitializeHooksOnce();
                ship.InitializeBlocksOnce();
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
            var controller = GetController(ship);
            if (controller != null)
            {
                controller.CustomData =
                    $"Thrusters: {ship.ThrusterCount}\n" +
                    $"Controllers: {ship.ControllerCount}\n" +
                    $"Volume: {ship.Volume}\n" +
                    $"Docked: {IsDockedSmallGrid(ship)}\n" +
                    $"NPC Owned: {Utilities.IsNpcOwned(ship.Grid)}\n";
            }

            if (ship.ThrusterCount == 0
            || ship.Volume <= settings.minimumGridVolume
            || IsDockedSmallGrid(ship)
            || (Utilities.IsNpcOwned(ship.Grid) && !ship.InCombat))
            {
                return;
            }

            RefreshWaterStatus(ship);
            RefreshAtmosphereStatus(ship);

            float shipSpeed = ship.LinearVelocity.Length();
            float shipMaxSpeed = 100; // SE Default Max Speed
            float shipMaxAngularSpeed = MAXIMUM_TURN_RATE;

            if (ship.Grid.GridSizeEnum == MyCubeSize.Small)
            {
                if (HasGasBasedThrusters(ship))
                { shipMaxSpeed = GetSmallGridJetBaseSpeed(ship); }
                else
                { shipMaxSpeed = GetSmallGridBaseSpeed(ship); }

                if (!ship.InCombat && IsOnSuperCruise(ship))
                {
                    shipMaxSpeed *= settings.smallGridBoostSpeedMultiplier;
                }

                shipMaxSpeed = MathHelper.Clamp(shipMaxSpeed, 0, settings.smallGridMaxSpeed);
                shipMaxAngularSpeed = GetSmallGridTurnRate(ship.Mass, shipSpeed, ship.AngularVelocity);
            }
            else if (ship.Grid.GridSizeEnum == MyCubeSize.Large)
            {
                if (HasGasBasedThrusters(ship))
                { shipMaxSpeed = GetLargeGridJetBaseSpeed(ship); }
                else
                { shipMaxSpeed = GetLargeGridBaseSpeed(ship); }

                if (!ship.InCombat && IsOnSuperCruise(ship))
                {
                    shipMaxSpeed *= settings.largeGridBoostSpeedMultiplier;
                    ApplyThrustBoost(ship);
                }
                else
                {
                    RemoveThrustBoost(ship);
                }

                shipMaxSpeed = MathHelper.Clamp(shipMaxSpeed, 0, settings.largeGridMaxSpeed);
            }
            else
            { /* A NEW SIZE?!?! DO NOTHING. */ }

            // Apply Speed Limit to Grid
            if (shipSpeed > shipMaxSpeed)
            {
                // Calculate grid speed buffer and unified direction
                Vector3 direction = ship.LinearVelocity.Normalized();
                if (Math.Abs(shipSpeed - shipMaxSpeed) >= 10)
                {
                    ship.LerpLinearSpeed = MathHelper.Lerp(shipSpeed, shipMaxSpeed, SPEED_RAMPDOWN_FACTOR);
                    Vector3 clampedSpeed = direction * ship.LerpLinearSpeed;
                    ship.Physics.SetSpeeds(clampedSpeed, ship.Physics.AngularVelocity);
                }
                else
                {
                    ship.LerpLinearSpeed = 0;
                    Vector3 clampedSpeed = direction * shipMaxSpeed;
                    ship.Physics.SetSpeeds(clampedSpeed, ship.Physics.AngularVelocity);
                }
            }

            // Apply Turn Rate Limit to Grid
            if (ship.InAtmosphere
            &&  ship.Grid.GridSizeEnum == MyCubeSize.Small)
            {
                float shipAngularSpeed = ship.AngularVelocity.Length();
                if (shipAngularSpeed > shipMaxAngularSpeed)
                {
                    Vector3 direction = ship.AngularVelocity.Normalized();
                    ship.LerpAngularSpeed = MathHelper.Lerp(shipAngularSpeed, shipMaxAngularSpeed, TURN_RATE_RAMPDOWN_FACTOR);
                    Vector3 clampedSpeed = direction * ship.LerpAngularSpeed;
                    ship.Physics.SetSpeeds(ship.LinearVelocity, clampedSpeed);
                }
            }
        }

        private float GetLargeGridBaseSpeed(Ship ship)
        {
            float weightKg = ship.Grid.Physics.Mass;
            float maxThrust = settings.largeGridSpeedFactor * (float)Math.Pow(weightKg, settings.largeGridWeightFactor + 1) * GRAVITY;
            float frontalArea = weightKg * FRONT_AREA_FACTOR;
            float fluidDrag = GetFluidDensity(ship) * LARGE_GRID_DRAG_COEFFICIENT * frontalArea;
            fluidDrag = fluidDrag <= 0 ? 1 : fluidDrag;
            return (float)Math.Sqrt((2 * maxThrust) / fluidDrag);
        }

        private float GetLargeGridJetBaseSpeed(Ship ship)
        {
            float weightKg = ship.Grid.Physics.Mass;
            float maxThrust = settings.largeGridJetSpeedFactor * (float)Math.Pow(weightKg, settings.largeGridJetWeightFactor + 1) * GRAVITY;
            float frontalArea = weightKg * FRONT_AREA_FACTOR;
            float fluidDrag = GetFluidDensity(ship) * LARGE_GRID_DRAG_COEFFICIENT * frontalArea;
            fluidDrag = fluidDrag <= 0 ? 1 : fluidDrag;
            return (float)Math.Sqrt((2 * maxThrust) / fluidDrag);
        }

        private float GetSmallGridBaseSpeed(Ship ship)
        {
            float weightKg = ship.Grid.Physics.Mass;
            float thrust = settings.smallGridSpeedFactor * (float)Math.Pow(weightKg, settings.smallGridWeightFactor + 1) * GRAVITY;
            float frontalArea = weightKg * FRONT_AREA_FACTOR;
            float fluidDrag = GetFluidDensity(ship) * SMALL_GRID_DRAG_COEFFICIENT * frontalArea;
            fluidDrag = fluidDrag <= 0 ? 1 : fluidDrag;
            return (float)Math.Sqrt((2 * thrust) / fluidDrag);
        }

        private float GetSmallGridJetBaseSpeed(Ship ship)
        {
            float weightKg = ship.Mass;
            float thrust = settings.smallGridJetSpeedFactor * (float)Math.Pow(weightKg, settings.smallGridJetWeightFactor + 1) * GRAVITY;
            float frontalArea = weightKg * FRONT_AREA_FACTOR;
            float fluidDrag = GetFluidDensity(ship) * SMALL_GRID_DRAG_COEFFICIENT * frontalArea;
            fluidDrag = fluidDrag <= 0 ? 1 : fluidDrag;
            return (float)Math.Sqrt((2 * thrust) / fluidDrag);
        }

        private float GetSmallGridTurnRate(float mass, float speed, Vector3 angularVelocity)
        {
            float pitch = angularVelocity.Y;
            float yaw = angularVelocity.Z;
            float roll = angularVelocity.X;

            if (roll > pitch && roll > yaw)
            {
                return MAXIMUM_ROLL_RATE;
            }
            else
            {
                float weightFactor = (float)Math.Pow(BASE_TURN_RATE_WEIGHT / mass, settings.smallGridTurnRateWeightFactor);
                float speedFactor = (float)Math.Pow(BASE_TURN_RATE_SPEED / speed, settings.smallGridTurnRateSpeedFactor);
                float targetTurnRateRad = BASE_TURN_RATE * weightFactor * speedFactor;
                float clampedTurnRate = MathHelper.Clamp(targetTurnRateRad, MINIMUM_TURN_RATE, MAXIMUM_TURN_RATE);
                return clampedTurnRate;
            }
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

        private bool HasGasBasedThrusters(Ship ship)
        {
            foreach (var thruster in ship.Thrusters)
            {
                if (thruster == null || !thruster.IsFunctional) { continue; }
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

        private bool IsDockedSmallGrid(Ship ship)
        {
            if (ship.Grid.GridSizeEnum == MyCubeSize.Small)
            {
                // Check if connected to a large grid
                List<IMyCubeGrid> grids = new List<IMyCubeGrid>();
                IMyGridGroupData group = ship.Grid.GetGridGroup(GridLinkTypeEnum.Physical);
                group.GetGrids(grids);
                foreach (IMyCubeGrid grid in grids)
                {
                    if (grid.GridSizeEnum == MyCubeSize.Large
                    &&  grid.WorldAABB.Size.Volume > MIN_PARENT_GRID_VOLUME)
                    { return true; }
                }
            }
            return false;
        }
        
        private bool IsOnSuperCruise(Ship ship)
        {
            List<IMyCockpit> cockpits = new List<IMyCockpit>(ship.Grid.GetFatBlocks<IMyCockpit>());
            foreach (var cockpit in cockpits)
            {
                var logic = cockpit.GameLogic?.GetAs<CombatInterdictionBlock>();
                if (logic != null && cockpit.IsFunctional && cockpit.CanControlShip)
                {
                    if (logic.SuperCruise) { return true; }
                    else { continue; }
                }
            }
            return false;
        }

        public void SyncBoostRequest(long grid, long block, bool value)
        {
            Ship ship = session.GetShip(grid);
            if (ship != null)
            {
                List<IMyCockpit> cockpits = new List<IMyCockpit>(ship.Grid.GetFatBlocks<IMyCockpit>());
                foreach (var cockpit in cockpits)
                {
                    var logic = cockpit.GameLogic?.GetAs<CombatInterdictionBlock>();
                    if (logic != null && cockpit.IsFunctional && cockpit.CanControlShip)
                    {
                        if (cockpit.EntityId == block)
                        {
                            logic.SuperCruise = value;
                            break;
                        }
                    }
                }
            }
        }

        private void ApplyThrustBoost(Ship ship)
        {
            // Get the total thrust count
            float totalThrust = 0;
            int activeThrusterCount = 0;
            IMyThrust[] thrusters = ship.Thrusters;

            foreach (var thruster in thrusters)
            {
                float currentThrust = thruster.CurrentThrust;
                if (Math.Abs(currentThrust) < 0.0001f) { continue; }
                totalThrust += currentThrust;
                activeThrusterCount++;
            }

            // Apply thruster based boost
            float maxThrust = ship.Grid.Physics.Mass * GRAVITY * settings.largeGridBoostTwr;
            foreach (var thruster in thrusters)
            {
                float currentThrust = thruster.CurrentThrust;
                if (Math.Abs(currentThrust) < 0.0001f) { continue; }

                float ratio = currentThrust / totalThrust;
                float boostForce = (maxThrust * ratio) - currentThrust;
                float boostMultiplier = (boostForce / currentThrust) / 100.0f;
                thruster.ThrustMultiplier = boostMultiplier;
            }

            ship.IsOnBoost = true;
        }

        private void RemoveThrustBoost(Ship ship)
        {
            if (!ship.IsOnBoost) { return; }

            foreach (IMyThrust thruster in ship.Thrusters)
            {
                thruster.ThrustMultiplier = 1.0f;
            }

            ship.IsOnBoost = false;
        }

        private bool IsNonCombatDamage(MyStringHash type)
        {
            if (type == MyDamageType.Bullet
            || type == MyDamageType.Explosion
            || type == MyDamageType.Rocket
            || type == MyDamageType.Mine
            || type == MyDamageType.Weapon
            || type == MyDamageType.Destruction
            || type == MyDamageType.Spider
            || type == MyDamageType.Wolf
            || type == MyDamageType.Unknown)
            { return false; }
            return true;
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
                        ship.InWater = submergedPoints >= 2;
                        ship.IsSubmerged = submergedPoints >= 8;
                    }
                }
            }
            catch
            { /* Failed to call WaterMod. Table flip! */ }
        }
    
        private IMyShipController GetController(Ship ship)
        {
            foreach (var controller in ship.Controllers)
            {
                if (controller.CanControlShip && controller.IsUnderControl)
                {
                    return controller;
                }
            }

            return null;
        }
    }
}
