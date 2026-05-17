using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Security.Policy;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("The Third Way", CalloutProbability.Medium, "Intrusion in the foundry", "Code 3")]

    internal class TheThirdWay : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Ped Michael, Franklin, Trevor, Lamar;
        private Vehicle FranklinCar;
        private Object Crate;
        private GameFiber ShootoutFiber;
        private bool NearSpawnMessageSent, PlayerNear, PursuitCreated;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(1085.99f, -1996.984f, 30f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            AddMinimumDistanceCheck(200f, SpawnPoint);
            CalloutMessage = "Intrusion in the foundry";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_GUNFIRE IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Michael = Characters.Michael.Create(new Vector3(1063.866f, -2004.744f, 32.06247f), 71, this.GetType().Name);
            Michael.Inventory.GiveNewWeapon("WEAPON_HEAVYSNIPER", 200, true);
            Michael.Tasks.PlayAnimation("anim@amb@inspect@crouch@male_a@idles", "idle_a", 1f, AnimationFlags.Loop);
            Michael.IsVisible = false;
            Michael.RelationshipGroup = RelationshipGroup.Gang1;

            Franklin = Characters.Franklin.Create(new Vector3(1077.906f, -1969.792f, 31.01477f), 55, this.GetType().Name);
            Franklin.Inventory.GiveNewWeapon("WEAPON_ASSAULTRIFLE", 200, true);
            Franklin.Tasks.PlayAnimation("anim@amb@inspect@crouch@male_a@idles", "idle_a", 1f, AnimationFlags.Loop);
            Franklin.IsVisible = false;
            Franklin.RelationshipGroup = RelationshipGroup.Gang1;

            Trevor = Characters.Trevor.Create(new Vector3(1088.384f, -2029.506f, 36.87004f), 130, this.GetType().Name);
            Trevor.Inventory.GiveNewWeapon("WEAPON_COMBATMG", 200, true);
            Trevor.Tasks.PlayAnimation("anim@amb@inspect@crouch@male_a@idles", "idle_a", 1f, AnimationFlags.Loop);
            Trevor.IsVisible = false;
            Trevor.RelationshipGroup = RelationshipGroup.Gang1;

            Lamar = Characters.Lamar.Create(new Vector3(1098.14f, -1970.964f, 31.01447f), 76, this.GetType().Name);
            Lamar.Inventory.GiveNewWeapon("WEAPON_ASSAULTRIFLE", 200, true);
            Lamar.Tasks.PlayAnimation("anim@amb@inspect@crouch@male_a@idles", "idle_a", 1f, AnimationFlags.Loop);
            Lamar.RelationshipGroup = RelationshipGroup.Gang1;

            FranklinCar = Vehicles.FranklinCar.Create(new Vector3(1080.096f, -1966.187f, 30.64678f), 294);

            Crate = new Object("prop_box_wood03a", new Vector3(1077f, -1969.3f, 30f), -20f)
            {
                IsPositionFrozen = true
            };

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Intrusion in the foundry"
            };

            NearSpawnMessageSent = false;
            PlayerNear = false;
            PursuitCreated = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 150)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message");

                CalloutInterfaceAPI.Functions.SendMessage(this, "Three armed suspects were reported to have broken into the foundry");
                NearSpawnMessageSent = true;
            }

            if (!PlayerNear && Game.LocalPlayer.Character.DistanceTo2D(new Vector3(1094.582f, -1996.949f, 20f)) < 50)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Checking if interior loaded");
                // TODO : Check is the interior is accessible
                bool FoundryInteriorReady = false;
                if (Settings.forceInteriorsEnabled || FoundryInteriorReady)
                {
                    Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Interior available: moving ped inside");

                    Michael.Position = new Vector3(1112.39f, -2003.71f, 35.44f);
                    Michael.Heading = 72f;
                    Franklin.Position = new Vector3(1113.32f, -2016.396f, 35.46208f);
                    Franklin.Heading = 56f;
                    Trevor.Position = new Vector3(1080.78f, -1980.85f, 34.63f);
                    Trevor.Heading = 234f;
                } else
                {
                    Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting shootout outside");

                    ShootoutFiber = GameFiber.StartNew(ShootoutLogic);

                    Pursuit = Functions.CreatePursuit();
                    Functions.AddPedToPursuit(Pursuit, Michael);
                    Functions.SetPursuitDisableAIForPed(Michael, true);
                    Functions.AddPedToPursuit(Pursuit, Franklin);
                    Functions.SetPursuitDisableAIForPed(Franklin, true);
                    Functions.AddPedToPursuit(Pursuit, Trevor);
                    Functions.SetPursuitDisableAIForPed(Trevor, true);
                    Functions.AddPedToPursuit(Pursuit, Lamar);
                    Functions.SetPursuitDisableAIForPed(Lamar, true);
                    Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                    PursuitCreated = true;
                }

                Michael.IsVisible = true;
                Franklin.IsVisible = true;
                Trevor.IsVisible = true;

                EventBlip.Delete();

                PlayerNear = true;
            }

            if (PlayerNear && !PursuitCreated && Game.LocalPlayer.Character.DistanceTo(new Vector3(1080.024f, -1989.572f, 30.87349f)) < 15)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting shootout inside");

                ShootoutFiber = GameFiber.StartNew(ShootoutLogic);

                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Michael);
                Functions.SetPursuitDisableAIForPed(Michael, true);
                Functions.AddPedToPursuit(Pursuit, Franklin);
                Functions.SetPursuitDisableAIForPed(Franklin, true);
                Functions.AddPedToPursuit(Pursuit, Trevor);
                Functions.SetPursuitDisableAIForPed(Trevor, true);
                Functions.AddPedToPursuit(Pursuit, Lamar);
                Functions.SetPursuitDisableAIForPed(Lamar, true);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                PursuitCreated = true;
            }

            if (Game.IsKeyDown(Keys.End)
                || (PursuitCreated && !Functions.IsPursuitStillRunning(Pursuit)))
                End();
        }

        public override void End()
        {
            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Ending callout");

            base.End();

            if (ShootoutFiber != null && ShootoutFiber.IsAlive)
                ShootoutFiber.Abort();

            if (EventBlip.Exists())
                EventBlip.Delete();
            if (Michael.Exists())
                Michael.Dismiss();
            if (Franklin.Exists())
                Franklin.Dismiss();
            if (Trevor.Exists())
                Trevor.Dismiss();
            if (Lamar.Exists())
                Lamar.Dismiss();
            if (FranklinCar.Exists())
                FranklinCar.Dismiss();
            if (Crate.Exists())
                Crate.Dismiss();

            Game.LogTrivial($"[{Main.pluginName}] 'The Third Way' callout has ended.");
        }

        private void ShootoutLogic()
        {
            while (true)
            {
                if (Michael.IsAlive && !Functions.IsPedGettingArrested(Michael) && !Functions.IsPedArrested(Michael))
                {
                    Ped[] MichaelEnnemies = Main.GetNearbyEnnemies(Michael.Position);
                    Ped MichaelEnnemy = MichaelEnnemies.Length > 0 ? MichaelEnnemies[MathHelper.GetRandomInteger(MichaelEnnemies.Length)] : Game.LocalPlayer.Character;
                    Michael.Tasks.FightAgainst(MichaelEnnemy);
                }
                if (Franklin.IsAlive && !Functions.IsPedGettingArrested(Franklin) && !Functions.IsPedArrested(Franklin))
                {
                    Ped[] FranklinEnnemies = Main.GetNearbyEnnemies(Franklin.Position);
                    Ped FranklinEnnemy = FranklinEnnemies.Length > 0 ? FranklinEnnemies[MathHelper.GetRandomInteger(FranklinEnnemies.Length)] : Game.LocalPlayer.Character;
                    Franklin.Tasks.FightAgainst(FranklinEnnemy);
                }
                if (Trevor.IsAlive && !Functions.IsPedGettingArrested(Trevor) && !Functions.IsPedArrested(Trevor))
                {
                    Ped[] TrevorEnnemies = Main.GetNearbyEnnemies(Trevor.Position);
                    Ped TrevorEnnemy = TrevorEnnemies.Length > 0 ? TrevorEnnemies[MathHelper.GetRandomInteger(TrevorEnnemies.Length)] : Game.LocalPlayer.Character;
                    Trevor.Tasks.FightAgainst(TrevorEnnemy);
                }
                if (Lamar.IsAlive && !Functions.IsPedGettingArrested(Lamar) && !Functions.IsPedArrested(Lamar))
                {
                    if (Lamar.DistanceTo2D(new Vector3(1086.092f, -1968.122f, 31.01467f)) > 5)
                    {
                        Lamar.Tasks.FollowNavigationMeshToPosition(new Vector3(1086.092f, -1968.122f, 31.01467f), 120f, 2).WaitForCompletion(10000);
                    } else
                    {
                        Ped[] LamarEnnemies = Main.GetNearbyEnnemies(Lamar.Position);
                        Ped LamarEnnemy = LamarEnnemies.Length > 0 ? LamarEnnemies[MathHelper.GetRandomInteger(LamarEnnemies.Length)] : Game.LocalPlayer.Character;
                        Lamar.Tasks.FightAgainst(LamarEnnemy);
                    }
                }

                GameFiber.Yield();
                GameFiber.Wait(5000);
            }
        }
    }
}
