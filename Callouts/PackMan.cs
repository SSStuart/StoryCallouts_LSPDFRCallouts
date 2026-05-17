using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Pack Man", CalloutProbability.Medium, "Stolen vehicles transport", "Code 3")]

    internal class PackMan : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Ped Trevor, Lamar, Franklin;
        private Vehicle Truck, Trailer, JB700, Monroe;
        private List<Object> Spikes;
        private GameFiber SpikesFiber;
        private bool NearSpawnMessageSent, ChaseCreated;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(-2194.68f, 4353.564f, 51.22617f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 50f);
            AddMinimumDistanceCheck(200f, SpawnPoint);
            CalloutMessage = "Stolen vehicles transport";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_GRAND_THEFT_AUTO IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Franklin = Characters.Franklin.Create(new Vector3(-2189.67f, 4373.65f, 53.96f), 0, this.GetType().Name);
            Franklin.KeepTasks = true;
            Trevor = Characters.Trevor.Create(new Vector3(-2184.719f, 4389.535f, 55.9315f), 0, this.GetType().Name);
            Lamar = Characters.Lamar.Create(new Vector3(-2180.466f, 4388.823f, 56.02013f), 0, this.GetType().Name);

            Truck = new Vehicle("packer", new Vector3(-2183.17f, 4387.66f, 55.86f), 340)
            {
                PrimaryColor = Color.FromArgb(74, 16, 0),
            };
            Trevor.WarpIntoVehicle(Truck, -1);
            Lamar.WarpIntoVehicle(Truck, 0);
            Trailer = new Vehicle("tr4", new Vector3(-2187.36f, 4375.00f, 55.5f), 340);
            Truck.Trailer = Trailer;
            JB700 = new Vehicle("jb7002", new Vector3(-2187.83f, 4372.91f, 59.20f), 340)
            {
                PrimaryColor = Color.FromArgb(41, 44, 46),
                IsEngineOn = false
            };
            // Add mounted weapons
            JB700.Mods.InstallModKit();
            JB700.Mods.ApplyAllMods();

            JB700.AttachTo(Trailer, 0, new Vector3(0, -5, 3.4f), new Rotator());
            Franklin.WarpIntoVehicle(JB700, -1);
            Monroe = new Vehicle("monroe", new Vector3(-2187.83f, 4372.91f, 55.9f), 340)
            {
                PrimaryColor = Color.FromArgb(217, 166, 0),
            };
            Monroe.AttachTo(Trailer, 0, new Vector3(0, -4.8f, 1f), new Rotator(5, 0, 0));

            EventBlip = Truck.AttachBlip();
            EventBlip.Color = Main.calloutWaypointColor;
            EventBlip.IsRouteEnabled = true;
            EventBlip.Name = "Stolen vehicles transport";

            Spikes = new List<Object>();

            NearSpawnMessageSent = false;
            ChaseCreated = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 500)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message & Driving task");

                CalloutInterfaceAPI.Functions.SendMessage(this, "A truck carrying stolen vehicles was spotted on the Great Ocean Highway");
                Trevor.Tasks.CruiseWithVehicle(70, VehicleDrivingFlags.FollowTraffic);
                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(Truck) < 100)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting chase & JB700 logic loop");

                EventBlip.Delete();

                GameFiber.StartNew(JB700Logic);

                Trevor.Tasks.Clear();
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Trevor);
                Functions.AddPedToPursuit(Pursuit, Lamar);
                Functions.AddPedToPursuit(Pursuit, Franklin);
                Functions.SetPursuitDisableAIForPed(Franklin, true);
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
            if (Franklin.Exists())
                Franklin.Dismiss();
            if (Trevor.Exists())
                Trevor.Dismiss();
            if (Lamar.Exists())
                Lamar.Dismiss();
            if (Truck.Exists())
                Truck.Dismiss();
            if (Trailer.Exists())
                Trailer.Dismiss();
            if (Monroe.Exists())
                Monroe.Dismiss();
            if (JB700.Exists())
                JB700.Dismiss();
            if (SpikesFiber.IsAlive)
                SpikesFiber.Abort();
            foreach (Object spike in Spikes)
                if (spike.Exists())
                    spike.Delete();

            Game.LogTrivial($"[{Main.pluginName}] 'Pack Man' callout has ended.");
        }

        private void JB700Logic()
        {
            GameFiber.Wait(10000);
            Trailer.GetDoors()[1].Open(false);
            JB700.IsEngineOn = true;
            GameFiber.Wait(2000);
            JB700.Detach();
            Franklin.Tasks.PerformDrivingManeuver(JB700, VehicleManeuver.ReverseStraight, 1000);
            do
            {
                JB700.AngularVelocity = new Rotator(JB700.AngularVelocity.Pitch, 0, JB700.AngularVelocity.Yaw);
                GameFiber.Yield();
            } while (JB700.HeightAboveGround > 1);
            Franklin.Tasks.PerformDrivingManeuver(JB700, VehicleManeuver.GoForwardStraight, 1000);

            NativeFunction.Natives.TASK_VEHICLE_FOLLOW(Franklin, JB700, Truck, 100f, 1074528293, 40);

            GameFiber.Wait(10000);
            Monroe.Detach();

            Franklin.KeepTasks = true;
            NativeFunction.Natives.SET_CURRENT_PED_VEHICLE_WEAPON<bool>(Franklin, Game.GetHashKey("VEHICLE_WEAPON_PLAYER_BULLET"));
            GameFiber.StartNew(MountedGunFiring);

            SpikesFiber = GameFiber.StartNew(SpikesLogic);

            while (Functions.IsPursuitStillRunning(Pursuit) && JB700.Exists() && Franklin.IsInVehicle(JB700, false))
            {
                GameFiber.Sleep(5000);

                List<Object> cleanedSpikes = new List<Object>();
                foreach (Object spike in Spikes)
                {
                    if (spike.DistanceTo2D(Game.LocalPlayer.Character) > 100 && JB700.Exists() && spike.DistanceTo2D(JB700) > 100)
                        spike.Delete();
                    else
                        cleanedSpikes.Add(spike);
                }
                Spikes = cleanedSpikes;

                if (JB700.Speed < 20 || Spikes.Count > 10)
                    continue;

                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Spawning spike");
                Object newSpike = new Object("prop_tyre_spike_01", JB700.Position)
                {
                    CollisionIgnoredEntity = JB700,
                    IsPersistent = true,
                };
                Blip spikeBlip = new Blip(newSpike)
                {
                    Sprite = (BlipSprite)913,
                    Color = Color.Red,
                    Alpha = 0.5f,
                    Scale = 0.8f,
                    Name = "Spikes"
                };
                Spikes.Add(newSpike);
            }
        }

        private void SpikesLogic()
        {
            while (true)
            {
                GameFiber.Yield();

                if (!Game.LocalPlayer.Character.IsInAnyVehicle(false))
                    continue;
                Vehicle playerVehicle = Game.LocalPlayer.Character.CurrentVehicle;

                VehicleWheel wheelLF = playerVehicle.Wheels[0];
                VehicleWheel wheelRF = playerVehicle.Wheels[1];
                VehicleWheel wheelLR = playerVehicle.Wheels[7];
                VehicleWheel wheelRR = playerVehicle.Wheels[5];

                Vector3 wheelLF_Position = playerVehicle.GetBonePosition(playerVehicle.GetBoneIndex("wheel_lf"));
                Vector3 wheelRF_Position = playerVehicle.GetBonePosition(playerVehicle.GetBoneIndex("wheel_rf"));
                Vector3 wheelLR_Position = playerVehicle.GetBonePosition(playerVehicle.GetBoneIndex("wheel_lr"));
                Vector3 wheelRR_Position = playerVehicle.GetBonePosition(playerVehicle.GetBoneIndex("wheel_rr"));

                foreach (Object spike in Spikes)
                {
                    float distanceWheelLF_X = spike.Position.X - wheelLF_Position.X;
                    float distanceWheelLF_Y = spike.Position.Y - wheelLF_Position.Y;
                    float distanceWheelLF = distanceWheelLF_X * distanceWheelLF_X + distanceWheelLF_Y * distanceWheelLF_Y;
                    float distanceWheelRF_X = spike.Position.X - wheelRF_Position.X;
                    float distanceWheelRF_Y = spike.Position.Y - wheelRF_Position.Y;
                    float distanceWheelRF = distanceWheelRF_X * distanceWheelRF_X + distanceWheelRF_Y * distanceWheelRF_Y;
                    float distanceWheelLR_X = spike.Position.X - wheelLR_Position.X;
                    float distanceWheelLR_Y = spike.Position.Y - wheelLR_Position.Y;
                    float distanceWheelLR = distanceWheelLR_X * distanceWheelLR_X + distanceWheelLR_Y * distanceWheelLR_Y;
                    float distanceWheelRR_X = spike.Position.X - wheelRR_Position.X;
                    float distanceWheelRR_Y = spike.Position.Y - wheelRR_Position.Y;
                    float distanceWheelRR = distanceWheelRR_X * distanceWheelRR_X + distanceWheelRR_Y * distanceWheelRR_Y;

                    if (distanceWheelLF < 0.5 && wheelLF.TireHealth == 1000)
                        wheelLF.BurstTire();
                    if (distanceWheelRF < 0.5 && wheelRF.TireHealth == 1000)
                        wheelRF.BurstTire();
                    if (distanceWheelLR < 0.5 && wheelLR.TireHealth == 1000)
                        wheelLR.BurstTire();
                    if (distanceWheelRR < 0.5 && wheelRR.TireHealth == 1000)
                        wheelRR.BurstTire();
                }
            }
        }

        private void MountedGunFiring()
        {
            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting JB700 mounted gun loop");

            uint firingStart = 0;
            List<Entity> copVehiclesInFront = new List<Entity>();

            while (Franklin.IsInVehicle(JB700, false) && Franklin.IsAlive)
            {
                GameFiber.Yield();

                if (Game.GameTime > firingStart + 2000)
                {
                    GameFiber.Wait(MathHelper.GetRandomInteger(10) * 1000);
                    firingStart = Game.GameTime;
                    copVehiclesInFront = World.GetEntities(JB700.GetOffsetPositionFront(40), 30, GetEntitiesFlags.ConsiderGroundVehicles).Where(veh => veh.Model.IsLawEnforcementVehicle).ToList();
                } else if (copVehiclesInFront.Count > 0)
                    NativeFunction.Natives.SET_VEHICLE_SHOOT_AT_TARGET(Franklin, Game.LocalPlayer.Character, Game.LocalPlayer.Character.Position.X, Game.LocalPlayer.Character.Position.Y, Game.LocalPlayer.Character.Position.Z);
            }
        }
    }
}
