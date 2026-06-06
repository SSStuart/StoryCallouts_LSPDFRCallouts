using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System.Drawing;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Paparazzo - The Meltdown", CalloutProbability.Medium, "Pursuit in progress", "Code 3")]

    internal class Paparazzo_TheMeltdown : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Ped Poppy, Franklin;
        private Vehicle PoppyCar, FranklinCar;
        private Object FranklinPhone;
        private TasksList PoppyTasks;
        private bool NearSpawnMessageSent, ChaseCreated, ChaseEnded, PoppyAIEnabled;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(450.56f, 289.41f, 102.38f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            AddMinimumDistanceCheck(100f, SpawnPoint);
            CalloutMessage = "Pursuit in progress";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_SUSPECT_ON_THE_RUN IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            PoppyCar = new Vehicle("COGCABRIO", SpawnPoint, 67f)
            {
                IsEngineOn = true,
                PrimaryColor = Color.FromArgb(143, 47, 85),
                SecondaryColor = Color.FromArgb(143, 47, 85),
                PearlescentColor = Color.FromArgb(240, 8, 8, 8)
            };
            Poppy = Characters.Poppy.Create(PoppyCar.GetOffsetPositionRight(2), 0, this.GetType().Name);
            Poppy.WarpIntoVehicle(PoppyCar, -1);

            PoppyTasks = new TasksList(Poppy, EndBehavior.Nothing);
            PoppyTasks.AddDriveTask(new Vector3(268.2399f, 342.9503f, 104.8352f), 80, 50);
            PoppyTasks.AddDriveTask(new Vector3(272.6068f, 584.2874f, 149.2664f), 50);
            PoppyTasks.AddDriveTask(new Vector3(295.3085f, 595.3762f, 153.6101f), 10, 10, VehicleDrivingFlags.IgnorePathFinding);
            PoppyTasks.AddDriveTask(new Vector3(339.7518f, 466.2842f, 148.3327f));
            PoppyTasks.AddDriveTask(new Vector3(643.7687f, 362.9601f, 112.2636f));
            PoppyTasks.AddDriveTask(new Vector3(724.1237f, 335.0163f, 112.3087f), 80, 20, VehicleDrivingFlags.IgnorePathFinding | VehicleDrivingFlags.DriveAroundVehicles);
            PoppyTasks.AddDriveTask(new Vector3(1043.209f, 469.2037f, 93.65176f), 80, 40);
            PoppyTasks.AddDriveTask(new Vector3(914.3631f, 48.57398f, 80.11324f), 80, 20, VehicleDrivingFlags.Emergency, 60 * 3);
            PoppyTasks.AddDriveTask(new Vector3(903.5438f, 18.61224f, 78.52175f), 50, 5, VehicleDrivingFlags.IgnorePathFinding);
            PoppyTasks.AddDriveTask(new Vector3(890.4988f, -9.562861f, 78.19432f), 50, 10, VehicleDrivingFlags.IgnorePathFinding);
            PoppyTasks.AddDriveTask(new Vector3(906.598f, -37.93615f, 78.3811f), 80, 10, VehicleDrivingFlags.IgnorePathFinding | VehicleDrivingFlags.DriveAroundVehicles);
            PoppyTasks.AddDriveTask(new Vector3(866.7357f, -106.2874f, 78.84051f));
            PoppyTasks.AddDriveTask(new Vector3(732.2515f, -141.412f, 74.12515f));
            PoppyTasks.AddDriveTask(new Vector3(473.0815f, -307.889f, 46.47322f));
            PoppyTasks.AddDriveTask(new Vector3(384.3998f, -394.6256f, 45.8311f));
            PoppyTasks.AddDriveTask(new Vector3(341.2007f, -410.2457f, 44.62664f), 80, 5, VehicleDrivingFlags.IgnorePathFinding);

            FranklinCar = Vehicles.FranklinCar.Create(new Vector3(468.8521f, 258.325f, 102.6082f), 343);
            FranklinCar.IsCollisionProof = true;
            Franklin = Characters.Franklin.Create(FranklinCar.GetOffsetPositionRight(2), 0, this.GetType().Name);
            Franklin.KeepTasks = true;
            Franklin.WarpIntoVehicle(FranklinCar, -1);

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Pursuit in progress"
            };

            NearSpawnMessageSent = false;
            ChaseCreated = false;
            ChaseEnded = false;
            PoppyAIEnabled = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 300)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message");

                CalloutInterfaceAPI.Functions.SendMessage(this, "Pursuit in progress of a potentially intoxicated person");
                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 200)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting chase");

                EventBlip.Delete();
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Poppy);
                Functions.SetPedResistanceChance(Poppy, 0f);
                Functions.SetPursuitDisableAIForPed(Poppy, true);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                PoppyTasks.StartTasks();

                NativeFunction.Natives.TASK_VEHICLE_FOLLOW(Franklin, FranklinCar, Poppy, 100f, 524861, 50);

                ChaseCreated = true;
            }

            if (PoppyTasks.TaskFinished && !ChaseEnded && Franklin.DistanceTo2D(Poppy) < 200)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Chase end scenario");

                GameFiber.StartNew(delegate
                {
                    if (Franklin.DistanceTo2D(Poppy) > 40)
                        Franklin.Tasks.DriveToPosition(Poppy.Position, 30, VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.AllowWrongWay, 40).WaitForCompletion(60000);

                    GameFiber.Wait(5000);
                    Franklin.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(10000);
                    Franklin.Face(Poppy);
                    GameFiber.Wait(2000);
                    FranklinPhone = new Object("prop_phone_ing_03", Franklin.GetOffsetPositionUp(-3));
                    FranklinPhone.AttachTo(Franklin, Franklin.GetBoneIndex(PedBoneId.LeftHand), new Vector3(0.15f, 0.05f, -0.02f), new Rotator(200, 30, 30));
                    Franklin.Tasks.PlayAnimation("cellphone@", "cellphone_photo_idle", 1f, AnimationFlags.UpperBodyOnly).WaitForCompletion(10000);
                    FranklinPhone.Delete();
                    Franklin.Tasks.EnterVehicle(FranklinCar, -1).WaitForCompletion(10000);
                    Franklin.Tasks.CruiseWithVehicle(40);
                    Franklin.KeepTasks = false;
                });

                ChaseEnded = true;
            }

            if (ChaseEnded && !PoppyAIEnabled && !Poppy.IsInVehicle(PoppyCar, true))
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Enabling Poppy AI");

                Functions.SetPedResistanceChance(Poppy, 0f);
                Functions.SetPursuitDisableAIForPed(Poppy, false);

                PoppyAIEnabled = true;
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
            if (Poppy.Exists())
                Poppy.Dismiss();
            if (PoppyCar.Exists())
                PoppyCar.Dismiss();
            if (Franklin.Exists())
                Franklin.Dismiss();
            if (FranklinCar.Exists())
                FranklinCar.Dismiss();

            Game.LogTrivial($"[{Main.pluginName}] 'Paparazzo - The Meltdown' callout has ended.");
        }
    }
}
