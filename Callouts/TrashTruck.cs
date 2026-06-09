using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Trash Truck", CalloutProbability.Medium, "Stolen trash truck", "Code 3")]

    internal class TrashTruck : Callout
    {
        private Vector3 SpawnPoint;
        private int SpawnHeading;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Ped StoryCharacter;
        private Vehicle Truck;
        private bool NearSpawnMessageSent, ChaseCreated;
        private int StoryCharacterVariant;

        public override bool OnBeforeCalloutDisplayed()
        {
            Vector3[] spawnpoints = {
                new Vector3(1204.665f, -330.9844f, 68.97367f),
                new Vector3(997.8835f, -449.4529f, 63.65305f),
                new Vector3(964.3202f, -647.4575f, 57.32551f),
                new Vector3(1147.65f, -766.5634f, 57.46485f)
            };
            int[] spawnHeadings =
            {
                186,
                126,
                215,
                276,
            };

            int spawnVariation = MathHelper.GetRandomInteger(4);

            SpawnPoint = spawnpoints[spawnVariation];
            SpawnHeading = spawnHeadings[spawnVariation];
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 50f);
            AddMinimumDistanceCheck(200f, SpawnPoint);
            CalloutMessage = "Stolen trash truck";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_GRAND_THEFT_AUTO IN_OR_ON_POSITION", SpawnPoint);

            StoryCharacterVariant = MathHelper.GetRandomInteger(3);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            StoryCharacter = StoryCharacterVariant switch
            {
                0 => Characters.Franklin.Create(new Vector3(1020.568f, -429.6385f, 65.03073f), 297, this.GetType().Name),
                1 => Characters.Michael.Create(new Vector3(1020.568f, -429.6385f, 65.03073f), 297, this.GetType().Name),
                _ => Characters.Trevor.Create(new Vector3(1020.568f, -429.6385f, 65.03073f), 297, this.GetType().Name),
            };

            Truck = new Vehicle("TRASH", SpawnPoint, SpawnHeading);
            StoryCharacter.WarpIntoVehicle(Truck, -1);

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Stolen trash truck"
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

                CalloutInterfaceAPI.Functions.SendMessage(this, "A trash truck was hijacked");
                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 200)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting chase");

                StoryCharacter.Tasks.DriveToPosition(new Vector3(1380.427f, -2071.951f, 51.71614f), 80, VehicleDrivingFlags.Emergency);

                GameFiber.Wait(3000);

                EventBlip.Delete();
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, StoryCharacter);
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
            if (StoryCharacter.Exists())
                StoryCharacter.Dismiss();
            if (Truck.Exists())
                Truck.Dismiss();

            Game.LogTrivial($"[{Main.pluginName}] 'Trash Truck' callout has ended.");
        }
    }
}
