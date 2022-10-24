using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using TI_General_Adjustments_Alterations.adjustment_alterations.core.missionrelated;
using TI_General_Adjustments_Alterations.adjustment_alterations.core.regionstate;
using TI_General_Adjustments_Alterations.adjustments_alterations.harmonypatches;
using TI_General_Adjustments_Alterations.adjustments_alterations.harmonypatches.factionstate;
using UnityEngine;
using UnityModManagerNet;
// ReSharper disable All

namespace TI_General_Adjustments_Alterations
{
    public class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry mod;
        private static Harmony harmony = null;

        public static bool Load(UnityModManager.ModEntry modEntry) {
            mod = modEntry;
            modEntry.OnToggle = OnToggle;
            Config.LoadValues();

            bool gameHasBeenUpdatedRerunExtendedInstall = false;
            if (Config.GetValue<bool>("extended_installation_completed"))
            {
                long originalSize = new System.IO.FileInfo(Path.GetFullPath("TerraInvicta_Data\\Managed\\Assembly-CSharp.dll")).Length;
                int extendSize = Config.GetValue<int>("extended_installation_main_dll_file_size");
                gameHasBeenUpdatedRerunExtendedInstall = originalSize != extendSize;
            }

            if (!Config.GetValue<bool>("extended_installation_completed") || gameHasBeenUpdatedRerunExtendedInstall)
            {
                var psi = new ProcessStartInfo();
                psi.CreateNoWindow = true;
                psi.FileName = Path.GetFullPath(modEntry.Path+"\\EXTENDED_INSTALL.bat");
                psi.UseShellExecute = false;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.WorkingDirectory = modEntry.Path;
                var process = new Process();
                process.StartInfo = psi;
                process.Start();
                Console.WriteLine("TI_General_Adjustments_Alterations -> running EXTENDED_INTALL.bat, game needs to restart");
                Application.Quit(0);
            }

            harmony = new Harmony(modEntry.Info.Id);

            applyGameStateListenerPatches(harmony);

            if (Config.GetValue<bool>("nuclear_barrage_related_configurations_enabled"))
            {
                var original = typeof(TIRegionState).GetMethod("ApplyDamageToRegion");
                var prefix = typeof(ApplyDamageToRegionPatch).GetMethod("Prefix");
                var postfix = typeof(ApplyDamageToRegionPatch).GetMethod("Postfix");
                harmony.Patch(original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
            }
            
            if (Config.GetValue<bool>("faction_related_configurations_enabled"))
            {
                var original = typeof(TIFactionState).GetMethod("GetMissionControlContributionFromHabs");
                var postfix = typeof(MissionControlContributionFromHabs_Patch).GetMethod("GetMissionControlContributionFromHabs_Postfix");
                harmony.Patch(original, null, new HarmonyMethod(postfix));

                var original2 = typeof(TIHabModuleTemplate).GetMethod("MonthlyResourceIncome");
                var prefix2 = typeof(MissionControlContributionFromHabs_Patch).GetMethod("MonthlyResourceIncome_Prefix");
                harmony.Patch(original2, new HarmonyMethod(prefix2));
                
            }

            if (Config.GetValue<bool>("agent_follow_up_success_failure_modifiers_enabled"))
            {
                TIMissionResolutionPatch.setConfigVariables();
                var original2 = typeof(TIMissionResolution_Contested).GetMethod("GetAllModifiers");
                var postfix2 = typeof(TIMissionResolutionPatch).GetMethod("GetAllModifiers_Postfix");
                harmony.Patch(original2, null, new HarmonyMethod(postfix2));

                TIMissionStatePatch.setConfigVariables();
                var original6 = typeof(TIMissionState).GetMethod("ResolveMission");
                var postfix6 = typeof(TIMissionStatePatch).GetMethod("ResolveMission_Postfix");
                harmony.Patch(original6, null, new HarmonyMethod(postfix6));
            }

            if (Config.IsDebugModeActive())
            {
                PrintOutAllGameAssemblyMethods();
            }
            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        public static void ApplyAdditionalPatchesAffectingGameAssets()
        {
            //var original5 = typeof(MarkerController).GetMethod("SetCPImages");
            //var prefix5 = typeof(MarketControllerSetCPImages_Replace).GetMethod("SetCPImages_MarkerController");
            //Main._harmony.Patch(original5, new HarmonyMethod(prefix5));
        }

        private static void PrintOutAllGameAssemblyMethods()
        {
            foreach (var method in typeof(TINationState).GetMethods())
            {
                var parameters = method.GetParameters();
                var parameterDescriptions = string.Join(", ", method.GetParameters()
                    .Select(x => x.ParameterType + " " + x.Name)
                    .ToArray());

                Console.WriteLine("{0} {1} ({2})",
                    method.ReturnType,
                    method.Name,
                    parameterDescriptions);
            }
        }

        private static void applyGameStateListenerPatches(Harmony harmony)
        {
            var clearGameDataOriginal = typeof(ViewControl).GetMethod("ClearGameData");
            var clearGameDataPrefix = typeof(LoadGameOrStartGame_Patch).GetMethod("ClearGameData_Prefix");
            harmony.Patch(clearGameDataOriginal, new HarmonyMethod(clearGameDataPrefix));
            
            var assetLoaderInitializeOriginal = typeof(AssetLoader).GetMethod("Initialize");
            var assetLoaderInitializePrefix = typeof(LoadGameOrStartGame_Patch).GetMethod("AssetLoad_Initialize_Prefix");
            harmony.Patch(assetLoaderInitializeOriginal, new HarmonyMethod(assetLoaderInitializePrefix));
        }
        
        
    }
}