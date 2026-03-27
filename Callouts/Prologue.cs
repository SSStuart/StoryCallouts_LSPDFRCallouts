using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Prologue", CalloutProbability.Medium, "Bobcat Security Depot under attack", "Code 3")]

    internal class Prologue : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Ped Michael, Trevor, Brad, LocalYokel;
        private Vehicle EscapeVehicle, RoadblockVehicle;
        private TasksList MichaelEscape, TrevorEscape, BradEscape;
        private bool NearSpawnMessageSent, ChaseCreated, EnteredVehicle, RoadblockEscape;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(5345.727f, -5188.353f, 82.7874f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            AddMinimumDistanceCheck(300f, SpawnPoint);
            AddMaximumDistanceCheck(2400, SpawnPoint);
            CalloutMessage = "Bobcat Security Depot under attack";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_ROBBERY IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Michael = Characters.Michael.Create(new Vector3(5331.559f, -5185.187f, 82.75967f), 260, this.GetType().Name);
            Michael.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_CARBINERIFLE"), 500, true);

            Trevor = Characters.Trevor.Create(new Vector3(5331.24f, -5191.96f, 82.76762f), 260, this.GetType().Name);
            Trevor.KeepTasks = true;
            Trevor.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_CARBINERIFLE"), 500, true);

            Brad = Characters.Brad.Create(new Vector3(5325.857f, -5185.191f, 82.77296f), 260, this.GetType().Name);
            Brad.KeepTasks = true;
            Brad.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_PUMPSHOTGUN"), 200, true);

            LocalYokel = new Ped("u_m_y_proldriver_01", new Vector3(5430.592f, -5116.286f, 78.21192f), 0)
            {
                IsPersistent = true,
                BlockPermanentEvents = true,
            };

            EscapeVehicle = new Vehicle("rancherxl2", new Vector3(5418.41f, -5118.01f, 79.80f), 106)
            {
                IsEngineOn = true,
                IsPersistent = true,
            };
            LocalYokel.WarpIntoVehicle(EscapeVehicle, -1);

            MichaelEscape = new TasksList(Michael);
            TrevorEscape = new TasksList(Trevor);
            BradEscape = new TasksList(Brad);
            int seatIndex = 0;
            foreach (TasksList EscapeToCar in new[] { MichaelEscape, TrevorEscape, BradEscape})
            {
                EscapeToCar.AddWalkTask(new Vector3(5375.041f, -5187.116f, 82.05238f).Around2D(0, 2), 4, 4);
                EscapeToCar.AddWalkTask(new Vector3(5404.926f, -5179.46f, 80.05573f).Around2D(0, 2), 4, 4);
                EscapeToCar.AddWalkTask(new Vector3(5427.249f, -5164.433f, 78.42853f).Around2D(0, 2), 4, 4);
                EscapeToCar.AddWalkTask(new Vector3(5435.263f, -5144.604f, 78.2371f).Around2D(0, 2), 4, 4);
                EscapeToCar.AddWalkTask(new Vector3(5423.29f, -5120.707f, 78.02209f).Around2D(0, 2), 4, 4);
                EscapeToCar.AddEnterVehicleTask(EscapeVehicle, seatIndex, 3);
                seatIndex++;
            }

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Bobcat Security Depot under attack"
            };

            ChaseCreated = false;
            NearSpawnMessageSent = false;
            EnteredVehicle = false;
            RoadblockEscape = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 300)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message");

                CalloutInterfaceAPI.Functions.SendMessage(this, "Three armed suspects have robbed the Bobcat Security Depot and are now exiting the building");
                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 500)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting pursuit & Spawning roadblock");

                EventBlip.Delete();
                MichaelEscape.StartTasks();
                TrevorEscape.StartTasks();
                BradEscape.StartTasks();

                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Michael);
                Functions.AddPedToPursuit(Pursuit, Trevor);
                Functions.AddPedToPursuit(Pursuit, Brad);
                Functions.SetPursuitDisableAIForPed(Michael, true);
                Functions.SetPursuitDisableAIForPed(Trevor, true);
                Functions.SetPursuitDisableAIForPed(Brad, true);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                RoadblockVehicle = new Vehicle("policeold1", new Vector3(3500.8f, -4868.237f, 111.4071f), 340)
                {
                    IsPersistent = true,
                    IsSirenOn = true,
                    IsEngineOn = true
                };

                ChaseCreated = true;
            }

            if (!EnteredVehicle && Michael.DistanceTo(EscapeVehicle) < 5)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Michael close to escape vehicle");

                Functions.AddPedToPursuit(Pursuit, LocalYokel);
                Functions.SetPursuitDisableAIForPed(LocalYokel, true);

                GameFiber.StartNew(delegate
                {
                    Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Waiting for all passengers...");

                    do
                    {
                        GameFiber.Yield();
                        GameFiber.Wait(500);

                        if (Functions.IsPedArrested(LocalYokel) || Functions.IsPedArrested(Michael) || Functions.IsPedArrested(Trevor) || Functions.IsPedArrested(Brad) || Game.LocalPlayer.Character.DistanceTo(EscapeVehicle) < 10)
                            break;

                    } while (!Michael.IsInVehicle(EscapeVehicle, false) || !Trevor.IsInVehicle(EscapeVehicle, false) || !Brad.IsInVehicle(EscapeVehicle, false));

                    Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting chase (if driver existing)");
                    if (EscapeVehicle.HasDriver)
                        EscapeVehicle.Driver.Tasks.CruiseWithVehicle(80, VehicleDrivingFlags.Emergency);

                    GameFiber.Yield();
                    GameFiber.Wait(1000);
                    Functions.SetPursuitDisableAIForPed(Michael, false);
                    Functions.SetPursuitDisableAIForPed(Trevor, false);
                    Functions.SetPursuitDisableAIForPed(Brad, false);
                    Functions.SetPursuitDisableAIForPed(LocalYokel, false);
                });

                EnteredVehicle = true;
            }

            if (ChaseCreated && EnteredVehicle && !RoadblockEscape && EscapeVehicle.DistanceTo2D(new Vector3(3526.76f, -4875.478f, 111.3817f)) < 50)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Turning before roadblock");

                Ped driver = EscapeVehicle.Driver;
                Functions.SetPursuitDisableAIForPed(driver, true);
                driver.Tasks.DriveToPosition(new Vector3(3545.96f, -4688.608f, 113.6559f), 80, VehicleDrivingFlags.Emergency).WaitForCompletion(10000);
                Functions.SetPursuitDisableAIForPed(driver, false);

                RoadblockEscape = true;
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
            if (Trevor.Exists())
                Trevor.Dismiss();
            if (Brad.Exists())
                Brad.Dismiss();
            if (LocalYokel.Exists())
                LocalYokel.Dismiss();
            if (EscapeVehicle.Exists())
                EscapeVehicle.Dismiss();
            if (RoadblockVehicle.Exists())
                RoadblockVehicle.Dismiss();

            Game.LogTrivial($"[{Main.pluginName}] 'Prologue' callout has ended.");
        }
    }
}
