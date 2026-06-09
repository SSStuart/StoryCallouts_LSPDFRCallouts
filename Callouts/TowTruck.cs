using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Tow Truck", CalloutProbability.Medium, "Stolen tow truck", "Code 2")]

    internal class TowTruck : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip, GPSAreaBlip;
        private LHandle Pursuit;
        private Ped Mechanic, StoryCharacter;
        private Vehicle TowTruckVehicle, BrokenDownCar;
        private GameFiber GPSAreaFiber;
        private bool NearSpawnMessageSent, MechanicAnimation, DialoguePlayed, ChaseCreated, DestinationReached;
        private int StoryCharacterVariant;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(-409.62f, -2175.89f, 10.32f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 10f);
            AddMinimumDistanceCheck(100f, SpawnPoint);
            CalloutMessage = "Stolen tow truck";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_GRAND_THEFT_AUTO IN_OR_ON_POSITION", SpawnPoint);

            StoryCharacterVariant = MathHelper.GetRandomInteger(3);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            StoryCharacter = StoryCharacterVariant switch
            {
                0 => Characters.Franklin.Create(new Vector3(-104.4347f, -2068.198f, 17.56091f), 297, this.GetType().Name),
                1 => Characters.Michael.Create(new Vector3(-104.4347f, -2068.198f, 17.56091f), 297, this.GetType().Name),
                _ => Characters.Trevor.Create(new Vector3(-104.4347f, -2068.198f, 17.56091f), 297, this.GetType().Name),
            };

            TowTruckVehicle = new Vehicle("TOWTRUCK", new Vector3(-99.50f, -2066.19f, 17.39f), 22f);
            StoryCharacter.WarpIntoVehicle(TowTruckVehicle, -1);

            BrokenDownCar = new Vehicle("PEYOTE", new Vector3(-412.35f, -2176.26f, 9.63f), 281f)
            {
                EngineHealth = 350f,
            };
            VehicleDoor[] CarDoors = BrokenDownCar.GetDoors();
            CarDoors[0].IsOpen = true;
            CarDoors[2].IsFullyOpen = true;

            Mechanic = new Ped("S_M_M_TRUCKER_01", new Vector3(-409.62f, -2175.89f, 10.32f), 102f);

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Stolen tow truck"
            };

            NearSpawnMessageSent = false;
            MechanicAnimation = false;
            DialoguePlayed = false;
            ChaseCreated = false;
            DestinationReached = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 200)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message");

                CalloutInterfaceAPI.Functions.SendMessage(this, "A mechanic has reported that his tow truck was stolen");

                Mechanic.Tasks.PlayAnimation("mini@repair", "fixing_a_ped", 1f, AnimationFlags.Loop);

                NearSpawnMessageSent = true;
            }

            if (!MechanicAnimation && Game.LocalPlayer.Character.DistanceTo(Mechanic) < 20)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Player on site, starting mechanic animations");

                Mechanic.Tasks.PlayAnimation("move_m@fat@a", "idle", 0.8f, AnimationFlags.None).WaitForCompletion(2000);
                Mechanic.Tasks.AchieveHeading(-20).WaitForCompletion(2000);
                Mechanic.Tasks.PlayAnimation("friends@frj@ig_1", "wave_a", 0.9f, AnimationFlags.None);

                MechanicAnimation = true;
            }

            if (!DialoguePlayed && Game.LocalPlayer.Character.DistanceTo(Mechanic) < 3)
            {
                if (!Game.IsControlPressed(0, GameControl.Context))
                {
                    Game.DisplayHelp("Press ~INPUT_CONTEXT~ to talk to the mechanic");
                }
                else
                {
                    Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Talked to mechanic");

                    EventBlip.Delete();

                    Game.HideHelp();
                    Game.DisplaySubtitle("<C>Mechanic:</C> Someone stole my ~b~tow truck~w~~s~, but luckily it has ~y~GPS tracking~w~~s~. ~n~Here, you should be able to find it with this.");
                    GameFiber.Wait(5000);

                    Game.DisplayHelp("Follow the ~y~GPS signal~w~~s~ to find the tow truck");

                    StoryCharacter.Tasks.DriveToPosition(new Vector3(1374.662f, -2077.536f, 51.96322f), 50, VehicleDrivingFlags.Emergency);

                    GPSAreaFiber = GameFiber.StartNew(delegate
                    {
                        UpdateGPSArea(new Vector3(-107.4256f, -2020.046f, 17.96909f));
                    });

                    DialoguePlayed = true;
                }
            }

            if (DialoguePlayed && !ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(TowTruckVehicle) < 30 && Math.Abs(TowTruckVehicle.Position.Z - Game.LocalPlayer.Character.Position.Z) < 3)
            {
                GPSAreaBlip.DisableRoute();
                GPSAreaBlip.Alpha = 0;

                if (StoryCharacter.DistanceTo(TowTruckVehicle) < 30)
                {
                    Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Player near tow truck and suspect near, starting chase");

                    Pursuit = Functions.CreatePursuit();
                    Functions.AddPedToPursuit(Pursuit, StoryCharacter);
                    Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                    ChaseCreated = true;
                }
                else
                {
                    End();
                }
            }

            if (DialoguePlayed && !DestinationReached && StoryCharacter.DistanceTo2D(new Vector3(1374.662f, -2077.536f, 51.96322f)) < 5)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Suspect reached destination, wandering");

                GameFiber.Sleep(2000);
                StoryCharacter.Tasks.Wander();

                DestinationReached = true;
            }

            if (Game.IsKeyDown(Keys.End)
                || (ChaseCreated && !Functions.IsPursuitStillRunning(Pursuit)))
                End();
        }

        public override void End()
        {
            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Ending callout");

            base.End();

            if (GPSAreaFiber != null && GPSAreaFiber.IsAlive)
                GPSAreaFiber.Abort();

            if (EventBlip.Exists())
                EventBlip.Delete();
            if (Mechanic.Exists())
                Mechanic.Dismiss();
            if (BrokenDownCar.Exists())
                BrokenDownCar.Dismiss();
            if (GPSAreaBlip.Exists())
                GPSAreaBlip.Delete();
            if (StoryCharacter.Exists())
                StoryCharacter.Dismiss();
            if (TowTruckVehicle.Exists())
                TowTruckVehicle.Dismiss();

            Game.LogTrivial($"[{Main.pluginName}] 'Tow Truck' callout has ended.");
        }

        private void UpdateGPSArea(Vector3 startingPosition)
        {
            if (!GPSAreaBlip.Exists())
            {
                GPSAreaBlip = new Blip(startingPosition, 200)
                {
                    Sprite = BlipSprite.Playerisvisible,
                    Name = "Tow truck's GPS signal",
                    RouteColor = Main.calloutWaypointColor,
                    IsRouteEnabled = true,
                };
            }

            uint nextUpdateTime = Game.GameTime + 1000 * 10;

            while (TowTruckVehicle.Exists())
            {
                GameFiber.WaitUntil(() => Game.GameTime > nextUpdateTime);

                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Updating GPS area blip");

                bool IsBlipActive = (DialoguePlayed && !ChaseCreated) ||
                    (ChaseCreated && Pursuit != null && Functions.IsPursuitStillRunning(Pursuit) && Functions.IsPedVisualLostLonger(StoryCharacter));

                if (IsBlipActive)
                {
                    GPSAreaBlip.Position = DestinationReached ? TowTruckVehicle.Position : TowTruckVehicle.Position.Around2D(20, 50);
                    GPSAreaBlip.EnableRoute(Main.calloutWaypointColor);
                }
                else
                {
                    GPSAreaBlip.DisableRoute();
                }
                GPSAreaBlip.Alpha = IsBlipActive ? 1 : 0;

                nextUpdateTime = Game.GameTime + 1000 * 10;

                if (ChaseCreated && !StoryCharacter.IsInVehicle(TowTruckVehicle, false))
                {
                    GPSAreaBlip.Delete();
                    break;
                }
            }
        }
    }
}
