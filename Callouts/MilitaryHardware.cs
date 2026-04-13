using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Military Hardware", CalloutProbability.Medium, "Military convoy under attack", "Code 3")]

    internal class MilitaryHardware : Callout
    {
        private Vector3 SpawnPoint;
        private int SpawnHeading;
        private Blip EventBlip, MilitaryTruckBlip;
        private LHandle Pursuit;
        private Ped Trevor, TruckDriver;
        private List<Ped> Soldiers = new List<Ped>();
        private Vehicle TrevorCar, MilitaryTruck, MilitaryEscortFront, MilitaryEscortBack;
        private bool NearSpawnMessageSent, ChaseCreated, TrevorEnterTruck, TrevorDriveAway, TrevorDriveEnd;

        public override bool OnBeforeCalloutDisplayed()
        {
            Vector3[] spawnpoints = {
                new Vector3(1075.815f, 2018.335f, 54.3681f),
                new Vector3(782.0167f, 2262.465f, 48.38659f),
                new Vector3(380.7696f, 2498.649f, 44.85152f),
                new Vector3(157.1253f, 3103.841f, 41.79741f),
                new Vector3(-103.3369f, 2980.566f, 36.35992f),
                new Vector3(-361.7182f, 2916.317f, 32.6874f),
                new Vector3(-868.5275f, 2753.925f, 22.8628f)
            };
            int[] spawnHeadings =
            {
                27,
                75,
                47,
                103,
                104,
                114,
                94
            };

            int spawnVariation = MathHelper.GetRandomInteger(7);

            SpawnPoint = spawnpoints[spawnVariation];
            SpawnHeading = spawnHeadings[spawnVariation];
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            AddMinimumDistanceCheck(200f, SpawnPoint);
            CalloutMessage = "Military convoy under attack";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Vector3 TrevorSpawn = World.GetNextPositionOnStreet(SpawnPoint.Around2D(50));

            Trevor = Characters.Trevor.Create(TrevorSpawn, SpawnHeading, this.GetType().Name);
            TrevorCar = Vehicles.TrevorCar.Create(TrevorSpawn, SpawnHeading, Trevor);

            MilitaryTruck = new Vehicle("BARRACKS", SpawnPoint, SpawnHeading)
            {
                IsEngineOn = true
            };
            TruckDriver = new Ped("s_m_y_marine_03", SpawnPoint.Around2D(10), 0)
            {
                BlockPermanentEvents = true
            };
            TruckDriver.WarpIntoVehicle(MilitaryTruck, -1);

            MilitaryEscortFront = new Vehicle("CRUSADER", MilitaryTruck.GetOffsetPositionFront(10), SpawnHeading)
            {
                IsEngineOn = true
            };
            for (int soldierNb = -1; soldierNb < 2; soldierNb++)
            {
                Ped soldier = new Ped((MathHelper.GetRandomInteger(2) == 0 ? "S_M_Y_MARINE_03" : "S_M_M_MARINE_01"), MilitaryEscortFront.Position.Around2D(5), 0);
                Soldiers.Add(soldier);
                soldier.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_CARBINERIFLE"), 200, true);
                soldier.WarpIntoVehicle(MilitaryEscortFront, soldierNb);
            }

            MilitaryEscortBack = new Vehicle("CRUSADER", MilitaryTruck.GetOffsetPositionFront(-10), SpawnHeading)
            {
                IsEngineOn = true
            };
            for (int soldierNb = -1; soldierNb < 2; soldierNb++)
            {
                Ped soldier = new Ped((MathHelper.GetRandomInteger(2) == 0 ? "S_M_Y_MARINE_03" : "S_M_M_MARINE_01"), MilitaryEscortBack.Position.Around2D(5), 0);
                Soldiers.Add(soldier);
                soldier.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_CARBINERIFLE"), 200, true);
                soldier.WarpIntoVehicle(MilitaryEscortBack, soldierNb);
            }

            EventBlip = new Blip(MilitaryTruck)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Military convoy under attack"
            };

            NearSpawnMessageSent = false;
            ChaseCreated = false;
            TrevorEnterTruck = false;
            TrevorDriveAway = false;
            TrevorDriveEnd = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 250)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message & Starting convoy");

                CalloutInterfaceAPI.Functions.SendMessage(this, "Military convoy heading to Fort Zancudo under attack");

                MilitaryEscortFront.Driver.Tasks.DriveToPosition(new Vector3(-1603.984f, 2817.603f, 17.17263f), 40, VehicleDrivingFlags.Normal, 20);
                NativeFunction.Natives.TASK_VEHICLE_CHASE(TruckDriver, MilitaryEscortFront.Driver);
                NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(TruckDriver, 32, true);
                NativeFunction.Natives.TASK_VEHICLE_CHASE(MilitaryEscortBack.Driver, TruckDriver);
                NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(MilitaryEscortBack.Driver, 32, true);

                NativeFunction.Natives.TASK_VEHICLE_CHASE(Trevor, MilitaryEscortBack.Driver);
                NativeFunction.Natives.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG(Trevor, 32, true);

                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(MilitaryTruck) < 200)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting chase");

                EventBlip.Delete();

                MilitaryTruckBlip = new Blip(MilitaryTruck)
                {
                    Color = Color.LightBlue,
                    Name = "Military truck",
                };

                Trevor.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_MICROSMG"), 100, true);

                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Trevor);
                Functions.SetPursuitDisableAIForPed(Trevor, true);
                foreach (Ped soldier in Soldiers)
                {
                    Functions.AddCopToPursuit(Pursuit, soldier);
                }
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                TruckDriver.Tasks.DriveToPosition(MilitaryTruck, new Vector3(-1603.984f, 2817.603f, 17.17263f), 60, VehicleDrivingFlags.Emergency, 20);

                Trevor.Tasks.ChaseWithGroundVehicle(TruckDriver);

                ChaseCreated = true;
            }

            if (ChaseCreated && !TrevorEnterTruck && (!TruckDriver.Exists() || !TruckDriver.IsAlive || TruckDriver.DistanceTo(MilitaryTruck) > 20)
                && Trevor.Exists() && Trevor.IsAlive && !Functions.IsPedGettingArrested(Trevor))
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Tasking Trevor to enter vehicle");

                GameFiber.StartNew(delegate
                {
                    if (Trevor.DistanceTo(MilitaryTruck) > 20)
                        Trevor.Tasks.DriveToPosition(MilitaryTruck.Position, 60, VehicleDrivingFlags.Emergency, 20).WaitForCompletion(60000);
                    GameFiber.Yield();
                    Trevor.Tasks.LeaveVehicle(TrevorCar, LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(10000);
                    GameFiber.Yield();
                    Trevor.Tasks.EnterVehicle(MilitaryTruck, 30000, -1, 3f, EnterVehicleFlags.AllowJacking);
                });

                TrevorEnterTruck = true;
            }

            if (TrevorEnterTruck && !TrevorDriveAway && Trevor.IsInVehicle(MilitaryTruck, false))
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Tasking Trevor to drive to Paleto");

                Trevor.Tasks.DriveToPosition(new Vector3(1374.305f, 3586.606f, 34.59678f), 80, VehicleDrivingFlags.Emergency);

                TrevorDriveAway = true;
            }

            if (TrevorDriveAway && !TrevorDriveEnd && Trevor.DistanceTo(new Vector3(1374.305f, 3586.606f, 34.59678f)) < 50)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Enabling Trevor pursuit IA");

                Functions.SetPursuitDisableAIForPed(Trevor, false);

                TrevorDriveEnd = true;
            }

            if (MilitaryTruckBlip.Exists())
                MilitaryTruckBlip.Alpha = 1 - Math.Min(Math.Max(Trevor.DistanceTo(MilitaryTruck) - 50, 0), 50) / 50;

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
            if (Trevor.Exists())
                Trevor.Dismiss();
            if (TrevorCar.Exists())
                TrevorCar.Dismiss();
            foreach (Ped soldier in Soldiers)
            {
                if (soldier.Exists())
                    soldier.Dismiss();
            }
            if (TruckDriver.Exists())
                TruckDriver.Dismiss();
            if (MilitaryTruckBlip.Exists())
                MilitaryTruckBlip.Delete();
            if (MilitaryTruck.Exists())
                MilitaryTruck.Dismiss();
            if (MilitaryEscortFront.Exists())
                MilitaryEscortFront.Dismiss();
            if (MilitaryEscortBack.Exists())
                MilitaryEscortBack.Dismiss();

            Game.LogTrivial($"[{Main.pluginName}] 'Military Hardware' callout has ended.");
        }
    }
}
