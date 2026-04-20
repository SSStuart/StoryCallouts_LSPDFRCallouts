using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Linq;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("The Paleto Score", CalloutProbability.Medium, "Paleto Bay Blaine County bank robbery", "Code 3")]

    internal class ThePaletoScore : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Ped Franklin, Michael, Trevor, Gunman, Fence1Target, Fence2Target, Fence3Target;
        private TasksList MichaelTask, TrevorTask, GunmanTask, FranklinDriveTask, FactoryEscapeTask;
        private Vehicle Bulldozer;
        private bool NearSpawnMessageSent, ChaseCreated, FranklinDispatched, FranklinArrivedPickupPoint, FranklinEnd, EscapeThroughFactory;
        private bool MichaelInScoop, TrevorInScoop;
        private int GunmanVariant;
        private uint FightTaskTimeout;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(-118.5427f, 6455.054f, 31.38188f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            AddMinimumDistanceCheck(300f, SpawnPoint);
            CalloutMessage = "Paleto Bay Blaine County bank robbery";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_ROBBERY IN_OR_ON_POSITION", SpawnPoint);

            GunmanVariant = MathHelper.GetRandomInteger(4);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Michael = Characters.Michael.Create(new Vector3(-115.0931f, 6457.916f, 31.46846f), 135, this.GetType().Name);
            Michael.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_MINIGUN"), 7000, true);
            Michael.Armor = 200;
            Michael.CanAttackFriendlies = false;
            Michael.RelationshipGroup = RelationshipGroup.Gang1;
            Michael.CanRagdoll = false;
            MichaelTask = new TasksList(Michael, EndBehavior.Nothing);

            Trevor = Characters.Trevor.Create(new Vector3(-116.2796f, 6459.558f, 31.46846f), 135, this.GetType().Name);
            Trevor.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_MINIGUN"), 7000, true);
            Trevor.Armor = 200;
            Trevor.CanAttackFriendlies = false;
            Trevor.RelationshipGroup = RelationshipGroup.Gang1;
            Trevor.CanRagdoll = false;
            TrevorTask = new TasksList(Trevor, EndBehavior.Nothing);

            switch (GunmanVariant)
            {
                case 0:
                    Gunman = Characters.Gustavo.Create(new Vector3(-113.4816f, 6456.168f, 31.46846f), 135, this.GetType().Name);
                    break;
                case 1:
                    Gunman = Characters.Norm.Create(new Vector3(-113.4816f, 6456.168f, 31.46846f), 135, this.GetType().Name);
                    break;
                case 2:
                    Gunman = Characters.Daryl.Create(new Vector3(-113.4816f, 6456.168f, 31.46846f), 135, this.GetType().Name);
                    break;
                case 3:
                default:
                    Gunman = Characters.Chef.Create(new Vector3(-113.4816f, 6456.168f, 31.46846f), 135, this.GetType().Name);
                    break;
            }
            Gunman.Inventory.GiveNewWeapon(new WeaponAsset("WEAPON_MINIGUN"), 7000, true);
            Gunman.Armor = 200;
            Gunman.CanAttackFriendlies = false;
            Gunman.RelationshipGroup = RelationshipGroup.Gang1;
            Gunman.CanRagdoll = false;
            GunmanTask = new TasksList(Gunman, EndBehavior.Nothing);

            Fence1Target = new Ped(new Vector3(-177.8922f, 6407.372f, 30.5f))
            {
                IsInvincible = true,
                IsCollisionEnabled = false,
                BlockPermanentEvents = true,
                IsVisible = false,
                IsPositionFrozen = true,
            };
            Fence2Target = new Ped(new Vector3(-188.2498f, 6390.396f, 30.5f))
            {
                IsInvincible = true,
                IsCollisionEnabled = false,
                BlockPermanentEvents = true,
                IsVisible = false,
                IsPositionFrozen = true,
            };
            Fence3Target = new Ped(new Vector3(-182f, 6370f, 30f))
            {
                IsInvincible = true,
                IsCollisionEnabled = false,
                BlockPermanentEvents = true,
                IsVisible = false,
                IsPositionFrozen = true,
            };

            foreach (TasksList EscapeTask in new[] { MichaelTask, TrevorTask, GunmanTask }) {
                EscapeTask.AddWalkAimingRandomEnemyTask(new Vector3(-140.9637f, 6442.315f, 31.35501f).Around2D(2), 1, 1, FiringPattern.BurstFireMG, 180);
                EscapeTask.AddWalkAimingRandomEnemyTask(new Vector3(-146.308f, 6436.221f, 31.4569f), 0.5f, 1, FiringPattern.BurstFireMG, 180);
                EscapeTask.AddWalkTask(new Vector3(-156.9891f, 6426.387f, 31.9159f).Around2D(1), 2, 2, false, true, 130, 60 * 4);
                EscapeTask.AddWalkAimingTask(new Vector3(-171.529f, 6414.329f, 31.93072f), Fence1Target, 4, 2, FiringPattern.BurstFireMG, 60 * 4);
                EscapeTask.AddWalkAimingTask(new Vector3(-183.3644f, 6396.686f, 31.87686f), Fence2Target, 4, 2, FiringPattern.BurstFireMG, 60 * 4);
                EscapeTask.AddWalkTask(new Vector3(-190.1321f, 6387.051f, 31.8625f), 1, 2, false, false, 154, 60 * 4);
                EscapeTask.AddWalkAimingTask(new Vector3(-187.1079f, 6372.472f, 31.33536f), Fence3Target, 2, 2, FiringPattern.FullAutomatic, 60 * 4);
                EscapeTask.AddWalkAimingRandomEnemyTask(new Vector3(-166.7034f, 6355.247f, 31.44296f).Around2D(4), 2, 2, FiringPattern.BurstFireMG, 60 * 4);
                EscapeTask.AddWalkAimingRandomEnemyTask(new Vector3(-142.8626f, 6320.144f, 31.42774f).Around2D(1), 2, 2, FiringPattern.BurstFireMG, 60 * 4);
                EscapeTask.AddWalkTask(new Vector3(-164.6296f, 6296.707f, 31.50465f).Around2D(2), 2, 2, false, false, 140, 60 * 4);
                EscapeTask.AddWalkTask(new Vector3(-169.7776f, 6291.647f, 31.48937f).Around2D(3), 1, 2, false, false, 110);
            }

            Bulldozer = new Vehicle("BULLDOZER", new Vector3(-103.9565f, 6394.27f, 31.05603f), 45f);

            Franklin = Characters.Franklin.Create(Bulldozer.GetOffsetPositionFront(-3), 0, this.GetType().Name);
            Franklin.WarpIntoVehicle(Bulldozer, -1);

            FranklinDriveTask = new TasksList(Franklin, EndBehavior.Fight);
            FranklinDriveTask.AddDriveTask(new Vector3(-120.8171f, 6410.202f, 30.9108f), 30, 3, VehicleDrivingFlags.IgnorePathFinding);
            FranklinDriveTask.AddDriveTask(new Vector3(-212.5374f, 6322.157f, 31.13777f), 30, 3, VehicleDrivingFlags.Emergency, 60 * 5);
            FranklinDriveTask.AddDriveTask(new Vector3(-182.7301f, 6287.158f, 31.11247f), 30, 1, VehicleDrivingFlags.IgnorePathFinding);
            FranklinDriveTask.AddDriveTask(new Vector3(-175.9508f, 6282.745f, 30.96562f), 0, 2, VehicleDrivingFlags.IgnorePathFinding, 8);  // 8 seconds pause
            FranklinDriveTask.AddDriveTask(new Vector3(-137.8681f, 6246.106f, 30.80941f), 30, 3, VehicleDrivingFlags.IgnorePathFinding);
            FranklinDriveTask.AddDriveTask(new Vector3(-92.25348f, 6272.836f, 30.97716f), 30, 4, VehicleDrivingFlags.IgnorePathFinding);
            FranklinDriveTask.AddDriveTask(new Vector3(-39.71321f, 6282.341f, 30.84587f), 30, 2, VehicleDrivingFlags.IgnorePathFinding);
            FranklinDriveTask.AddDriveTask(new Vector3(-16.5505f, 6256.933f, 30.90694f), 30, 5, VehicleDrivingFlags.Emergency);

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Paleto Bay Blaine County bank robbery"
            };

            NearSpawnMessageSent = false;
            ChaseCreated = false;
            FranklinDispatched = false;
            FranklinArrivedPickupPoint = false;
            FranklinEnd = false;
            EscapeThroughFactory = false;
            FightTaskTimeout = 0;

            MichaelInScoop = false;
            TrevorInScoop = false;

            FactoryEscapeTask = new TasksList(Franklin);
            FactoryEscapeTask.AddWalkTask(new Vector3(-69.74802f, 6262.051f, 31.09014f), 2, 1, true);
            FactoryEscapeTask.AddWalkTask(new Vector3(-74.21358f, 6253.986f, 31.08989f), 2, 1, true);
            FactoryEscapeTask.AddWalkTask(new Vector3(-67.83837f, 6241.721f, 31.08146f), 2, 1, true);
            FactoryEscapeTask.AddWalkTask(new Vector3(-80.81792f, 6232.855f, 31.09248f), 2, 1, true);
            FactoryEscapeTask.AddWalkTask(new Vector3(-74.85892f, 6222.317f, 31.08985f), 2, 1, true);
            FactoryEscapeTask.AddWalkTask(new Vector3(-89.0691f, 6213.109f, 31.04994f), 2, 2, true);
            FactoryEscapeTask.AddWalkTask(new Vector3(-98.94785f, 6210.548f, 31.02502f), 2, 1, true);
            FactoryEscapeTask.AddWalkTask(new Vector3(-104.6416f, 6201.765f, 31.0257f), 2, 1, true);
            FactoryEscapeTask.AddWalkTask(new Vector3(-99.51251f, 6195.361f, 30.99146f), 2, 2, true);
            FactoryEscapeTask.AddWalkTask(new Vector3(-137.6864f, 6156.805f, 31.21545f), 2, 2, true);
            FactoryEscapeTask.AddWalkTask(new Vector3(-146.6389f, 6160.124f, 31.20619f), 2, 2, true);
            FactoryEscapeTask.AddWalkTask(new Vector3(-154.1554f, 6151.401f, 31.20634f), 2, 3, true);

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
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting escape");

                Entity[] closeEntities = World.GetEntities(new Vector3(-118.5799f, 6454.789f, 30.92607f), 20, GetEntitiesFlags.ConsiderAllVehicles | GetEntitiesFlags.ConsiderAllPeds);

                foreach (Entity entity in closeEntities)
                {
                    if (entity != Michael && entity != Trevor && entity != Gunman)
                        entity.Delete();
                }

                EventBlip.Delete();
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Michael);
                Functions.AddPedToPursuit(Pursuit, Trevor);
                Functions.AddPedToPursuit(Pursuit, Gunman);
                Functions.SetPursuitDisableAIForPed(Michael, true);
                Functions.SetPursuitDisableAIForPed(Trevor, true);
                Functions.SetPursuitDisableAIForPed(Gunman, true);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                MichaelTask.StartTasks();
                TrevorTask.StartTasks();
                GunmanTask.StartTasks();

                Vehicle NooseHeli = Functions.RequestBackup(SpawnPoint, LSPD_First_Response.EBackupResponseType.Code2, LSPD_First_Response.EBackupUnitType.NooseAirUnit);

                ChaseCreated = true;
            }

            if (Fence1Target.Exists())
                Fence1Target.Position = new Vector3(-177 + (float)Math.Sin(Game.GameTime / 1000), Fence1Target.Position.Y, Fence1Target.Position.Z);
            if (Fence2Target.Exists())
                Fence2Target.Position = new Vector3(-188 + (float)Math.Sin(Game.GameTime / 1000), Fence2Target.Position.Y, Fence2Target.Position.Z);
            if (Fence3Target.Exists())
                Fence3Target.Position = new Vector3(Fence3Target.Position.X, 6369 + (float)Math.Sin(Game.GameTime / 1000), Fence3Target.Position.Z);

            if (!FranklinDispatched && Michael.Exists() && Michael.DistanceTo(new Vector3(-172.3499f, 6290.975f, 30.97311f)) < 40)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting Franklin tasks");

                FranklinDriveTask.StartTasks();

                Michael.CanRagdoll = true;
                Trevor.CanRagdoll = true;
                Gunman.CanRagdoll = true;

                FranklinDispatched = true;
            }

            if (FranklinDispatched && Game.GameTime > FightTaskTimeout + 3000)
            {
                Ped[] nearPeds = World.GetEntities(new Vector3(-176.261f, 6286.955f, 30.96572f), 40, GetEntitiesFlags.ConsiderHumanPeds).OfType<Ped>().Where(ped => Functions.IsPedACop(ped) || ped.IsLocalPlayer).ToArray();

                FightTaskTimeout = Game.GameTime;
                if (nearPeds.Length > 0)
                {
                    Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Making callout suspects shoot while waiting Franklin");
                    if (!FranklinArrivedPickupPoint && Michael.Exists() && Michael.IsAlive && MichaelTask.TaskFinished)
                        Michael.Tasks.FightAgainst(nearPeds[MathHelper.GetRandomInteger(nearPeds.Length)]);
                    if (!FranklinArrivedPickupPoint && Trevor.Exists() && Trevor.IsAlive && TrevorTask.TaskFinished)
                        Trevor.Tasks.FightAgainst(nearPeds[MathHelper.GetRandomInteger(nearPeds.Length)]);
                    if (Gunman.Exists() && Gunman.IsAlive && GunmanTask.TaskFinished)
                        Gunman.Tasks.FightAgainst(nearPeds[MathHelper.GetRandomInteger(nearPeds.Length)]);
                }
            }

            if (FranklinDispatched && !FranklinArrivedPickupPoint && Franklin.DistanceTo2D(new Vector3(-182.7301f, 6287.158f, 31.11247f)) < 5)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Making Michael and Trevor enter the bulldoze scoop");
             
                FranklinArrivedPickupPoint = true;
                
                Functions.AddPedToPursuit(Pursuit, Franklin);
                Functions.SetPursuitDisableAIForPed(Franklin, true);

                GameFiber taskFiber = GameFiber.StartNew(delegate
                {
                    if (Michael.Exists() && Michael.IsAlive && MichaelTask.TaskFinished && !Functions.IsPedArrested(Michael) && !Functions.IsPedGettingArrested(Michael))
                    {
                        MichaelInScoop = true;
                        Michael.Tasks.FollowNavigationMeshToPosition(Bulldozer.GetOffsetPositionFront(6), Bulldozer.Heading - 180, 2);
                    }
                    if (Trevor.Exists() && Trevor.IsAlive && TrevorTask.TaskFinished && !Functions.IsPedArrested(Trevor) && !Functions.IsPedGettingArrested(Trevor))
                    {
                        TrevorInScoop = true;
                        Trevor.Tasks.FollowNavigationMeshToPosition(Bulldozer.GetOffsetPositionFront(6), Bulldozer.Heading - 180, 2).WaitForCompletion(20000);
                    }
                });

                do
                {
                    GameFiber.Wait(500);
                    GameFiber.Yield();
                } while (taskFiber.IsAlive);

                if (MichaelInScoop)
                {
                    Michael.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_wash@male@low@idle_a"), "idle_a", 1, AnimationFlags.Loop);
                    Michael.AttachTo(Bulldozer, Bulldozer.GetBoneIndex("seat_dside_r"), new Vector3(0, 0, 0.8f), new Rotator());
                }
                if (TrevorInScoop)
                {
                    Trevor.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_wash@male@low@idle_a"), "idle_a", 1, AnimationFlags.Loop);
                    Trevor.AttachTo(Bulldozer, Bulldozer.GetBoneIndex("seat_pside_r"), new Vector3(0, 0, 0.8f), new Rotator());
                }
            }

            if (MichaelInScoop && Functions.IsPedGettingArrested(Michael))
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Michael getting arrested while in sccop, detaching");

                DetachPedFromScoop(Michael);
            }
            if (TrevorInScoop && Functions.IsPedGettingArrested(Trevor))
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Trevor getting arrested while in sccop, detaching");

                DetachPedFromScoop(Trevor);
            }

            if ((Settings.forceInteriorsEnabled || Rage.Native.NativeFunction.Natives.IS_IPL_ACTIVE<bool>("cs1_02_cf_onmission1")) && FranklinArrivedPickupPoint && Franklin.DistanceTo(new Vector3(-75.54154f, 6271.578f, 31.05142f)) < 5 && !EscapeThroughFactory)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Interior detected - Making everyone exit the bulldozer and start escaping through factory");

                FranklinDriveTask.AbortTasks();

                if (Franklin.IsInVehicle(Bulldozer, false))
                    Franklin.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(5000);

                if (MichaelInScoop)
                {
                    DetachPedFromScoop(Michael, true);
                    Michael.Tasks.FollowToOffsetFromEntity(Franklin, new Vector3(0, -1.5f, 0));
                }
                if (TrevorInScoop)
                {
                    DetachPedFromScoop(Trevor, true);
                    Trevor.Tasks.FollowToOffsetFromEntity(Franklin, new Vector3(0, -3, 0));
                }
                FactoryEscapeTask.StartTasks();

                EscapeThroughFactory = true;
            }

            if (EscapeThroughFactory && FactoryEscapeTask.TaskFinished && !FranklinEnd)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Franklin reached inside factory - re-enabling ped AI");
                if (Michael.Exists() && Michael.IsAlive)
                {
                    Michael.Tasks.ClearImmediately();
                    Functions.SetPursuitDisableAIForPed(Michael, false);
                }

                if (Trevor.Exists() && Trevor.IsAlive)
                {
                    Trevor.Tasks.ClearImmediately();
                    Functions.SetPursuitDisableAIForPed(Trevor, false);
                }

                FranklinEnd = true;
            }

            if (FranklinArrivedPickupPoint && FranklinDriveTask.TaskFinished && !EscapeThroughFactory && !FranklinEnd)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Making everyone exit the bulldozer - scripted chase end");

                if (MichaelInScoop)
                {
                    DetachPedFromScoop(Michael);
                    Michael.Tasks.FollowToOffsetFromEntity(Franklin, new Vector3(0, -1.5f, 0));
                }
                if (TrevorInScoop)
                {
                    DetachPedFromScoop(Trevor);
                    Trevor.Tasks.FollowToOffsetFromEntity(Franklin, new Vector3(0, -3, 0));
                }

                FranklinEnd = true;
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
            if (Trevor.Exists())
                Trevor.Dismiss();
            if (Gunman.Exists())
                Gunman.Dismiss();
            if (Franklin.Exists())
            {
                if (Bulldozer.Exists() && Franklin.IsInVehicle(Bulldozer, false))
                    Franklin.Tasks.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion(5000);
                Franklin.Dismiss();
            }
            if (Bulldozer.Exists())
                Bulldozer.Dismiss();
            if (Fence1Target.Exists())
                Fence1Target.Dismiss();
            if (Fence2Target.Exists())
                Fence2Target.Dismiss();
            if (Fence3Target.Exists())
                Fence3Target.Dismiss();

            Game.LogTrivial($"[{Main.pluginName}] 'The Paleto Score' callout has ended.");
        }

        private void DetachPedFromScoop(Ped ped, bool disabledIA = false)
        {
            ped.Detach();
            ped.Position = Bulldozer.GetOffsetPositionFront(4);
            ped.Tasks.ClearImmediately();
            Functions.SetPursuitDisableAIForPed(ped, disabledIA);

            if (ped == Michael)
                    MichaelInScoop = false;
            if (ped == Trevor)
                TrevorInScoop = false;
        }
    }
}
