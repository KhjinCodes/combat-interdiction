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

        // Environment constants
        private const float GRAVITY = 9.81f;                        // Earth gravtiy in m/s²
        private const float AIR_DENSITY = 1.225f;                   // Earth air density at sea level in kg/m³
        private const float WATER_DENSITY = 1026.0f;                // Earth sea water density at sea level in kg/m³
        private const float MIN_PARENT_GRID_VOLUME = 400.0f;        // Minimum grid volume of LG to be considered as parent

        // Drag global factors
        private const float LARGE_GRID_DRAG_COEFFICIENT = 0.105f;
        private const float SMALL_GRID_DRAG_COEFFICIENT = 0.047f;
        private const float FRONT_AREA_FACTOR = 0.05f;

        // Dive speed global factors
        private const float DIVE_MAX_DRAG_REDUCTION = 0.80f;
        private const float DIVE_MIN_ANGLE = 45.0f;

        // Smoothen speed and turns
        private const float ENGINEER_SAFE_MAXIMUM_ROLL_RATE = (float)(300.0f * (Math.PI / 180));
        private const float ENGINEER_SAFE_MAXIMUM_TURN_RATE = (float)(40.0f * (Math.PI / 180));
        private const float TURN_RATE_RAMPDOWN_FACTOR = 0.05f;
        private const float SPEED_RAMPDOWN_FACTOR = 0.01f;

        private struct SpeedFactors
        {
            public float DragCoefficient;
            public float BaseWeight;
            public float BaseTwr;
            public float MinimumTwr;
            public float MaximumTwr;
            public float SpeedFactor;
            public float WeightFactor;
        }

        private struct TurnFactors 
        {
            public float BaseWeight;
            public float BaseTurnRate;
            public float BaseTurnRateSpeed;
            public float MinimumTurnRate;
            public float MaximumTurnRate;
            public float TurnRateSpeedFactor;
            public float TurnRateWeightFactor;
        }

        private struct CombatMessage
        {
            public long Recipient;
            public string Message;
            public string Color;
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
            combatMessages.Enqueue(new CombatMessage()
            {
                Recipient = identiyId,
                Message = message,
                Color = color
            });
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
                ship.UpdateMass();
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
            float shipMaxAngularSpeed = ENGINEER_SAFE_MAXIMUM_TURN_RATE;

            if (ship.Grid.GridSizeEnum == MyCubeSize.Small)
            {
                if (HasGasBasedThrusters(ship))
                {
                    GetSmallGridJetBaseSpeed(ship, shipSpeed, out shipMaxSpeed, out shipMaxAngularSpeed);
                }
                else
                {
                    GetSmallGridBaseSpeed(ship, shipSpeed, out shipMaxSpeed, out shipMaxAngularSpeed);
                }

                if (!ship.InCombat && IsOnSuperCruise(ship))
                {
                    shipMaxSpeed *= settings.smallGridBoostSpeedMultiplier;
                }

                shipMaxSpeed = MathHelper.Clamp(shipMaxSpeed, 0, settings.smallGridMaxSpeed);
            }
            else if (ship.Grid.GridSizeEnum == MyCubeSize.Large)
            {
                if (HasGasBasedThrusters(ship))
                {
                    GetLargeGridJetBaseSpeed(ship, shipSpeed, out shipMaxSpeed, out shipMaxAngularSpeed);
                }
                else
                {
                    GetLargeGridBaseSpeed(ship, shipSpeed, out shipMaxSpeed, out shipMaxAngularSpeed);
                }

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
                    ship.Physics.SetSpeeds(clampedSpeed, ship.AngularVelocity);
                }
                else
                {
                    ship.LerpLinearSpeed = 0;
                    Vector3 clampedSpeed = direction * shipMaxSpeed;
                    ship.Physics.SetSpeeds(clampedSpeed, ship.AngularVelocity);
                }
            }

            // Apply Turn Rate Limit to Grid
            if (ship.Grid.GridSizeEnum == MyCubeSize.Small
            && ship.InAtmosphere && !ship.InWater)
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

        private void GetLargeGridBaseSpeed(Ship ship, float currentSpeed, out float maxSpeed, out float maxTurnRate)
        {
            maxSpeed = GetGridBaseSpeed(ship, new SpeedFactors()
            {
                DragCoefficient = SMALL_GRID_DRAG_COEFFICIENT,
                BaseWeight = settings.largeGridBaseWeight,
                BaseTwr = settings.largeGridBaseTwr,
                MinimumTwr = settings.largeGridMinimumTwr,
                MaximumTwr = settings.largeGridMaximumTwr,
                SpeedFactor = settings.largeGridSpeedFactor,
                WeightFactor = settings.largeGridWeightFactor
            });

            maxTurnRate = GetGridTurnRate(ship, currentSpeed, new TurnFactors()
            {
                BaseWeight = settings.largeGridBaseWeight,
                BaseTurnRate = settings.largeGridBaseTurnRate,
                BaseTurnRateSpeed = settings.largeGridBaseTurnRateSpeed,
                MinimumTurnRate = settings.largeGridMinimumTurnRate,
                MaximumTurnRate = settings.largeGridMaximumTurnRate,
                TurnRateSpeedFactor = settings.largeGridTurnRateSpeedFactor,
                TurnRateWeightFactor = settings.largeGridTurnRateWeightFactor
            });
        }

        private void GetLargeGridJetBaseSpeed(Ship ship, float currentSpeed, out float maxSpeed, out float maxTurnRate)
        {
            maxSpeed = GetGridBaseSpeed(ship, new SpeedFactors()
            {
                DragCoefficient = LARGE_GRID_DRAG_COEFFICIENT,
                BaseWeight = settings.largeGridJetBaseWeight,
                BaseTwr = settings.largeGridJetBaseTwr,
                MinimumTwr = settings.largeGridJetMinimumTwr,
                MaximumTwr = settings.largeGridJetMaximumTwr,
                SpeedFactor = settings.largeGridJetSpeedFactor,
                WeightFactor = settings.largeGridJetWeightFactor
            });

            maxTurnRate = GetGridTurnRate(ship, currentSpeed, new TurnFactors()
            {
                BaseWeight = settings.largeGridJetBaseWeight,
                BaseTurnRate = settings.largeGridJetBaseTurnRate,
                BaseTurnRateSpeed = settings.largeGridJetBaseTurnRateSpeed,
                MinimumTurnRate = settings.largeGridJetMinimumTurnRate,
                MaximumTurnRate = settings.largeGridJetMaximumTurnRate,
                TurnRateSpeedFactor = settings.largeGridJetTurnRateSpeedFactor,
                TurnRateWeightFactor = settings.largeGridJetTurnRateWeightFactor
            });
        }

        private void GetSmallGridBaseSpeed(Ship ship, float currentSpeed, out float maxSpeed, out float maxTurnRate)
        {
            maxSpeed = GetGridBaseSpeed(ship, new SpeedFactors()
            {
                DragCoefficient = SMALL_GRID_DRAG_COEFFICIENT,
                BaseWeight = settings.smallGridBaseWeight,
                BaseTwr = settings.smallGridBaseTwr,
                MinimumTwr = settings.smallGridMinimumTwr,
                MaximumTwr = settings.smallGridMaximumTwr,
                SpeedFactor = settings.smallGridSpeedFactor,
                WeightFactor = settings.smallGridWeightFactor
            });

            maxTurnRate = GetGridTurnRate(ship, currentSpeed, new TurnFactors()
            {
                BaseWeight = settings.smallGridBaseWeight,
                BaseTurnRate = settings.smallGridBaseTurnRate,
                BaseTurnRateSpeed = settings.smallGridBaseTurnRateSpeed,
                MinimumTurnRate = settings.smallGridMinimumTurnRate,
                MaximumTurnRate = settings.smallGridMaximumTurnRate,
                TurnRateSpeedFactor = settings.smallGridTurnRateSpeedFactor,
                TurnRateWeightFactor = settings.smallGridTurnRateWeightFactor
            });
        }

        private void GetSmallGridJetBaseSpeed(Ship ship, float currentSpeed, out float maxSpeed, out float maxTurnRate)
        {
            maxSpeed = GetGridBaseSpeed(ship, new SpeedFactors()
            {
                DragCoefficient = SMALL_GRID_DRAG_COEFFICIENT,
                BaseWeight = settings.smallGridJetBaseWeight,
                BaseTwr = settings.smallGridJetBaseTwr,
                MinimumTwr = settings.smallGridJetMinimumTwr,
                MaximumTwr = settings.smallGridJetMaximumTwr,
                SpeedFactor = settings.smallGridJetSpeedFactor,
                WeightFactor = settings.smallGridJetWeightFactor,
            });

            maxTurnRate = GetGridTurnRate(ship, currentSpeed, new TurnFactors()
            {
                BaseWeight = settings.smallGridJetBaseWeight,
                BaseTurnRate = settings.smallGridJetBaseTurnRate,
                BaseTurnRateSpeed = settings.smallGridJetBaseTurnRateSpeed,
                MinimumTurnRate = settings.smallGridJetMinimumTurnRate,
                MaximumTurnRate = settings.smallGridJetMaximumTurnRate,
                TurnRateSpeedFactor = settings.smallGridJetTurnRateSpeedFactor,
                TurnRateWeightFactor = settings.smallGridJetTurnRateWeightFactor
            });
        }

        private float GetGridBaseSpeed(Ship ship, SpeedFactors values)
        {
            // Calculate thrust from mass and base TWR, considering weight penalty
            float twrPenalty = 1f - (values.WeightFactor * (float)Math.Log(ship.DryMass / values.BaseWeight));
            float twr = MathHelper.Clamp(values.BaseTwr * twrPenalty, values.MinimumTwr, values.MaximumTwr);
            float thrust = (ship.DryMass * GRAVITY) * twr * values.SpeedFactor;

            // Calculate overall drag
            float frontalArea = FRONT_AREA_FACTOR * ship.DryMass;
            float fluidDrag = 0.5f * GetFluidDensity(ship) * values.DragCoefficient * frontalArea;
            
            // Calculate drag adjustments
            if (ship.InAtmosphere && !ship.InWater)
            {
                float angleRadians = GetAngleFromGravityRadians(ship);
                float angleDegrees = MathHelper.ToDegrees(angleRadians);
                float angleFactor = (float)Math.Sin(angleRadians);

                if (angleDegrees <= DIVE_MIN_ANGLE)
                {
                    fluidDrag -= (fluidDrag * DIVE_MAX_DRAG_REDUCTION * Math.Abs(angleFactor));
                }
            }

            float baseSpeed = (float)Math.Sqrt((2 * thrust) / fluidDrag);
            return baseSpeed;
        }

        private float GetAngleFromGravityRadians(Ship ship)
        {
            // Normalize both vectors
            Vector3 normalizedVelocity = ship.Grid.Physics.LinearVelocity.Normalized();
            Vector3 normalizedGravity = ship.Grid.NaturalGravity.Normalized();

            // Calculate the dot product and angle between the vectors
            float dotProduct = Vector3.Dot(normalizedVelocity, normalizedGravity);
            float angle = (float)Math.Acos(dotProduct); // Angle in radians

            return angle;
        }

        private float GetGridTurnRate(Ship ship, float currentSpeed, TurnFactors values)
        {
            float pitch = ship.AngularVelocity.Y;
            float yaw = ship.AngularVelocity.Z;
            float roll = ship.AngularVelocity.X;

            if (roll > pitch && roll > yaw)
            {
                return ENGINEER_SAFE_MAXIMUM_ROLL_RATE;
            }
            else
            {
                float baseTurnRateRad = MathHelper.ToRadians(values.BaseTurnRate);
                float minimumTurnRateRad = MathHelper.ToRadians(values.MinimumTurnRate);
                float maximumTurnRateRad = MathHelper.ToRadians(values.MaximumTurnRate);

                float weightFactor = (float)Math.Pow(values.BaseWeight / ship.DryMass, values.TurnRateWeightFactor);
                float speedFactor = (float)Math.Pow(values.BaseTurnRateSpeed / currentSpeed, values.TurnRateSpeedFactor);
                float targetTurnRateRad = baseTurnRateRad * weightFactor * speedFactor;
                float clampedTurnRate = MathHelper.Clamp(targetTurnRateRad, minimumTurnRateRad, maximumTurnRateRad);
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
    }
}
