using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Drawing;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Carbine Rifles", CalloutProbability.Medium, "Attacked tactical team truck", "Code 3")]

    internal class CarbineRifles : Callout
    {
        private Vector3 SpawnPoint;
        private int SpawnHeading;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Ped Michael, Swat1, Swat2, Swat3, Swat4;
        private Vehicle MichaelCar, SwatTruck;
        private bool NearSpawnMessageSent, ChaseCreated;

        public override bool OnBeforeCalloutDisplayed()
        {
            Vector3[] spawnpoints = {
                new Vector3(955.1821f, -838.9335f, 33.49604f),
                new Vector3(957.7491f, -1147.979f, 38.1308f),
                new Vector3(566.7161f, -1173.31f, 41.62981f),
                new Vector3(156.7613f, -1161.411f, 36.87383f),
                new Vector3(156.9498f, -1028.639f, 28.59406f),
                new Vector3(-1447.038f, -608.3371f, 30.13616f),
                new Vector3(-1128.596f, -411.38f, 35.78541f),
                new Vector3(-907.8056f, -304.2189f, 39.06298f),
                new Vector3(-806.744f, -233.711f, 36.4781f),
                new Vector3(-625.4119f, -59.50063f, 40.89163f),
                new Vector3(-392.1553f, -308.3743f, 32.99871f),
                new Vector3(-234.9747f, -581.8378f, 33.82686f),
                new Vector3(-14.86258f, -761.3625f, 31.66068f)
            };
            int[] spawnHeadings =
            {
                208,
                123,
                93,
                87,
                248,
                306,
                282,
                292,
                30,
                268,
                234,
                160,
                247
            };

            int spawnVariation = MathHelper.GetRandomInteger(13);

            SpawnPoint = spawnpoints[spawnVariation];
            SpawnHeading = spawnHeadings[spawnVariation];
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            AddMinimumDistanceCheck(200f, SpawnPoint);
            CalloutMessage = "Attacked tactical team truck";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            SwatTruck = new Vehicle("fbi2", SpawnPoint, SpawnHeading)
            {
                IsEngineOn = true,
                IsSirenOn = true,
            };

            Swat1 = new Ped("S_M_Y_SWAT_01", SpawnPoint.Around2D(3), SpawnHeading);
            Swat1.Inventory.GiveNewWeapon("WEAPON_SMG", 200, true);
            Swat1.WarpIntoVehicle(SwatTruck, -1);
            Swat2 = new Ped("S_M_Y_SWAT_01", SpawnPoint.Around2D(3), SpawnHeading);
            Swat2.Inventory.GiveNewWeapon("WEAPON_SMG", 200, true);
            Swat2.WarpIntoVehicle(SwatTruck, 0);
            Swat3 = new Ped("S_M_Y_SWAT_01", SpawnPoint.Around2D(3), SpawnHeading);
            Swat3.Inventory.GiveNewWeapon("WEAPON_SMG", 200, true);
            Swat3.WarpIntoVehicle(SwatTruck, 1);
            Swat4 = new Ped("S_M_Y_SWAT_01", SpawnPoint.Around2D(3), SpawnHeading);
            Swat4.Inventory.GiveNewWeapon("WEAPON_SMG", 200, true);
            Swat4.WarpIntoVehicle(SwatTruck, 2);

            Michael = Characters.Michael.Create(SwatTruck.GetOffsetPositionFront(-10), 0, this.GetType().Name);
            Michael.Inventory.GiveNewWeapon("WEAPON_MICROSMG", 100, true);
            Michael.Health = 300;

            MichaelCar = Vehicles.MichaelCar.Create(SwatTruck.GetOffsetPositionFront(-15), SpawnHeading, Michael);
            MichaelCar.IsEngineOn = true;

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Attacked tactical team truck"
            };

            NearSpawnMessageSent = false;
            ChaseCreated = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 200)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message & Driving task");

                CalloutInterfaceAPI.Functions.SendMessage(this, "Suspect stopped is car in front of the tactical team truck");

                Michael.Tasks.DriveToPosition(SwatTruck.GetOffsetPositionFront(10), 60, VehicleDrivingFlags.Emergency | VehicleDrivingFlags.StopAtDestination);

                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 100)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting attack & pursuit");

                EventBlip.Delete();

                GameFiber.StartNew(delegate
                {
                    Michael.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(2000);
                    Michael.Tasks.TakeCoverFrom(Swat1, 1000).WaitForCompletion(5000);
                    Michael.Tasks.FireWeaponAt(SwatTruck, 5000, FiringPattern.BurstFireInCover);
                    Functions.SetPursuitDisableAIForPed(Michael, false);
                });

                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Michael);
                Functions.SetPursuitDisableAIForPed(Michael, true);
                Functions.AddCopToPursuit(Pursuit, Swat1);
                Functions.AddCopToPursuit(Pursuit, Swat2);
                Functions.AddCopToPursuit(Pursuit, Swat3);
                Functions.AddCopToPursuit(Pursuit, Swat4);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

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
            if (MichaelCar.Exists())
                MichaelCar.Dismiss();

            GameFiber.StartNew(PostEndLogic);

            Game.LogTrivial($"[{Main.pluginName}] 'Carbine Rifles' callout has ended.");
        }

        private void PostEndLogic()
        {
            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Executing post-end logic...");
         
            if (!SwatTruck.Exists() || SwatTruck.IsUpsideDown || SwatTruck.EngineHealth <= 300)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Truck not driveable, ending");

                if (SwatTruck.Exists())
                    SwatTruck.Dismiss();
                if (Swat1.Exists())
                    Swat1.Dismiss();
                if (Swat2.Exists())
                    Swat2.Dismiss();
                if (Swat3.Exists())
                    Swat3.Dismiss();
                if (Swat4.Exists())
                    Swat4.Dismiss();
            } else
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Waiting for a driver in the truck...");
                do
                {
                    GameFiber.Yield();
                    GameFiber.Wait(1000);
                } while (SwatTruck.Exists() && !SwatTruck.HasDriver && Game.LocalPlayer.Character.DistanceTo(SwatTruck) < 300);

                if (SwatTruck.Exists() && SwatTruck.HasDriver && SwatTruck.Driver != Game.LocalPlayer.Character)
                {
                    Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Tasking driver to go to police station");

                    SwatTruck.Driver.Tasks.DriveToPosition(new Vector3(447.5636f, -1020.003f, 28.51361f), 70, VehicleDrivingFlags.Normal | VehicleDrivingFlags.DriveAroundVehicles, 10).WaitForCompletion();
                    if (SwatTruck.Exists() && SwatTruck.HasDriver)
                    {
                        SwatTruck.Driver.Tasks.DriveToPosition(new Vector3(452.5414f, -995.7122f, 25.78276f), 1, VehicleDrivingFlags.StopAtDestination, 1);
                        SwatTruck.IsSirenOn = false;
                        SwatTruck.IsEngineOn = false;
                    }
                }

                if (SwatTruck.Exists())
                    SwatTruck.Dismiss();
                if (Swat1.Exists())
                    Swat1.Dismiss();
                if (Swat2.Exists())
                    Swat2.Dismiss();
                if (Swat3.Exists())
                    Swat3.Dismiss();
                if (Swat4.Exists())
                    Swat4.Dismiss();
            }

            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Post-end logic ended");
        }
    }
}
