using LSPD_First_Response.Mod.API;
using Rage;

namespace StoryCallouts
{
    internal abstract class Task
    {
        public Vector3 Position { get; protected set; }
        protected Ped _ped;

        public abstract void Execute(int timeoutSeconds = 60);
    }

    internal class DriveTo : Task
    {
        public int Speed { get; }
        public VehicleDrivingFlags DrivingFlags { get; }
        public int AcceptedDistance { get; }

        public DriveTo(Ped ped, Vector3 position, int drivingSpeed, VehicleDrivingFlags drivingFlags, int acceptedDistance)
        {
            _ped = ped;
            Position = position;
            Speed = drivingSpeed;
            DrivingFlags = drivingFlags;
            AcceptedDistance = acceptedDistance;
        }

        public override void Execute(int timeoutSeconds)
        {
            int counterSec = 0;
            if (!_ped.IsInAnyVehicle(false))
                return;
            Rage.Task task = _ped.Tasks.DriveToPosition(Position, Speed, DrivingFlags, AcceptedDistance);

            do
            {
                GameFiber.Yield();
                GameFiber.Wait(100);
                counterSec++;
            }
            while (counterSec < timeoutSeconds && task != null && _ped.Exists() && _ped.Tasks.CurrentTaskStatus == TaskStatus.InProgress && !Functions.IsPedGettingArrested(_ped) && !Functions.IsPedArrested(_ped));
            if (_ped.Exists())
                _ped.Tasks.Clear();
        }
    }

    internal class Chase : Task
    {
        private readonly Ped _target;

        public Chase(Ped ped, Ped target)
        {
            _ped = ped;
            _target = target;
        }
        public override void Execute(int timeoutSeconds)
        {
            if (_ped.Exists() && _target.Exists())
                _ped.Tasks.ChaseWithGroundVehicle(_target);
        }
    }

    internal class WalkTo : Task
    {
        public int Speed { get; }
        private readonly bool _force;
        private readonly int _heading;

        public WalkTo(Ped ped, Vector3 position, int walkingSpeed, bool force, int heading)
        {
            _ped = ped;
            Position = position;
            Speed = walkingSpeed;
            _force = force;
            _heading = heading;
        }

        public override void Execute(int timeoutSeconds)
        {
            if (_ped.Exists())
            {
                _ped.Tasks.GoStraightToPosition(Position, Speed, _heading == 360 ? _ped.Heading : _heading, 1, 60000).WaitForCompletion(timeoutSeconds * 1000);
                if (_force && _ped.DistanceTo(Position) > 1)
                    _ped.Position = Position;
            }
        }
    }

    internal class ClimbLadder : Task
    {
        public ClimbLadder(Ped ped)
        {
            _ped = ped;
        }
        public override void Execute(int timeoutSeconds)
        {
            if (_ped.Exists())
                _ped.Tasks.ClimbLadder().WaitForCompletion(timeoutSeconds * 1000);
        }
    }
    internal class Climb : Task
    {
        public Climb(Ped ped)
        {
            _ped = ped;
        }
        public override void Execute(int timeoutSeconds)
        {
            if (_ped.Exists())
                _ped.Tasks.Climb().WaitForCompletion(timeoutSeconds * 1000);
        }
    }
}
