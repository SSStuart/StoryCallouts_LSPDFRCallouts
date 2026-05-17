using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Blitz Play", CalloutProbability.Medium, "Armored transport vehicle attacked", "Code 3")]

    internal class BlitzPlay : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Ped Michael, Franklin, Trevor;
        private Vehicle TrashTruck, TowTruck, ArmoredTruck;
        private bool NearSpawnMessageSent, ChaseCreated;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(892.6642f, -2353.564f, 30.41554f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 50f);
            AddMinimumDistanceCheck(200f, SpawnPoint);
            CalloutMessage = "Armored transport vehicle attacked";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_ROBBERY IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Michael = Characters.Michael.Create(new Vector3(881.53f, -2334.04f, 34.91f), 260, this.GetType().Name);
            Michael.Inventory.GiveNewWeapon("WEAPON_COMBATMG", 300, true);
            Michael.Accuracy = 20;

            Franklin = Characters.Franklin.Create(new Vector3(873.8723f, -2353.05f, 30.3312f), 260, this.GetType().Name);
            Franklin.Inventory.GiveNewWeapon("WEAPON_CARBINERIFLE", 300, true);
            Franklin.Accuracy = 20;

            Trevor = Characters.Trevor.Create(new Vector3(804.7164f, -2330.207f, 62.09619f), 260, this.GetType().Name);
            Trevor.Inventory.GiveNewWeapon("WEAPON_RPG", 50, true);
            Trevor.Accuracy = 0;

            TrashTruck = new Vehicle("TRASH", new Vector3(908.42f, -2375.82f, 30.21f), 248f);
            TowTruck = new Vehicle("TOWTRUCK", new Vector3(896.05f, -2364.41f, 30.43f), 84f);
            ArmoredTruck = new Vehicle("STOCKADE", new Vector3(889.13f, -2364.33f, 30.5f), 180f)
            {
                Rotation = new Rotator(0, 80, 180),
                IsPositionFrozen = true,
            };
            ArmoredTruck.Position = new Vector3(889.13f, -2364.33f, 30.5f);
            ArmoredTruck.Doors[2].BreakOff();
            ArmoredTruck.Doors[3].BreakOff();

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Armored transport vehicle attacked"
            };

            NearSpawnMessageSent = false;
            ChaseCreated = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 300)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message");

                CalloutInterfaceAPI.Functions.SendMessage(this, "An armored vehicle was attacked; three armed suspects have been reported");
                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 100)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting fight");

                EventBlip.Delete();
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Michael);
                Functions.SetPursuitDisableAIForPed(Michael, true);
                Functions.AddPedToPursuit(Pursuit, Franklin);
                Functions.SetPursuitDisableAIForPed(Franklin, true);
                Functions.AddPedToPursuit(Pursuit, Trevor);
                Functions.SetPursuitDisableAIForPed(Trevor, true);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                Michael.Tasks.FireWeaponAt(Game.LocalPlayer.Character, 1000 * 60 * 10, FiringPattern.BurstFireInCover);
                Franklin.Tasks.FireWeaponAt(Game.LocalPlayer.Character, 1000 * 60 * 10, FiringPattern.BurstFireInCover);
                Trevor.Tasks.FireWeaponAt(Game.LocalPlayer.Character, 1000 * 60 * 10, FiringPattern.BurstFireSlowFireTank);
                
                GameFiber.StartNew(delegate
                {
                    GameFiber.Wait(10000);
                    Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Spawning backup heli");
                    Vehicle HeliBackup = Functions.RequestBackup(Trevor.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.NooseAirUnit);

                    while (Trevor.Exists() && Trevor.IsAlive && !Functions.IsPedArrested(Trevor))
                    {
                        Entity[] Helicopters = World.GetEntities(GetEntitiesFlags.ConsiderHelicopters);
                        Vehicle Heli = null;
                        if (Helicopters.Length > 0)
                            Heli = (Vehicle)Helicopters[0];

                        if (Helicopters.Length > 0 && Heli.Exists() && Heli.Position.Z > Trevor.Position.Z)
                        {
                            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Trevor shooting at nearby heli");
                            Trevor.Tasks.FireWeaponAt(Heli.Driver, 30000, FiringPattern.BurstFire);
                        }
                        else if (Trevor.DistanceTo2D(Game.LocalPlayer.Character) > 50)
                        {
                            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Trevor shooting at player");
                            Trevor.Tasks.FireWeaponAt(Game.LocalPlayer.Character, 1000 * 60 * 10, FiringPattern.BurstFireSlowFireTank);
                        }
                        else
                        {
                            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Trevor shooting at ground");
                            Trevor.Tasks.FireWeaponAt(new Vector3(890.9384f, -2349.93f, 30.35868f), 1000 * 60 * 10, FiringPattern.BurstFireSlowFireTank);
                        }

                        GameFiber.Yield();
                        GameFiber.Wait(10000);
                    }
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
            if (Franklin.Exists())
                Franklin.Dismiss();
            if (Trevor.Exists())
                Trevor.Dismiss();
            if (TrashTruck.Exists())
                TrashTruck.Dismiss();
            if (TowTruck.Exists())
                TowTruck.Dismiss();
            if (ArmoredTruck.Exists())
                ArmoredTruck.Dismiss();

            Game.LogTrivial($"[{Main.pluginName}] 'Blitz Play' callout has ended.");
        }
    }
}
