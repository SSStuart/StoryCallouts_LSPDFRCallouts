using Rage;

namespace StoryCallouts
{
    internal static class Settings
    {
        internal static bool useStoryModeCharacters = true;

        internal static bool callout_Prologue = false;
        internal static bool callout_FranklinAndLamar = true;
        internal static bool callout_TheLongStretch = true;
        internal static bool callout_CarbineRifles = true;
        internal static bool callout_BugstarsEquipment = true;
        internal static bool callout_BZGasGrenades = true;
        internal static bool callout_TheJewelStoreJob = true;
        internal static bool callout_DeadManWalking = true;
        internal static bool callout_HoodSafari = true;
        internal static bool callout_TheHotelAssassination = true;
        internal static bool callout_GrassRoots_ThePickup = true;
        internal static bool callout_BlitzPlay = true;
        internal static bool callout_MilitaryHardware = true;
        internal static bool callout_ThePaletoScore = true;
        internal static bool callout_PackMan = true;
        internal static bool callout_LegalTrouble = true;
        internal static bool callout_TheBureauRaid = true;
        internal static bool callout_Driller = true;
        internal static bool callout_TheBigScore = true;
        internal static bool callout_TheThirdWay = true;
        internal static bool callout_Paparazzo_TheMeltdown = true;
        internal static bool callout_RE_GetawayDriver = true;
        internal static bool callout_RE_CountrysideRobbery = true;
        internal static bool callout_RE_CountrysideGangFight = true;

        internal static bool forceInteriorsEnabled = false;

        internal static string path = "Plugins/LSPDFR/StoryCallouts.ini";
        internal static InitializationFile ini = new InitializationFile(path);

