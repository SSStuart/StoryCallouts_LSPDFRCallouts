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
        private readonly Ped _ped;
        private readonly EndBehavior _endBehavior;

        public TasksList(Ped ped, EndBehavior endBehavior = EndBehavior.Fight)
        {
            _tasks = new List<Task>();
            _currentTaskIndex = 0;
            _ped = ped;
            _endBehavior = endBehavior;
        }

        public void AddDriveTask(Vector3 position, int drivingSpeed = 80, int acceptedDistance = 20, VehicleDrivingFlags drivingFlags = VehicleDrivingFlags.Emergency)
        {
            DriveTo drivetask = new DriveTo(_ped, position, drivingSpeed, drivingFlags, acceptedDistance);
            _tasks.Add(drivetask);
        }
        public void AddWalkTask(Vector3 position, int walkingSpeed = 3, bool force = false, int heading = 360)
        {
            WalkTo walkTask = new WalkTo(_ped, position, walkingSpeed, force, heading);
            _tasks.Add(walkTask);
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
            GameFiber.StartNew(delegate
            {
                Blip DEBUGBlip = new Blip(_tasks[0].Position);
                while (_currentTaskIndex < _tasks.Count)
                {
                    GameFiber.Yield();

                    if (!_ped.Exists() || _ped.IsDead || Functions.IsPedArrested(_ped) || Functions.IsPedGettingArrested(_ped))
                        break;

                    Task currentTask = _tasks[_currentTaskIndex];
                    DEBUGBlip.Position = currentTask.Position;
                    DEBUGBlip.Sprite = _currentTaskIndex > 0 && _currentTaskIndex <= 10 ? (BlipSprite)_currentTaskIndex + 16 : BlipSprite.Darts;

                    Game.LogTrivial($"Executing task #{_currentTaskIndex}");
                    currentTask.Execute();
                    _currentTaskIndex++;
                }
                Game.LogTrivial("Exiting tasks loop");
                DEBUGBlip.Delete();

                if (!_ped.Exists() || !_ped.IsAlive)
                    return;
                if (Functions.IsPedArrested(_ped) || Functions.IsPedGettingArrested(_ped))
                {
                    Game.LogTrivial("Reenabling Ped IA and returning");
                    Functions.SetPursuitDisableAIForPed(_ped, false);
                    return;
                }

                Game.LogTrivial("Clearing ped task");
                _ped.Tasks.Clear();

                Game.LogTrivial("End Behaviour logic...");
                switch (_endBehavior)
                {
                    case EndBehavior.Nothing:
                        break;
                    case EndBehavior.Auto:
                        Functions.SetPursuitDisableAIForPed(_ped, false);
                        break;
                    case EndBehavior.Drive:
                        if (!_ped.IsInAnyVehicle(false))
                            break;
                        _ped.Tasks.CruiseWithVehicle(80, VehicleDrivingFlags.Emergency);
                        break;
                    case EndBehavior.Fight:
                        if (!_ped.IsInAnyVehicle(false))
                        {
                            Game.LogTrivial("Reenabling Ped IA");
                            Functions.SetPursuitDisableAIForPed(_ped, false);
                            break;
                        }
                        Game.LogTrivial("Leaving Vehicle");
                        _ped.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                        Game.LogTrivial("Reenabling Ped IA");
                        Functions.SetPursuitDisableAIForPed(_ped, false);
                        break;
                    default:
                        break;
                }
            });
        }
    }
}
