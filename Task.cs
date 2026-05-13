using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using System.Linq;

namespace StoryCallouts
{
    internal abstract class Task
    {
        public Vector3 Position { get; protected set; }
        protected Ped _ped;
        protected int _timeoutSec;

        public abstract void Execute();
    }

    internal class DriveTo : Task
    {
        private readonly int _speed;
        private readonly VehicleDrivingFlags _flags;
        private readonly int _acceptedDistance;

        public DriveTo(Ped ped, Vector3 position, int drivingSpeed, VehicleDrivingFlags drivingFlags, int acceptedDistance, int timeoutSec)
        {
            _ped = ped;
            Position = position;
            _speed = drivingSpeed;
            _flags = drivingFlags;
            _acceptedDistance = acceptedDistance;
            _timeoutSec = timeoutSec;
        }

        public override void Execute()
        {
            int counterSec = 0;
            if (!_ped.IsInAnyVehicle(false) || ((_ped.CurrentVehicle.IsBoat || _ped.CurrentVehicle.Model.Name.ToUpper() == "SEASHARK") && !_ped.CurrentVehicle.IsInWater))
                return;
            Rage.Task task = _ped.Tasks.DriveToPosition(Position, _speed, _flags, _acceptedDistance);

            do
            {
                GameFiber.Yield();
                GameFiber.Wait(50);
                counterSec++;
            }
            while (counterSec < _timeoutSec * 10 && task != null && _ped.Exists() && _ped.IsAlive && (_ped.Tasks.CurrentTaskStatus == TaskStatus.InProgress || _ped.Tasks.CurrentTaskStatus == TaskStatus.Interrupted) && !Functions.IsPedGettingArrested(_ped) && !Functions.IsPedArrested(_ped));
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
        public override void Execute()
        {
            if (_ped.Exists() && _target.Exists())
                _ped.Tasks.ChaseWithGroundVehicle(_target);
        }
    }

    internal class FollowInVehicle : Task
    {
        private readonly Ped _target;
        private readonly int _distance;

        public FollowInVehicle(Ped ped, Ped target, int distance)
        {
            _ped = ped;
            _target = target;
            _distance = distance;
        }

        public override void Execute()
        {
            if (_ped.Exists() && _ped.IsInAnyVehicle(false) && _target.Exists())
            {
                NativeFunction.Natives.TASK_VEHICLE_FOLLOW(_ped, _ped.CurrentVehicle, _target, 100f, 1074528293, _distance);
                _ped.KeepTasks = true;

                while (_ped.Exists() && _ped.IsAlive && _target.Exists() && _target.IsInAnyVehicle(false))
                {
                    GameFiber.Yield();
                    GameFiber.Wait(500);
                }
            }
        }
    }

    internal class WalkTo : Task
    {
        private readonly int _speed;
        private readonly float _acceptedDistance;
        private readonly bool _followNavMesh;
        private readonly bool _force;
        private readonly int _heading;

        public WalkTo(Ped ped, Vector3 position, int walkingSpeed, float acceptedDistance, bool followNavMesh, bool force, int heading, int timeoutSec)
        {
            _ped = ped;
            Position = position;
            _speed = walkingSpeed;
            _acceptedDistance = acceptedDistance;
            _followNavMesh = followNavMesh;
            _force = force;
            _heading = heading;
            _timeoutSec = timeoutSec;
        }

        public override void Execute()
        {
            if (_ped.Exists())
            {
                int counterSec = 0;
                Rage.Task task;
                if (_followNavMesh)
                    task = _ped.Tasks.FollowNavigationMeshToPosition(Position, _heading == 360 ? _ped.Heading : _heading, _speed, _acceptedDistance, _timeoutSec * 1000);
                else
                    task = _ped.Tasks.GoStraightToPosition(Position, _speed, _heading == 360 ? _ped.Heading : _heading, 1, _timeoutSec * 1000);

                do
                {
                    GameFiber.Yield();
                    GameFiber.Wait(100);
                    counterSec++;
                }
                while (counterSec < _timeoutSec * 10 && task != null && _ped.Exists() && _ped.IsAlive && (_ped.Tasks.CurrentTaskStatus == TaskStatus.InProgress || _ped.Tasks.CurrentTaskStatus == TaskStatus.Interrupted) && _ped.DistanceTo(Position) > _acceptedDistance && !Functions.IsPedGettingArrested(_ped) && !Functions.IsPedArrested(_ped));
                if (_ped.Exists())
                {
                    if (_force)
                        _ped.Position = Position;
                    _ped.Tasks.Clear();
                }
            }
        }
    }

