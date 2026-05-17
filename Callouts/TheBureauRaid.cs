using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System.Drawing;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("The Bureau Raid", CalloutProbability.Medium, "FIB building attacked", "Code 3")]

    internal class TheBureauRaid : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Ped Michael, Franklin, Gunman, Driver;
        private Vehicle EscapeVehicle, EmergencyVeh1, EmergencyVeh2;
        private TasksList MichaelEscape, FranklinEscape, GunmanEscape, DriverTask;
        private GameFiber EnterVehicleFiber;
        private bool Exploded, NearSpawnMessageSent, ChaseCreated, FarDriverTasked, AddedDriverToPursuit, EnteringVehicle;
        private int DriverVariant, GunmanVariant, EmergencyVehSite1, EmergencyVehSite2;
        private uint DriverTaskTimeout;
        private PoolHandle ParticleHandle;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(93.74509f, -742.8656f, 45.75531f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            AddMinimumDistanceCheck(200f, SpawnPoint);
            CalloutMessage = "FIB building attacked";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_GUNFIRE IN_OR_ON_POSITION", SpawnPoint);

            DriverVariant = MathHelper.GetRandomInteger(2);
            GunmanVariant = MathHelper.GetRandomInteger(4);
            EmergencyVehSite1 = MathHelper.GetRandomInteger(3);
            EmergencyVehSite2 = MathHelper.GetRandomInteger(3);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Michael = Characters.Michael.Create(new Vector3(155.3556f, -723.0673f, 47.07694f), 70, this.GetType().Name);
            Michael.CanAttackFriendlies = false;
            Michael.RelationshipGroup = RelationshipGroup.Gang1;
            Michael.Inventory.GiveNewWeapon("WEAPON_CARBINERIFLE", 300, true);
            MichaelEscape = new TasksList(Michael);

            Franklin = Characters.Franklin.Create(Michael.GetOffsetPositionFront(-2), 70, this.GetType().Name);
            Franklin.CanAttackFriendlies = false;
            Franklin.RelationshipGroup = RelationshipGroup.Gang1;
            Franklin.Inventory.GiveNewWeapon("WEAPON_CARBINERIFLE", 300, true);
            FranklinEscape = new TasksList(Franklin);

            switch (GunmanVariant)
            {
                case 0:
                    Gunman = Characters.Gustavo.Create(Michael.GetOffsetPositionFront(-4), 70, this.GetType().Name);
                    break;
                case 1:
                    Gunman = Characters.Hugh.Create(Michael.GetOffsetPositionFront(-4), 70, this.GetType().Name);
                    break;
                case 2:
                    Gunman = Characters.Norm.Create(Michael.GetOffsetPositionFront(-4), 70, this.GetType().Name);
                    break;
                case 3:
                default:
                    Gunman = Characters.Daryl.Create(Michael.GetOffsetPositionFront(-4), 70, this.GetType().Name);
                    break;
            }
            Gunman.CanAttackFriendlies = false;
            Gunman.RelationshipGroup = RelationshipGroup.Gang1;
            Gunman.Inventory.GiveNewWeapon("WEAPON_CARBINERIFLE", 300, true);
            GunmanEscape = new TasksList(Gunman);

            switch (DriverVariant)
            {
                case 0:
                    EscapeVehicle = new Vehicle("AMBULANCE", new Vector3(63.90407f, -732.5234f, 43.91517f), 342f)
                    {
                        IsSirenOn = true,
                        IsSirenSilent = true,
                        IsEngineOn = true,
                    };

                    Driver = Characters.Eddie.Create(EscapeVehicle.GetOffsetPositionRight(2), 0, this.GetType().Name);
                    break;
                case 1:
                default:
                    EscapeVehicle = new Vehicle("BURRITO3", new Vector3(-312.0298f, -631.8735f, 32.40771f), 160f)
                    {
                        PrimaryColor = Color.FromArgb(255, 18, 17, 16),
                        SecondaryColor = Color.FromArgb(255, 18, 17, 16),
                        PearlescentColor = Color.FromArgb(0, 18, 17, 16),
                    };

                    Driver = Characters.Karim.Create(EscapeVehicle.GetOffsetPositionRight(2), 0, this.GetType().Name);
                    break;
            }

            Driver.WarpIntoVehicle(EscapeVehicle, -1);

            int seatIndex = 0;
            foreach (TasksList EscapeTask in new[] { MichaelEscape, FranklinEscape, GunmanEscape })
            {
                EscapeTask.AddWalkAimingRandomEnemyTask(new Vector3(121.1023f, -720.787f, 47.0766f));
                EscapeTask.AddWalkAimingRandomEnemyTask(new Vector3(101.496f, -741.0348f, 45.75511f));
                EscapeTask.AddWalkAimingRandomEnemyTask(new Vector3(65.58693f, -738.7165f, 44.22049f));
                if (DriverVariant == 0)
                    EscapeTask.AddEnterVehicleTask(EscapeVehicle, seatIndex, 3);
                seatIndex++;
            }

            if (DriverVariant == 1)
            {
                DriverTask = new TasksList(Driver, EndBehavior.Nothing);
                DriverTask.AddDriveTask(new Vector3(43.9543f, -765.8791f, 43.75058f), 80, 10);
                DriverTask.AddDriveTask(new Vector3(60.17799f, -736.3732f, 44.10456f), 80, 10);
            }

            if (EmergencyVehSite1 != 0)
            {
                EmergencyVeh1 = new Vehicle(EmergencyVehSite1 == 1 ? "firetruk" : "ambulance", new Vector3(35.38736f, -718.4564f, 44.14671f), 160)
                {
                    IsSirenOn = true,
                    IsSirenSilent = true,
                };
            }
            if (EmergencyVehSite2 != 0)
            {
                EmergencyVeh2 = new Vehicle(EmergencyVehSite2 == 1 ? "firetruk" : "ambulance", new Vector3(-14.7093f, -777.6899f, 44.30624f), 316)
                {
                    IsSirenOn = true,
                    IsSirenSilent = true,
                };
            }

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "FIB building attacked"
            };

            Exploded = false;
            NearSpawnMessageSent = false;
            ChaseCreated = false;
            FarDriverTasked = false;
            AddedDriverToPursuit = false;
            EnteringVehicle = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!Exploded && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 500)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Exlposion and particles");

                GameFiber.StartNew(delegate
                {
                    GameFiber.Wait(1000 * (2 + MathHelper.GetRandomInteger(10)));
                    World.SpawnExplosion(new Vector3(116f, -768f, 210f), 34, 30, true, false, 0.5f);
                    GameFiber.Wait(1000 * (1 + MathHelper.GetRandomInteger(3)));
                    World.SpawnExplosion(new Vector3(116f, -768f, 210f), 34, 30, true, false, 0.5f);

                    ParticleHandle = StartParticle("core", "ent_amb_smoke_general", new Vector3(115f, -764f, 208f));
                });

                Exploded = true;
            }

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 300)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message");

                CalloutInterfaceAPI.Functions.SendMessage(this, "Suspects entered the building via the roof");
                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 100)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting chase");

                EventBlip.Delete();
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Michael);
                Functions.AddPedToPursuit(Pursuit, Franklin);
                Functions.AddPedToPursuit(Pursuit, Gunman);
                Functions.SetPursuitDisableAIForPed(Michael, true);
                Functions.SetPursuitDisableAIForPed(Franklin, true);
                Functions.SetPursuitDisableAIForPed(Gunman, true);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                MichaelEscape.StartTasks();
                FranklinEscape.StartTasks();
                GunmanEscape.StartTasks();

                DriverTaskTimeout = Game.GameTime + 1000 * 10;

                ChaseCreated = true;
            }

            if (DriverVariant == 1 && ChaseCreated && !FarDriverTasked && Game.GameTime > DriverTaskTimeout)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting Driver task (far)");

                DriverTask.StartTasks();

                FarDriverTasked = true;
            }

            if (!AddedDriverToPursuit && DriverVariant == 0 && Game.LocalPlayer.Character.DistanceTo(Driver) < 5)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Adding Driver to pursuit (near)");

                Functions.AddPedToPursuit(Pursuit, Driver);
                Functions.SetPursuitDisableAIForPed(Driver, true);

                AddedDriverToPursuit = true;
            }

            if (!AddedDriverToPursuit && DriverVariant == 1 && Driver.DistanceTo(new Vector3(60.17799f, -736.3732f, 44.10456f)) < 30) {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Adding Driver to pursuit (far)");

                Functions.AddPedToPursuit(Pursuit, Driver);
                Functions.SetPursuitDisableAIForPed(Driver, true);

                AddedDriverToPursuit = true;
            }

            if (!EnteringVehicle && MichaelEscape.TaskFinished && FranklinEscape.TaskFinished && GunmanEscape.TaskFinished)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Making suspect enter vehicle");
                
                EnterVehicleFiber = GameFiber.StartNew(delegate
                {
                    if (DriverVariant == 1)
                    {
                        Functions.SetPursuitDisableAIForPed(Michael, true);
                        Functions.SetPursuitDisableAIForPed(Franklin, true);
                        Functions.SetPursuitDisableAIForPed(Gunman, true);

                        do
                        {
                            GameFiber.Sleep(1000);

                            Ped[] nearPeds = Main.GetNearbyEnnemies(new Vector3(65.58693f, -738.7165f, 44.22049f));

                            if (nearPeds.Length > 0)
                            {
                                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Making callout suspects shoot while waiting for driver");
                                if (Michael.Exists() && Michael.IsAlive)
                                {
                                    if (Michael.DistanceTo(new Vector3(65.58693f, -738.7165f, 44.22049f)) < 10)
                                        Michael.Tasks.FightAgainst(nearPeds[MathHelper.GetRandomInteger(nearPeds.Length)]);
                                    else
                                        Michael.Tasks.GoToWhileAiming(new Vector3(65.58693f, -738.7165f, 44.22049f), nearPeds[MathHelper.GetRandomInteger(nearPeds.Length)], 2, 2, true, FiringPattern.BurstFire);
                                }
                                if (Franklin.Exists() && Franklin.IsAlive)
                                {
                                    if (Franklin.DistanceTo(new Vector3(65.58693f, -738.7165f, 44.22049f)) < 10)
                                        Franklin.Tasks.FightAgainst(nearPeds[MathHelper.GetRandomInteger(nearPeds.Length)]);
                                    else
                                        Franklin.Tasks.GoToWhileAiming(new Vector3(65.58693f, -738.7165f, 44.22049f), nearPeds[MathHelper.GetRandomInteger(nearPeds.Length)], 2, 2, true, FiringPattern.BurstFire);
                                }
                                if (Gunman.Exists() && Gunman.IsAlive)
                                {
                                    if (Gunman.DistanceTo(new Vector3(65.58693f, -738.7165f, 44.22049f)) < 10)
                                        Gunman.Tasks.FightAgainst(nearPeds[MathHelper.GetRandomInteger(nearPeds.Length)]);
                                    else
                                        Gunman.Tasks.GoToWhileAiming(new Vector3(65.58693f, -738.7165f, 44.22049f), nearPeds[MathHelper.GetRandomInteger(nearPeds.Length)], 2, 2, true, FiringPattern.BurstFire);
                                }
                            }

                            if (!Driver.IsAlive || Functions.IsPedArrested(Driver))
                            {
                                Functions.SetPursuitDisableAIForPed(Michael, false);
                                Functions.SetPursuitDisableAIForPed(Franklin, false);
                                Functions.SetPursuitDisableAIForPed(Gunman, false);
                                return;
                            }
                        } while (EscapeVehicle.DistanceTo(new Vector3(60.17799f, -736.3732f, 44.10456f)) > 15);

                        DriverTask.AbortTasks();
                        Driver.Tasks.PerformDrivingManeuver(VehicleManeuver.GoForwardStraightBraking).WaitForCompletion(5000);

                        if (Michael.Exists() && Michael.IsAlive && !Functions.IsPedArrested(Michael))
                        {
                            if (Michael.DistanceTo(EscapeVehicle) < 30)
                                Michael.Tasks.EnterVehicle(EscapeVehicle, 0, 3f);
                            else
                                Functions.SetPursuitDisableAIForPed(Michael, false);
                        }
                        if (Franklin.Exists() && Franklin.IsAlive && !Functions.IsPedArrested(Franklin))
                        {
                            if (Franklin.DistanceTo(EscapeVehicle) < 30)
                                Franklin.Tasks.EnterVehicle(EscapeVehicle, 1, 3f);
                            else
                                Functions.SetPursuitDisableAIForPed(Franklin, false);
                        }
                        if (Gunman.Exists() && Gunman.IsAlive && !Functions.IsPedArrested(Gunman))
                        {
                            if (Gunman.DistanceTo(EscapeVehicle) < 30)
                                Gunman.Tasks.EnterVehicle(EscapeVehicle, 2, 3f);
                            else
                                Functions.SetPursuitDisableAIForPed(Gunman, false);
                        }
                    }

                    do
                    {
                        GameFiber.Sleep(1000);

                        if (
                            (!Michael.Exists() || !Michael.IsAlive || Michael.DistanceTo2D(EscapeVehicle) > 40 || Functions.IsPedArrested(Michael))
                            || (!Franklin.Exists() || !Franklin.IsAlive || Franklin.DistanceTo2D(EscapeVehicle) > 40 || Functions.IsPedArrested(Franklin))
                            || (!Gunman.Exists() || !Gunman.IsAlive || Gunman.DistanceTo2D(EscapeVehicle) > 40 || Functions.IsPedArrested(Gunman)))
                            break;

                    } while (!Michael.IsInVehicle(EscapeVehicle, true) || !Franklin.IsInVehicle(EscapeVehicle, true) || !Gunman.IsInVehicle(EscapeVehicle, true));

                    if (Driver.IsAlive && Driver.IsInVehicle(EscapeVehicle, false) && !Functions.IsPedArrested(Driver))
                    {
                        Driver.Tasks.CruiseWithVehicle(80, VehicleDrivingFlags.Emergency).WaitForCompletion(2000);
                        Functions.AddPedToPursuit(Pursuit, Driver);
                        Functions.SetPursuitDisableAIForPed(Driver, false);
                        Functions.SetPursuitDisableAIForPed(Michael, false);
                        Functions.SetPursuitDisableAIForPed(Franklin, false);
                        Functions.SetPursuitDisableAIForPed(Gunman, false);
                    }
                });

                EnteringVehicle = true;
            }

            if (Game.IsKeyDown(Keys.End)
                || (ChaseCreated && !Functions.IsPursuitStillRunning(Pursuit)))
                End();
        }

        public override void End()
        {
            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Ending callout");

            base.End();

            if (EnterVehicleFiber != null && EnterVehicleFiber.IsAlive)
                EnterVehicleFiber.Abort();

            if (EventBlip.Exists())
                EventBlip.Delete();
            if (Michael.Exists())
                Michael.Dismiss();
            if (Franklin.Exists())
                Franklin.Dismiss();
            if (Gunman.Exists())
                Gunman.Dismiss();
            if (Driver.Exists())
                Driver.Dismiss();
            if (EscapeVehicle.Exists())
                EscapeVehicle.Dismiss();
            if (EmergencyVeh1.Exists())
                EmergencyVeh1.Dismiss();
            if (EmergencyVeh2.Exists())
                EmergencyVeh2.Dismiss();
            if (!ParticleHandle.IsZero)
                NativeFunction.Natives.STOP_PARTICLE_FX_LOOPED(ParticleHandle.Value, false);

            Game.LogTrivial($"[{Main.pluginName}] 'The Bureau Raid' callout has ended.");
        }

        private PoolHandle StartParticle(string dictName, string particleName, Vector3 position)
        {
            NativeFunction.Natives.REQUEST_NAMED_PTFX_ASSET(dictName);

            uint timeout = Game.GameTime + 1000;
            while (!NativeFunction.Natives.HAS_NAMED_PTFX_ASSET_LOADED<bool>(dictName) && Game.GameTime < timeout)
            {
                GameFiber.Sleep(10);
            }

            NativeFunction.Natives.USE_PARTICLE_FX_ASSET(dictName);

            return NativeFunction.Natives.START_PARTICLE_FX_LOOPED_AT_COORD<uint>(
                particleName,
                position.X, position.Y, position.Z,
                0f, 0f, 0f,
                3f, // Scale
                false, false, false, false
            );
        }
    }
}
