using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using System.Collections.Generic;

namespace StoryCallouts
{
    enum EndBehavior
    {
        Nothing,
        Auto,
        Drive,
        Cruise,
        Fight
    }

    internal class TasksList
    {
        private readonly List<Task> _tasks;
        private int _currentTaskIndex;
        public bool TaskFinished { get; private set; }
        private readonly Ped _ped;
        private readonly EndBehavior _endBehavior;
        private GameFiber _taskFiber;
        private Blip DEBUG_BLIP;
        private GameFiber DEBUG_CHECKPOINT_FIBER;

        public TasksList(Ped ped, EndBehavior endBehavior = EndBehavior.Auto)
        {
            _tasks = new List<Task>();
            _currentTaskIndex = 0;
            TaskFinished = false;
            _ped = ped;
            _endBehavior = endBehavior;
        }

        public void AddDriveTask(Vector3 position, int drivingSpeed = 80, int acceptedDistance = 20, VehicleDrivingFlags drivingFlags = VehicleDrivingFlags.Emergency, int timeoutSec = 60)
        {
            DriveTo drivetask = new DriveTo(_ped, position, drivingSpeed, drivingFlags, acceptedDistance, timeoutSec);
            _tasks.Add(drivetask);
        }
        public void AddChaseTask(Ped target)
        {
            Chase chaseTask = new Chase(_ped, target);
            _tasks.Add(chaseTask);
        }
        public void AddFollowInVehicleTask(Ped target, int distance = 20)
        {
            FollowInVehicle followInVehicleTask = new FollowInVehicle(_ped, target, distance);
            _tasks.Add(followInVehicleTask);
        }
        public void AddWalkTask(Vector3 position, int walkingSpeed = 3, float acceptedDistance = 1, bool followNavMesh = false, bool force = false, int heading = 0, int timeoutSec = 60)
        {
            WalkTo walkTask = new WalkTo(_ped, position, walkingSpeed, acceptedDistance, followNavMesh, force, heading, timeoutSec);
            _tasks.Add(walkTask);
        }
        public void AddWalkAimingTask(Vector3 position, Entity entityToAimAt, float acceptedDistance = 1, int walkingSpeed = 3, FiringPattern firingPattern = FiringPattern.BurstFire, int timeoutSec = 60)
        {
            WalkToAiming walkAimingTask = new WalkToAiming(_ped, position, entityToAimAt, walkingSpeed, acceptedDistance, true, firingPattern, timeoutSec);
            _tasks.Add(walkAimingTask);
        }
        public void AddWalkAimingRandomEnemyTask(Vector3 position, float acceptedDistance = 1, int walkingSpeed = 3, FiringPattern firingPattern = FiringPattern.BurstFire, int timeoutSec = 60)
        {
            WalkToAiming walkAimingRandomEnemyTask = new WalkToAiming(_ped, position, walkingSpeed, acceptedDistance, true, firingPattern, timeoutSec);
            _tasks.Add(walkAimingRandomEnemyTask);
        }
        public void AddEnterVehicleTask(Vehicle vehicle, int seatIndex = -2, float speed = 1, EnterVehicleFlags flags = EnterVehicleFlags.None, int timeoutSec = 60)
        {
            EnterVehicle enterVehicleTask = new EnterVehicle(_ped, vehicle, seatIndex, speed, flags, timeoutSec);
            _tasks.Add(enterVehicleTask);
        }
        public void AddExitVehicleTask(LeaveVehicleFlags flags = LeaveVehicleFlags.None, int timeoutSec = 60)
        {
            ExitVehicle exitVehicleTask = new ExitVehicle(_ped, flags, timeoutSec);
            _tasks.Add(exitVehicleTask);
        }
        public void AddClimbLadderTask(int timeoutSec = 60)
        {
            ClimbLadder climbLadderTask = new ClimbLadder(_ped, timeoutSec);
            _tasks.Add(climbLadderTask);
        }
        public void AddClimbTask(int timeoutSec = 60)
        {
            Climb climbTask = new Climb(_ped, timeoutSec);
            _tasks.Add(climbTask);
        }

