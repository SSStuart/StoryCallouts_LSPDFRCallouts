using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System;
using System.Drawing;
using System.Windows.Forms;
using Object = Rage.Object;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Legal Trouble", CalloutProbability.Medium, "Esorted person driving recklessly at the airport", "Code 3")]

    internal class LegalTrouble : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip, MollyBlip;
        private LHandle Pursuit;
        private Ped Michael, Molly, LandingJetPilot, HangarJetPilot, EscapeRouteTarget;
        private Vehicle MichaelCar, MollyCar, LandingJet, HangarJet, EscapeJet;
        private Object FilmReel;
        private TasksList MollyTasks;
        private GameFiber MichaelChaseFiber;
        private bool NearSpawnMessageSent, ChaseCreated, StartLandingJet, MollyNowASuspect, MollyExitedCar, JetEngineOn, MollyKilled;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(-1108.874f, -2904.203f, 13.53968f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 50f);
            AddMinimumDistanceCheck(200f, SpawnPoint);
            CalloutMessage = "Esorted person driving recklessly at the airport";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_SUSPECT_ON_THE_RUN IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Molly = Characters.Molly.Create(SpawnPoint.Around2D(3), 0, this.GetType().Name);
            MollyCar = new Vehicle("COGCABRIO", SpawnPoint, 90)
            {
                PrimaryColor = Color.FromArgb(255, 105, 0, 0),
                SecondaryColor = Color.FromArgb(255, 105, 0, 0),
                PearlescentColor = Color.FromArgb(0, 41, 44, 46),
                IsEngineOn = true
            };
            Molly.WarpIntoVehicle(MollyCar, -1);

            MollyTasks = new TasksList(Molly);
            MollyTasks.AddDriveTask(new Vector3(-1248.411f, -2844.598f, 13.49896f), 80, 30, VehicleDrivingFlags.IgnorePathFinding);
            MollyTasks.AddDriveTask(new Vector3(-1318.653f, -2776.937f, 13.24451f), 80, 20, VehicleDrivingFlags.IgnorePathFinding);
            MollyTasks.AddDriveTask(new Vector3(-1290.709f, -2653.937f, 13.3685f), 80, 20, VehicleDrivingFlags.IgnorePathFinding);
            MollyTasks.AddDriveTask(new Vector3(-1169.623f, -2512.278f, 13.26293f), 80, 10, VehicleDrivingFlags.IgnorePathFinding);
            MollyTasks.AddDriveTask(new Vector3(-1125.168f, -2432.806f, 13.49878f), 80, 30, VehicleDrivingFlags.IgnorePathFinding);
            MollyTasks.AddDriveTask(new Vector3(-1106.153f, -2319.22f, 13.49903f), 80, 40, VehicleDrivingFlags.IgnorePathFinding);
            MollyTasks.AddDriveTask(new Vector3(-1140.886f, -2285.261f, 13.68234f), 80, 40, VehicleDrivingFlags.IgnorePathFinding);
            MollyTasks.AddDriveTask(new Vector3(-1296.04f, -2188.958f, 13.51853f), 80, 20, VehicleDrivingFlags.IgnorePathFinding);
            MollyTasks.AddDriveTask(new Vector3(-1552.43f, -2606.787f, 13.50094f), 80, 20);
            MollyTasks.AddDriveTask(new Vector3(-1544.332f, -2724.432f, 13.49656f), 80, 20, VehicleDrivingFlags.IgnorePathFinding);
            MollyTasks.AddDriveTask(new Vector3(-1485.468f, -2808.306f, 13.5031f), 80, 30, VehicleDrivingFlags.IgnorePathFinding);
            MollyTasks.AddDriveTask(new Vector3(-1487.198f, -2855.648f, 13.50499f), 80, 30, VehicleDrivingFlags.IgnorePathFinding);
            MollyTasks.AddDriveTask(new Vector3(-1021.208f, -3118.569f, 13.49373f), 80, 30, VehicleDrivingFlags.IgnorePathFinding);
            MollyTasks.AddDriveTask(new Vector3(-910.7573f, -3072.828f, 13.4963f), 80, 20, VehicleDrivingFlags.IgnorePathFinding);
            MollyTasks.AddDriveTask(new Vector3(-869.4869f, -2996.891f, 13.28419f), 80, 20, VehicleDrivingFlags.IgnorePathFinding);
            MollyTasks.AddDriveTask(new Vector3(-887.6467f, -2945.188f, 13.50327f), 80, 20, VehicleDrivingFlags.IgnorePathFinding);
            MollyTasks.AddDriveTask(new Vector3(-928.552f, -2916.578f, 13.39358f), 80, 10, VehicleDrivingFlags.IgnorePathFinding);
            MollyTasks.AddWalkTask(new Vector3(-937.1634f, -2931.86f, 13.94513f), 3, 1, false);
            MollyTasks.AddWalkTask(new Vector3(-918.7474f, -2942.682f, 13.94507f), 3, 2, true);
            MollyTasks.AddWalkTask(new Vector3(-933.1793f, -2965.922f, 13.95531f), 3, 4, true, false, 150);
            MollyTasks.AddWalkTask(new Vector3(-934.0757f, -2978.374f, 13.94508f), 3, 4);
            MollyTasks.AddWalkTask(new Vector3(-914.1453f, -3030.82f, 13.94555f));

            FilmReel = new Object("prop_cs_film_reel_01", Molly.GetOffsetPositionRight(-2));
            FilmReel.AttachTo(Molly, Molly.GetBoneIndex(PedBoneId.LeftHand), new Vector3(0.2f, 0, 0.05f), new Rotator(0, 0, 0));

            Michael = Characters.Michael.Create(MollyCar.GetOffsetPositionFront(-25), 100, this.GetType().Name);
            MichaelCar = Vehicles.MichaelCar.Create(MollyCar.GetOffsetPositionFront(-30), 100, Michael);

            // Removing eventual existing plane
            Entity[] planeToRemove = World.GetEntities(new Vector3(-1610.484f, -2683.857f, 13.2891f), 30, GetEntitiesFlags.ConsiderPlanes);
            if (planeToRemove.Length > 0)
                planeToRemove[0].Delete();
            LandingJet = new Vehicle("JET", new Vector3(-927.5688f, -1497.594f, 233.2677f), 150f)
            {
                IsEngineOn = true,
                IsPositionFrozen = true,
                Opacity = 0,
            };
            LandingJetPilot = new Ped("S_M_M_GENTRANSPORT", LandingJet.GetOffsetPositionUp(10), 0f)
            {
                KeepTasks = true,
                BlockPermanentEvents = true,
            };
            LandingJetPilot.WarpIntoVehicle(LandingJet, -1);

            // Removing eventual existing plane
            planeToRemove = World.GetEntities(new Vector3(-952.13f, -2990.27f, 22.74f), 10, GetEntitiesFlags.ConsiderPlanes);
            if (planeToRemove.Length > 0)
                planeToRemove[0].Delete();
            HangarJet = new Vehicle("JET", new Vector3(-952.13f, -2990.27f, 13), 240f);

            HangarJetPilot = new Ped("s_m_m_pilot_01", HangarJet.GetOffsetPositionRight(10), 0);
            HangarJetPilot.WarpIntoVehicle(HangarJet, -1);

            // Removing eventual existing plane
            planeToRemove = World.GetEntities(new Vector3(-968.37f, -2952.32f, 14.57f), 10, GetEntitiesFlags.ConsiderPlanes);
            if (planeToRemove.Length > 0)
                planeToRemove[0].Delete();
            EscapeJet = new Vehicle("SHAMAL", new Vector3(-968.37f, -2952.32f, 14.57f), 115f);

            EscapeRouteTarget = new Ped(new Vector3(-847.7235f, -2140.87f, 101.3963f), 0)
            {
                IsVisible = false,
                IsPositionFrozen = true,
                IsInvincible = true,
                BlockPermanentEvents = true
            };

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Esorted person driving recklessly at the airport"
            };

            NearSpawnMessageSent = false;
            ChaseCreated = false;
            StartLandingJet = false;
            MollyNowASuspect = false;
            MollyExitedCar = false;
            JetEngineOn = false;
            MollyKilled = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 300)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message");

                CalloutInterfaceAPI.Functions.SendMessage(this, "A person being escorted to the airport has started driving recklessly");
                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 200)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting chase");

                CalloutInterfaceAPI.Functions.SendMessage(this, "Another unauthorized individual appears to be following the escort");

                MollyTasks.StartTasks();

                NativeFunction.Natives.TASK_VEHICLE_FOLLOW(Michael, MichaelCar, Molly, 100f, 4457020, 30);

                MollyBlip = new Blip(Molly)
                {
                    Color = Color.FromArgb(234, 142, 80),
                };

                EventBlip.Delete();
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Michael);
                Functions.SetPursuitDisableAIForPed(Michael, true);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                Functions.RequestBackup(MollyCar.GetOffsetPositionFront(20), LSPD_First_Response.EBackupResponseType.Pursuit, LSPD_First_Response.EBackupUnitType.LocalUnit, "lspd", true, true);

                ChaseCreated = true;
            }

            if (ChaseCreated && !StartLandingJet && Molly.DistanceTo2D(new Vector3(-1227.677f, -2585.197f, 13.62339f)) < 30)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting landing jet sequence");
                
                LandingJet.IsPositionFrozen = false;
                LandingJet.Opacity = 1;
                LandingJetPilot.Tasks.LandPlane(new Vector3(-1331.008f, -2201.203f, 13.62391f), new Vector3(-1605.616f, -2676.81f, 13.5929f));
                GameFiber.StartNew(delegate
                {
                    while (LandingJet.DistanceTo2D(new Vector3(-1381.439f, -2289.579f, 50f)) > 50)
                    {
                        float speedBoost = Math.Max(0, LandingJet.DistanceTo2D(new Vector3(-1381.439f, -2289.579f, 13.60108f))) / 20;
                        LandingJet.ApplyForce(new Vector3(0f, speedBoost, 0f), new Vector3(), true, false);
                        LandingJet.SetRotationPitch(Math.Min(-2f, LandingJet.Rotation.Pitch));
                        GameFiber.Yield();
                    }
                });

                StartLandingJet = true;
            }

            if (ChaseCreated && !MollyNowASuspect && Molly.DistanceTo2D(new Vector3(-1296.04f, -2188.958f, 13.51853f)) < 20)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Molly on runway, adding to pursuit");

                Functions.AddPedToPursuit(Pursuit, Molly);
                Functions.SetPursuitDisableAIForPed(Molly, true);

                MollyBlip.Delete();

                MollyNowASuspect = true;
            }

            if (!MollyExitedCar && (Molly.DistanceTo2D(new Vector3(-928.552f, -2916.578f, 13.39358f)) < 20 || Molly.IsDead))
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting Michael chase logic");

                MichaelChaseFiber = GameFiber.StartNew(MichaelChaseLogic);

                MollyExitedCar = true;
            }

            if (!JetEngineOn && Molly.DistanceTo2D(new Vector3(-927.2167f, -2955.557f, 13.94507f)) < 5)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Turning on engine of big jet");

                HangarJet.IsEngineOn = true;

                JetEngineOn = true;
            }

            if (ChaseCreated && !MollyKilled && Molly.DistanceTo2D(new Vector3(-931.4089f, -2989.927f, 13.94507f)) < 3)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Throwing Molly into jet reactor");
                
                Molly.IsRagdoll = true;
                FilmReel.Detach();
                GameFiber.Sleep(100);
                Molly.ApplyForce(new Vector3(-800, 800, 800), new Vector3(), false, false);
                FilmReel.Velocity = new Vector3();
                Molly.Kill();

                GameFiber.StartNew(delegate
                {
                    GameFiber.Sleep(5000);
                    HangarJet.IsEngineOn = false;
                });

                MollyKilled = true;
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
            if (MichaelCar.Exists())
                MichaelCar.Dismiss();
            if (MollyBlip.Exists())
                MollyBlip.Delete();
            if (Molly.Exists())
                Molly.Dismiss();
            if (MollyCar.Exists())
                MollyCar.Dismiss();
            if (FilmReel.Exists())
                FilmReel.Dismiss();
            if (HangarJetPilot.Exists())
                HangarJetPilot.Dismiss();
            if (HangarJet.Exists())
                HangarJet.Dismiss();
            if (EscapeJet.Exists())
                EscapeJet.Dismiss();
            if (LandingJetPilot.Exists())
                LandingJetPilot.Dismiss();
            if (LandingJet.Exists())
                LandingJet.Dismiss();

            Game.LogTrivial($"[{Main.pluginName}] 'Legal Trouble' callout has ended.");
        }

        private void MichaelChaseLogic()
        {
            if (!Michael.Exists() || Functions.IsPedArrested(Michael) || !MichaelCar.Exists() || !Molly.Exists() || !FilmReel.Exists()) return;

            GameFiber.StartNew(delegate
            {
                while (Michael.Exists() && Michael.IsAlive && !Functions.IsPedGettingArrested(Michael) && !Functions.IsPedArrested(Michael))
                {
                    GameFiber.Sleep(100);
                }

                MichaelChaseFiber.Abort();
            });

            Michael.Tasks.DriveToPosition(new Vector3(-970.6552f, -2976.935f, 13.94508f), 80, VehicleDrivingFlags.Emergency | VehicleDrivingFlags.StopAtDestination);

            do
            {
                GameFiber.Yield();
                GameFiber.Wait(500);
            } while (Michael.DistanceTo2D(new Vector3(-970.6552f, -2976.935f, 13.94508f)) > 20 || Michael.Speed > 1);

            Michael.Tasks.GoStraightToPosition(MichaelCar.GetOffsetPositionRight(-3), 2, 240, 0, 3000).WaitForCompletion(3000);
            Michael.Tasks.FireWeaponAt(Molly, 60000, FiringPattern.BurstFire);

            do
            {
                GameFiber.Yield();
                GameFiber.Wait(500);
            } while (Molly.Exists() && Molly.IsAlive && !Functions.IsPedGettingArrested(Molly));

            Michael.Tasks.GoStraightToPosition(FilmReel.Position, 3, 240, 2, 60000);

            do
            {
                GameFiber.Yield();
                GameFiber.Wait(500);

                if (Michael.IsTouching(HangarJet))
                {
                    Michael.Position = Michael.GetOffsetPositionRight(0.1f);
                }
            } while (FilmReel.Exists() && Michael.DistanceTo(FilmReel) > 2);

            GameFiber.Wait(1000);
            FilmReel.AttachTo(Michael, Michael.GetBoneIndex(PedBoneId.LeftHand), new Vector3(0.2f, 0, 0.05f), new Rotator(0, 0, 0));
            GameFiber.Wait(1000);

            if (EscapeJet.Exists())
            {
                Michael.Tasks.GoStraightToPosition(EscapeJet.GetOffsetPosition(new Vector3(-2, 4, 0)), 3, 50, 1, 60000);

                do
                {
                    GameFiber.Yield();
                    GameFiber.Wait(500);
                } while (EscapeJet.Exists() && Michael.DistanceTo(EscapeJet) > 6);

                GameFiber.Wait(1000);
                Michael.Tasks.EnterVehicle(EscapeJet, -1, EnterVehicleFlags.WarpTo);

                do
                {
                    GameFiber.Yield();
                    GameFiber.Wait(500);
                } while (!Michael.IsInVehicle(EscapeJet, false));

                Michael.Tasks.DriveToPosition(new Vector3(-1372.915f, -3213.127f, 125.5794f), 200, VehicleDrivingFlags.IgnorePathFinding);
                GameFiber.Wait(10000);
                Michael.Tasks.ChaseWithPlane(EscapeRouteTarget, new Vector3(0, 0, 30));
                GameFiber.Wait(2000);
                NativeFunction.Natives.CONTROL_LANDING_GEAR(EscapeJet, 1);
            } else
            {
                Functions.SetPursuitDisableAIForPed(Michael, false);
            }
        }
    }
}
