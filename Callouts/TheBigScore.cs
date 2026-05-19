using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Object = Rage.Object;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("The Big Score", CalloutProbability.Medium, "Union Depository robbery", "Code 3")]

    internal class TheBigScore : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Ped Michael, Trevor, Franklin, Driver, Truck1Driver, Truck2Driver;
        private Ped GunmanDiversion;
        private Vehicle MichaelGauntlet, TrevorGauntlet, FranklinGauntlet, DriverGauntlet, Truck1, Truck1Trailer, Truck2, Truck2Trailer, Backup1, Backup2;
        private Vehicle DiversionEscapeVehicle;
        private Object FenceLarge, FenceSmall, Barrier;
        private TasksList MichaelDriveTask, TrevorDriveTask, FranklinDriveTask, DriverDriveTask;
        private TasksList MichaelDiversionEscapeTask, GunmanDiversionEscapeTask, FranklinDiversionEscapeTask;
        private List<GameFiber> EnterTruckHelperFibers;
        private Vector3 EndTunnelPos;
        private GameFiber BackupsPlacementFiber;
        private int ApproachVariant, DriverVariant, GoldGunmanVariant, DiversionGunmanVariant;
        private bool NearSpawnMessageSent, ChaseCreated, TrucksStarting, TrucksInPosition, MichaelEnteringTruck, TrevorEnteringTruck, FranklinEnteringTruck, DriverEnteringTruck, MichaelEnteredTruck, TrevorEnteredTruck, FranklinEnteredTruck, DriverEnteredTruck, TrucksTooFar, Truck1VehiclesDetached, Truck2VehiclesDetached;
        private bool FranklinDiversionTaskStarted, WaitingForDiversionInVehicle;

        public override bool OnBeforeCalloutDisplayed()
        {
            ApproachVariant = MathHelper.GetRandomInteger(2);

            SpawnPoint = ApproachVariant == 0 ? new Vector3(154.9018f, -1290.577f, 28.58763f): new Vector3(-27.46582f, -677.475f, 49.09523f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 50f);
            AddMinimumDistanceCheck(200f, SpawnPoint);
            CalloutMessage = ApproachVariant == 0 ? "Shooting behind the Vanilla Unicorn" : "Union Depository robbery";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_SUSPECT_ON_THE_RUN IN_OR_ON_POSITION", SpawnPoint);

            if (ApproachVariant == 0)
                DriverVariant = MathHelper.GetRandomInteger(2);
            else
            {
                DriverVariant = MathHelper.GetRandomInteger(2);
                GoldGunmanVariant = MathHelper.GetRandomInteger(6);
                DiversionGunmanVariant = MathHelper.GetRandomInteger(6);
                if (DiversionGunmanVariant == GoldGunmanVariant)
                    DiversionGunmanVariant = (DiversionGunmanVariant + 1) % 6;
            }

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (ApproachVariant == 0)
                OnCalloutAccepted_Subtle();
            else
                OnCalloutAccepted_Loud();

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = ApproachVariant == 0 ? "Shooting behind the Vanilla Unicorn" : "Union Depository robbery"
            };

            NearSpawnMessageSent = false;
            ChaseCreated = false;
            TrucksStarting = false;
            TrucksInPosition = false;
            MichaelEnteringTruck = false;
            TrevorEnteringTruck = false;
            FranklinEnteringTruck = false;
            DriverEnteringTruck = false;
            MichaelEnteredTruck = false;
            TrevorEnteredTruck = false;
            FranklinEnteredTruck = false;
            DriverEnteredTruck = false;
            TrucksTooFar = false;
            Truck1VehiclesDetached = false;
            Truck2VehiclesDetached = false;

            FranklinDiversionTaskStarted = false;
            WaitingForDiversionInVehicle = false;

            EndTunnelPos = new Vector3(-1567, -743, 11);

            EnterTruckHelperFibers = new List<GameFiber>();

            return base.OnCalloutAccepted();
        }

        private void OnCalloutAccepted_Subtle()
        {
            Michael = Characters.Michael.Create(new Vector3(133.2909f, -1180.953f, 29.08216f), 0, this.GetType().Name + "Subtle");

            MichaelGauntlet = new Vehicle("gauntlet", new Vector3(133.2909f, -1186.953f, 29.08216f), 180f);
            ModGauntlet(MichaelGauntlet);
            Michael.WarpIntoVehicle(MichaelGauntlet, -1);
            MichaelDriveTask = new TasksList(Michael);

            Trevor = Characters.Trevor.Create(new Vector3(130.2066f, -1180.806f, 29.08948f), 0, this.GetType().Name + "Subtle");

            TrevorGauntlet = new Vehicle("gauntlet", new Vector3(130.2066f, -1186.806f, 29.08948f), 180f);
            ModGauntlet(TrevorGauntlet);
            Trevor.WarpIntoVehicle(TrevorGauntlet, -1);
            TrevorDriveTask = new TasksList(Trevor);

            Franklin = Characters.Franklin.Create(new Vector3(136.334f, -1180.996f, 29.08496f), 0, this.GetType().Name + "Subtle");

            FranklinGauntlet = new Vehicle("gauntlet", new Vector3(136.334f, -1186.996f, 29.08496f), 180f);
            ModGauntlet(FranklinGauntlet);
            Franklin.WarpIntoVehicle(FranklinGauntlet, -1);
            FranklinDriveTask = new TasksList(Franklin);

            Driver = DriverVariant == 0 ?
                Characters.Eddie.Create(new Vector3(127.192f, -1180.693f, 29.08446f), 0, this.GetType().Name + "Subtle") :
                Characters.Karim.Create(new Vector3(127.192f, -1180.693f, 29.08446f), 0, this.GetType().Name + "Subtle");

            DriverGauntlet = new Vehicle("gauntlet", new Vector3(127.192f, -1186.693f, 29.08446f), 180f);
            ModGauntlet(DriverGauntlet);
            Driver.WarpIntoVehicle(DriverGauntlet, -1);
            DriverDriveTask = new TasksList(Driver);


            foreach (TasksList DriveTask in new[] { MichaelDriveTask, TrevorDriveTask, FranklinDriveTask, DriverDriveTask })
            {
                DriveTask.AddDriveTask(new Vector3(134.6696f, -1202.786f, 28.8756f), 20, 5, VehicleDrivingFlags.IgnorePathFinding);
                DriveTask.AddDriveTask(new Vector3(132.4912f, -1255.952f, 29.52134f), 20, 5, VehicleDrivingFlags.IgnorePathFinding);
                DriveTask.AddDriveTask(new Vector3(158.7882f, -1293.576f, 29.22045f), 10, 10, VehicleDrivingFlags.IgnorePathFinding);
                DriveTask.AddDriveTask(new Vector3(149.3153f, -1316.881f, 28.58874f), 10, 5, VehicleDrivingFlags.IgnorePathFinding);
                DriveTask.AddDriveTask(new Vector3(181.6429f, -1334.837f, 28.82754f));
                DriveTask.AddDriveTask(new Vector3(240.6617f, -996.6027f, 28.68919f), 80, 40);
                DriveTask.AddDriveTask(new Vector3(231.7843f, -658.3119f, 38.10378f));
                DriveTask.AddDriveTask(new Vector3(146.3f, -443.9323f, 40.65706f));
                DriveTask.AddDriveTask(new Vector3(92.84899f, -389.5665f, 40.8308f));
                DriveTask.AddDriveTask(new Vector3(54.26226f, -388.0674f, 39.48753f));
                DriveTask.AddDriveTask(new Vector3(6.111208f, -291.8043f, 46.90184f));
                DriveTask.AddDriveTask(new Vector3(-218.6871f, -212.9608f, 48.70966f), 80, 40);
                DriveTask.AddDriveTask(new Vector3(-348.4392f, -197.0384f, 37.58513f), 80, 30, VehicleDrivingFlags.IgnorePathFinding);
                DriveTask.AddDriveTask(new Vector3(-633.1158f, -418.4432f, 34.37214f), 80, 40);
                DriveTask.AddDriveTask(new Vector3(-1113.123f, -626.185f, 13.41855f));
                DriveTask.AddDriveTask(new Vector3(-1249.016f, -699.9026f, 10.38302f), 20);
            }

            Truck1 = new Vehicle("packer", new Vector3(-1230.416f, -681.7884f, 11.04869f), 118)
            {
                IsEngineOn = true,
            };
            Truck1Trailer = new Vehicle("trailers3", new Vector3(-1223.39f, -678.23f, 13.2f), 118);
            Truck1.Trailer = Truck1Trailer;

            Truck1Driver = new Ped(Truck1.GetOffsetPositionFront(4))
            {
                KeepTasks = true,
                BlockPermanentEvents = true
            };
            Truck1Driver.WarpIntoVehicle(Truck1, -1);

            Truck2 = new Vehicle("packer", new Vector3(-1275.187f, -700.8675f, 11.05006f), 110)
            {
                IsEngineOn = true,
            };
            Truck2Trailer = new Vehicle("trailers3", new Vector3(-1267.806f, -698.1442f, 13.19924f), 110);
            Truck2.Trailer = Truck2Trailer;

            Truck2Driver = new Ped(Truck2.GetOffsetPositionFront(4))
            {
                KeepTasks = true,
                BlockPermanentEvents = true
            };
            Truck2Driver.WarpIntoVehicle(Truck2, -1);
        }

        private void OnCalloutAccepted_Loud()
        {
            Michael = Characters.Michael.Create(new Vector3(-1.173188f, -664.074f, 49.47756f), 0, this.GetType().Name + "Loud");
            Michael.Inventory.GiveNewWeapon("WEAPON_ADVANCEDRIFLE", 300, true);
            Michael.CanAttackFriendlies = false;
            Michael.RelationshipGroup = RelationshipGroup.Gang1;
            MichaelDiversionEscapeTask = new TasksList(Michael, EndBehavior.Nothing);

            switch (DiversionGunmanVariant)
            {
                case 0:
                    GunmanDiversion = Characters.Gustavo.Create(new Vector3(-2.495926f, -670.0272f, 49.47768f), 0, this.GetType().Name + "Loud");
                    break;
                case 1:
                    GunmanDiversion = Characters.Karl.Create(new Vector3(-2.495926f, -670.0272f, 49.47768f), 0, this.GetType().Name + "Loud");
                    break;
                case 2:
                    GunmanDiversion = Characters.Hugh.Create(new Vector3(-2.495926f, -670.0272f, 49.47768f), 0, this.GetType().Name + "Loud");
                    break;
                case 3:
                    GunmanDiversion = Characters.Norm.Create(new Vector3(-2.495926f, -670.0272f, 49.47768f), 0, this.GetType().Name + "Loud");
                    break;
                case 4:
                    GunmanDiversion = Characters.Daryl.Create(new Vector3(-2.495926f, -670.0272f, 49.47768f), 0, this.GetType().Name + "Loud");
                    break;
                case 5:
                default:
                    GunmanDiversion = Characters.Chef.Create(new Vector3(-2.495926f, -670.0272f, 49.47768f), 0, this.GetType().Name + "Loud");
                    break;
            }
            GunmanDiversion.Inventory.GiveNewWeapon("WEAPON_CARBINERIFLE", 300, true);
            GunmanDiversion.CanAttackFriendlies = false;
            GunmanDiversion.RelationshipGroup = RelationshipGroup.Gang1;
            GunmanDiversionEscapeTask = new TasksList(GunmanDiversion, EndBehavior.Nothing);

            Franklin = Characters.Franklin.Create(new Vector3(-40.89185f, -674.7073f, 40.72684f), 0, this.GetType().Name + "Loud");
            Franklin.Inventory.GiveNewWeapon("WEAPON_ADVANCEDRIFLE", 300, true);
            Franklin.CanAttackFriendlies = false;
            Franklin.RelationshipGroup = RelationshipGroup.Gang1;
            FranklinDiversionEscapeTask = new TasksList(Franklin, EndBehavior.Nothing);

            DiversionEscapeVehicle = new Vehicle("intruder", new Vector3(-166.1553f, -621.9838f, 32.06397f), 68f);
            DiversionEscapeVehicle.Mods.InstallModKit();
            DiversionEscapeVehicle.Mods.EngineModIndex = DiversionEscapeVehicle.Mods.EngineModCount - 1;
            DiversionEscapeVehicle.Mods.BrakesModIndex = DiversionEscapeVehicle.Mods.BrakesModCount - 1;

            foreach (TasksList DiversionEscape in new[] { MichaelDiversionEscapeTask, GunmanDiversionEscapeTask })
            {
                DiversionEscape.AddWalkAimingRandomEnemyTask(new Vector3(-43.86681f, -686.1209f, 49.50374f), 2, 2, FiringPattern.BurstFire, 60 * 3);
            }

            int seatIndex = -1;
            foreach (TasksList DiversionEscape in new[] { MichaelDiversionEscapeTask, GunmanDiversionEscapeTask, FranklinDiversionEscapeTask })
            {
                DiversionEscape.AddWalkAimingRandomEnemyTask(new Vector3(-86.83004f, -689.665f, 42.48434f), 2, 2, FiringPattern.BurstFire, 60 * 3); /* Before footbridge */
                DiversionEscape.AddWalkAimingRandomEnemyTask(new Vector3(-120.9642f, -677.1685f, 40.51041f), 2, 2, FiringPattern.BurstFire, 60 * 3); /* Footbridge end */
                DiversionEscape.AddWalkAimingRandomEnemyTask(new Vector3(-161.3253f, -667.6812f, 40.46915f), 2, 2, FiringPattern.BurstFire, 60 * 3); /* Middle before stairs */
                DiversionEscape.AddWalkAimingRandomEnemyTask(new Vector3(-204.2099f, -655.1133f, 40.4893f), 2, 2, FiringPattern.BurstFire, 60 * 3); /* Bottom stairs before going up */
                DiversionEscape.AddWalkAimingRandomEnemyTask(new Vector3(-185.1989f, -637.7662f, 48.67397f), 2, 2, FiringPattern.BurstFire, 60 * 3); /* Middle of plaza */
                DiversionEscape.AddWalkAimingRandomEnemyTask(new Vector3(-171.0332f, -603.8388f, 48.22417f), 2, 2, FiringPattern.BurstFire, 60 * 3); /* End of plaza */
                DiversionEscape.AddWalkAimingRandomEnemyTask(new Vector3(-164.5792f, -559.0748f, 48.23022f), 2, 2, FiringPattern.BurstFire, 60 * 3); /* Top of the stairs */
                DiversionEscape.AddWalkAimingRandomEnemyTask(new Vector3(-170.2269f, -559.4491f, 40.49517f), 2, 2, FiringPattern.BurstFire, 60 * 3); /* Bottom of the stairs */
                DiversionEscape.AddWalkAimingRandomEnemyTask(new Vector3(-199.6209f, -574.9931f, 40.48926f), 2, 2, FiringPattern.BurstFire, 60 * 3); /* Before the slope going down */
                DiversionEscape.AddWalkAimingRandomEnemyTask(new Vector3(-217.5356f, -616.592f, 33.93494f), 2, 2, FiringPattern.BurstFire, 60 * 3); /* Bottom of the slope */
                DiversionEscape.AddWalkAimingRandomEnemyTask(new Vector3(-175.0155f, -630.5033f, 32.42431f), 2, 2, FiringPattern.BurstFire, 60 * 3); /* Parking garage */
                DiversionEscape.AddEnterVehicleTask(DiversionEscapeVehicle, seatIndex, 2);
                seatIndex++;
            }

            FenceLarge = new Object("prop_const_fence02b", new Vector3(23f, -670.4f, 46.7f), 71f);
            FenceSmall = new Object("prop_const_fence01b_cr", new Vector3(25.02341f, -664.6926f, 47.5f), 71f);
            Barrier = new Object("prop_barrier_work06a", new Vector3(33.52821f, -675.924f, 45f), 48f);
        }

        public override void Process()
        {
            base.Process();

            if (ApproachVariant == 0)
                Process_Sublte();
            else
                Process_Loud();

            if (Game.IsKeyDown(Keys.End)
                || (ChaseCreated && !Functions.IsPursuitStillRunning(Pursuit)))
                End();
        }

        private void Process_Sublte()
        {
            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 300)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message");

                CalloutInterfaceAPI.Functions.SendMessage(this, "A shooting has been reported under the highway behind the Vanilla Unicorn");
                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 200)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting chase");

                Backup1 = new Vehicle("police", new Vector3(146.7486f, -803.7667f, 30.56549f), 320f)
                {
                    IsEngineOn = true,
                    IsSirenOn = true,
                    IsSirenSilent = true,
                    LockStatus = VehicleLockStatus.Locked
                };
                Backup2 = new Vehicle("police", new Vector3(245.6158f, -610.837f, 41.58808f), 220f)
                {
                    IsEngineOn = true,
                    IsSirenOn = true,
                    IsSirenSilent = true,
                    LockStatus = VehicleLockStatus.Locked
                };

                BackupsPlacementFiber = GameFiber.StartNew(AutoBackupsPlacement);

                EventBlip.Delete();
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Michael);
                Functions.AddPedToPursuit(Pursuit, Trevor);
                Functions.AddPedToPursuit(Pursuit, Franklin);
                Functions.AddPedToPursuit(Pursuit, Driver);
                Functions.SetPursuitDisableAIForPed(Michael, true);
                Functions.SetPursuitDisableAIForPed(Trevor, true);
                Functions.SetPursuitDisableAIForPed(Franklin, true);
                Functions.SetPursuitDisableAIForPed(Driver, true);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                MichaelDriveTask.StartTasks();
                GameFiber.Wait(1000);
                TrevorDriveTask.StartTasks();
                GameFiber.Wait(1000);
                FranklinDriveTask.StartTasks();
                GameFiber.Wait(1000);
                DriverDriveTask.StartTasks();

                ChaseCreated = true;
            }

            if (ChaseCreated && !TrucksStarting && 
                (Michael.DistanceTo2D(new Vector3(-947, -537, 18)) < 50
                || Trevor.DistanceTo2D(new Vector3(-947, -537, 18)) < 50
                || Franklin.DistanceTo2D(new Vector3(-947, -537, 18)) < 50
                || Driver.DistanceTo2D(new Vector3(-947, -537, 18)) < 50)
            )
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting trucks");

                Truck2Driver.Tasks.CruiseWithVehicle(5);
                Truck2Trailer.GetDoors()[0].Open(false);
                GameFiber.StartNew(delegate
                {
                    Truck1Driver.Tasks.DriveToPosition(new Vector3(-1295.658f, -725.2289f, 11.11278f), 40, VehicleDrivingFlags.IgnorePathFinding, 5).WaitForCompletion(20000);
                    Truck1Driver.Tasks.CruiseWithVehicle(5);
                    Truck1Trailer.GetDoors()[0].Open(false);

                    TrucksInPosition = true;
                });

                TrucksStarting = true;
            }

            if (TrucksInPosition && !TrucksTooFar)
            {
                if (!MichaelEnteringTruck && Michael.Exists() && Michael.IsAlive && !Functions.IsPedArrested(Michael) && Michael.DistanceTo(Truck2Trailer) < 400)
                {
                    Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Trucks in position, starting Michael enter trucks logic");
                    MichaelDriveTask.AbortTasks();

                    GameFiber michaelEnterTruckFiber = GameFiber.StartNew(delegate {
                        EnterTruckHelperLogic(Michael, Truck2Trailer, Truck2Driver, ref FranklinEnteredTruck);
                        MichaelEnteredTruck = true;
                    });
                    EnterTruckHelperFibers.Add(michaelEnterTruckFiber);
                    MichaelEnteringTruck = true;
                }
                if (!TrevorEnteringTruck && Trevor.Exists() && Trevor.IsAlive && !Functions.IsPedArrested(Trevor) && Trevor.DistanceTo(Truck2Trailer) < 400)
                {
                    Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Trucks in position, starting Trevor enter trucks logic");
                    TrevorDriveTask.AbortTasks();

                    GameFiber TrevorEnterTruckFiber = GameFiber.StartNew(delegate {
                        EnterTruckHelperLogic(Trevor, Truck1Trailer, Truck1Driver, ref DriverEnteredTruck);
                        TrevorEnteredTruck = true;
                    });
                    EnterTruckHelperFibers.Add(TrevorEnterTruckFiber);
                    TrevorEnteringTruck = true;
                }
                if (!FranklinEnteringTruck && Franklin.Exists() && Franklin.IsAlive && !Functions.IsPedArrested(Franklin) && Franklin.DistanceTo(Truck2Trailer) < 400)
                {
                    Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Trucks in position, starting Franklin enter trucks logic");
                    FranklinDriveTask.AbortTasks();

                    GameFiber FranklinEnterTruckFiber = GameFiber.StartNew(delegate {
                        EnterTruckHelperLogic(Franklin, Truck2Trailer, Truck2Driver, ref MichaelEnteredTruck);
                        FranklinEnteredTruck = true;
                    });
                    EnterTruckHelperFibers.Add(FranklinEnterTruckFiber);
                    FranklinEnteringTruck = true;
                }
                if (!DriverEnteringTruck && Driver.Exists() && Driver.IsAlive && !Functions.IsPedArrested(Driver) && Driver.DistanceTo(Truck2Trailer) < 400)
                {
                    Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Trucks in position, starting Driver enter trucks logic");
                    DriverDriveTask.AbortTasks();

                    GameFiber DriverEnterTruckFiber = GameFiber.StartNew(delegate {
                        EnterTruckHelperLogic(Driver, Truck1Trailer, Truck1Driver, ref TrevorEnteredTruck);
                        DriverEnteredTruck = true;
                    });
                    EnterTruckHelperFibers.Add(DriverEnterTruckFiber);
                    DriverEnteringTruck = true;
                }
            }

            if (TrucksInPosition && !TrucksTooFar && Truck1.DistanceTo2D(EndTunnelPos) < 20 || (MichaelEnteredTruck && TrevorEnteredTruck && FranklinEnteredTruck && DriverEnteredTruck))
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Trucks reached end of tunnel");

                foreach (GameFiber fiber in EnterTruckHelperFibers)
                {
                    if (fiber.IsAlive)
                        fiber.Abort();
                }
                Functions.SetPursuitDisableAIForPed(Michael, MichaelEnteredTruck);
                Functions.SetPursuitDisableAIForPed(Trevor, TrevorEnteredTruck);
                Functions.SetPursuitDisableAIForPed(Franklin, FranklinEnteredTruck);
                Functions.SetPursuitDisableAIForPed(Driver, DriverEnteredTruck);

                if (Truck1Driver.IsInVehicle(Truck1, false))
                {
                    Truck1Trailer.GetDoors()[0].Close(false);
                    Truck1Driver.Tasks.CruiseWithVehicle(70);
                }
                if (Truck2Driver.IsInVehicle(Truck2, false))
                {
                    Truck2Trailer.GetDoors()[0].Close(false);
                    Truck2Driver.Tasks.CruiseWithVehicle(70);
                }

                TrucksTooFar = true;
            }

            if (TrucksStarting && (!Truck1.HasDriver || Truck1.IsDead) && !Truck1VehiclesDetached)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Truck 1 stopped");

                Truck1Trailer.GetDoors()[0].Open(false);

                if (TrevorEnteredTruck)
                    DetachGauntlet(Trevor, TrevorGauntlet);
                if (DriverEnteredTruck)
                    DetachGauntlet(Driver, DriverGauntlet);

                Truck1VehiclesDetached = true;
            }

            if (TrucksStarting && (!Truck2.HasDriver || Truck2.IsDead) && !Truck2VehiclesDetached)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Truck 2 stopped");

                Truck2Trailer.GetDoors()[0].Open(false);

                if (MichaelEnteredTruck)
                    DetachGauntlet(Michael, MichaelGauntlet);
                if (FranklinEnteredTruck)
                    DetachGauntlet(Franklin, FranklinGauntlet);

                Truck2VehiclesDetached = true;
            }
        }

        private void Process_Loud()
        {
            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 300)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message");

                CalloutInterfaceAPI.Functions.SendMessage(this, "Two armed suspects fled the Union Depository after an attempted robbery");
                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 200)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting Michael & Gunman escape");

                EventBlip.Delete();
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Michael);
                Functions.AddPedToPursuit(Pursuit, GunmanDiversion);
                Functions.SetPursuitDisableAIForPed(Michael, true);
                Functions.SetPursuitDisableAIForPed(GunmanDiversion, true);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                MichaelDiversionEscapeTask.StartTasks();
                GunmanDiversionEscapeTask.StartTasks();

                ChaseCreated = true;
            }


            if (!FranklinDiversionTaskStarted && Michael.DistanceTo(new Vector3(-58.20437f, -692.52f, 49.49045f)) < 5)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting Franklin escape");

                Functions.AddPedToPursuit(Pursuit, Franklin);
                Functions.SetPursuitDisableAIForPed(Franklin, true);

                FranklinDiversionEscapeTask.StartTasks();

                FranklinDiversionTaskStarted = true;
            }

            if (!WaitingForDiversionInVehicle && DiversionEscapeVehicle.HasDriver)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Waiting for all passengers in escape vehicle");

                GameFiber.StartNew(delegate
                {
                    bool waitingForMichael = Michael.Exists() && Michael.IsAlive && !Functions.IsPedArrested(Michael) && !Functions.IsPedGettingArrested(Michael) && !Michael.IsInVehicle(DiversionEscapeVehicle, true);
                    bool waitingForGunman = GunmanDiversion.Exists() && GunmanDiversion.IsAlive && !Functions.IsPedArrested(GunmanDiversion) && !Functions.IsPedGettingArrested(GunmanDiversion) && !GunmanDiversion.IsInVehicle(DiversionEscapeVehicle, true);
                    bool waitingForFranklin = Franklin.Exists() && Franklin.IsAlive && !Functions.IsPedArrested(Franklin) && !Functions.IsPedGettingArrested(Franklin) && !Franklin.IsInVehicle(DiversionEscapeVehicle, true);
                    do
                    {
                        GameFiber.Wait(500);
                        GameFiber.Yield();

                        waitingForMichael = Michael.Exists() && Michael.IsAlive && !Functions.IsPedArrested(Michael) && !Functions.IsPedGettingArrested(Michael) && !Michael.IsInVehicle(DiversionEscapeVehicle, true);
                        waitingForGunman = GunmanDiversion.Exists() && GunmanDiversion.IsAlive && !Functions.IsPedArrested(GunmanDiversion) && !Functions.IsPedGettingArrested(GunmanDiversion) && !GunmanDiversion.IsInVehicle(DiversionEscapeVehicle, true);
                        waitingForFranklin = Franklin.Exists() && Franklin.IsAlive && !Functions.IsPedArrested(Franklin) && !Functions.IsPedGettingArrested(Franklin) && !Franklin.IsInVehicle(DiversionEscapeVehicle, true);
                    } while (waitingForMichael || waitingForGunman || waitingForFranklin);

                    if (DiversionEscapeVehicle.HasDriver)
                    {
                        DiversionEscapeVehicle.Driver.Tasks.CruiseWithVehicle(100, VehicleDrivingFlags.Emergency);
                        GameFiber.Wait(2000);
                    }
                    else if (DiversionEscapeVehicle.HasPassengers)
                    {
                        DiversionEscapeVehicle.Passengers[0].Tasks.CruiseWithVehicle(DiversionEscapeVehicle, 100, VehicleDrivingFlags.Emergency);
                        GameFiber.Wait(5000);
                    }
                    Functions.SetPursuitDisableAIForPed(Michael, false);
                    Functions.SetPursuitDisableAIForPed(GunmanDiversion, false);
                    Functions.SetPursuitDisableAIForPed(Franklin, false);
                });

                WaitingForDiversionInVehicle = true;
            }
        }

        public override void End()
        {
            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Ending callout");

            base.End();

            foreach (GameFiber fiber in EnterTruckHelperFibers)
            {
                if (fiber.IsAlive)
                    fiber.Abort();
            }

            if (BackupsPlacementFiber != null && BackupsPlacementFiber.IsAlive)
                BackupsPlacementFiber.Abort();

            if (EventBlip.Exists())
                EventBlip.Delete();
            if (Michael.Exists())
                Michael.Dismiss();
            if (MichaelGauntlet.Exists())
                MichaelGauntlet.Dismiss();
            if (Trevor.Exists())
                Trevor.Dismiss();
            if (TrevorGauntlet.Exists())
                TrevorGauntlet.Dismiss();
            if (Franklin.Exists())
                Franklin.Dismiss();
            if (FranklinGauntlet.Exists())
                FranklinGauntlet.Dismiss();
            if (Driver.Exists())
                Driver.Dismiss();
            if (DriverGauntlet.Exists())
                DriverGauntlet.Dismiss();
            if (Truck1Driver.Exists())
                Truck1Driver.Dismiss();
            if (Truck1.Exists())
                Truck1.Dismiss();
            if (Truck1Trailer.Exists())
                Truck1Trailer.Dismiss();
            if (Truck2Driver.Exists())
                Truck2Driver.Dismiss();
            if (Truck2.Exists())
                Truck2.Dismiss();
            if (Truck2Trailer.Exists())
                Truck2Trailer.Dismiss();
            if (Backup1.Exists())
                Backup1.Dismiss();
            if (Backup2.Exists())
                Backup2.Dismiss();

            if (GunmanDiversion.Exists())
                GunmanDiversion.Dismiss();
            if (DiversionEscapeVehicle.Exists())
                DiversionEscapeVehicle.Dismiss();
            if (FenceLarge.Exists())
                FenceLarge.Delete();
            if (FenceSmall.Exists())
                FenceSmall.Delete();
            if (Barrier.Exists())
                Barrier.Delete();

            Game.LogTrivial($"[{Main.pluginName}] 'The Big Score' callout has ended.");
        }

        private void ModGauntlet(Vehicle vehicle)
        {
            vehicle.Mods.InstallModKit();
            vehicle.Mods.ArmorModIndex = vehicle.Mods.ArmorModCount - 1;
            vehicle.Mods.EngineModIndex = vehicle.Mods.EngineModCount - 1;
            vehicle.Mods.BrakesModIndex = vehicle.Mods.BrakesModCount - 1;
            vehicle.Mods.SetWheelMod(VehicleWheelType.Offroad, 8, false);
            vehicle.CanTiresBurst = false;
        }

        private void AutoBackupsPlacement()
        {
            float distancePoint1, distancePoint2, distancePoint3, distancePoint4;

            do
            {
                GameFiber.Yield();
                GameFiber.Wait(2000);

                distancePoint1 = Main.DistanceSquared2D(Game.LocalPlayer.Character.Position, new Vector3(146.7486f, -803.7667f, 30.56549f));
                distancePoint2 = Main.DistanceSquared2D(Game.LocalPlayer.Character.Position, new Vector3(215.8591f, -702.9615f, 34.96501f));
                distancePoint3 = Main.DistanceSquared2D(Game.LocalPlayer.Character.Position, new Vector3(131.5979f, -578.5113f, 43.02826f));
                distancePoint4 = Main.DistanceSquared2D(Game.LocalPlayer.Character.Position, new Vector3(-8.012558f, -274.296f, 46.39484f));

                if (distancePoint1 < distancePoint3 && distancePoint1 < distancePoint4 && Main.DistanceSquared2D(Backup1.Position, new Vector3(146.7486f, -803.7667f, 30.56549f)) > 10)
                {
                    Backup1.Position = new Vector3(146.7486f, -803.7667f, 30.56549f);
                    Backup1.Heading = 320f;
                    Backup1.IsSirenOn = true;
                }
                if (distancePoint2 < distancePoint3 && distancePoint2 < distancePoint4 && Main.DistanceSquared2D(Backup2.Position, new Vector3(245.6158f, -610.837f, 41.58808f)) > 10)
                {
                    Backup2.Position = new Vector3(245.6158f, -610.837f, 41.58808f);
                    Backup2.Heading = 220f;
                    Backup2.IsSirenOn = true;
                }
                if (distancePoint3 < distancePoint2 && distancePoint3 < distancePoint4 && Main.DistanceSquared2D(Backup1.Position, new Vector3(155.2685f, -400.3386f, 40.71062f)) > 10)
                {
                    Backup1.Position = new Vector3(155.2685f, -400.3386f, 40.71062f);
                    Backup1.Heading = 240f;
                    Backup1.IsSirenOn = true;
                    Backup2.Position = new Vector3(166.8861f, -404.2699f, 40.62871f);
                    Backup2.Heading = 85f;
                    Backup2.IsSirenOn = true;
                } else if (distancePoint4 < distancePoint3 && Main.DistanceSquared2D(Backup1.Position, new Vector3(-129.3019f, -214.7852f, 44.07787f)) > 10)
                {
                    Backup1.Position = new Vector3(-129.3019f, -214.7852f, 44.07787f);
                    Backup1.Heading = 150f;
                    Backup1.IsSirenOn = true;
                }
            } while (!TrucksTooFar);
        }

        private void EnterTruckHelperLogic(Ped ped, Vehicle trailer, Ped truckDriver, ref bool firstPlaceOccupied)
        {
            //NativeFunction.Natives.TASK_VEHICLE_CHASE(ped, truckDriver);
            //NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(ped, 32, true);
            //NativeFunction.Natives.TASK_VEHICLE_FOLLOW(ped, ped.CurrentVehicle, trailer, 50f, 4, 20);

            do
            {
                ped.Tasks.DriveToPosition(trailer.GetOffsetPositionFront(-20), trailer.Speed + 20, VehicleDrivingFlags.Emergency);
                GameFiber.Wait(500);
                GameFiber.Yield();
            } while (!TrucksInPosition);


            // Trailer Approach
            do
            {
                GameFiber.Wait(500);
                GameFiber.Yield();
            } while (ped.DistanceTo2D(trailer.GetOffsetPositionFront(-20)) > 30 && !TrucksTooFar);

            Vector3 targetPos;
            do
            {
                if (!ped.IsInAnyVehicle(false))
                    return;
                ped.Tasks.DriveToPosition(trailer.GetOffsetPositionFront(-5), trailer.Speed + 5, VehicleDrivingFlags.IgnorePathFinding);
                GameFiber.Wait(500);
                GameFiber.Yield();
                targetPos = trailer.GetOffsetPosition(new Vector3(0, -15, -2));
            } while (ped.IsInAnyVehicle(false) && ped.CurrentVehicle.DistanceTo(targetPos) > 5);

            do
            {
                if (!ped.IsInAnyVehicle(false))
                    return;
                ped.Tasks.DriveToPosition(trailer.GetOffsetPositionFront(firstPlaceOccupied ? 7 : 15), trailer.Speed + 5, VehicleDrivingFlags.IgnorePathFinding);
                GameFiber.Wait(100);
                GameFiber.Yield();
                if (!ped.IsInAnyVehicle(false))
                    return;

                if (ped.CurrentVehicle.Heading > trailer.Heading + 1)
                    ped.CurrentVehicle.SetRotationYaw(ped.CurrentVehicle.Rotation.Yaw - 1);
                else if (ped.CurrentVehicle.Heading < trailer.Heading - 1)
                    ped.CurrentVehicle.SetRotationYaw(ped.CurrentVehicle.Rotation.Yaw + 1);
                float lateralForce = 10 - Math.Min(Math.Max(ped.CurrentVehicle.DistanceTo2D(trailer.GetOffsetPositionFront(-5)) - 10, 0), 20) / 2;
                if (ped.DistanceTo2D(trailer.GetOffsetPosition(new Vector3(-2, -5, 0))) < ped.CurrentVehicle.DistanceTo2D(trailer.GetOffsetPosition(new Vector3(2, -5, 0))))
                    ped.CurrentVehicle.ApplyForce(new Vector3(lateralForce * 5, 0, 0), new Vector3(), true, true);
                else
                    ped.CurrentVehicle.ApplyForce(new Vector3(-lateralForce * 5, 0, 0), new Vector3(), true, true);

                targetPos = trailer.GetOffsetPosition(new Vector3(0, firstPlaceOccupied ? -2 : 2, -1));
            } while (ped.IsInAnyVehicle(false) && ped.CurrentVehicle.DistanceTo(targetPos) > 0.5);
            GameFiber.Wait(1000);
            if (!ped.IsInAnyVehicle(false))
                return;
            ped.CurrentVehicle.AttachTo(trailer, 0, new Vector3(0.2f, firstPlaceOccupied ? -2.5f : 2.5f, -1), new Rotator());
            ped.AttachTo(trailer, 0, new Vector3(-0.9f, firstPlaceOccupied ? -0.3f : 5.5f, -0.5f), new Rotator());

            if (Game.LocalPlayer.Character.DistanceTo2D(ped) > 100)
                Functions.RemovePedFromPursuit(ped);
        }

        private void DetachGauntlet(Ped ped, Vehicle vehicle)
        {
            vehicle.Detach();
            vehicle.SetRotationRoll(0);
            ped.WarpIntoVehicle(vehicle, -1);
            ped.Tasks.PerformDrivingManeuver(VehicleManeuver.ReverseStraight75).WaitForCompletion(3000);
            Functions.SetPursuitDisableAIForPed(ped, false);
        }
    }
}
