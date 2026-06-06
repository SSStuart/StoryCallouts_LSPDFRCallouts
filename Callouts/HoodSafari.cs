using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Hood Safari", CalloutProbability.Medium, "Gang shootout", "Code 3")]

    internal class HoodSafari : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Ped Franklin, Trevor, Lamar, Ballas1, Ballas2, MCClip, ClipPed1, ClipPed2;
        private Vehicle FranklinJetski, TrevorJetski, LamarJetski;
        private TasksList FranklinTasks, TrevorTasks, LamarTasks;
        private bool NearSpawnMessageSent, ChaseCreated, MCClipFleeing, FranklinTasksEnded;
        private int ChaseEndVariation;

        public override bool OnBeforeCalloutDisplayed()
        {
            ChaseEndVariation = MathHelper.GetRandomInteger(3);
            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Varriant: " + ChaseEndVariation);

            SpawnPoint = new Vector3(-10.61828f, -1852.908f, 24.76468f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 40f);
            AddMinimumDistanceCheck(200f, SpawnPoint);
            CalloutMessage = "Gang shootout";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_GUNFIRE IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Franklin = Characters.Franklin.Create(SpawnPoint, 70, this.GetType().Name);
            Franklin.IsInvincible = true;
            Franklin.Inventory.GiveNewWeapon("WEAPON_SMG", 300, true);

            FranklinJetski = new Vehicle("SEASHARK", new Vector3(-128.59f, -1861.97f, -0.13f), 224);

            FranklinTasks = new TasksList(Franklin);
            FranklinTasks.AddEnterVehicleTask(FranklinJetski, -1, 3, EnterVehicleFlags.None, 60 * 4);

            Trevor = Characters.Trevor.Create(Franklin.GetOffsetPositionFront(7), 70, this.GetType().Name);
            Trevor.IsInvincible = true;
            Trevor.Inventory.GiveNewWeapon("WEAPON_PUMPSHOTGUN", 100, true);

            TrevorJetski = new Vehicle("SEASHARK", new Vector3(-123.1158f, -1866.819f, -0.03468414f), 224);

            TrevorTasks = new TasksList(Trevor);
            TrevorTasks.AddEnterVehicleTask(TrevorJetski, -1, 3, EnterVehicleFlags.None, 60 * 4);

            Lamar = Characters.Lamar.Create(Franklin.GetOffsetPositionFront(8), 125, this.GetType().Name);
            Lamar.IsInvincible = true;
            Lamar.Inventory.GiveNewWeapon("WEAPON_ASSAULTRIFLE", 300, true);

            LamarJetski = new Vehicle("SEASHARK", new Vector3(-115.3505f, -1872.951f, -0.02148503f), 224);

            LamarTasks = new TasksList(Lamar);
            LamarTasks.AddEnterVehicleTask(LamarJetski, -1, 3, EnterVehicleFlags.None, 60 * 4);

            foreach (TasksList JetskiEscape in new[] { FranklinTasks, TrevorTasks, LamarTasks })
            {
                JetskiEscape.AddDriveTask(new Vector3(-67.95099f, -1919.967f, 0.9416156f), 20, 5);
                JetskiEscape.AddDriveTask(new Vector3(-10.58898f, -1968.144f, 0.03371241f), 40, 5);
                JetskiEscape.AddDriveTask(new Vector3(27.81363f, -1999.19f, -0.03704061f), 20, 5); // start trun
                JetskiEscape.AddDriveTask(new Vector3(49.92439f, -2027.256f, 0.01080674f), 20, 5); // turn
                JetskiEscape.AddDriveTask(new Vector3(60.56663f, -2051.656f, 0.0231747f), 20, 5); // turn
                JetskiEscape.AddDriveTask(new Vector3(66.49683f, -2080.155f, 0.3509791f), 50, 5);// end turn
                JetskiEscape.AddDriveTask(new Vector3(60.66803f, -2260.811f, 0.04424132f));
                JetskiEscape.AddDriveTask(new Vector3(105.3922f, -2316.01f, 1.276396f));
                JetskiEscape.AddDriveTask(new Vector3(340.1497f, -2309.744f, 0.06611459f), 40, 10);
                JetskiEscape.AddDriveTask(new Vector3(484.8021f, -2407.851f, -0.2996312f), 30, 5); // start pillars
                JetskiEscape.AddDriveTask(new Vector3(590.0311f, -2511.601f, 0.3246982f), 30, 8); // end pillars
                JetskiEscape.AddDriveTask(new Vector3(677.2398f, -2562.032f, 0.7167103f), 30, 10);
                JetskiEscape.AddDriveTask(new Vector3(754.8144f, -2587.77f, -0.3863186f), 50);
            }

            switch (ChaseEndVariation)
            {
                case 0:
                    FranklinTasks.AddDriveTask(new Vector3(1276.117f, -2851.114f, 0.5522938f));
                    break;

                case 1:
                    FranklinTasks.AddFollowInVehicleTask(Trevor, 10);
                    FranklinTasks.AddExitVehicleTask();
                    FranklinTasks.AddWalkTask(new Vector3(767.1197f, -2919.813f, 0.07052082f), 2, 1, false, false, 180);
                    FranklinTasks.AddClimbLadderTask();
                    break;

                case 2:
                    FranklinTasks.AddFollowInVehicleTask(Lamar, 10);
                    FranklinTasks.AddExitVehicleTask();
                    GameFiber.Wait(3000);
                    FranklinTasks.AddWalkTask(new Vector3(1195.02f, -2640.952f, 9.139727f));
                    FranklinTasks.AddWalkTask(new Vector3(1173.089f, -2621.521f, 22.8566f));
                    FranklinTasks.AddWalkTask(new Vector3(1177.548f, -2583.267f, 36.14999f));
                    break;

                default:
                    break;
            }

            TrevorTasks.AddDriveTask(new Vector3(796.9094f, -2715.086f, 0.3374312f));
            TrevorTasks.AddDriveTask(new Vector3(757.7037f, -2915.058f, 0.4871701f));
            TrevorTasks.AddExitVehicleTask();
            TrevorTasks.AddWalkTask(new Vector3(767.1197f, -2919.813f, 0.07052082f), 2, 1, false, false, 180);
            TrevorTasks.AddClimbLadderTask();
            TrevorTasks.AddWalkTask(new Vector3(775.9891f, -2959.658f, 5.800721f), 3, 5);

            LamarTasks.AddDriveTask(new Vector3(926.0121f, -2723.802f, 1.470533f));
            LamarTasks.AddDriveTask(new Vector3(1053.413f, -2753.886f, -0.5206543f));
            LamarTasks.AddDriveTask(new Vector3(1161.633f, -2692.565f, 0.8653155f));
            LamarTasks.AddExitVehicleTask();
            LamarTasks.AddWalkTask(new Vector3(1195.02f, -2640.952f, 9.139727f));
            LamarTasks.AddWalkTask(new Vector3(1173.089f, -2621.521f, 22.8566f));
            LamarTasks.AddWalkTask(new Vector3(1177.548f, -2583.267f, 36.14999f));

            Ballas1 = new Ped("G_M_Y_BALLAORIG_01", new Vector3(4.157356f, -1837.544f, 24.76185f), 135)
            {
                RelationshipGroup = RelationshipGroup.AmbientGangBallas
            };
            Ballas1.Inventory.GiveNewWeapon("WEAPON_SMG", 200, true);

            Ballas2 = new Ped("G_M_Y_BALLAORIG_01", new Vector3(-14.11972f, -1814.12f, 25.90099f), 135)
            {
                RelationshipGroup = RelationshipGroup.AmbientGangBallas
            };
            Ballas2.Inventory.GiveNewWeapon("WEAPON_CARBINERIFLE", 200, true);

            MCClip = Characters.MCClip.Create(new Vector3(-122.669f, -1858.916f, 1.45896f), 263, this.GetType().Name);

            ClipPed1 = new Ped("A_F_Y_BEACH_01", FranklinJetski.GetOffsetPositionRight(-1), 250);
            ClipPed1.WarpIntoVehicle(FranklinJetski, 0);
            ClipPed2 = new Ped("A_F_Y_BEACH_01", LamarJetski.GetOffsetPositionRight(-1), 250);
            ClipPed2.WarpIntoVehicle(LamarJetski, 0);

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Gang shootout"
            };

            NearSpawnMessageSent = false;
            ChaseCreated = false;
            MCClipFleeing = false;
            FranklinTasksEnded = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 400)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message & starting fight");
                Ballas1.Tasks.FireWeaponAt(Trevor, 30000, FiringPattern.BurstFireShortBursts);
                Ballas2.Tasks.FireWeaponAt(Franklin, 30000, FiringPattern.BurstFireShortBursts);

                CalloutInterfaceAPI.Functions.SendMessage(this, "Gang shootout on Grove Street");
                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 200)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting chase");

                FranklinTasks.StartTasks();
                TrevorTasks.StartTasks();
                LamarTasks.StartTasks();

                EventBlip.Delete();
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Franklin);
                Functions.SetPursuitDisableAIForPed(Franklin, true);
                Functions.AddPedToPursuit(Pursuit, Trevor);
                Functions.SetPursuitDisableAIForPed(Trevor, true);
                Functions.AddPedToPursuit(Pursuit, Lamar);
                Functions.SetPursuitDisableAIForPed(Lamar, true);
                Functions.AddPedToPursuit(Pursuit, Ballas1);
                Functions.AddPedToPursuit(Pursuit, Ballas2);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                GameFiber.StartNew(delegate
                {
                    GameFiber.Wait(2000);
                    Franklin.IsInvincible = false;
                    Trevor.IsInvincible = false;
                    Lamar.IsInvincible = false;
                });

                ChaseCreated = true;
            }

            if (ChaseCreated && !MCClipFleeing && Franklin.DistanceTo(MCClip) < 20)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] MCClip fleeing");

                GameFiber.StartNew(delegate
                {
                    MCClip.Tasks.ReactAndFlee(Franklin);
                    GameFiber.Sleep(1000);
                    ClipPed1.Tasks.ReactAndFlee(Franklin);
                    GameFiber.Sleep(500);
                    ClipPed2.Tasks.ReactAndFlee(Lamar);
                    GameFiber.Sleep(10000);
                    Functions.RequestBackup(new Vector3(-51.55439f, -2297.757f, 21.82132f), LSPD_First_Response.EBackupResponseType.Pursuit, LSPD_First_Response.EBackupUnitType.AirUnit);
                    CalloutInterfaceAPI.Functions.SendMessage(this, "Three suspects are attempting to escape on jet skis");
                });

                MCClipFleeing = true;
            }

            if (!FranklinTasksEnded && ChaseCreated)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Ending Franklin Tasks at chase end");

                if ((ChaseEndVariation == 1 && Franklin.DistanceTo(new Vector3(757.7037f, -2915.058f, 0.4871701f)) < 30)
                    || (ChaseEndVariation == 2 && Franklin.DistanceTo(new Vector3(1161.633f, -2692.565f, 0.8653155f)) < 30))
                {
                    FranklinTasks.ExecuteEndBehavior();
                    FranklinTasksEnded = true;
                }
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
            if (FranklinJetski.Exists())
                FranklinJetski.Dismiss();
            if (Trevor.Exists())
                Trevor.Dismiss();
            if (TrevorJetski.Exists())
                TrevorJetski.Dismiss();
            if (Lamar.Exists())
                Lamar.Dismiss();
            if (LamarJetski.Exists())
                LamarJetski.Dismiss();
            if (Ballas1.Exists())
                Ballas1.Dismiss();
            if (Ballas2.Exists())
                Ballas2.Dismiss();
            if (MCClip.Exists())
                MCClip.Dismiss();
            if (ClipPed1.Exists())
                ClipPed1.Dismiss();
            if (ClipPed2.Exists())
                ClipPed2.Dismiss();

            Game.LogTrivial($"[{Main.pluginName}] 'Hood Safari' callout has ended.");
        }
    }
}
