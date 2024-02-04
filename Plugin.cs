using BepInEx;
using System;
using System.Collections.Generic;
using BepInEx.Logging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using GameNetcodeStuff;
using BepInEx.Configuration;
using System.IO;

namespace SpawnableItems
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class SpawnableItemsBase : BaseUnityPlugin
    {
        private const string modGUID = "Snowlance.SpawnableItems";
        private const string modName = "Spawnable Items";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static SpawnableItemsBase Instance;
        public static ManualLogSource LoggerInstance { get; private set; }

        public static ConfigEntry<bool> configShouldScrapSpawn;
        public static ConfigEntry<string> configItemSpawnSequence; // Accepted Values: BeforeScrap, WithScrap, AfterScrap. Will be used only if configShouldScrapSpawn is true.
        public static ConfigEntry<int> configMaxItemsToSpawn; // -1 for unlimited
        public static ConfigEntry<string> configItemsToSpawn;
        public static ConfigEntry<bool> configIncludeDefensiveItems;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            LoggerInstance = this.Logger;
            LoggerInstance.LogInfo($"Plugin {modName} loaded successfully.");

            configShouldScrapSpawn = Config.Bind("General", "ShouldScrapSpawn", true, "Should items spawn when scrapping?");
            configItemSpawnSequence = Config.Bind("General", "ItemSpawnSequence", "WithScrap", "When should the items spawn? Accepted Values: BeforeScrap, WithScrap, AfterScrap\nSets the timing for item spawns relative to initial scrap spawning.");
            configMaxItemsToSpawn = Config.Bind("General", "MaxItemsToSpawn", -1, "Maximum number of items to spawn.\n-1 for unlimited (ItemSpawnSequence will default to 'WithScrap' and items will just be added to the loot table)");
            configIncludeDefensiveItems = Config.Bind("General", "IncludeDefensiveItems", true, "Should defensive items be included in the item spawning?\nYield Sign, Shotgun, Shells, etc.");
            
            // TO DO: set configitemstospawn based on level
            
            harmony.PatchAll();
        }
    }
}
