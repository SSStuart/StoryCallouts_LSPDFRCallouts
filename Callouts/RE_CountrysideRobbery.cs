using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Windows.Forms;

namespace StoryCallouts.Callouts
{
    [CalloutInterfaceAPI.CalloutInterface("Countryside Robbery", CalloutProbability.Medium, "Criminals resisting arrest", "Code 3")]

    internal class RE_CountrysideRobbery : Callout
    {
        private Vector3 SpawnPoint;
        private Blip EventBlip;
        private LHandle Pursuit;
        private Ped Criminal1, Criminal2, StoryCharacter, Cop1, Cop2;
        private Vehicle CriminalCar, CopCar;
        private Object Briefcase1, Briefcase2;
        private bool NearSpawnMessageSent, ChaseCreated, InitialCopsDead, CriminalsDead, PickedUpBriefcases, DroppedBriefcases;
        private int StoryCharacterVariant;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(324.9475f, 2636.903f, 44.56925f);
            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            AddMinimumDistanceCheck(200f, SpawnPoint);
            CalloutMessage = "Criminals resisting arrest";
            CalloutPosition = SpawnPoint;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_RESIST_ARREST IN_OR_ON_POSITION", SpawnPoint);

            StoryCharacterVariant = MathHelper.GetRandomInteger(3);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Criminal1 = new Ped("A_M_Y_GENSTREET_01", new Vector3(326.88f, 2624.54f, 44.50f), 49f)
            {
                BlockPermanentEvents = true,
                Health = 400,
            };
            Criminal1.Inventory.GiveNewWeapon("WEAPON_PISTOL", 100, true);

            Criminal2 = new Ped("A_M_Y_GENSTREET_01", new Vector3(328.9166f, 2626.354f, 44.49976f), 50f)
            {
                BlockPermanentEvents = true,
                Health = 400,
            };
            Criminal2.Inventory.GiveNewWeapon("WEAPON_PISTOL", 100, true);

            CriminalCar = new Vehicle("PHOENIX", new Vector3(327.205f, 2626.989f, 43.82396f), 34f);
            CriminalCar.GetDoors()[0].IsOpen = true;
            CriminalCar.GetDoors()[1].IsOpen = true;

            Briefcase1 = new Object("prop_security_case_01", new Vector3(328.51f, 2624.372f, 43.6f), MathHelper.GetRandomSingle(-180, 180));
            Briefcase2 = new Object("prop_security_case_01", new Vector3(328.5627f, 2627.276f, 43.6f), MathHelper.GetRandomSingle(-180, 180));

            StoryCharacter = StoryCharacterVariant switch
            {
                0 => Characters.Franklin.Create(new Vector3(317.0602f, 2613.423f, 44.47596f), 297, this.GetType().Name),
                1 => Characters.Michael.Create(new Vector3(317.0602f, 2613.423f, 44.47596f), 297, this.GetType().Name),
                _ => Characters.Trevor.Create(new Vector3(317.0602f, 2613.423f, 44.47596f), 297, this.GetType().Name),
            };

            CopCar = new Vehicle("sheriff", new Vector3(325.7083f, 2640.782f, 44.19189f), 157f)
            {
                IsSirenOn = true
            };

            Cop1 = new Ped("s_" + (MathHelper.GetRandomInteger(2) == 0 ? "m" : "f") + "_y_sheriff_01", new Vector3(326.1575f, 2643.858f, 44.59733f), 188f)
            {
                BlockPermanentEvents = true,
            };
            Cop1.Inventory.GiveNewWeapon("WEAPON_PUMPSHOTGUN", 50, true);

            Cop2 = new Ped("s_" + (MathHelper.GetRandomInteger(2) == 0 ? "m" : "f") + "_y_sheriff_01", new Vector3(326.1575f, 2643.858f, 44.59733f), 188f)
            {
                BlockPermanentEvents = true,
            };
            Cop2.Inventory.GiveNewWeapon("WEAPON_PISTOL", 100, true);

            EventBlip = new Blip(SpawnPoint)
            {
                Color = Main.calloutWaypointColor,
                IsRouteEnabled = true,
                Name = "Criminals resisting arrest"
            };

            NearSpawnMessageSent = false;
            ChaseCreated = false;
            InitialCopsDead = false;
            CriminalsDead = false;
            PickedUpBriefcases = false;
            DroppedBriefcases = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!NearSpawnMessageSent && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 200)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Sending CI message, and appying tasks");

                CalloutInterfaceAPI.Functions.SendMessage(this, "Two armed criminals refuse to surrender after a robbery");

                Criminal1.Tasks.AimWeaponAt(Cop1, -1);
                Criminal2.Tasks.AimWeaponAt(Cop2, -1);
                Cop1.Tasks.AimWeaponAt(Criminal1, -1);
                Cop2.Tasks.AimWeaponAt(Criminal2, -1);

