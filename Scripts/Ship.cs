using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRageMath;

namespace Khjin.CombatInterdiction
{
    public class Ship
    {
        public IMyCubeGrid Grid { private set; get; }
        public int InterdictionDuration;
        public float SpeedBuffer;
        public float SpeedBufferDecayRate;
        public float NaturalGravity;
        public float AirDensity;
        public bool InWater;
        public bool IsSubmerged;
        public List<IMyThrust> Thrusters;
        public List<IMyCubeGrid> Grids;

        public Ship(IMyCubeGrid grid)
        {
            Grid = Utilities.GetBaseGrid(grid);
            InterdictionDuration = 0;
            SpeedBuffer = 0;
            SpeedBufferDecayRate = 0;
            Thrusters = new List<IMyThrust>();
            Grids = new List<IMyCubeGrid>();
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
    }
}