        internal static int LoadSettings()
        {
            InitializeSettings();

            int enabledCallouts = 0;

            Game.LogTrivial($"[{Main.pluginName}] Loading config file:");

            ini.Create();
            useStoryModeCharacters = ini.ReadBoolean("General", "useStoryModeCharacters", true);
            Game.LogTrivial($"- Use story mode characters: {(useStoryModeCharacters ? "Yes" : "No")}");

            callout_Prologue = ini.ReadBoolean("Callouts", "Prologue", true);
            Game.LogTrivial($"- Callout 'Prologue' {(callout_Prologue ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_Prologue ? 1 : 0;

            callout_FranklinAndLamar = ini.ReadBoolean("Callouts", "FranklinAndLamar", true);
            Game.LogTrivial($"- Callout 'FranklinAndLamar' {(callout_FranklinAndLamar ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_FranklinAndLamar ? 1 : 0;

            callout_TheLongStretch = ini.ReadBoolean("Callouts", "TheLongStretch", true);
            Game.LogTrivial($"- Callout 'TheLongStretch' {(callout_TheLongStretch ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_TheLongStretch ? 1 : 0;

            callout_CarbineRifles = ini.ReadBoolean("Callouts", "CarbineRifles", true);
            Game.LogTrivial($"- Callout 'CarbineRifles' {(callout_CarbineRifles ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_CarbineRifles ? 1 : 0;

            callout_BugstarsEquipment = ini.ReadBoolean("Callouts", "BugstarsEquipment", true);
            Game.LogTrivial($"- Callout 'BugstarsEquipment' {(callout_BugstarsEquipment ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_BugstarsEquipment ? 1 : 0;

            callout_BZGasGrenades = ini.ReadBoolean("Callouts", "BZGasGrenades", true);
            Game.LogTrivial($"- Callout 'BZGasGrenades' {(callout_BZGasGrenades ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_BZGasGrenades ? 1 : 0;

            callout_TheJewelStoreJob = ini.ReadBoolean("Callouts", "TheJewelStoreJob", true);
            Game.LogTrivial($"- Callout 'TheJewelStoreJob' {(callout_TheJewelStoreJob ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_TheJewelStoreJob ? 1 : 0;

            callout_DeadManWalking = ini.ReadBoolean("Callouts", "DeadManWalking", true);
            Game.LogTrivial($"- Callout 'DeadManWalking' {(callout_DeadManWalking ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_DeadManWalking ? 1 : 0;

            callout_HoodSafari = ini.ReadBoolean("Callouts", "HoodSafari", true);
            Game.LogTrivial($"- Callout 'HoodSafari' {(callout_HoodSafari ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_HoodSafari ? 1 : 0;

            callout_TheHotelAssassination = ini.ReadBoolean("Callouts", "TheHotelAssassination", true);
            Game.LogTrivial($"- Callout 'TheHotelAssassination' {(callout_TheHotelAssassination ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_TheHotelAssassination ? 1 : 0;

            callout_GrassRoots_ThePickup = ini.ReadBoolean("Callouts", "GrassRoots_ThePickup", true);
            Game.LogTrivial($"- Callout 'GrassRoots_ThePickup' {(callout_GrassRoots_ThePickup ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_GrassRoots_ThePickup ? 1 : 0;

            callout_BlitzPlay = ini.ReadBoolean("Callouts", "BlitzPlay", true);
            Game.LogTrivial($"- Callout 'BlitzPlay' {(callout_BlitzPlay ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_BlitzPlay ? 1 : 0;

            callout_MilitaryHardware = ini.ReadBoolean("Callouts", "MilitaryHardware", true);
            Game.LogTrivial($"- Callout 'MilitaryHardware' {(callout_MilitaryHardware ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_MilitaryHardware ? 1 : 0;

            callout_ThePaletoScore = ini.ReadBoolean("Callouts", "ThePaletoScore", true);
            Game.LogTrivial($"- Callout 'ThePaletoScore' {(callout_ThePaletoScore ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_ThePaletoScore ? 1 : 0;

            callout_PackMan = ini.ReadBoolean("Callouts", "PackMan", true);
            Game.LogTrivial($"- Callout 'PackMan' {(callout_PackMan ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_PackMan ? 1 : 0;

            callout_LegalTrouble = ini.ReadBoolean("Callouts", "LegalTrouble", true);
            Game.LogTrivial($"- Callout 'LegalTrouble' {(callout_LegalTrouble ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_LegalTrouble ? 1 : 0;

            callout_TheBureauRaid = ini.ReadBoolean("Callouts", "TheBureauRaid", true);
            Game.LogTrivial($"- Callout 'TheBureauRaid' {(callout_TheBureauRaid ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_TheBureauRaid ? 1 : 0;

            callout_Driller = ini.ReadBoolean("Callouts", "Driller", true);
            Game.LogTrivial($"- Callout 'Driller' {(callout_Driller ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_Driller ? 1 : 0;

            callout_TheBigScore = ini.ReadBoolean("Callouts", "TheBigScore", true);
            Game.LogTrivial($"- Callout 'TheBigScore' {(callout_TheBigScore ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_TheBigScore ? 1 : 0;

            callout_TheThirdWay = ini.ReadBoolean("Callouts", "TheThirdWay", true);
            Game.LogTrivial($"- Callout 'TheThirdWay' {(callout_TheThirdWay ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_TheThirdWay ? 1 : 0;

            callout_Paparazzo_TheMeltdown = ini.ReadBoolean("Callouts", "Paparazzo_TheMeltdown", true);
            Game.LogTrivial($"- Callout 'Paparazzo_TheMeltdown' {(callout_Paparazzo_TheMeltdown ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_Paparazzo_TheMeltdown ? 1 : 0;

            callout_RE_GetawayDriver = ini.ReadBoolean("Callouts", "RandomEvent_GetawayDriver", true);
            Game.LogTrivial($"- Callout 'RandomEvent_GetawayDriver' {(callout_RE_GetawayDriver ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_RE_GetawayDriver ? 1 : 0;

            callout_RE_CountrysideRobbery = ini.ReadBoolean("Callouts", "RandomEvent_CountrysideRobbery", true);
            Game.LogTrivial($"- Callout 'RandomEvent_CountrysideRobbery' {(callout_RE_CountrysideRobbery ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_RE_CountrysideRobbery ? 1 : 0;

            callout_RE_CountrysideGangFight = ini.ReadBoolean("Callouts", "RandomEvent_CountrysideGangFight", true);
            Game.LogTrivial($"- Callout 'RandomEvent_CountrysideGangFight' {(callout_RE_CountrysideGangFight ? "[x] enabled" : "[ ] disabled")}");
            enabledCallouts += callout_RE_CountrysideGangFight ? 1 : 0;

            forceInteriorsEnabled = ini.ReadBoolean("Interiors", "forceInteriorsEnabled", false);
            Game.LogTrivial($"- Force enable interiors : {(forceInteriorsEnabled ? "Yes" : "No")}");

            Game.LogTrivial($"[{Main.pluginName}] Plugin settings loaded.");

            return enabledCallouts;
        }

        internal static void InitializeSettings()
        {
            if (!ini.DoesKeyExist("General", "useStoryModeCharacters"))
                ini.Write("General", "useStoryModeCharacters", true);

            if (!ini.DoesKeyExist("Callouts", "Prologue"))
                ini.Write("Callouts", "Prologue", false);
            if (!ini.DoesKeyExist("Callouts", "FranklinAndLamar"))
                ini.Write("Callouts", "FranklinAndLamar", true);
            if (!ini.DoesKeyExist("Callouts", "TheLongStretch"))
                ini.Write("Callouts", "TheLongStretch", true);
            if (!ini.DoesKeyExist("Callouts", "CarbineRifles"))
                ini.Write("Callouts", "CarbineRifles", true);
            if (!ini.DoesKeyExist("Callouts", "BugstarsEquipment"))
                ini.Write("Callouts", "BugstarsEquipment", true);
            if (!ini.DoesKeyExist("Callouts", "BZGasGrenades"))
                ini.Write("Callouts", "BZGasGrenades", true);
            if (!ini.DoesKeyExist("Callouts", "TheJewelStoreJob"))
                ini.Write("Callouts", "TheJewelStoreJob", true);
            if (!ini.DoesKeyExist("Callouts", "DeadManWalking"))
                ini.Write("Callouts", "DeadManWalking", true);
            if (!ini.DoesKeyExist("Callouts", "HoodSafari"))
                ini.Write("Callouts", "HoodSafari", true);
            if (!ini.DoesKeyExist("Callouts", "TheHotelAssassination"))
                ini.Write("Callouts", "TheHotelAssassination", true);
            if (!ini.DoesKeyExist("Callouts", "GrassRoots_ThePickup"))
                ini.Write("Callouts", "GrassRoots_ThePickup", true);
            if (!ini.DoesKeyExist("Callouts", "BlitzPlay"))
                ini.Write("Callouts", "BlitzPlay", true);
            if (!ini.DoesKeyExist("Callouts", "MilitaryHardware"))
                ini.Write("Callouts", "MilitaryHardware", true);
            if (!ini.DoesKeyExist("Callouts", "ThePaletoScore"))
                ini.Write("Callouts", "ThePaletoScore", true);
            if (!ini.DoesKeyExist("Callouts", "PackMan"))
                ini.Write("Callouts", "PackMan", true);
            if (!ini.DoesKeyExist("Callouts", "LegalTrouble"))
                ini.Write("Callouts", "LegalTrouble", true);
            if (!ini.DoesKeyExist("Callouts", "TheBureauRaid"))
                ini.Write("Callouts", "TheBureauRaid", true);
            if (!ini.DoesKeyExist("Callouts", "Driller"))
                ini.Write("Callouts", "Driller", true);
            if (!ini.DoesKeyExist("Callouts", "TheBigScore"))
                ini.Write("Callouts", "TheBigScore", true);
            if (!ini.DoesKeyExist("Callouts", "TheThirdWay"))
                ini.Write("Callouts", "TheThirdWay", true);
            if (!ini.DoesKeyExist("Callouts", "Paparazzo_TheMeltdown"))
                ini.Write("Callouts", "Paparazzo_TheMeltdown", true);
            if (!ini.DoesKeyExist("Callouts", "RandomEvent_GetawayDriver"))
                ini.Write("Callouts", "RandomEvent_GetawayDriver", true);
            if (!ini.DoesKeyExist("Callouts", "RandomEvent_CountrysideRobbery"))
                ini.Write("Callouts", "RandomEvent_CountrysideRobbery", true);
            if (!ini.DoesKeyExist("Callouts", "RandomEvent_CountrysideGangFight"))
                ini.Write("Callouts", "RandomEvent_CountrysideGangFight", true);

            if (!ini.DoesKeyExist("Interiors", "forceInteriorsEnabled"))
                ini.Write("Interiors", "forceInteriorsEnabled", true);
        }
    }
}
