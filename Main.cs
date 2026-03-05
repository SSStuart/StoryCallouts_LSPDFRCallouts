using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Drawing;
using System.Reflection;

namespace StoryCallouts
{
    public class Main : Plugin
    {
        public static string pluginName = "Story Callouts";
        public static string pluginVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static Localization l10n = new Localization();

        public static Color calloutWaypointColor = Color.Yellow;

        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;

            Game.LogTrivial($"{pluginName} v{pluginVersion} has been initialised.");
            //Settings.LoadSettings();
            //ENABLE_OVERLAY = Settings.EnableOverlay;
            //WARP_PLAYER = Settings.WarpPlayerInHeli;
            //PLAYER_BEHAVIOUR = Settings.PlayerBehaviour;
            //HELI_TYPE = Settings.HeliType;
            //ON_PED_ARREST_BEHAVIOR = Settings.OnPedArrestBehavior;
            Game.LogTrivial($"Go on duty to fully load {pluginName}.");

            UpdateChecker.CheckForUpdates();

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LSPDFRResolveEventHandler);
        }
        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                RegisterCallouts();
                Game.DisplayNotification("mpinventory", "custom_mission", pluginName, $"V {pluginVersion}", l10n.GetString("loaded"));
            }
        }

        public override void Finally()
        {
            Game.LogTrivial($"{pluginName} has been unloaded.");
        }

        private static void RegisterCallouts()
        {
             Functions.RegisterCallout(typeof(Callouts.FranklinAndLamar));
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
