using LSPD_First_Response.Mod.API;
using Rage;
using System.Collections.Generic;

namespace StoryCallouts
{
    internal class Waypoint
    {
        public Vector3 Position { get; }
        public int Speed { get; }
        public VehicleDrivingFlags DrivingFlags { get; }
        public int AcceptedDistance { get; }

        public Waypoint(Vector3 position, int drivingSpeed, VehicleDrivingFlags drivingFlags, int acceptedDistance)
        {
            Position = position;
            Speed = drivingSpeed;
            DrivingFlags = drivingFlags;
            AcceptedDistance = acceptedDistance;
        }
    }

    enum EndBehavior
    {
        Nothing,
        Auto,
        Drive,
        Fight
    }

    internal class WaypointsList
    {
        private readonly List<Waypoint> _waypoints;
        private int _currentWaypoint;
        private readonly Ped _ped;
        //private readonly Vehicle _vehicle;
        private readonly EndBehavior _endBehavior;

        public WaypointsList(Ped ped, EndBehavior endBehavior = EndBehavior.Fight)
        {
            _waypoints = new List<Waypoint>();
            _currentWaypoint = 0;
            _ped = ped;
            _endBehavior = endBehavior;
        }
        //public WaypointsList(Ped ped, Vehicle vehicle, EndBehavior endBehavior = EndBehavior.Auto)
        //{
        //    _waypoints = new List<Waypoint>();
        //    _currentWaypoint = 0;
        //    _ped = ped;
        //    _vehicle = vehicle;
        //    _endBehavior = endBehavior;
        //}

        public void AddWaypoint(Vector3 position, int drivingSpeed = 80, int acceptedDistance = 20, VehicleDrivingFlags drivingFlags = VehicleDrivingFlags.Emergency)
        {
            Waypoint waypoint = new Waypoint(position, drivingSpeed, drivingFlags, acceptedDistance);
            _waypoints.Add(waypoint);
        }

        public void StartTasks()
        {
            GameFiber.StartNew(delegate
            {
                Blip DEBUGBlip = new Blip(_waypoints[0].Position);
                while (_currentWaypoint < _waypoints.Count)
                {
                    GameFiber.Yield();

                    if (!_ped.Exists() || !_ped.IsInAnyVehicle(false))
                        break;

                    Waypoint currWaypoint = _waypoints[_currentWaypoint];
                    DEBUGBlip.Position = currWaypoint.Position;
                    DEBUGBlip.Sprite = _currentWaypoint == 0 ? BlipSprite.Player : (BlipSprite)_currentWaypoint + 16;
                    _ped.Tasks.DriveToPosition(currWaypoint.Position, currWaypoint.Speed, currWaypoint.DrivingFlags, currWaypoint.AcceptedDistance).WaitForCompletion(60000);
                    _currentWaypoint++;
                }
                DEBUGBlip.Delete();

                if (!_ped.Exists() || !_ped.IsAlive)
                    return;

                _ped.Tasks.Clear();

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
                        _ped.Tasks.CruiseWithVehicle(_waypoints[_currentWaypoint - 1].Speed, _waypoints[_currentWaypoint - 1].DrivingFlags);
                        break;
                    case EndBehavior.Fight:
                        if (!_ped.IsInAnyVehicle(false))
                            break;
                        _ped.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                        Functions.SetPursuitDisableAIForPed(_ped, false);
                        break;
                    default:
                        break;
                }
            });
        }
    }
}
