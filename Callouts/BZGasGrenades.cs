using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Drawing;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("BZ Gas Grenades", CalloutProbability.Medium, "Humane van under attack", "Code 3")]

    internal class BZGasGrenades : Callout
    {
        private Vector3 SpawnPoint;
        private int SpawnHeading;
        private Blip EventBlip, HumaneVanBlip;
        private LHandle Pursuit;
        private Ped HumaneDriver, Michael;
        private Vehicle HumaneVan, MichaelCar;
        private Object Gaz;
        private GameFiber GrenadeLogicFiber;
        private bool NearSpawnMessageSent, ChaseCreated;

        public override bool OnBeforeCalloutDisplayed()
        {
            Vector3[] spawnpoints = {
                new Vector3(1128.603f, 429.0241f, 83.12436f),
                new Vector3(792.4785f, 37.626f, 65.15231f),
                new Vector3(519.5471f, -400.9573f, 31.5487f),
                new Vector3(320.9503f, -742.6863f, 29.07636f),
                new Vector3(210.414f, -1184.573f, 29.08143f),
                new Vector3(26.91578f, -1536.99f, 29.05922f),
                new Vector3(-465.5196f, -1874.583f, 17.90938f),
                new Vector3(-791.9401f, -2209.744f, 16.62965f),
            };
            int[] spawnHeadings =
            {
                134,
                144,
                143,
                160,
                185,
                139,
                127,
                132
            };

            int spawnVariation = MathHelper.GetRandomInteger(8);

            SpawnPoint = spawnpoints[spawnVariation];
            SpawnHeading = spawnHeadings[spawnVariation];
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            AddMinimumDistanceCheck(200f, SpawnPoint);
            CalloutMessage = "Humane van under attack";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_GRAND_THEFT_AUTO IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            HumaneDriver = new Ped("S_M_M_ARMOURED_01", SpawnPoint.Around2D(3), 0)
            {
                Health = 400,
            };

            HumaneVan = new Vehicle("boxville3", SpawnPoint, SpawnHeading)
            {
                IsEngineOn = true,
            };
            HumaneDriver.WarpIntoVehicle(HumaneVan, -1);

            HumaneVanBlip = new Blip(HumaneVan)
            {
                Color = Color.LightBlue,
                Name = "Humane van"
            };

            Gaz = new Object("prop_idol_case_02", HumaneVan.GetOffsetPosition(new Vector3(0, -2, 0)));
            Gaz.AttachTo(HumaneVan, 0, new Vector3(0, -2, 0), new Rotator());

            Michael = Characters.Michael.Create(HumaneVan.GetOffsetPositionFront(-10), 0, this.GetType().Name);
            Michael.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_MICROSMG"), 100, true);

            MichaelCar = Vehicles.MichaelCar.CreateWithDriver(HumaneVan.GetOffsetPositionFront(-15), SpawnHeading, Michael);
            MichaelCar.IsEngineOn = true;

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Humane van under attack"
            };

            ChaseCreated = false;
            NearSpawnMessageSent = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 250)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message");

                CalloutInterfaceAPI.Functions.SendMessage(this, "A Humane van driver reported supicious car following");
                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 200)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting chase");

                HumaneDriver.Tasks.CruiseWithVehicle(60, VehicleDrivingFlags.Emergency);

                Michael.Tasks.ChaseWithGroundVehicle(HumaneDriver);

                EventBlip.Delete();
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Michael);
                Functions.SetPursuitDisableAIForPed(Michael, true);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                GrenadeLogicFiber = GameFiber.StartNew(GrenadesLogic);

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

            if (GrenadeLogicFiber.IsAlive)
                GrenadeLogicFiber.Abort();

            if (EventBlip.Exists())
                EventBlip.Delete();
            if (Michael.Exists())
                Michael.Dismiss();
            if (MichaelCar.Exists())
                MichaelCar.Dismiss();
            if (HumaneDriver.Exists())
                HumaneDriver.Dismiss();
            if (HumaneVanBlip.Exists())
                HumaneVanBlip.Delete();
            if (HumaneVan.Exists())
                HumaneVan.Dismiss();
            if (Gaz.Exists())
            {
                if (Gaz.DistanceTo(HumaneVan) < 2)
                    Gaz.Delete();
                else
                {
                    Gaz.Detach();
                    Gaz.Dismiss();
                }
            }

            Game.LogTrivial($"[{Main.pluginName}] 'BZ Gas Grenades' callout has ended.");
        }

        private void GrenadesLogic()
        {
            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting Gaz logic");

            GameFiber.StartNew(delegate
            {
                while (Michael.Exists() && !Functions.IsPedGettingArrested(Michael) && !Functions.IsPedArrested(Michael))
                {
                    GameFiber.Sleep(100);
                }

                GrenadeLogicFiber.Abort();
            });

            do
            {
                GameFiber.Yield();
                GameFiber.Wait(1000);

                if (!HumaneVan.Exists() || !HumaneVan.HasDriver)
                    return;
            } while (HumaneVan.Health > 600);

            if (!HumaneVan.Exists() || !Gaz.Exists())
                return;

            if (HumaneVanBlip.Exists())
                HumaneVanBlip.Delete();

            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Opening van door, and tasking Micheal to get gaz");

            Gaz.Detach();
            HumaneVan.GetDoors()[2].Open(false);
            HumaneVan.GetDoors()[3].Open(false);
            Michael.Tasks.DriveToPosition(HumaneVan.GetOffsetPositionFront(-5), 40, VehicleDrivingFlags.AllowWrongWay | VehicleDrivingFlags.AllowMedianCrossing).WaitForCompletion();
            GameFiber.Yield();
            GameFiber.Wait(2000);
            Michael.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(4000);
            if (!Gaz.Exists())
                return;
            Michael.Tasks.GoToOffsetFromEntity(Gaz, 0, 0, 3).WaitForCompletion(15000);
            if (!Gaz.Exists())
                return;
            Gaz.AttachTo(Michael, Michael.GetBoneIndex(PedBoneId.RightHand), new Vector3(0.1f, 0, 0), new Rotator(180, 30, 90));
            if (!MichaelCar.Exists())
                return;
            Michael.Tasks.EnterVehicle(MichaelCar, -1, 3f).WaitForCompletion(15000);
            if (Gaz.Exists())
                Gaz.Delete();
            Functions.SetPursuitDisableAIForPed(Michael, false);
        }
    }
}
