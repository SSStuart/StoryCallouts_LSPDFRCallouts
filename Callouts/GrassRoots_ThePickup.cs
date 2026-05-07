using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Grass Roots - The Pickup", CalloutProbability.Medium, "Vehicle under surveillance", "Code 3")]

    internal class GrassRoots_ThePickup : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Ped Franklin, Cop1, Cop2, Cop3, Cop4;
        private Vehicle Truck, PoliceCar1, PoliceCar2;
        private Object Drugs;
        private TasksList FranklinEscape;
        private bool NearSpawnMessageSent, ChaseCreated;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(1205.535f, -1282.151f, 35.22677f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            AddMinimumDistanceCheck(100f, SpawnPoint);
            CalloutMessage = "Vehicle under surveillance";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_GRAND_THEFT_AUTO IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Franklin = Characters.Franklin.Create(new Vector3(1204.356f, -1264.81f, 35.22673f), 0, this.GetType().Name);

            FranklinEscape = new TasksList(Franklin);
            FranklinEscape.AddDriveTask(new Vector3(1206.921f, -1285.642f, 34.90635f), 30, 5, VehicleDrivingFlags.DriveAroundObjects);
            FranklinEscape.AddDriveTask(new Vector3(1228.407f, -1291.004f, 34.80341f));

            Truck = new Vehicle("DLOADER", new Vector3(1199.80f, -1259.22f, 34.94f), 174f);
            Franklin.WarpIntoVehicle(Truck, -1);

            Drugs = new Object("prop_weed_tub_01b", new Vector3(1197.543f, -1255.969f, 35.22673f));
            Drugs.AttachTo(Truck, 0, new Vector3(0, -1, 0.6f), new Rotator());

            PoliceCar1 = new Vehicle("POLICE4", new Vector3(1195.28f, -1312.74f, 34.75f), 282f);
            Cop1 = new Ped("A_M_Y_BUSINESS_01", PoliceCar1.Position.Around2D(5), 0);
            Cop1.WarpIntoVehicle(PoliceCar1, -1);
            Functions.SetPedAsCop(Cop1);
            Cop2 = new Ped("A_M_Y_BUSINESS_01", PoliceCar1.Position.Around2D(5), 0);
            Cop2.WarpIntoVehicle(PoliceCar1, 0);
            Functions.SetPedAsCop(Cop2);

            PoliceCar2 = new Vehicle("POLICE4", new Vector3(1162.71f, -1357.86f, 34.35f), 266f);
            Cop3 = new Ped("A_M_Y_BUSINESS_01", PoliceCar2.Position.Around2D(5), 0);
            Cop3.WarpIntoVehicle(PoliceCar2, -1);
            Functions.SetPedAsCop(Cop3);
            Cop4 = new Ped("A_M_Y_BUSINESS_01", PoliceCar2.Position.Around2D(5), 0);
            Cop4.WarpIntoVehicle(PoliceCar2, 0);
            Functions.SetPedAsCop(Cop4);

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Vehicle under surveillance"
            };

            NearSpawnMessageSent = false;
            ChaseCreated = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 150)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message");

                CalloutInterfaceAPI.Functions.SendMessage(this, "Suspect entered a vehicle under surveillance in the lumber yard");
                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 100)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting chase");

                FranklinEscape.StartTasks();

                EventBlip.Delete();
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Franklin);
                Functions.SetPursuitDisableAIForPed(Franklin, true);
                Functions.AddCopToPursuit(Pursuit, Cop1);
                Functions.AddCopToPursuit(Pursuit, Cop2);
                Functions.AddCopToPursuit(Pursuit, Cop3);
                Functions.AddCopToPursuit(Pursuit, Cop4);
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
            if (Drugs.Exists())
                Drugs.Dismiss();
            if (Truck.Exists())
                Truck.Dismiss();
            if (Cop1.Exists())
                Cop1.Dismiss();
            if (Cop2.Exists())
                Cop2.Dismiss();
            if (Cop3.Exists())
                Cop3.Dismiss();
            if (Cop4.Exists())
                Cop4.Dismiss();
            if (PoliceCar1.Exists())
                PoliceCar1.Dismiss();
            if (PoliceCar2.Exists())
                PoliceCar2.Dismiss();

            Game.LogTrivial($"[{Main.pluginName}] 'Grass Roots - The Pickup' callout has ended.");
        }
    }
}
