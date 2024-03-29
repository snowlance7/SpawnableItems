﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

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
        public static ConfigEntry<bool> configIncludeDefensiveItems;
        public static ConfigEntry<int> configMinItemsToSpawn;
        public static ConfigEntry<int> configMaxItemsToSpawn; // -1 for unlimited
        public static ConfigEntry<string> configItemsToSpawn;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            LoggerInstance = this.Logger;
            LoggerInstance.LogInfo($"Plugin {modName} loaded successfully.");

            configShouldScrapSpawn = Config.Bind("General", "ShouldScrapSpawn", true, "Should items spawn when scrapping?\nIf set to false, ItemSpawnSequence will default to 'WithScrap' and items will just be added to the loot table");
            configItemSpawnSequence = Config.Bind("General", "ItemSpawnSequence", "WithScrap", "When should the items spawn? Accepted Values: BeforeScrap, WithScrap, AfterScrap\nSets the timing for item spawns relative to initial scrap spawning.");
            configIncludeDefensiveItems = Config.Bind("General", "IncludeDefensiveItems", true, "Should defensive items be included in the item spawning?\nYield Sign, Shotgun, Shells, etc.");
            configMinItemsToSpawn = Config.Bind("Item Counts", "MinItemsToSpawn", 0, "Minimum number of items to spawn.");
            configMaxItemsToSpawn = Config.Bind("Item Counts", "MaxItemsToSpawn", -1, "Maximum number of items to spawn.\n-1 for unlimited (ItemSpawnSequence will default to 'WithScrap' and items will just be added to the loot table)");
            configItemsToSpawn = Config.Bind("Item Customization", "ItemsToSpawn", "", "Items to spawn with their rarity." +
                        "\nIMPORTANT: This will fill with all items when the terminal is loaded in the game. MAKE SURE THIS IS EMPTY TO FILL WITH DEFAULT VALUES, run the game and load into a lobby, close the game and edit it here." +
                        "\nFormat: ItemName:Rarity,ItemName:Rarity,ItemName:Rarity" +
                        "\nExample: Shotgun:1,YieldSign:2,Shells:3");
            
            LoggerInstance.LogDebug($"configItemsToSpawn.Value = {configItemsToSpawn.Value}");
            // TODO: set configitemstospawn based on level/moon

            harmony.PatchAll();
        }
    }
}