    internal class WalkToAiming : Task
    {
        private Entity _targetEntity;
        private readonly float _speed;
        private readonly float _acceptedDistance;
        private readonly bool _fireWeapon;
        private readonly FiringPattern _firingPattern;

        public WalkToAiming(Ped ped, Vector3 position, Entity targetEntity, float speed, float acceptedDistance,  bool fireWeapon, FiringPattern firingPattern, int timeoutSec)
        {
            _ped = ped;
            Position = position;
            _targetEntity = targetEntity;
            _speed = speed;
            _acceptedDistance = acceptedDistance;
            _fireWeapon = fireWeapon;
            _firingPattern = firingPattern;
            _timeoutSec = timeoutSec;
        }

        public WalkToAiming(Ped ped, Vector3 position, float speed, float acceptedDistance, bool fireWeapon, FiringPattern firingPattern, int timeoutSec)
        {
            _ped = ped;
            Position = position;
            _targetEntity = null;
            _speed = speed;
            _acceptedDistance = acceptedDistance;
            _fireWeapon = fireWeapon;
            _firingPattern = firingPattern;
            _timeoutSec = timeoutSec;
        }

        public override void Execute()
        {
            if (_ped.Exists())
            {
                int counterSec = 0;
                if (_targetEntity == null)
                {
                    Ped[] enemies = Main.GetNearbyEnnemies(_ped.Position);
                    _targetEntity = enemies.Length > 0 ? enemies[MathHelper.GetRandomInteger(enemies.Length)] : Game.LocalPlayer.Character;
                }

                Rage.Task task = _ped.Tasks.GoToWhileAiming(Position, _targetEntity, _acceptedDistance, _speed, _fireWeapon, _firingPattern);

                do
                {
                    GameFiber.Yield();
                    GameFiber.Wait(100);
                    counterSec++;
                }
                while (counterSec < _timeoutSec * 10 && task != null && _ped.Exists() && _ped.IsAlive && (_ped.Tasks.CurrentTaskStatus == TaskStatus.InProgress || _ped.Tasks.CurrentTaskStatus == TaskStatus.Interrupted) && _ped.DistanceTo(Position) > _acceptedDistance && !Functions.IsPedGettingArrested(_ped) && !Functions.IsPedArrested(_ped));
                if (_ped.Exists())
                    _ped.Tasks.Clear();
            }
        }
    }

    internal class EnterVehicle : Task
    {
        private readonly Vehicle _vehicle;
        private readonly int _seatIndex;
        private readonly float _speed;
        private readonly EnterVehicleFlags _flags;
        public EnterVehicle(Ped ped, Vehicle vehicle, int seatIndex, float speed, EnterVehicleFlags flags, int timeoutSec)
        {
            _ped = ped;
            _vehicle = vehicle;
            _seatIndex = seatIndex;
            _speed = speed;
            _flags = flags;
            _timeoutSec = timeoutSec;
        }

        public override void Execute()
        {
            if (_ped.Exists() && _ped.IsAlive && _vehicle.Exists())
                _ped.Tasks.EnterVehicle(_vehicle, _timeoutSec * 1000, _seatIndex, _speed, _flags).WaitForCompletion(_timeoutSec * 1000);
        }
    }

    internal class ExitVehicle : Task
    {
        private readonly LeaveVehicleFlags _flags;
        public ExitVehicle(Ped ped, LeaveVehicleFlags flags, int timeoutSec)
        {
            _ped = ped;
            _flags = flags;
            _timeoutSec = timeoutSec;
        }

        public override void Execute()
        {
            if (_ped.Exists() && _ped.IsAlive && _ped.IsInAnyVehicle(false))
                _ped.Tasks.LeaveVehicle(_flags).WaitForCompletion(_timeoutSec * 1000);
        }
    }

    internal class ClimbLadder : Task
    {
        public ClimbLadder(Ped ped, int timeoutSec)
        {
            _ped = ped;
            _timeoutSec = timeoutSec;
        }
        public override void Execute()
        {
            if (_ped.Exists() && _ped.IsAlive)
                _ped.Tasks.ClimbLadder().WaitForCompletion(_timeoutSec * 1000);
        }
    }
    internal class Climb : Task
    {
        public Climb(Ped ped, int timeoutSec)
        {
            _ped = ped;
            _timeoutSec = timeoutSec;
        }
        public override void Execute()
        {
            if (_ped.Exists() && _ped.IsAlive)
                _ped.Tasks.Climb().WaitForCompletion(_timeoutSec * 1000);
        }
    }
}
