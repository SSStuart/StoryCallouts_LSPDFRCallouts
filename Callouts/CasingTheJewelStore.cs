using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Casing the Jewel Store", CalloutProbability.Medium, "Jewel store heist", "Code 3")]

    internal class CasingTheJewelStore : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Vehicle FranklinBike, DriverBike, GunmanBike, Truck, CopCar;
        private Ped Franklin, Michael, Hacker, Driver, Gunman, Cop;
        private TasksList FranklinEscape, DriverEscape, GunmanEscape, TruckEscape;
        private GameFiber DeleteDoorsFiber;
        private bool NearSpawnMessageSent, ChaseCreated, TruckChaseCreated, ScriptedChaseEnd;
        private int DriverVariant, GunmanVariant, HackerVariant;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(1045.857f, -289.0619f, 49.25853f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 50f);
            AddMinimumDistanceCheck(100f, SpawnPoint);
            CalloutMessage = "Jewel store heist";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_ROBBERY SUSPECT_LAST_SEEN DRIVING_A VEHICLE_CATEGORY_MOTORCYCLE IN_OR_ON_POSITION", SpawnPoint);

            DriverVariant = MathHelper.GetRandomInteger(2);
            GunmanVariant = MathHelper.GetRandomInteger(3);
            HackerVariant = MathHelper.GetRandomInteger(3);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Franklin = Characters.Franklin.Create(new Vector3(1012.071f, -178.5481f, 40.89464f), 0, this.GetType().Name);

            FranklinBike = new Vehicle((DriverVariant == 0 ? "sanchez" : "bati2"), new Vector3(1016.769f, -179.7228f, 40.30211f), 204)
            {
                IsPersistent = true,
                IsEngineOn = true
            };
            Franklin.WarpIntoVehicle(FranklinBike, -1);

            Driver = DriverVariant == 0 ?
                Characters.Eddie.Create(new Vector3(993.7144f, -153.3229f, 34.88849f), 0, this.GetType().Name) :
                Characters.Karim.Create(new Vector3(993.7144f, -153.3229f, 34.88849f), 0, this.GetType().Name);
            Driver.KeepTasks = true;
            Driver.CanBeKnockedOffBikes = false;

            DriverBike = new Vehicle((DriverVariant == 0 ? "sanchez" : "bati2"), new Vector3(993.2284f, -153.9241f, 34.32867f), 236)
            {
                IsPersistent = true,
                IsEngineOn = true
            };
            Driver.WarpIntoVehicle(DriverBike, -1);

            Gunman = GunmanVariant == 0 ?
                Characters.Gustavo.Create(new Vector3(980.2578f, -139.1873f, 34.37346f), 0, this.GetType().Name) :
                (GunmanVariant == 1 ?
                Characters.Patrick.Create(new Vector3(980.2578f, -139.1873f, 34.37346f), 0, this.GetType().Name) :
                Characters.Norm.Create(new Vector3(980.2578f, -139.1873f, 34.37346f), 0, this.GetType().Name));
            Gunman.KeepTasks = true;
            Gunman.CanBeKnockedOffBikes = false;

            GunmanBike = new Vehicle((DriverVariant == 0 ? "sanchez" : "bati2"), new Vector3(979.9433f, -139.9055f, 33.69652f), 234)
            {
                IsPersistent = true,
                IsEngineOn = true
            };
            Gunman.WarpIntoVehicle(GunmanBike, -1);

            Michael = Characters.Michael.Create(new Vector3(1095.147f, -243.3691f, 57.5906f), 0, this.GetType().Name);

            Hacker = HackerVariant == 0 ?
                Characters.Paige.Create(new Vector3(1091.209f, -241.3812f, 57.58831f), 0, this.GetType().Name) :
                (GunmanVariant == 1 ?
                Characters.Christian.Create(new Vector3(1091.209f, -241.3812f, 57.58831f), 0, this.GetType().Name) :
                Characters.Rickie.Create(new Vector3(1091.209f, -241.3812f, 57.58831f), 0, this.GetType().Name));

            Truck = new Vehicle("benson", new Vector3(1093.894f, -241.3573f, 57.52023f), 143)
            {
                IsPersistent = true,
                IsEngineOn = true
            };
            Michael.WarpIntoVehicle(Truck, -1);
            Hacker.WarpIntoVehicle(Truck, 0);

            Cop = new Ped("s_m_y_cop_01", new Vector3(1046.576f, -278.7255f, 50.55828f), 60)
            {
                IsPersistent = true,
                BlockPermanentEvents = true,
            };

            CopCar = new Vehicle("police", new Vector3(1045.042f, -275.975f, 50.36977f), 163);

            VehicleDrivingFlags beeLineDrivingFlags = VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.IgnorePathFinding;

            FranklinEscape = new TasksList(Franklin);
            DriverEscape = new TasksList(Driver);
            GunmanEscape = new TasksList(Gunman);

            DriverEscape.AddDriveTask(new Vector3(1019.542f, -177.163f, 40.4483f), 20, 5, beeLineDrivingFlags);
            GunmanEscape.AddDriveTask(new Vector3(1019.542f, -177.163f, 40.4483f), 20, 5, beeLineDrivingFlags);

            foreach (TasksList BikeEscape in new[] { FranklinEscape, DriverEscape, GunmanEscape })
            {
                BikeEscape.AddDriveTask(new Vector3(1035.3f, -281.0321f, 50.14124f), 20, 5, beeLineDrivingFlags);
                BikeEscape.AddDriveTask(new Vector3(988.0059f, -357.8585f, 45.97958f).Around2D(0, 2), 80, 20, beeLineDrivingFlags);
                BikeEscape.AddDriveTask(new Vector3(925.319f, -386.1585f, 41.4594f), 80, 10, beeLineDrivingFlags);
                BikeEscape.AddDriveTask(new Vector3(746.2045f, -422.3382f, 19.94626f).Around2D(0, 2), 80, 35, beeLineDrivingFlags);
                BikeEscape.AddDriveTask(new Vector3(669.025f, -467.9777f, 15.70607f).Around2D(0, 2), 80, 35, beeLineDrivingFlags);
                BikeEscape.AddDriveTask(new Vector3(611.0027f, -534.1649f, 14.75732f).Around2D(0, 2), 80, 10, beeLineDrivingFlags);
                BikeEscape.AddDriveTask(new Vector3(572.9976f, -649.5505f, 13.5995f).Around2D(0, 2), 80, 35, beeLineDrivingFlags);
                BikeEscape.AddDriveTask(new Vector3(612.8348f, -771.8531f, 11.36879f).Around2D(0, 2), 80, 35, beeLineDrivingFlags);
                BikeEscape.AddDriveTask(new Vector3(614.8856f, -907.6492f, 10.69296f).Around2D(0, 2), 80, 35, beeLineDrivingFlags);
                BikeEscape.AddDriveTask(new Vector3(615.7725f, -1172.931f, 9.964421f).Around2D(0, 2), 80, 40, beeLineDrivingFlags);
                BikeEscape.AddDriveTask(new Vector3(612.1369f, -1268.227f, 9.715595f).Around2D(0, 2), 80, 35, beeLineDrivingFlags);
                BikeEscape.AddDriveTask(new Vector3(580.866f, -1317.901f, 9.693883f), 80, 5, beeLineDrivingFlags);
                BikeEscape.AddDriveTask(new Vector3(622.1667f, -1463.797f, 9.689598f).Around2D(0, 2), 80, 35, beeLineDrivingFlags);
                BikeEscape.AddDriveTask(new Vector3(656.3445f, -1606.847f, 9.660782f).Around2D(0, 2), 80, 10, beeLineDrivingFlags);
                BikeEscape.AddDriveTask(new Vector3(644.7076f, -1764.995f, 9.979259f).Around2D(0, 2), 80, 35, beeLineDrivingFlags);
                BikeEscape.AddDriveTask(new Vector3(637.4592f, -1843.054f, 9.25897f), 80, 30, beeLineDrivingFlags);
            }

            TruckEscape = new TasksList(Michael);
            TruckEscape.AddDriveTask(new Vector3(1022.295f, -352.8022f, 47.91706f), 80, 5, beeLineDrivingFlags);
            TruckEscape.AddDriveTask(new Vector3(881.3533f, -421.4201f, 30.84567f));
            TruckEscape.AddDriveTask(new Vector3(732.917f, -436.3146f, 17.56546f), 80, 10, beeLineDrivingFlags);
            TruckEscape.AddDriveTask(new Vector3(631.4451f, -507.6327f, 15.26594f), 80, 20, beeLineDrivingFlags);
            TruckEscape.AddDriveTask(new Vector3(576.8307f, -651.0452f, 13.58107f));
            TruckEscape.AddDriveTask(new Vector3(619.5798f, -838.1539f, 10.88419f), 80, 20, beeLineDrivingFlags);
            TruckEscape.AddDriveTask(new Vector3(623.5605f, -1288.618f, 9.35399f), 80, 20, beeLineDrivingFlags);
            TruckEscape.AddDriveTask(new Vector3(639.7378f, -1364.642f, 9.326334f));
            TruckEscape.AddDriveTask(new Vector3(636.4721f, -1519.239f, 9.666596f), 80, 20, beeLineDrivingFlags);
            TruckEscape.AddDriveTask(new Vector3(654.4713f, -1593.673f, 9.297204f));
            TruckEscape.AddDriveTask(new Vector3(642.871f, -1784.101f, 9.655074f), 80, 20, beeLineDrivingFlags);

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Jewel store heist"
            };

            NearSpawnMessageSent = false;
            ChaseCreated = false;
            TruckChaseCreated = false;
            ScriptedChaseEnd = false;

            GameFiber.StartNew(delegate {
                GameFiber.Wait(5000);
                CalloutInterfaceAPI.Functions.SendMessage(this, "Suspects escaped through tunnels. They are expected to exit the sewers into the LS River, where a suspicious truck was spotted");
            });

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 250)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message");

                CalloutInterfaceAPI.Functions.SendMessage(this, "Eyewitness claims to have seen three suspects on motorcycles");
                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 200)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting bikes chase");

                EventBlip.Delete();
                FranklinEscape.StartTasks();
                DriverEscape.StartTasks();
                GunmanEscape.StartTasks();
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Franklin);
                Functions.AddPedToPursuit(Pursuit, Driver);
                Functions.AddPedToPursuit(Pursuit, Gunman);
                Functions.SetPursuitDisableAIForPed(Franklin, true);
                Functions.SetPursuitDisableAIForPed(Driver, true);
                Functions.SetPursuitDisableAIForPed(Gunman, true);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                Functions.AddCopToPursuit(Pursuit, Cop);

                ChaseCreated = true;
            }

            if (!TruckChaseCreated && ChaseCreated && Franklin.DistanceTo(new Vector3(1031.18f, -265.59f, 50.37f)) < 10)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting truck chase");
                
                TruckEscape.StartTasks();
                Functions.AddPedToPursuit(Pursuit, Michael);
                Functions.AddPedToPursuit(Pursuit, Hacker);
                Functions.SetPursuitDisableAIForPed(Michael, true);
                Functions.SetPursuitDisableAIForPed(Hacker, true);

                TruckChaseCreated = true;
            }

            if (!ScriptedChaseEnd && Franklin.DistanceTo(new Vector3(637.4592f, -1843.054f, 9.25897f)) < 20) {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Ending scripted chase, restoring ped AI");

                TruckEscape.AbortTasks();
                Driver.CanBeKnockedOffBikes = true;
                Gunman.CanBeKnockedOffBikes = true;

                Functions.SetPursuitDisableAIForPed(Franklin, false);
                Functions.SetPursuitDisableAIForPed(Driver, false);
                Functions.SetPursuitDisableAIForPed(Gunman, false);
                Functions.SetPursuitDisableAIForPed(Michael, false);
                Functions.SetPursuitDisableAIForPed(Hacker, false);

                // Delete doors at the end
                DeleteDoorsFiber = GameFiber.StartNew(delegate
                {
                    while (Game.LocalPlayer.Character.DistanceTo2D(new Vector3(456.0397f, -1999.073f, 23.21957f)) < 500)
                    {
                        GameFiber.Yield();
                        GameFiber.Wait(100);

                        foreach (Entity entity in World.GetEntities(new Vector3(456.0397f, -1999.073f, 23.21957f), 10, GetEntitiesFlags.ConsiderAllObjects))
                        {
                            if (entity.Model.Name.Contains("PROP_FACGATE_01"))
                            {
                                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Deleting door");
                                entity.Delete();
                            }
                        }
                    }

                });

                ScriptedChaseEnd = true;
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
            if (Driver.Exists())
                Driver.Dismiss();
            if (Gunman.Exists())
                Gunman.Dismiss();
            if (Michael.Exists())
                Michael.Dismiss();
            if (Hacker.Exists())
                Hacker.Dismiss();

            if (FranklinBike.Exists())
                FranklinBike.Dismiss();
            if (DriverBike.Exists())
                DriverBike.Dismiss();
            if (GunmanBike.Exists())
                GunmanBike.Dismiss();
            if (Truck.Exists())
                Truck.Dismiss();
            if (Cop.Exists())
                Cop.Dismiss();
            if (CopCar.Exists())
                CopCar.Dismiss();

            if (DeleteDoorsFiber.IsAlive)
                DeleteDoorsFiber.Abort();

            Game.LogTrivial($"[{Main.pluginName}] 'Casing the Jewel Store' callout has ended.");
        }
    }
}
