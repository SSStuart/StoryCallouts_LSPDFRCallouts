using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Dead Man Walking", CalloutProbability.Medium, "Suspect escaped from the morgue", "Code 3")]

    internal class DeadManWalking : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Ped Michael;
        private Vehicle EscapeVehicle;
        private bool NearSpawnMessageSent, ChaseCreated;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(234.195f, -1356.027f, 31.08425f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            AddMinimumDistanceCheck(200f, SpawnPoint);
            CalloutMessage = "Suspect escaped from the morgue";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_SUSPECT_ON_THE_RUN IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Michael = Characters.Michael.Create(SpawnPoint, 0, this.GetType().Name);
            Michael.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_PISTOL"), 100, true);

            EscapeVehicle = new Vehicle("felon", new Vector3(223.1245f, -1352.664f, 30.27094f), 234);

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Suspect escaped from the morgue"
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

                CalloutInterfaceAPI.Functions.SendMessage(this, "Suspect escaped through a window on the north-west side and is believed to be armed");
                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 100)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting chase");

                EventBlip.Delete();
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Michael);
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
            if (EscapeVehicle.Exists())
                EscapeVehicle.Dismiss();

            Game.LogTrivial($"[{Main.pluginName}] 'Dead Man Walking' callout has ended.");
        }
    }
}
