using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
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
            Initialize();
        }

        public void Initialize()
        {
            // Add block update events so we don't have to track blocks
            // block changes manually
            gridGroupData = grid.GetGridGroup(GridLinkTypeEnum.Mechanical);
            gridGroupData.GetGrids(grids);
            lock (grids)
            {
                foreach(IMyCubeGrid grid in grids)
                {
                    grid.OnMarkForClose += OnMarkForClose;
                    grid.OnBlockAdded += OnBlockAdded;
                    grid.OnBlockRemoved += OnBlockRemoved;

                    lock (thrusters)
                    {
                        thrusters.AddRange(grid.GetFatBlocks<IMyThrust>());
                    }
                    
                    lock (controllers)
                    {
                        controllers.AddRange(grid.GetFatBlocks<IMyShipController>());
                    }
                }
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

        public float Mass
        {
            get { return Physics.Mass; }
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

        private void OnMarkForClose(IMyEntity grid)
        {
            this.grid.OnBlockAdded -= OnBlockAdded;
            this.grid.OnBlockRemoved -= OnBlockRemoved;
            this.grid.OnMarkForClose -= OnMarkForClose;
        }

        private void OnBlockAdded(IMySlimBlock block)
        {
            lock (gridGroupData)
            {
                gridGroupData = grid.GetGridGroup(GridLinkTypeEnum.Mechanical);
                IMyGridGroupData blockGridGroupData = block.CubeGrid.GetGridGroup(GridLinkTypeEnum.Mechanical);
                if (blockGridGroupData != gridGroupData) { return; }
            }

            if (block is IMyThrust)
            {
                lock (thrusters)
                {
                    IMyThrust thruster = block as IMyThrust;
                    if (!thrusters.Contains(thruster))
                    {
                        thrusters.Add(thruster);
                    }
                }
            }
            else if (block is IMyShipController)
            {
                lock (controllers)
                {
                    IMyShipController controller = block as IMyShipController;
                    if (!controllers.Contains(controller))
                    {
                        controllers.Add(controller);
                    }
                }
            }
            else
            {
                // DO NOTHING
            }
        }

        private void OnBlockRemoved(IMySlimBlock block)
        {
            lock (gridGroupData)
            {
                gridGroupData = grid.GetGridGroup(GridLinkTypeEnum.Mechanical);
                IMyGridGroupData blockGridGroupData = block.CubeGrid.GetGridGroup(GridLinkTypeEnum.Mechanical);
                if (blockGridGroupData != gridGroupData) { return; }
            }

            if (block is IMyThrust)
            {
                lock (thrusters)
                {
                    IMyThrust thruster = block as IMyThrust;
                    if (thrusters.Contains(thruster))
                    {
                        thrusters.Remove(thruster);
                    }
                }
            }
            else if (block is IMyShipController)
            {
                lock (controllers)
                {
                    IMyShipController controller = block as IMyShipController;
                    if (controllers.Contains(controller))
                    {
                        controllers.Remove(controller);
                    }
                }
            }
            else
            {
                // DO NOTHING
            }
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
