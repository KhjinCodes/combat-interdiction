using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRageMath;

namespace Khjin.CombatInterdiction
{
    public class Ship
    {
        private readonly IMyCubeGrid grid;
        private IMyGridGroupData gridGroupData;
        private readonly List<IMyCubeGrid> grids;
        private readonly List<IMyThrust> thrusters;
        private readonly List<IMyShipController> controllers;
        private IEnumerable<IMyCubeBlock> fatBlocks;
        private bool isBlocksInitialized;
        private bool isHooksInitialized;
        private int waitHookInitialize;
        private float baseMass;
        private float physicalMass;
        private float lastMassRefreshTicks;

        public int InterdictionDuration;
        public float NaturalGravity;
        public float AirDensity;
        public bool InWater;
        public bool IsSubmerged;
        public bool IsOnBoost;
        public float LerpLinearSpeed;
        public float LerpAngularSpeed;
        public List<Vector3> BoostForces;

        public Ship(IMyCubeGrid grid)
        {
            this.grid = grid;
            grids = new List<IMyCubeGrid>();
            thrusters = new List<IMyThrust>();
            controllers = new List<IMyShipController>();
            BoostForces = new List<Vector3>();
            isBlocksInitialized = false;
            isHooksInitialized = false;
            waitHookInitialize = 60 * 5;
            lastMassRefreshTicks = 0;
        }

        public void InitializeBlocksOnce()
        {
            if (isBlocksInitialized) { return; }
            isBlocksInitialized = true;

            gridGroupData = grid.GetGridGroup(GridLinkTypeEnum.Mechanical);
            lock (grids)
            {
                gridGroupData.GetGrids(grids);
                foreach (IMyCubeGrid grid in grids)
                {
                    fatBlocks = grid.GetFatBlocks<IMyCubeBlock>();
                    lock (fatBlocks)
                    {
                        foreach (IMyCubeBlock block  in fatBlocks)
                        {
                            IMyShipController controller = block as IMyShipController;
                            if (controller != null && !controllers.Contains(controller))
                            {
                                lock (controllers) { controllers.Add(controller); }
                                continue;
                            }
                            IMyThrust thruster = block as IMyThrust;
                            if (thruster != null && !thrusters.Contains(thruster))
                            {
                                lock (thrusters) { thrusters.Add(thruster); }
                                continue;
                            }
                        }
                    }
                }
            }
        }

        public void InitializeHooksOnce()
        {
            if (isHooksInitialized) { return; }
            if (waitHookInitialize > 0) { waitHookInitialize--; return; }
            isHooksInitialized = true;

            gridGroupData = grid.GetGridGroup(GridLinkTypeEnum.Mechanical);
            gridGroupData.GetGrids(grids);

            lock (grids)
            {
                foreach (IMyCubeGrid grid in grids)
                {
                    ((MyCubeGrid)grid).OnFatBlockAdded += Grid_OnFatBlockAdded;
                    ((MyCubeGrid)grid).OnFatBlockRemoved += Grid_OnFatBlockRemoved;
                    ((MyCubeGrid)grid).OnMarkForClose += Grid_OnMarkForClose;
                }
            }
        }

        public void UpdateMass()
        {
            lastMassRefreshTicks--;
            if (lastMassRefreshTicks <= 0)
            {
                ((MyCubeGrid)grid).GetCurrentMass(out baseMass, out physicalMass, GridLinkTypeEnum.Mechanical);
                lastMassRefreshTicks = 60;
            }
        }

        private void Grid_OnMarkForClose(MyEntity obj)
        {
            ((MyCubeGrid)obj).OnFatBlockAdded -= Grid_OnFatBlockAdded;
            ((MyCubeGrid)obj).OnFatBlockRemoved -= Grid_OnFatBlockRemoved;
            ((MyCubeGrid)obj).OnMarkForClose -= Grid_OnMarkForClose;
        }

        private void Grid_OnFatBlockAdded(MyCubeBlock obj)
        {
            IMyThrust thruster = obj as IMyThrust;
            if (thruster != null && !thrusters.Contains(thruster))
            {
                thrusters.Add(thruster); return;
            }
            IMyShipController controller = obj as IMyShipController;
            if (controller != null && !controllers.Contains(controller))
            {
                controllers.Add(controller); return;
            }
        }

        private void Grid_OnFatBlockRemoved(MyCubeBlock obj)
        {
            IMyThrust thruster = obj as IMyThrust;
            if (thruster != null && thrusters.Contains(thruster))
            {
                thrusters.Remove(thruster); return;
            }
            IMyShipController controller = obj as IMyShipController;
            if (controller != null && controllers.Contains(controller))
            {
                controllers.Remove(controller); return;
            }
        }

        public IMyCubeGrid Grid
        {
            get { return grid; }
        }

        public long EntityId
        {
            get { return grid.EntityId; }
        }

        public string Name
        {
            get { return grid.Name; }
        }

        public MyPhysicsComponentBase Physics
        {
            get { return grid.Physics; }
        }

        public float DryMass
        {
            get { return baseMass; }
        }

        public float TotalMass
        {
            get { return physicalMass; }
        }

        public Vector3 LinearVelocity
        {
            get { return Physics.LinearVelocity; }
        }

        public Vector3 AngularVelocity
        {
            get { return Physics.AngularVelocity; }
        }

        public float Volume
        {
            get { return (float)grid.WorldAABB.Size.Volume; }
        }

        public BoundingBoxD BoundingBox
        {
            get { return grid.WorldAABB; }
        }

        public Vector3D Position
        {
            get { return grid.WorldMatrix.Translation; }
        }

        public bool InCombat
        {
            get { return InterdictionDuration > 0; }
        }

        public bool InAtmosphere
        {
            get { return Math.Abs(AirDensity) > 0; }
        }

        public bool InGravity
        {
            get { return Math.Abs(NaturalGravity) > 0; }
        }

        public bool MarkedForClose
        {
            get { return Grid.MarkedForClose; }
        }

        public IMyThrust[] Thrusters
        {
            get { lock (thrusters) { return thrusters.ToArray(); } }
        }

        public int ThrusterCount
        {
            get { return thrusters.Count; }
        }
    
        public IMyShipController[] Controllers
        {
            get { lock (controllers) { return controllers.ToArray(); } }
        }

        public int ControllerCount
        {
            get { return controllers.Count; }
        }
    }
}
