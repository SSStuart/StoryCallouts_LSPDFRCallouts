using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("The Hotel Assassination", CalloutProbability.Medium, "VIP escort", "Code 1")]

    internal class TheHotelAssassination : Callout
    {
        private Vector3 SpawnPoint, StickyBombPos;
        private Blip EventBlip, VIPVehicleBlip, EscortVehicleBlip;
        private LHandle Pursuit;
        private Ped Franklin, VIP, BodyguardDriver, BodyguardEscort;
        private Vehicle VIPVehicle, EscortVehicle, FranklinCar;
        private GameFiber EnteringVehicleFiber, DrivingAwayFiber;
        private TasksList DriveAway;
        private bool NearSpawnMessageSent, EnteringVehicles, EnteredVehicles, ExitSequenceStarted, ChaseCreated;
        private int WeaponVariation;

        public override bool OnBeforeCalloutDisplayed()
        {
            WeaponVariation = MathHelper.GetRandomInteger(2);
            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Varriant: "+WeaponVariation);

            SpawnPoint = new Vector3(-1251.225f, -216.2316f, 40.61005f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 20f);
            AddMinimumDistanceCheck(100f, SpawnPoint);
            CalloutMessage = "VIP escort";
            CalloutPosition = SpawnPoint;
            CalloutAdvisory = "Respond Code 1";
            Functions.PlayScannerAudioUsingPosition("ASSISTANCE_REQUIRED IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Franklin = Characters.Franklin.Create(new Vector3(-1259.232f, -243.2478f, 47.0454f), 320, this.GetType().Name);
            if (WeaponVariation == 0)
               Franklin.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_SNIPERRIFLE"), 10, true);
            else
                Franklin.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_STICKYBOMB"), 5, true);

            FranklinCar = Vehicles.FranklinCar.Create(new Vector3(-1259.304f, -227.9357f, 46.69148f), 126);

            VIPVehicle = new Vehicle("FBI2", new Vector3(-1223.938f, -195.462f, 38.79724f), 157)
            {
                IsEngineOn = true,
            };

            BodyguardDriver = new Ped("S_M_M_HIGHSEC_02", new Vector3(-1215.084f, -193.5517f, 39.32497f), 52);
            DriveAway = new TasksList(BodyguardDriver, EndBehavior.Cruise);
            DriveAway.AddDriveTask(new Vector3(-1241.81f, -222.3827f, 39.74752f), 10, 10);
            DriveAway.AddDriveTask(new Vector3(-1217.333f, -282.4748f, 37.20264f), 30, 10);

            VIP = new Ped("A_M_Y_BEACHVESP_02", new Vector3(-1213.353f, -192.0665f, 39.32497f), 55);
            VIP.ResetVariation();

            EscortVehicle = new Vehicle("WASHINGTON", new Vector3(-1224.26f, -183.94f, 38.70f), 202)
            {
                PrimaryColor = Color.FromArgb(15, 15, 15),
                IsEngineOn = true,
                IndicatorLightsStatus = VehicleIndicatorLightsStatus.Both
            };

            BodyguardEscort = new Ped("S_M_M_HIGHSEC_01", new Vector3(-1222.127f, -185.0323f, 39.17526f), 104);

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "VIP escort"
            };

            StickyBombPos = new Vector3(-1238.723f, -219.3204f, 40.0353f);

            NearSpawnMessageSent = false;
            EnteringVehicles = false;
            EnteredVehicles = false;
            ExitSequenceStarted = false;
            ChaseCreated = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 300)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message");

                CalloutInterfaceAPI.Functions.SendMessage(this, "A VIP needs to be escorted from the Von Crastenburg Hotel. Assist the two bodyguards already on site");
                NearSpawnMessageSent = true;
            }

            if (Game.LocalPlayer.Character.DistanceTo(Franklin) < 15 && (Franklin.Position.Z - Game.LocalPlayer.Character.Position.Z) < 2 && !ChaseCreated)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Player close to Franklin, skipping to chase start");

                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Franklin);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                if (EnteringVehicleFiber != null && EnteringVehicleFiber.IsAlive)
                    EnteringVehicleFiber.Abort();
                if (DrivingAwayFiber != null && DrivingAwayFiber.IsAlive)
                    DrivingAwayFiber.Abort();

                if (WeaponVariation == 1)
                {
                    GameFiber.Wait(2000);
                    NativeFunction.Natives.EXPLODE_PROJECTILES(Franklin, Game.GetHashKey("WEAPON_STICKYBOMB"), false);
                }

                DriveAway.StartTasks();
                NativeFunction.Natives.TASK_VEHICLE_FOLLOW(BodyguardEscort, EscortVehicle, BodyguardDriver, 100f, 1074528293, 10);

                if (VIPVehicleBlip.Exists())
                    VIPVehicleBlip.Delete();
                if (EscortVehicleBlip.Exists())
                    EscortVehicleBlip.Delete();

                ChaseCreated = true;
            }

            if (!EnteringVehicles && !ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 50)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting VIP & bodyguard tasks");

                Game.DisplayHelp("Go to the back of the ~b~convoy~w~ and wait for the VIP to get into the vehicle");

                if (EventBlip.Exists())
                    EventBlip.Delete();

                VIPVehicleBlip = new Blip(VIPVehicle)
                {
                    Color = Main.alliesColor,
                    Name = "VIP vehicle"
                };
                EscortVehicleBlip = new Blip(EscortVehicle)
                {
                    Color = Main.alliesColor,
                    Name = "VIP escort vehicle"
                };

                BodyguardDriver.Tasks.EnterVehicle(VIPVehicle, -1);
                VIP.Tasks.EnterVehicle(VIPVehicle, 1);

                EnteringVehicleFiber = GameFiber.StartNew(delegate
                {
                    GameFiber.Wait(2000);
                    GameFiber.Yield();
                    BodyguardEscort.Tasks.EnterVehicle(EscortVehicle, -1);

                    Franklin.Tasks.GoStraightToPosition(new Vector3(-1255.98f, -235.4348f, 47.04544f), 2, 316, 1, 5000).WaitForCompletion(5000);
                    NativeFunction.Natives.TASK_THROW_PROJECTILE(Franklin, StickyBombPos.X, StickyBombPos.Y, StickyBombPos.Z, 0, false);
                    GameFiber.Yield();
                    GameFiber.Wait(2000);

                    do
                    {
                        GameFiber.Yield();
                        GameFiber.Wait(1000);
                    } while (!VIP.IsInVehicle(VIPVehicle, false) && VIP.IsAlive);
                    EnteredVehicles = true;

                    Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] VIP has entered vehicle");
                });

                EnteringVehicles = true;
            }

            if (EnteredVehicles && !ChaseCreated && !ExitSequenceStarted)
            {
                NativeFunction.Natives.DRAW_MARKER(
                    1,                              // Marker type
                    -1234.2f, -179.2f, 38.2f,       // Pos X, Y ,Z
                    0f, 0f, 0f,                     // Dir X, Y, Z
                    0f, 0f, 0f,                     // Rot X, Y, Z
                    2f, 2f, 1f,                     // Scale X, Y, Z
                    255, 255, 255, 50,              // R, G, B, A
                    false, false,                   // BobUpDown, FaceCamera
                    2,
                    false,                          // Rotate
                    0, 0,                           // TextureDict, Name
                    false                           // DrawOnEnts
                );
                if (Game.LocalPlayer.Character.DistanceTo(new Vector3(-1234.244f, -179.2005f, 39.21351f)) < 3)
                {
                    Game.DisplayHelp("Press ~b~E~w~ to start the convoy");
                }
            }

            if (EnteredVehicles && !ChaseCreated && !ExitSequenceStarted && Game.LocalPlayer.Character.DistanceTo(new Vector3(-1234.244f, -179.2005f, 39.21351f)) < 3 && Game.IsKeyDown(Keys.E))
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting convoy");

                Game.HideHelp();

                DrivingAwayFiber = GameFiber.StartNew(delegate
                {
                    DriveAway.StartTasks();
                    NativeFunction.Natives.TASK_VEHICLE_FOLLOW(BodyguardEscort, EscortVehicle, BodyguardDriver, 100f, 1074528293, 10);

                    if (WeaponVariation == 0)
                    {
                        Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Tasking Franklin to shoot at VIP & starting pursuit");

                        Pursuit = Functions.CreatePursuit();
                        Functions.AddPedToPursuit(Pursuit, Franklin);
                        Functions.SetPursuitDisableAIForPed(Franklin, true);
                        Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                        Franklin.Tasks.FireWeaponAt(VIP, 10000, FiringPattern.BurstFireRifle).WaitForCompletion(10000);
                        GameFiber.Yield();
                    }
                    else
                    {
                        Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Waiting for VIP vehicle to be close to bomb");

                        uint stickyTimeout = Game.GameTime + 10000;
                        do
                        {
                            GameFiber.Yield();
                            GameFiber.Wait(100);
                        } while (VIPVehicle.DistanceTo(StickyBombPos) > 3 && Game.GameTime < stickyTimeout);
                        Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Exploding bomb & starting pursuit");

                        NativeFunction.Natives.EXPLODE_PROJECTILES(Franklin, Game.GetHashKey("WEAPON_STICKYBOMB"), false);

                        Pursuit = Functions.CreatePursuit();
                        Functions.AddPedToPursuit(Pursuit, Franklin);
                        Functions.SetPursuitDisableAIForPed(Franklin, true);
                        Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                    }

                    Franklin.Tasks.EnterVehicle(FranklinCar, -1).WaitForCompletion(5000);
                    if (!Franklin.IsInVehicle(FranklinCar, true))
                        Franklin.WarpIntoVehicle(FranklinCar, -1);
                    GameFiber.Yield();
                    Franklin.Tasks.CruiseWithVehicle(50);

                    Functions.SetPursuitDisableAIForPed(Franklin, false);

                    if (VIPVehicleBlip.Exists())
                        VIPVehicleBlip.Delete();
                    if (EscortVehicleBlip.Exists())
                        EscortVehicleBlip.Delete();
                
                    ChaseCreated = true;
                });

                ExitSequenceStarted = true;
            }

            if (Game.IsKeyDown(Keys.End)
                || (ChaseCreated && !Functions.IsPursuitStillRunning(Pursuit)))
                End();
        }

        public override void End()
        {
            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Ending callout");

            base.End();

            if (EnteringVehicleFiber != null && EnteringVehicleFiber.IsAlive)
                EnteringVehicleFiber.Abort();
            if (DrivingAwayFiber != null && DrivingAwayFiber.IsAlive)
                DrivingAwayFiber.Abort();

            if (EventBlip.Exists())
                EventBlip.Delete();
            if (Franklin.Exists())
                Franklin.Dismiss();
            if (FranklinCar.Exists())
                FranklinCar.Dismiss();
            if (VIP.Exists())
                VIP.Dismiss();
            if (BodyguardDriver.Exists())
                BodyguardDriver.Dismiss();
            if (BodyguardEscort.Exists())
                BodyguardEscort.Dismiss();
            if (VIPVehicleBlip.Exists())
                VIPVehicleBlip.Delete();
            if (VIPVehicle.Exists())
                VIPVehicle.Dismiss();
            if (EscortVehicleBlip.Exists())
                EscortVehicleBlip.Delete();
            if (EscortVehicle.Exists())
                EscortVehicle.Dismiss();

            Game.LogTrivial($"[{Main.pluginName}] 'The Hotel Assassination' callout has ended.");
        }
    }
}