        public void StartTasks()
        {
            _taskFiber = GameFiber.StartNew(delegate
            {
                if (Settings.debug)
                    DEBUG_BLIP = new Blip(_tasks[0].Position);
                while (_currentTaskIndex < _tasks.Count)
                {
                    GameFiber.Yield();

                    if (!_ped.Exists() || _ped.IsDead || Functions.IsPedArrested(_ped) || Functions.IsPedGettingArrested(_ped))
                        break;

                    Task currentTask = _tasks[_currentTaskIndex];
                    if (Settings.debug)
                    {
                        if (currentTask.Position != Vector3.Zero)
                            DEBUG_BLIP.Position = currentTask.Position;
                        else
                            DEBUG_BLIP.Position = _ped.Position;
                        DEBUG_BLIP.Sprite = _currentTaskIndex > 0 && _currentTaskIndex <= 10 ? (BlipSprite)_currentTaskIndex + 16 : BlipSprite.Darts;
                        DEBUG_BLIP.Name = $"Task #{_currentTaskIndex} {currentTask.GetType().Name} for {_ped.Model.Name}";

                        //Game.LogTrivial($"Executing task #{_currentTaskIndex} {currentTask.GetType().Name} for {_ped.Model.Name}");
                        DEBUG_CHECKPOINT_FIBER = GameFiber.StartNew(delegate
                        {
                            while (true)
                            {
                                GameFiber.Yield();
                                float markerWidth = 3f;
                                if (currentTask.GetType().Name == "DriveTo")
                                {
                                    DriveTo currentDriveTask = (DriveTo)currentTask;
                                    markerWidth = currentDriveTask._acceptedDistance;
                                }
                                NativeFunction.Natives.DRAW_MARKER(
                                    _currentTaskIndex < _tasks.Count-1 ? 1 : 4,
                                    currentTask.Position.X, currentTask.Position.Y, currentTask.Position.Z + (_currentTaskIndex == _tasks.Count - 1 ? 5 : -2),
                                    0f, 0f, 0f,
                                    0f, 0f, 0f,
                                    markerWidth, markerWidth, 10f,
                                    133, 85, 255, 175,
                                    false, _currentTaskIndex == _tasks.Count -1, 2, false, 0, 0, false);
                            }
                        });
                    }
                    currentTask.Execute();
                    if (Settings.debug && DEBUG_CHECKPOINT_FIBER != null && DEBUG_CHECKPOINT_FIBER.IsAlive)
                        DEBUG_CHECKPOINT_FIBER.Abort();
                    _currentTaskIndex++;
                }
                if (Settings.debug)
                    DEBUG_BLIP.Delete();

                if (!_ped.Exists() || !_ped.IsAlive)
                {
                    TaskFinished = true;
                    return;
                }
                if (Functions.IsPedArrested(_ped) || Functions.IsPedGettingArrested(_ped))
                {
                    Functions.SetPursuitDisableAIForPed(_ped, false);
                    TaskFinished = true;
                    return;
                }

                ExecuteEndBehavior();
            });
        }

        public void ExecuteEndBehavior()
        {
            if (TaskFinished)
                return;

            //Game.LogTrivial("End Behaviour logic...");
            switch (_endBehavior)
            {
                case EndBehavior.Nothing:
                    break;
                case EndBehavior.Auto:
                    _ped.Tasks.Clear();
                    Functions.SetPursuitDisableAIForPed(_ped, false);
                    break;
                case EndBehavior.Drive:
                    if (!_ped.IsInAnyVehicle(false))
                    {
                        _ped.Tasks.Clear();
                        break;
                    }
                    _ped.Tasks.CruiseWithVehicle(80, VehicleDrivingFlags.Emergency);
                    break;
                case EndBehavior.Cruise:
                    if (!_ped.IsInAnyVehicle(false))
                    {
                        _ped.Tasks.Clear();
                        break;
                    }
                    _ped.Tasks.CruiseWithVehicle(50);
                    break;
                case EndBehavior.Fight:
                    if (!_ped.IsInAnyVehicle(false))
                    {
                        //Game.LogTrivial("Reenabling Ped IA");
                        _ped.Tasks.Clear();
                        Functions.SetPursuitDisableAIForPed(_ped, false);
                        break;
                    }
                    //Game.LogTrivial("Leaving Vehicle");
                    _ped.Tasks.Clear();
                    _ped.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                    //Game.LogTrivial("Reenabling Ped IA");
                    Functions.SetPursuitDisableAIForPed(_ped, false);
                    break;
                default:
                    break;
            }

            if (DEBUG_BLIP.Exists())
                DEBUG_BLIP.Delete();

            TaskFinished = true;
        }

        public void AbortTasks()
        {
            _taskFiber.Abort();
            if (_ped.Exists())
                _ped.Tasks.Clear();

            if (DEBUG_BLIP.Exists())
                DEBUG_BLIP.Delete();

            TaskFinished = true;
        }
    }
}
