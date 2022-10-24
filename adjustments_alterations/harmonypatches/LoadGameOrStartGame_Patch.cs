using System;
using AssetBundles;
using PavonisInteractive.TerraInvicta;

namespace TI_General_Adjustments_Alterations.adjustments_alterations.harmonypatches
{
    public class LoadGameOrStartGame_Patch
    {
        public static bool ClearGameData_Prefix()
        {
            Config.GameStartedOrLoaded = true;
            Console.WriteLine("Game Loaded or New Game started");
            return true;
        }

        public static bool AssetLoad_Initialize_Prefix()
        {   
            Log.Time("AssetBundleManager Initialize", new Action(AssetBundleManager.Initialize));
            Config.GameAssetsLoaded = true;
            Console.WriteLine("Game Assets fully loaded");
            Main.ApplyAdditionalPatchesAffectingGameAssets();
            return false;
        }
    }
}