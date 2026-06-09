using BepInEx;
using BepInEx.Logging;
using Accolades.Plugin;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Accolade {
    [BepInPlugin(modGUID, modName, modVersion)]
    public class AccoladeModBase : BaseUnityPlugin {
        private const string modGUID = "GHAccoladesMod";
        private const string modName = "GH Accolades";
        private const string modVersion = "2.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static AccoladeModBase Instance;

        internal ManualLogSource LOGGER;

        void Awake() {
            if (Instance == null) {
                Instance = this;
            }

            LOGGER = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            LOGGER.LogInfo("GH Accolades has awoken!!");

            harmony.PatchAll(typeof(AccoladeModBase));
            harmony.PatchAll(typeof(AccoladePlugin));
        }
    }
}
