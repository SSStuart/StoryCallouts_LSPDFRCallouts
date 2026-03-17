using LSPD_First_Response.Mod.API;
using Rage;
using System.Collections.Generic;

namespace StoryCallouts
{
    enum EndBehavior
    {
        Nothing,
        Auto,
        Drive,
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

        public TasksList(Ped ped, EndBehavior endBehavior = EndBehavior.Auto)
        {
            _tasks = new List<Task>();
            _currentTaskIndex = 0;
            TaskFinished = false;
            _ped = ped;
            _endBehavior = endBehavior;
        }

        public void AddDriveTask(Vector3 position, int drivingSpeed = 80, int acceptedDistance = 20, VehicleDrivingFlags drivingFlags = VehicleDrivingFlags.Emergency)
        {
            DriveTo drivetask = new DriveTo(_ped, position, drivingSpeed, drivingFlags, acceptedDistance);
            _tasks.Add(drivetask);
        }

        public void AddChaseTask(Ped target)
        {
            Chase chaseTask = new Chase(_ped, target);
            _tasks.Add(chaseTask);
        }
        public void AddWalkTask(Vector3 position, int walkingSpeed = 3, float acceptedDistance = 1, bool force = false, int heading = 360)
        {
            WalkTo walkTask = new WalkTo(_ped, position, walkingSpeed, acceptedDistance, force, heading);
            _tasks.Add(walkTask);
        }
        public void AddEnterVehicleTask(Vehicle vehicle, int seatIndex = -1, float speed = 1, EnterVehicleFlags flags = EnterVehicleFlags.None)
        {
            EnterVehicle enterVehicleTask = new EnterVehicle(_ped, vehicle, seatIndex, speed, flags);
            _tasks.Add(enterVehicleTask);
        }
        public void AddClimbLadderTask()
        {
            ClimbLadder climbLadderTask = new ClimbLadder(_ped);
            _tasks.Add(climbLadderTask);
        }
        public void AddClimbTask()
        {
            Climb climbTask = new Climb(_ped);
            _tasks.Add(climbTask);
        }

        public void StartTasks()
        {
            _taskFiber = GameFiber.StartNew(delegate
            {
                Blip DEBUGBlip = new Blip(_tasks[0].Position);
                while (_currentTaskIndex < _tasks.Count)
                {
                    GameFiber.Yield();

                    if (!_ped.Exists() || _ped.IsDead || Functions.IsPedArrested(_ped) || Functions.IsPedGettingArrested(_ped))
                        break;

                    Task currentTask = _tasks[_currentTaskIndex];
                    if (currentTask.Position != Vector3.Zero)
                        DEBUGBlip.Position = currentTask.Position;
                    else
                        DEBUGBlip.Position = _ped.Position;
                    DEBUGBlip.Sprite = _currentTaskIndex > 0 && _currentTaskIndex <= 10 ? (BlipSprite)_currentTaskIndex + 16 : BlipSprite.Darts;

                    Game.LogTrivial($"Executing task #{_currentTaskIndex} for {_ped.Model.Name}");
                    currentTask.Execute();
                    _currentTaskIndex++;
                }
                Game.LogTrivial("Exiting tasks loop");
                DEBUGBlip.Delete();

                if (!_ped.Exists() || !_ped.IsAlive)
                {
                    TaskFinished = true;
                    return;
                }
                if (Functions.IsPedArrested(_ped) || Functions.IsPedGettingArrested(_ped))
                {
                    Game.LogTrivial("Reenabling Ped IA and returning");
                    Functions.SetPursuitDisableAIForPed(_ped, false);
                    TaskFinished = true;
                    return;
                }

                Game.LogTrivial("End Behaviour logic...");
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
                    case EndBehavior.Fight:
                        if (!_ped.IsInAnyVehicle(false))
                        {
                            Game.LogTrivial("Reenabling Ped IA");
                            _ped.Tasks.Clear();
                            Functions.SetPursuitDisableAIForPed(_ped, false);
                            break;
                        }
                        Game.LogTrivial("Leaving Vehicle");
                        _ped.Tasks.Clear();
                        _ped.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                        Game.LogTrivial("Reenabling Ped IA");
                        Functions.SetPursuitDisableAIForPed(_ped, false);
                        break;
                    default:
                        break;
                }
                TaskFinished = true;
            });
        }

        public void AbortTasks()
        {
            _taskFiber.Abort();
            if (_ped.Exists())
                _ped.Tasks.Clear();

            TaskFinished = true;
        }
    }
}
