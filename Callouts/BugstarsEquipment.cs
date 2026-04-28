using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Bugstars Equipment", CalloutProbability.Medium, "Stolen Bugstars van", "Code 3")]

    internal class BugstarsEquipment : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip, BugstarsEmployeeBlip;
        private LHandle Pursuit;
        private Ped Michael, BugstarsEmployee, TaxiDriver;
        private Vehicle BugstarsVan, Taxi;
        private bool NearSpawnMessageSent, ChaseCreated;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(148.7194f, -3089.305f, 5.695568f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 20f);
            AddMinimumDistanceCheck(100f, SpawnPoint);
            CalloutMessage = "Stolen Bugstars van";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_GRAND_THEFT_AUTO IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Michael = Characters.Michael.Create(SpawnPoint, 0, this.GetType().Name);

            BugstarsVan = new Vehicle("BURRITO2", new Vector3(148.7194f, -3089.305f, 5.695568f), 270)
            {
                IsEngineOn = true,
            };
            Michael.WarpIntoVehicle(BugstarsVan, -1);

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Stolen Bugstars van"
            };

            ChaseCreated = false;
            NearSpawnMessageSent = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 300)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message");

                CalloutInterfaceAPI.Functions.SendMessage(this, "Bugstars employees have reported that one of their vans was stolen");
                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 200)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting pursuit");

                EventBlip.Delete();
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Michael);
                Functions.SetPursuitDisableAIForPed(Michael, true);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                GameFiber.StartNew(delegate
                {
                    Michael.Tasks.DriveToPosition(new Vector3(193.2855f, -3013.432f, 5.390405f), 80, VehicleDrivingFlags.Emergency, 30).WaitForCompletion(20000);
                    Functions.SetPursuitDisableAIForPed(Michael, false);
                });

                ChaseCreated = true;
            }

            if (Game.IsKeyDown(Keys.End)
                || (ChaseCreated && !Functions.IsPursuitStillRunning(Pursuit)))
                End();
        }

        public override void End()
        {
            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Ending callout");

            base.End();

            if (EventBlip.Exists())
                EventBlip.Delete();
            if (Michael.Exists())
                Michael.Dismiss();

            GameFiber.StartNew(PostEndLogic);

            Game.LogTrivial($"[{Main.pluginName}] 'Bugstars Equipment' callout has ended.");
        }

        private void PostEndLogic()
        {
            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Executing post-end logic...");
            if (!BugstarsVan.Exists() || BugstarsVan.IsUpsideDown || BugstarsVan.EngineHealth <= 300 && Game.LocalPlayer.Character.DistanceTo(BugstarsVan) < 200)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Van not driveable/too far, ending");
                if (BugstarsVan.Exists())
                    BugstarsVan.Dismiss();
            }
            else
            {
                CalloutInterfaceAPI.Functions.SendMessage(this, "Bugstars has been informed of the van's location. An employee will retrieve it. ");

                if (BugstarsVan.DistanceTo2D(SpawnPoint) < 300)
                {
                    Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Van near warehouse, spawning employee on foot");
                    BugstarsEmployee = new Ped("s_m_y_pestcont_01", SpawnPoint, 0)
                    {
                        BlockPermanentEvents = true,
                    };
                    BugstarsEmployeeBlip = new Blip(BugstarsEmployee)
                    {
                        Color = Main.alliesColor,
                        Scale = 0.75f,
                        Name = "Bugstars employee"
                    };
                    BugstarsEmployee.Tasks.FollowNavigationMeshToPosition(BugstarsVan.Position, BugstarsVan.Heading, 2, 5f).WaitForCompletion();
                    if (BugstarsEmployee.Exists() && BugstarsVan.Exists())
                        BugstarsEmployee.Tasks.EnterVehicle(BugstarsVan, -1).WaitForCompletion(15000);
                    BugstarsEmployeeBlip.Delete();
                    if (BugstarsEmployee.Exists() && BugstarsVan.Exists())
                        BugstarsEmployee.Tasks.DriveToPosition(BugstarsVan, new Vector3(142.006f, -3089.013f, 5.508811f), 50, VehicleDrivingFlags.Normal | VehicleDrivingFlags.StopAtDestination, 5);
                }
                else
                {
                    Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Van far from warehouse, spawning taxi");
                    Taxi = new Vehicle("taxi", World.GetNextPositionOnStreet(BugstarsVan.Position.Around2D(200)));
                    TaxiDriver = new Ped("a_m_y_beachvesp_02", Taxi.GetOffsetPositionFront(5), 0);
                    TaxiDriver.WarpIntoVehicle(Taxi, -1);
                    BugstarsEmployee = new Ped("s_m_y_pestcont_01", Taxi.GetOffsetPositionFront(4), 0)
                    {
                        BlockPermanentEvents = true,
                    };
                    BugstarsEmployeeBlip = new Blip(BugstarsEmployee)
                    {
                        Color = Main.alliesColor,
                        Scale = 0.75f,
                        Name = "Bugstars employee"
                    };
                    BugstarsEmployee.WarpIntoVehicle(Taxi, 1);
                    TaxiDriver.Tasks.DriveToPosition(BugstarsVan.Position, 50, VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.AllowWrongWay, 40).WaitForCompletion(240000);
                    GameFiber.Yield();
                    GameFiber.Wait(3000);
                    if (BugstarsEmployee.Exists() && Taxi.Exists() && BugstarsVan.Exists() && BugstarsEmployee.DistanceTo(BugstarsVan) < 100)
                    {
                        BugstarsEmployee.Tasks.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion(5000);
                        BugstarsEmployee.Tasks.FollowNavigationMeshToPosition(BugstarsVan.GetOffsetPositionRight(-2), BugstarsVan.Heading, 2, 5f).WaitForCompletion(30000);
                        BugstarsEmployee.Tasks.EnterVehicle(BugstarsVan, -1).WaitForCompletion(10000);
                    }
                    if (TaxiDriver.Exists() && Taxi.Exists())
                    {
                        TaxiDriver.Tasks.CruiseWithVehicle(50);
                    }
                    if (BugstarsEmployee.Exists() && BugstarsVan.Exists() && BugstarsEmployee.DistanceTo(BugstarsVan) < 100)
                    {
                        BugstarsEmployee.KeepTasks = true;
                        BugstarsEmployee.Tasks.DriveToPosition(BugstarsVan, new Vector3(142.006f, -3089.013f, 5.508811f), 50, VehicleDrivingFlags.Normal | VehicleDrivingFlags.StopAtDestination, 5);
                    }
                }

                if (BugstarsVan.Exists())
                    BugstarsVan.Dismiss();
                if (BugstarsEmployee.Exists())
                    BugstarsEmployee.Dismiss();
                if (BugstarsEmployeeBlip.Exists())
                    BugstarsEmployeeBlip.Delete();

                if (TaxiDriver.Exists())
                    TaxiDriver.Dismiss();
                if (Taxi.Exists())
                    Taxi.Dismiss();
            }

            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Post-end logic ended");
        }
    }
}
