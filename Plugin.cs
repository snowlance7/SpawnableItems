using BepInEx;
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
        public static ConfigEntry<int> configMaxItemsToSpawn; // -1 for unlimited
        public static ConfigEntry<string> configItemsToSpawn; // this will fill when terminal is awake
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
            //configItemsToSpawn = Config.Bind("General", "ItemsToSpawn", "", "Items to spawn with their rarity.\nIMPORTANT: This will fill with all items when the terminal is loaded in the game. Run the game first and load into a lobby to get string and edit it here.\nFormat: ItemName:Rarity,ItemName:Rarity,ItemName:Rarity\nExample: Shotgun:1,YieldSign:2,Shells:3");
            configIncludeDefensiveItems = Config.Bind("General", "IncludeDefensiveItems", true, "Should defensive items be included in the item spawning?\nYield Sign, Shotgun, Shells, etc.");
            
            // TO DO: set configitemstospawn based on level/moon
            
            harmony.PatchAll();
        }
    }
}
