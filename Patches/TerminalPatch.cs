using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SpawnableItems
{
    [HarmonyPatch]
    internal static class TerminalPatch
    {
        private static SpawnableItemsBase Instance = SpawnableItemsBase.Instance;
        private static readonly ManualLogSource LoggerInstance = SpawnableItemsBase.LoggerInstance;

        public static List<SpawnableItemWithRarity> itemsToSpawn;
        public static string itemsToSpawnString { get { return string.Join(",", itemsToSpawn.Select((SpawnableItemWithRarity itemWithRarity) => $"{itemWithRarity.spawnableItem.itemName}:{itemWithRarity.rarity}")); } } // itemsToSpawn should not be null TODO: add null handling

        /*private static List<SpawnableItemWithRarity> GetDefaultSpawnableItems()
        {
            
        }*/ // might not be needed

        [HarmonyPatch(typeof(Terminal), "Awake")]
        [HarmonyPostfix]
        private static void TerminalAwakePostFix() // runs when terminal is awake
        {
            // testing
            //SetDefaultItemsToSpawn();

            // if configItemsToSpawn is null, set to default values
            if (SpawnableItemsBase.configItemsToSpawn == null)
            {
                LoggerInstance.LogDebug("configItemsToSpawn is null. Setting to default.");
                //return; // remove this line when implementing the patch

                SetDefaultItemsToSpawn();
                
                SpawnableItemsBase.configItemsToSpawn = Instance.Config.Bind("General", "ItemsToSpawn", itemsToSpawnString, "Items to spawn with their rarity." +
                    "\nIMPORTANT: This will fill with all items when the terminal is loaded in the game. MAKE SURE THIS IS EMPTY, run the game and load into a lobby to fill this with default values, close the game and edit it here." +
                    "\nFormat: ItemName:Rarity,ItemName:Rarity,ItemName:Rarity" +
                    "\nExample: Shotgun:1,YieldSign:2,Shells:3");
            }
            else
            {
                LoggerInstance.LogDebug("configItemsToSpawn is not null. Setting to config value.");
                //return; // remove this line when implementing the patch
                // TODO: Set rarities based on configItemsToSpawn, DO NOT bind to configItemsToSpawn
                GetitemsToSpawnFromConfig();
            }
        }

        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        [HarmonyPrefix]
        private static void SpawnScrapInLevelPreFix(RoundManager __instance)
        {
            return; // remove this line when implementing the patch
            if (!SpawnableItemsBase.configShouldScrapSpawn.Value) // if configShouldScrapSpawn is false, clear the spawnablescrap list
            {
                __instance.currentLevel.spawnableScrap.Clear();
            }

            if (SpawnableItemsBase.configItemSpawnSequence.Value == "BeforeScrap")
            {
                // spawn number of items before scrap is spawned

            }
            else if (SpawnableItemsBase.configItemSpawnSequence.Value == "WithScrap")
            {
                // add items to spawnablescrap

            }

            // if configItemSpawnSequence is "BeforeScrap", then spawn items before scrap is spawned
            //__instance.currentLevel.spawnableScrap.Add(); // use this to add items to spawnablescrap in foreach loop, only use if configItemSpawnSequence is "WithScrap"
            // if configItemSpawnSequence is "AfterScrap", then spawn items after scrap is spawned in spawnscrapinlevelpostfix(should be automatic)
        }

        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        [HarmonyPostfix]
        public static void SpawnScrapInLevelPostFix(RoundManager __instance) // runs after the switch is pulled
        {
            throw new NotImplementedException(); // remove this line when implementing the patch
            /*SpawnableItemWithRarity[] array = StartOfRound.Instance.allItemsList.itemsList.Where((Item item) => !item.isScrap && (bool)item.spawnPrefab).Select(item => GetSpawnableItemWithRarity(item)).ToArray(); // TODO: rework this
            int[] weights = array.Select((SpawnableItemWithRarity f) => f.rarity).ToArray();
            List<RandomScrapSpawn> list = (from s in UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>()
                                           where !s.spawnUsed
                                           select s).ToList();
            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed - 7);
            int num = random.Next(SpawnableItemsBase.configMaxItemsToSpawn.Value / 2, SpawnableItemsBase.configMaxItemsToSpawn.Value);
            for (int i = 0; i < num; i++)
            {
                if (list.Count <= 0)
                {
                    break;
                }
                int index = random.Next(0, list.Count);
                RandomScrapSpawn randomScrapSpawn = list[index];
                Vector3 vector = randomScrapSpawn.transform.position;
                if (randomScrapSpawn.spawnedItemsCopyPosition)
                {
                    list.RemoveAt(index);
                }
                else
                {
                    vector = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit);
                }
                int randomWeightedIndex = RoundManager.Instance.GetRandomWeightedIndex(weights, random);
                GameObject gameObject = UnityEngine.Object.Instantiate(array[randomWeightedIndex].spawnableItem.spawnPrefab, vector + Vector3.up * 0.5f, Quaternion.identity, StartOfRound.Instance.propsContainer);
                gameObject.GetComponent<GrabbableObject>().fallTime = 0f;
                gameObject.GetComponent<NetworkObject>().Spawn();
            }*/
        }

        private static void SpawnItemsInLevel(RoundManager __instance)
        {
            throw new NotImplementedException(); // remove this line when implementing the patch
            // TODO: spawn items in level, may need to rework this, check SpawnScrapInLevel in ILSpy for reference
            /*SpawnableItemWithRarity[] array = StartOfRound.Instance.allItemsList.itemsList.Where((Item item) => !item.isScrap && (bool)item.spawnPrefab).Select(item => GetSpawnableItemWithRarity(item)).ToArray(); // TODO: REWORK THIS
            int[] weights = array.Select((SpawnableItemWithRarity f) => f.rarity).ToArray(); // TODO: REWORK THIS TO A LIST INSTEAD OF AN ARRAY
            List<RandomScrapSpawn> list = (from s in UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>()
                                           where !s.spawnUsed
                                           select s).ToList();
            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed - 7);
            int num = random.Next(SpawnableItemsBase.configMaxItemsToSpawn.Value / 2, SpawnableItemsBase.configMaxItemsToSpawn.Value);
            for (int i = 0; i < num; i++) // TODO: figure out how this works
            {
                if (list.Count <= 0)
                {
                    break;
                }
                int index = random.Next(0, list.Count);
                RandomScrapSpawn randomScrapSpawn = list[index];
                Vector3 vector = randomScrapSpawn.transform.position;
                if (randomScrapSpawn.spawnedItemsCopyPosition)
                {
                    list.RemoveAt(index);
                }
                else
                {
                    vector = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit);
                }
                int randomWeightedIndex = RoundManager.Instance.GetRandomWeightedIndex(weights, random);
                GameObject gameObject = UnityEngine.Object.Instantiate(array[randomWeightedIndex].spawnableItem.spawnPrefab, vector + Vector3.up * 0.5f, Quaternion.identity, StartOfRound.Instance.propsContainer);
                gameObject.GetComponent<GrabbableObject>().fallTime = 0f;
                gameObject.GetComponent<NetworkObject>().Spawn();
            }*/
        }

        private static void GetitemsToSpawnFromConfig()  // should get items from config and set to itemsToSpawn otherwise set to defaults // idea fill a generic object class with the values and then set to itemsToSpawn with rarity values
        {
            //throw new NotImplementedException(); // remove this line when implementing the patch
            try
            {
                string itemsString = SpawnableItemsBase.configItemsToSpawn.Value.Replace(" ", "");
                itemsToSpawn = itemsString.Split(',')
                    .Select(itemString =>
                    {
                        var parts = itemString.Split(':');
                        Item tempSpawnableItem = StartOfRound.Instance.allItemsList.itemsList.Where((Item item) => item.itemName == parts[0]).First();   // TODO there may be an easier way of doing this.
                        return new SpawnableItemWithRarity { spawnableItem = tempSpawnableItem, rarity = int.Parse(parts[1]) };
                    })
                    .ToList();  // ERROR TODO might throw an exception, but it's fine for now
                LoggerInstance.LogDebug($"List retrieved:\n{itemsToSpawn}"); // idea convert to array with .ToArray() and see if it shows
            }
            catch (Exception)
            {
                LoggerInstance.LogError("Error parsing config string, using default values...");
                itemsToSpawn.Clear();
                SetDefaultItemsToSpawn();
            }
        } 

        private static void SetDefaultItemsToSpawn()
        {
            //throw new NotImplementedException(); // remove this line when implementing the patch
            itemsToSpawn = StartOfRound.Instance.allItemsList.itemsList
                    .Where((Item item) => (!item.isScrap && (bool)item.spawnPrefab) || (SpawnableItemsBase.configIncludeDefensiveItems.Value && item.isDefensiveWeapon))            // THE HOLY GRAIL MUAH
                    .Select(item => GetSpawnableItemWithRarity(item)) //
                    .Where(item => item != null) // Filter out null values // ERROR: MIGHT THROW AN EXCEPTION, BUT IT'S FINE FOR NOW
                    .ToList(); // WORKS!!!
            // TODO: set to default values, should calculate rarity based on price or value, otherwise set to average of all item rarities in that level
        }   

        private static SpawnableItemWithRarity GetSpawnableItemWithRarity(Item item) // TODO: main function is to convert item to spawnableitemwithrarity
        {
            //throw new NotImplementedException(); // remove this line when implementing the patch
            LoggerInstance.LogDebug($"Setting SpawnableItemWithRarity for item {item.itemName}");
            
            if (item.itemName == "Mapper" || item.itemName == "Binoculars" || item.itemName == "Key" ) // check if items are mapper or binoculars
            {
                return null; // ERROR Might throw an exception, but it's fine for now
            }
            
            // temporary test
            return new SpawnableItemWithRarity
            {
                rarity = 1,
                spawnableItem = item
            };

            // TODO: should check configs and set rarity accordingly otherwise do below
            // should check if it has a price or value and set rarity accordingly, otherwise set to average of all item rarities in that level
            // TODO: This needs to somehow have access to the config rarity values for each item

            return new SpawnableItemWithRarity
            {
                rarity = 1,
                spawnableItem = item
            };
        }
    }
}
