using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRageMath;

namespace Khjin.CombatInterdiction
{
    public class Ship
    {
        public readonly IMyCubeGrid Grid;
        public int InterdictionDuration;
        public float NaturalGravity;
        public float AirDensity;
        public bool InWater;
        public bool IsSubmerged;
        public bool IsOnBoost;
        public float CurrentLinearSpeed;
        public float CurrentAngularSpeed;
        private readonly List<IMyCubeGrid> grids;
        private readonly List<IMyThrust> thrusters;
        private readonly object gridLock = new object();
        private readonly object thrusterLock = new object();
        private readonly object wheelLock = new object();

        public Ship(IMyCubeGrid grid)
        {
            Grid = grid;
            grids = new List<IMyCubeGrid>();
            thrusters = new List<IMyThrust>();
            InterdictionDuration = 0;
        }

        public long EntityId
        {
            get { return Grid.EntityId; }
        }

        public string Name
        {
            get { return Grid.Name; }
        }

        public float Mass
        {
            get { return Grid.Physics.Mass; }
        }

        public float Volume
        {
            get { return (float) Grid.WorldAABB.Size.Volume; }
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

        public IMyGridTerminalSystem GTS
        {
            get { return MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Grid); }
        }

        public Vector3 LinearVelocity
        {
            get { return Grid.Physics.LinearVelocity; }
        }

        public Vector3 AngularVelocity
        {
            get { return Grid.Physics.AngularVelocity; }
        }

        public BoundingBoxD BoundingBox
        {
            get { return Grid.WorldAABB; }
        }

        public Vector3D Position
        {
            get { return Grid.WorldMatrix.Translation; }
        }

        public bool MarkedForClose
        {
            get { return Grid.MarkedForClose; }
        }
    
        public void HoldThrusters(Action<List<IMyThrust>> action)
        {
            lock(thrusterLock)
            {
                action(thrusters);
            }
        }

        public void HoldGrids(Action<List<IMyCubeGrid>> action)
        {
            lock (gridLock)
            {
                action(grids);
            }
        }

        public int ThrusterCount
        {
            get { lock (thrusterLock) { return thrusters.Count; } }
        }
    }
}
