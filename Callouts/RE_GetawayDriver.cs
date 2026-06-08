using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Getaway Driver", CalloutProbability.Medium, "Drugstore robbery", "Code 3")]

    internal class RE_GetawayDriver : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Ped Patrick, Robber, EscapeDriver, ShopKeeper;
        private Vehicle EscapeVehicle;
        private bool NearSpawnMessageSent, ChaseCreated;
        private int EscapePedVariant;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(57.79152f, -1567.111f, 29.25389f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 20f);
            AddMinimumDistanceCheck(200f, SpawnPoint);
            CalloutMessage = "Drugstore robbery";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_ROBBERY IN_OR_ON_POSITION", SpawnPoint);

            EscapePedVariant = MathHelper.GetRandomInteger(3);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            EscapeDriver = EscapePedVariant == 0 ?
                Characters.Franklin.Create(new Vector3(61.83673f, -1564.017f, 29.46009f), 0, this.GetType().Name) :
                (EscapePedVariant == 1 ?
                Characters.Michael.Create(new Vector3(61.83673f, -1564.017f, 29.46009f), 0, this.GetType().Name) :
                Characters.Trevor.Create(new Vector3(61.83673f, -1564.017f, 29.46009f), 0, this.GetType().Name));

            EscapeVehicle = EscapePedVariant == 0 ?
                Vehicles.FranklinCar.Create(new Vector3(55.26971f, -1565.4f, 29.24365f), 165, EscapeDriver) :
                (EscapePedVariant == 1 ?
                Vehicles.MichaelCar.Create(new Vector3(55.26971f, -1565.4f, 29.24365f), 165, EscapeDriver) :
                Vehicles.TrevorCar.Create(new Vector3(55.26971f, -1565.4f, 29.24365f), 165, EscapeDriver));
            EscapeVehicle.IsEngineOn = true;

            Patrick = Characters.Patrick.Create(new Vector3(63.862f, -1567.91f, 29.46022f), 254, this.GetType().Name);
            Patrick.Inventory.GiveNewWeapon("WEAPON_PISTOL", 50, true);

            Robber = new Ped("G_M_Y_MEXGOON_02", new Vector3(65.74f, -1564.75f, 29.46f), 216f)
            {
                BlockPermanentEvents = true
            };
            Robber.Inventory.GiveNewWeapon("WEAPON_APPISTOL", 50, true);

            ShopKeeper = new Ped("MP_M_SHOPKEEP_01", new Vector3(68.46f, -1569.78f, 29.60f), 50f)
            {
                BlockPermanentEvents = true
            };

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Drugstore robbery"
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
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message, and appying tasks");

                CalloutInterfaceAPI.Functions.SendMessage(this, "A drugstore is being robbed by two armed suspects");

                Patrick.Tasks.AimWeaponAt(ShopKeeper, -1);
                Robber.Tasks.AimWeaponAt(ShopKeeper, -1);

                ShopKeeper.Tasks.PlayAnimation("anim@scripted@npc@bounty_ig_surrender@heeled@", "surrender_idle_bounty", 1f, AnimationFlags.Loop);

                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 50)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting chase");

                EventBlip.Delete();
                Pursuit = Functions.CreatePursuit();
                Patrick.Tasks.EnterVehicle(EscapeVehicle, 0, 3f);
                Robber.Tasks.EnterVehicle(EscapeVehicle, 1, 3f);
                GameFiber.StartNew(delegate
                {
                    do
                    {
                        GameFiber.Wait(100);
                    } while ((!Patrick.IsInVehicle(EscapeVehicle, false) || !Robber.IsInVehicle(EscapeVehicle, false)) && Game.LocalPlayer.Character.DistanceTo(EscapeVehicle) > 10);
                    Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Robbers have entered the vehicle");

                    Functions.SetPursuitDisableAIForPed(EscapeDriver, false);
                    Functions.SetPursuitDisableAIForPed(Patrick, false);
                    Functions.SetPursuitDisableAIForPed(Robber, false);

                    GameFiber.Wait(3000);
                    ShopKeeper.Tasks.Clear();

                });

                Functions.AddPedToPursuit(Pursuit, EscapeDriver);
                Functions.AddPedToPursuit(Pursuit, Patrick);
                Functions.AddPedToPursuit(Pursuit, Robber);
                Functions.SetPursuitDisableAIForPed(EscapeDriver, true);
                Functions.SetPursuitDisableAIForPed(Patrick, true);
                Functions.SetPursuitDisableAIForPed(Robber, true);
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
            if (EscapeDriver.Exists())
                EscapeDriver.Dismiss();
            if (Patrick.Exists())
                Patrick.Dismiss();
            if (Robber.Exists())
                Robber.Dismiss();
            if (ShopKeeper.Exists())
                ShopKeeper.Dismiss();
            if (EscapeVehicle.Exists())
                EscapeVehicle.Dismiss();

            Game.LogTrivial($"[{Main.pluginName}] 'Getaway Driver' callout has ended.");
        }
    }
}
