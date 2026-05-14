using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Driller", CalloutProbability.Medium, "Stolen construction equipment", "Code 3")]

    internal class Driller : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Ped Michael, SecurityGuard1, SecurityGuard2;
        private Vehicle Truck, Trailer;
        private bool NearSpawnMessageSent, ChaseCreated;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(885f, -1565f, 30f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            AddMinimumDistanceCheck(200f, SpawnPoint);
            CalloutMessage = "Stolen construction equipment";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_GRAND_THEFT_AUTO IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Michael = Characters.Michael.Create(SpawnPoint, 0, this.GetType().Name);
            Michael.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_PISTOL"), 100, true);

            Truck = new Vehicle("PACKER", new Vector3(919.29f, -1553.88f, 30.85f), 167f);
            Trailer = new Vehicle("armytrailer2", new Vector3(921.170f, -1545.62f, 32.62f), 167f);
            Truck.Trailer = Trailer;

            Michael.WarpIntoVehicle(Truck, -1);

            SecurityGuard1 = new Ped("S_M_M_SECURITY_01", new Vector3(861.8859f, -1564.478f, 30.32067f), 113f);
            SecurityGuard1.Inventory.GiveNewWeapon("WEAPON_PISTOL", 100, true);

            SecurityGuard2 = new Ped("S_M_M_SECURITY_01", new Vector3(938.4023f, -1574.191f, 30.38027f), 273f);
            SecurityGuard2.Inventory.GiveNewWeapon("WEAPON_PISTOL", 100, true);

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Stolen construction equipment"
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
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message");

                CalloutInterfaceAPI.Functions.SendMessage(this, "Truck with construction equipment was stolen");
                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 100)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting chase");

                EventBlip.Delete();
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Michael);
                Functions.AddCopToPursuit(Pursuit, SecurityGuard1);
                Functions.AddCopToPursuit(Pursuit, SecurityGuard2);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                GameFiber.StartNew(delegate
                {
                    // Helping truck start
                    do
                    {
                        Truck.ApplyForce(new Vector3(0, 5, 0), new Vector3(), true, false);
                        GameFiber.Yield();
                    } while (Truck.DistanceTo2D(new Vector3(919.29f, -1553.88f, 30.85f)) < 5);
                });

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
            if (Truck.Exists())
                Truck.Dismiss();
            if (Trailer.Exists())
                Trailer.Dismiss();
            if (SecurityGuard1.Exists())
                SecurityGuard1.Dismiss();
            if (SecurityGuard2.Exists())
                SecurityGuard2.Dismiss();

            Game.LogTrivial($"[{Main.pluginName}] 'Driller' callout has ended.");
        }
    }
}