                StoryCharacter.Tasks.PlayAnimation("move_crouch_proto", "idle", 1, AnimationFlags.Loop);

                NearSpawnMessageSent = true;
            }

            if (!ChaseCreated && Game.LocalPlayer.Character.DistanceTo2D(SpawnPoint) < 100)
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Starting shootout");

                EventBlip.Delete();
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Criminal1);
                Functions.AddPedToPursuit(Pursuit, Criminal2);
                Functions.SetPursuitDisableAIForPed(Criminal1, true);
                Functions.SetPursuitDisableAIForPed(Criminal2, true);
                Functions.AddCopToPursuit(Pursuit, Cop1);
                Functions.AddCopToPursuit(Pursuit, Cop2);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                Criminal1.Tasks.FireWeaponAt(Cop1, -1, FiringPattern.BurstFirePistol);
                Criminal2.Tasks.FireWeaponAt(Cop2, -1, FiringPattern.BurstFirePistol);

                ChaseCreated = true;
            }

            if (!InitialCopsDead &&
                (!Cop1.Exists() || (Cop1.Exists() && Cop1.IsDead)) &&
                (!Cop2.Exists() || (Cop2.Exists() && Cop2.IsDead)))
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Initials cops dead, enabling criminals AI");

                Functions.SetPursuitDisableAIForPed(Criminal1, false);
                Functions.SetPursuitDisableAIForPed(Criminal2, false);

                InitialCopsDead = true;
            }

            if (!InitialCopsDead && !CriminalsDead &&
                (!Criminal1.Exists() || Criminal1.IsDead || Functions.IsPedArrested(Criminal1)) &&
                (!Criminal2.Exists() || Criminal2.IsDead || Functions.IsPedArrested(Criminal2)) &&
                Briefcase1.Exists() && Briefcase2.Exists())
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Criminals dead/arrested, picking up briefcases");

                GameFiber.StartNew(delegate
                {
                    GameFiber.Wait(3000);

                    StoryCharacter.Tasks.GoToOffsetFromEntity(Briefcase1, 0.5f, 0f, 3f).WaitForCompletion();
                    Briefcase1.AttachTo(StoryCharacter, StoryCharacter.GetBoneIndex(PedBoneId.LeftHand), new Vector3(0.15f, 0, 0), new Rotator(40, -90, -10));
                    StoryCharacter.Tasks.GoToOffsetFromEntity(Briefcase2, 0.5f, 0f, 3f).WaitForCompletion();

                    Pursuit = Functions.CreatePursuit();
                    Functions.AddPedToPursuit(Pursuit, StoryCharacter);
                    Functions.SetPursuitDisableAIForPed(StoryCharacter, true);
                    Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                    Briefcase2.AttachTo(StoryCharacter, StoryCharacter.GetBoneIndex(PedBoneId.RightHand), new Vector3(0.1f, 0, 0), new Rotator(-30, -90, -10));
                    StoryCharacter.Tasks.Flee(Game.LocalPlayer.Character, 200, -1);

                    Functions.SetPursuitDisableAIForPed(StoryCharacter, false);

                    PickedUpBriefcases = true;
                });

                CriminalsDead = true;
            }

            if (PickedUpBriefcases && !DroppedBriefcases && (StoryCharacter.IsDead || Functions.IsPedGettingArrested(StoryCharacter) || StoryCharacter.IsRagdoll))
            {
                Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Dropping briefcases");


                if (Briefcase1.Exists())
                    Briefcase1.Detach();
                if (Briefcase2.Exists())
                    Briefcase2.Detach();

                DroppedBriefcases = true;
            }

            if (Game.IsKeyDown(Keys.End)
                || (ChaseCreated && !Functions.IsPursuitStillRunning(Pursuit) &&
                (!CriminalsDead || (CriminalsDead && PickedUpBriefcases))))
                End();
        }

        public override void End()
        {
            Game.LogTrivial($"[{Main.pluginName} - '{this.GetType().Name}'] Ending callout");

            base.End();

            if (EventBlip.Exists())
                EventBlip.Delete();
            if (Criminal1.Exists())
                Criminal1.Dismiss();
            if (Criminal2.Exists())
                Criminal2.Dismiss();
            if (CriminalCar.Exists())
                CriminalCar.Dismiss();
            if (StoryCharacter.Exists())
                StoryCharacter.Dismiss();
            if (Cop1.Exists())
                Cop1.Dismiss();
            if (Cop2.Exists())
                Cop2.Dismiss();
            if (CopCar.Exists())
                CopCar.Dismiss();
            if (Briefcase1.Exists())
                Briefcase1.Dismiss();
            if (Briefcase2.Exists())
                Briefcase2.Dismiss();

            Game.LogTrivial($"[{Main.pluginName}] 'Countryside Robbery' callout has ended.");
        }
    }
}
