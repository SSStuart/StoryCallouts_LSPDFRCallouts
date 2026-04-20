using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Drawing;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("The Long Stretch", CalloutProbability.Medium, "Gunshots reported at recycling plant", "Code 3")]

    internal class TheLongStretch : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Vehicle LamarVehicle, EscapeVehicle;
        private Ped Franklin, Lamar, Stretch;
        private TasksList EscapeTasks;
        private bool NearSpawnMessageSent, TaskDrive ,ChaseCreated;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(-577.6483f, -1636.18f, 19.48733f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 50f);
            AddMinimumDistanceCheck(200f, SpawnPoint);
            CalloutMessage = "Gunshots reported at recycling plant";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_GUNFIRE IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Franklin = Characters.Franklin.Create(SpawnPoint, 0, this.GetType().Name);
            Franklin.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_PUMPSHOTGUN"), 200, true);

            Lamar = Characters.Lamar.Create(SpawnPoint + new Vector3(-1, 0, 0), 0, this.GetType().Name);
            Lamar.KeepTasks = true;
            Lamar.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_PUMPSHOTGUN"), 200, true);

            Stretch = Characters.Stretch.Create(SpawnPoint + new Vector3(-2, 0, 0), 0, this.GetType().Name);
            Stretch.KeepTasks = true;
            Stretch.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_PISTOL"), 300, true);

            LamarVehicle = new Vehicle("emperor", new Vector3(-616.1136f, -1604.719f, 26.25291f), 197)
            {
                IsPersistent = true,
                PrimaryColor = Color.FromArgb(0, 16, 41),
            };
            EscapeVehicle = new Vehicle("jackal", new Vector3(-593.7469f, -1711.722f, 23.18381f), 55)
            {
                IsPersistent = true,
                PrimaryColor = Color.FromArgb(8, 8, 8),
            };

            EscapeTasks = new TasksList(Franklin, EndBehavior.Nothing);
            EscapeTasks.AddWalkTask(new Vector3(-592.8911f, -1642.595f, 20.66263f), 2, 1, false, true, 146);
            EscapeTasks.AddClimbLadderTask();
            EscapeTasks.AddWalkTask(new Vector3(-605.5242f, -1667.384f, 25.71412f), 2, 1, false, true, 150);
            EscapeTasks.AddWalkTask(new Vector3(-597.491f, -1688.984f, 26.63565f), 2, 1, false, true, 213);
            EscapeTasks.AddWalkTask(new Vector3(-602.9593f, -1697.976f, 25.04387f), 2, 1, false, true, 130);
            EscapeTasks.AddClimbTask();
            EscapeTasks.AddWalkAimingTask(new Vector3(-595.0407f, -1712.682f, 23.30997f), Game.LocalPlayer.Character, 3, 3, FiringPattern.BurstFirePumpShotgun);
            EscapeTasks.AddEnterVehicleTask(EscapeVehicle, -1, 2);

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Gunfire"
            };

            TaskDrive = false;
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

                CalloutInterfaceAPI.Functions.SendMessage(this, "Potential gang activity at the recycling plant");
                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 200)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting pursuit");

                CalloutInterfaceAPI.Functions.SendMessage(this, "Three suspects seen leaving southern side of factory");

                EventBlip.Delete();
                EscapeTasks.StartTasks();
                Lamar.Tasks.GoToOffsetFromEntity(Franklin, 1, 0, 1.5f);
                Stretch.Tasks.GoToOffsetFromEntity(Franklin, 2, 0, 1.5f);
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Franklin);
                Functions.AddPedToPursuit(Pursuit, Lamar);
                Functions.AddPedToPursuit(Pursuit, Stretch);
                Functions.SetPursuitDisableAIForPed(Franklin, true);
                Functions.SetPursuitDisableAIForPed(Lamar, true);
                Functions.SetPursuitDisableAIForPed(Stretch, true);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                Functions.RequestBackup(new Vector3(-550.597f, -1639.814f, 33.21288f), LSPD_First_Response.EBackupResponseType.Pursuit, LSPD_First_Response.EBackupUnitType.AirUnit);

                ChaseCreated = true;
            }

            if (ChaseCreated && !TaskDrive && Game.LocalPlayer.Character.DistanceTo(Franklin) > 10 && Franklin.DistanceTo(EscapeVehicle) < 2)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Waiting for peds to be close to the escape vehicle");

                do
                {
                    GameFiber.Yield();
                    GameFiber.Sleep(1000);

                    if (EscapeVehicle.HasDriver && !EscapeVehicle.IsEngineStarting && !EscapeVehicle.IsEngineOn)
                        EscapeVehicle.IsEngineOn = true;

                    if (Lamar.DistanceTo(EscapeVehicle) < 8 && !Lamar.IsInVehicle(EscapeVehicle, true) && !Functions.IsPedGettingArrested(Lamar) && !Functions.IsPedArrested(Lamar))
                        Lamar.Tasks.EnterVehicle(EscapeVehicle, 20000, 0, 2);
                    if (Stretch.DistanceTo(EscapeVehicle) < 8 && !Stretch.IsInVehicle(EscapeVehicle, true) && !Functions.IsPedGettingArrested(Stretch) && !Functions.IsPedArrested(Stretch))
                        Stretch.Tasks.EnterVehicle(EscapeVehicle, 20000, 2, 2);

                    if (Functions.IsPedArrested(Franklin) || Franklin.IsDead || Functions.IsPedArrested(Lamar) || Functions.IsPedArrested(Stretch) || Game.LocalPlayer.Character.DistanceTo(EscapeVehicle) < 10)
                        break;
                }
                while (!Franklin.IsInVehicle(EscapeVehicle, false) || !Lamar.IsInVehicle(EscapeVehicle, false) || !Stretch.IsInVehicle(EscapeVehicle, false));

                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting chase (if driver existing)");
                if (EscapeVehicle.HasDriver)
                {
                    EscapeVehicle.Driver.Tasks.CruiseWithVehicle(80, VehicleDrivingFlags.Emergency);
                }
                Functions.SetPursuitDisableAIForPed(Franklin, false);
                Functions.SetPursuitDisableAIForPed(Lamar, false);
                Functions.SetPursuitDisableAIForPed(Stretch, false);

                TaskDrive = true;
            }
            if (ChaseCreated && !TaskDrive && Game.LocalPlayer.Character.DistanceTo(Franklin) < 10)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Player too close, canceling tasks & restoring peds IA");

                Franklin.Tasks.Clear();
                Lamar.Tasks.Clear();
                Stretch.Tasks.Clear();
                Functions.SetPursuitDisableAIForPed(Franklin, false);
                Functions.SetPursuitDisableAIForPed(Lamar, false);
                Functions.SetPursuitDisableAIForPed(Stretch, false);

                TaskDrive = true;
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
            if (Franklin.Exists())
                Franklin.Dismiss();
            if (Lamar.Exists())
                Lamar.Dismiss();
            if (Stretch.Exists())
                Stretch.Dismiss();
            if (LamarVehicle.Exists())
                LamarVehicle.Dismiss();
            if (EscapeVehicle.Exists())
                EscapeVehicle.Dismiss();

            Game.LogTrivial($"[{Main.pluginName}] 'The Long Stretch' callout has ended.");
        }
    }
}
