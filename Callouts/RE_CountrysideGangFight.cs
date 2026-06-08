using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Collections.Generic;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Countryside Gang Fight", CalloutProbability.Medium, "Suspicious gang activity", "Code 2")]

    internal class RE_CountrysideGangFight : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip, VictimBlip;
        private LHandle Pursuit;
        private Ped Victim, Biker1, Biker2;
        private List<Ped> BackupBikers;
        private Vehicle Bike1, Bike2;
        private List<Vehicle> BackupBikes;
        private bool NearSpawnMessageSent, ChaseCreated, BackupSpawned, BackupAddedInPursuit;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(973.66f, 3617.09f, 32.60f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 20f);
            AddMinimumDistanceCheck(200f, SpawnPoint);
            CalloutMessage = "Suspicious gang activity";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_BRANDISHING_WEAPON IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Victim = new Ped("A_M_M_SKIDROW_01", new Vector3(973.66f, 3617.09f, 32.60f), 101f)
            {
                BlockPermanentEvents = true
            };

            Biker1 = new Ped("G_M_Y_LOST_02", new Vector3(971.31f, 3618.73f, 32.53f), 251f)
            {
                BlockPermanentEvents = true,
                CanAttackFriendlies = false,
                RelationshipGroup = RelationshipGroup.AmbientGangLost,
            };
            Biker1.Inventory.GiveNewWeapon("WEAPON_SAWNOFFSHOTGUN", 20, true);

            Biker2 = new Ped("G_M_Y_LOST_01", new Vector3(970.44f, 3615.29f, 32.67f), 306f)
            {
                BlockPermanentEvents = true,
                CanAttackFriendlies = false,
                RelationshipGroup = RelationshipGroup.AmbientGangLost,
            };
            Biker2.Inventory.GiveNewWeapon("WEAPON_PISTOL", 50, true);

            Bike1 = new Vehicle("hexer", new Vector3(968.24f, 3611.80f, 32.26f), 297f);
            Bike2 = new Vehicle("hexer", new Vector3(970.87f, 3625.59f, 31.82f), 212f);

            BackupBikes = new List<Vehicle>();
            BackupBikers = new List<Ped>();

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Suspicious gang activity"
            };

            NearSpawnMessageSent = false;
            ChaseCreated = false;
            BackupSpawned = false;
            BackupAddedInPursuit = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 200)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message");

                CalloutInterfaceAPI.Functions.SendMessage(this, "Two gang members were reported holding up a person");

                Biker1.Tasks.AimWeaponAt(Victim, -1);
                Biker2.Tasks.AimWeaponAt(Victim, -1);
                Victim.Tasks.PlayAnimation("missminuteman_1ig_2", "handsup_base", 1f, AnimationFlags.Loop);

                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 20)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting pursuit");

                EventBlip.Delete();

                VictimBlip = new Blip(Victim)
                {
                    Color = Main.alliesColor,
                    Scale = 0.75f,
                    Name = "Robbed person"
                };

                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Biker1);
                Functions.AddPedToPursuit(Pursuit, Biker2);
                Functions.SetPursuitDisableAIForPed(Biker1, true);
                Functions.SetPursuitDisableAIForPed(Biker2, true);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                Biker1.Tasks.FightAgainst(Main.GetNearbyEnnemies(Biker1.Position)[0]);
                Biker2.Tasks.FightAgainst(Main.GetNearbyEnnemies(Biker2.Position)[0]);

                GameFiber.StartNew(delegate
                {
                    GameFiber.Wait(2000);
                    Victim.Tasks.PlayAnimation("anim@scripted@npc@bounty_ig_surrender@heeled@", "surrender_idle_bounty", 0.8f, AnimationFlags.Loop);

                    GameFiber.Wait(10000);

                    Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Spawning bikers backup");

                    for (int backup = 0; backup < 4; backup++)
                    {
                        Vehicle Bike = new Vehicle("hexer", new Vector3(724f + backup * 2, 3570f, 32.77562f), 350f);
                        BackupBikes.Add(Bike);

                        int pedVariation = MathHelper.GetRandomInteger(3) + 1;
                        Ped Biker = new Ped("g_m_y_lost_0" + pedVariation, Bike.GetOffsetPositionRight(0.5f), 350f)
                        {
                            CanAttackFriendlies = false,
                            RelationshipGroup = RelationshipGroup.AmbientGangLost,
                        };
                        Biker.Inventory.GiveNewWeapon("WEAPON_PISTOL", 50, true);
                        Biker.WarpIntoVehicle(Bike, -1);
                        BackupBikers.Add(Biker);

                        Biker.Tasks.ChaseWithGroundVehicle(Game.LocalPlayer.Character);
                    }

                    BackupSpawned = true;
                });

                ChaseCreated = true;
            }

            if (BackupSpawned && !BackupAddedInPursuit && BackupBikers[0].DistanceTo2D(Game.LocalPlayer.Character) < 40)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Adding bikers backup to pursuit");

                if (!Functions.IsPursuitStillRunning(Pursuit))
                    Pursuit = Functions.CreatePursuit();

                foreach (Ped BackupBiker in BackupBikers)
                {
                    if (BackupBiker.Exists())
                    {
                        Functions.AddPedToPursuit(Pursuit, BackupBiker);
                        Functions.SetPursuitDisableAIForPed(BackupBiker, true);
                        BackupBiker.Tasks.FightAgainst(Game.LocalPlayer.Character);
                    }
                }

                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                BackupAddedInPursuit = true;
            }

            if (Game.IsKeyDown(Keys.End)
                || (ChaseCreated && !Functions.IsPursuitStillRunning(Pursuit) && BackupAddedInPursuit))
                End();
        }

        public override void End()
        {
            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Ending callout");

            base.End();

            if (EventBlip.Exists())
                EventBlip.Delete();
            if (VictimBlip.Exists())
                VictimBlip.Delete();
            if (Victim.Exists())
                Victim.Dismiss();
            if (Biker1.Exists())
                Biker1.Dismiss();
            if (Biker2.Exists())
                Biker2.Dismiss();
            if (Bike1.Exists())
                Bike1.Dismiss();
            if (Bike2.Exists())
                Bike2.Dismiss();
            foreach (Ped BackupBiker in BackupBikers)
            {
                if (BackupBiker.Exists())
                    BackupBiker.Dismiss();
            }
            foreach (Vehicle BackupBike in BackupBikes)
            {
                if (BackupBike.Exists())
                    BackupBike.Dismiss();
            }

            Game.LogTrivial($"[{Main.pluginName}] 'Countryside Gang Fight' callout has ended.");
        }
    }
}
