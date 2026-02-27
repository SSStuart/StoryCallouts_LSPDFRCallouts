using LSPD_First_Response.Engine;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Franklin and Lamar", CalloutProbability.Medium, "Illegal Street Race", "Code 3")]

    internal class FranklinAndLamar : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Vehicle FranklinVehicle, LamarVehicle;
        private Ped Franklin, Lamar;
        private Object LadderBarrier;
        private WaypointsList FranklinWaypoints;
        private bool NearSpawnMessageSent, ChaseCreated;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(46.19795f, -642.249f, 31.26925f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            AddMinimumDistanceCheck(100f, SpawnPoint);
            AddMaximumDistanceCheck(1000f, SpawnPoint);
            CalloutMessage = "Illegal Street Race";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_GRAND_THEFT_AUTO IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Franklin = new Ped("player_one", SpawnPoint, 0);
            FranklinVehicle = new Vehicle("rapidgt2", SpawnPoint, -20)
            {
                PrimaryColor = System.Drawing.Color.DarkRed,
                ConvertibleRoofState = VehicleConvertibleRoofState.Lowered,
                IsEngineOn = true
            };
            Franklin.WarpIntoVehicle(FranklinVehicle, -1);
            Lamar = new Ped("ig_lamardavis", SpawnPoint + new Vector3(-5, 0, 0), 0);
            LamarVehicle = new Vehicle("ninef2", SpawnPoint + new Vector3(-5, 0, 0), -20)
            {
                PrimaryColor = System.Drawing.Color.White,
                ConvertibleRoofState = VehicleConvertibleRoofState.Lowered,
                IsEngineOn = true
            };
            Lamar.WarpIntoVehicle(LamarVehicle, -1);

            FranklinWaypoints = new WaypointsList(Franklin);
            FranklinWaypoints.AddWaypoint(new Vector3(34.117f, -758.7211f, 31.24227f));
            FranklinWaypoints.AddWaypoint(new Vector3(87.78441f, -818.0637f, 30.82329f));
            FranklinWaypoints.AddWaypoint(new Vector3(76.84824f, -845.1758f, 30.41537f));
            FranklinWaypoints.AddWaypoint(new Vector3(37.86562f, -975.2443f, 29.02815f));
            FranklinWaypoints.AddWaypoint(new Vector3(29.2566f, -1037.328f, 28.84876f));
            FranklinWaypoints.AddWaypoint(new Vector3(154.3781f, -1129.831f, 28.91429f), 50);
            FranklinWaypoints.AddWaypoint(new Vector3(155.296f, -1016.29f, 29.02305f));
            FranklinWaypoints.AddWaypoint(new Vector3(32.46893f, -1040.906f, 29.03797f));
            FranklinWaypoints.AddWaypoint(new Vector3(-53.92566f, -1031.56f, 28.14666f));
            FranklinWaypoints.AddWaypoint(new Vector3(-14.92295f, -1086.378f, 26.30285f));

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Illegal Street Race"
            };

            ChaseCreated = false;
            NearSpawnMessageSent = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 150)
            {
                CalloutInterfaceAPI.Functions.SendMessage(this, "Parking lot guard reported seeing the two vehicles in a parking lot behind the Union Depository");
                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 50)
            {
                EventBlip.Delete();
                FranklinWaypoints.StartTasks();
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Franklin);
                Functions.AddPedToPursuit(Pursuit, Lamar);
                Functions.SetPursuitDisableAIForPed(Franklin, true);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                LadderBarrier = new Object("prop_plas_barier_01a", new Vector3(33.60389f, -1007.118f, 28.5f), 90);

                ChaseCreated = true;
            }

            if (Game.IsKeyDown(Keys.End)
                || (ChaseCreated && !Functions.IsPursuitStillRunning(Pursuit)))
                End();
        }

        public override void End()
        {
            base.End();

            if (EventBlip.Exists())
                EventBlip.Delete();
            if (Franklin.Exists())
                Franklin.Dismiss();
            if (FranklinVehicle.Exists())
                FranklinVehicle.Dismiss();
            if (Lamar.Exists())
                Lamar.Dismiss();
            if (LamarVehicle.Exists())
                LamarVehicle.Dismiss();
            if (LadderBarrier.Exists())
                LadderBarrier.Delete();

            Game.LogTrivial($"[{Main.pluginName}] 'Franklin and Lamar' callout has ended.");
        }
    }
}
