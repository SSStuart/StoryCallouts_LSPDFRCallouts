using Rage;
using StoryCallouts.Callouts;
using System.Text.RegularExpressions;

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
        internal static bool callout_CasingTheJewelStore = true;
        internal static bool callout_DeadManWalking = true;
        internal static bool callout_HoodSafari = true;
        internal static bool callout_TheHotelAssassination = true;
        internal static bool callout_GrassRoots_ThePickup = true;
        internal static bool callout_BlitzPlay = true;
        internal static bool callout_MilitaryHardware = true;
        internal static bool callout_ThePaletoScore = true;
        internal static bool callout_PackMan = true;

        internal static bool forceInteriorsEnabled = false;

        internal static string path = "Plugins/LSPDFR/StoryCallouts.ini";
        internal static InitializationFile ini = new InitializationFile(path);

        internal static void LoadSettings()
        {
            Game.LogTrivial($"[{Main.pluginName}] Loading config file:");

            ini.Create();
            useStoryModeCharacters = ini.ReadBoolean("General", "useStoryModeCharacters", true);
            Game.LogTrivial($"- Use story mode characters: {(useStoryModeCharacters ? "Yes" : "No")}");

            callout_Prologue = ini.ReadBoolean("Callouts", "Prologue", true);
            Game.LogTrivial($"- Callout 'Prologue' {(callout_Prologue ? "[x] enabled" : "[ ] disabled")}");
            callout_FranklinAndLamar = ini.ReadBoolean("Callouts", "FranklinAndLamar", true);
            Game.LogTrivial($"- Callout 'FranklinAndLamar' {(callout_FranklinAndLamar ? "[x] enabled" : "[ ] disabled")}");
            callout_TheLongStretch = ini.ReadBoolean("Callouts", "TheLongStretch", true);
            Game.LogTrivial($"- Callout 'TheLongStretch' {(callout_TheLongStretch ? "[x] enabled" : "[ ] disabled")}");
            callout_CarbineRifles = ini.ReadBoolean("Callouts", "CarbineRifles", true);
            Game.LogTrivial($"- Callout 'CarbineRifles' {(callout_CarbineRifles ? "[x] enabled" : "[ ] disabled")}");
            callout_BugstarsEquipment = ini.ReadBoolean("Callouts", "BugstarsEquipment", true);
            Game.LogTrivial($"- Callout 'BugstarsEquipment' {(callout_BugstarsEquipment ? "[x] enabled" : "[ ] disabled")}");
            callout_BZGasGrenades = ini.ReadBoolean("Callouts", "BZGasGrenades", true);
            Game.LogTrivial($"- Callout 'BZGasGrenades' {(callout_BZGasGrenades ? "[x] enabled" : "[ ] disabled")}");
            callout_CasingTheJewelStore = ini.ReadBoolean("Callouts", "CasingTheJewelStore", true);
            Game.LogTrivial($"- Callout 'CasingTheJewelStore' {(callout_CasingTheJewelStore ? "[x] enabled" : "[ ] disabled")}");
            callout_DeadManWalking = ini.ReadBoolean("Callouts", "DeadManWalking", true);
            Game.LogTrivial($"- Callout 'DeadManWalking' {(callout_DeadManWalking ? "[x] enabled" : "[ ] disabled")}");
            callout_HoodSafari = ini.ReadBoolean("Callouts", "HoodSafari", true);
            Game.LogTrivial($"- Callout 'HoodSafari' {(callout_HoodSafari ? "[x] enabled" : "[ ] disabled")}");
            callout_TheHotelAssassination = ini.ReadBoolean("Callouts", "TheHotelAssassination", true);
            Game.LogTrivial($"- Callout 'TheHotelAssassination' {(callout_TheHotelAssassination ? "[x] enabled" : "[ ] disabled")}");
            callout_GrassRoots_ThePickup = ini.ReadBoolean("Callouts", "GrassRoots_ThePickup", true);
            Game.LogTrivial($"- Callout 'GrassRoots_ThePickup' {(callout_GrassRoots_ThePickup ? "[x] enabled" : "[ ] disabled")}");
            callout_BlitzPlay = ini.ReadBoolean("Callouts", "BlitzPlay", true);
            Game.LogTrivial($"- Callout 'BlitzPlay' {(callout_BlitzPlay ? "[x] enabled" : "[ ] disabled")}");
            callout_MilitaryHardware = ini.ReadBoolean("Callouts", "MilitaryHardware", true);
            Game.LogTrivial($"- Callout 'MilitaryHardware' {(callout_MilitaryHardware ? "[x] enabled" : "[ ] disabled")}");
            callout_ThePaletoScore = ini.ReadBoolean("Callouts", "ThePaletoScore", true);
            Game.LogTrivial($"- Callout 'ThePaletoScore' {(callout_ThePaletoScore ? "[x] enabled" : "[ ] disabled")}");
            callout_PackMan = ini.ReadBoolean("Callouts", "PackMan", true);
            Game.LogTrivial($"- Callout 'PackMan' {(callout_PackMan ? "enabled" : "disabled")}");

            forceInteriorsEnabled = ini.ReadBoolean("Interiors", "forceEnabled", false);
            Game.LogTrivial($"- Force enable interiors : {(forceInteriorsEnabled ? "Yes" : "No")}");

            Game.LogTrivial($"[{Main.pluginName}] Plugin settings loaded.");
        }
    }
}
