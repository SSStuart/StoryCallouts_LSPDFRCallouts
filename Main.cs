using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Drawing;
using System.Reflection;
using System.IO;

namespace StoryCallouts
{
    public class Main : Plugin
    {
        public static string pluginName = "Story Callouts";
        public static string pluginVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static Localization l10n = new Localization();
        public static int nbCalloutsEnabled = 0;

        public static Color calloutWaypointColor = Color.FromArgb(240, 200, 80);
        public static Color alliesColor = Color.FromArgb(93, 182, 229);

        public override void Initialize()
        {
            if (!File.Exists("CalloutInterfaceAPI.dll"))
            {
                Game.LogTrivial($"{pluginName} loading has been aborted: CalloutInterfaceAPI.dll is missing!");
                Game.DisplayNotification("mpinventory", "custom_mission", pluginName, $"V {pluginVersion}", l10n.GetString("missingPrereq"));
                return;
            }

            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;

            Game.LogTrivial($"{pluginName} v{pluginVersion} has been initialised.");
            nbCalloutsEnabled = Settings.LoadSettings();
            Game.LogTrivial($"Go on duty to fully load {pluginName}.");

            UpdateChecker.CheckForUpdates();

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LSPDFRResolveEventHandler);
        }
        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                RegisterCallouts();
                Game.DisplayNotification("mpinventory", "custom_mission", pluginName, $"V {pluginVersion}", l10n.GetString("loaded").Replace(":nbCalloutsEnabled", nbCalloutsEnabled.ToString()));
            }
        }

        public override void Finally()
        {
            Game.LogTrivial($"{pluginName} has been unloaded.");
        }

        private static void RegisterCallouts()
        {
            if (Settings.callout_Prologue)
                Functions.RegisterCallout(typeof(Callouts.Prologue));
            if (Settings.callout_FranklinAndLamar)
                Functions.RegisterCallout(typeof(Callouts.FranklinAndLamar));
            if (Settings.callout_TheLongStretch)
                Functions.RegisterCallout(typeof(Callouts.TheLongStretch));
            if (Settings.callout_CarbineRifles)
                Functions.RegisterCallout(typeof(Callouts.CarbineRifles));
            if (Settings.callout_BugstarsEquipment)
                Functions.RegisterCallout(typeof(Callouts.BugstarsEquipment));
            if (Settings.callout_BZGasGrenades)
                Functions.RegisterCallout(typeof(Callouts.BZGasGrenades));
            if (Settings.callout_CasingTheJewelStore)
                Functions.RegisterCallout(typeof(Callouts.CasingTheJewelStore));
            if (Settings.callout_DeadManWalking)
                Functions.RegisterCallout(typeof(Callouts.DeadManWalking));
            if (Settings.callout_HoodSafari)
                Functions.RegisterCallout(typeof(Callouts.HoodSafari));
            if (Settings.callout_TheHotelAssassination)
                Functions.RegisterCallout(typeof(Callouts.TheHotelAssassination));
            if (Settings.callout_GrassRoots_ThePickup)
                Functions.RegisterCallout(typeof(Callouts.GrassRoots_ThePickup));
            if (Settings.callout_BlitzPlay)
                Functions.RegisterCallout(typeof(Callouts.BlitzPlay));
            if (Settings.callout_MilitaryHardware)
                Functions.RegisterCallout(typeof(Callouts.MilitaryHardware));
            if (Settings.callout_ThePaletoScore)
                Functions.RegisterCallout(typeof(Callouts.ThePaletoScore));
            if (Settings.callout_PackMan)
                Functions.RegisterCallout(typeof(Callouts.PackMan));
        }


        public static Assembly LSPDFRResolveEventHandler(object sender, ResolveEventArgs args)
        {
            foreach (Assembly assembly in Functions.GetAllUserPlugins())
            {
                if (args.Name.ToLower().Contains(assembly.GetName().Name.ToLower()))
                {
                    return assembly;
                }
            }
            return null;
        }

        public static bool IsLSPDFRPluginRunning(string Plugin, Version minversion = null)
        {
            foreach (Assembly assembly in Functions.GetAllUserPlugins())
            {
                AssemblyName assemblyName = assembly.GetName();
                if (assemblyName.Name.ToLower() == Plugin.ToLower())
                {
                    if (minversion == null || assemblyName.Version.CompareTo(minversion) >= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
