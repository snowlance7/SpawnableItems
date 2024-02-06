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

        private static List<SpawnableItemWithRarity> defaultItemsToSpawn;
        private static List<SpawnableItemWithRarity> itemsToSpawn;
        private static string defaultItemsToSpawnString { get { return string.Join(",", GetDefaultItemsToSpawn().Select((SpawnableItemWithRarity itemWithRarity) => $"{itemWithRarity.spawnableItem.itemName}:{itemWithRarity.rarity}")); } } // defaultItemsToSpawn should not be null TODO: add null handling TODO: REMOVE \n
        private static string itemsToSpawnString { get { return string.Join(",", itemsToSpawn.Select((SpawnableItemWithRarity itemWithRarity) => $"{itemWithRarity.spawnableItem.itemName}:{itemWithRarity.rarity}")); } } // itemsToSpawn should not be null TODO: add null handling TODO: REMOVE \n

        private static List<SpawnableItemWithRarity> GetDefaultItemsToSpawn()
        {
            if (defaultItemsToSpawn == null)
            {
                //throw new NotImplementedException(); // remove this line when implementing the patch
                return StartOfRound.Instance.allItemsList.itemsList
                        .Where((Item item) => (!item.isScrap && (bool)item.spawnPrefab) || (SpawnableItemsBase.configIncludeDefensiveItems.Value && item.isDefensiveWeapon))            // THE HOLY GRAIL MUAH
                        .Select(GetSpawnableItemWithRarity) //
                        .Where(item => item != null) // Filter out null values // ERROR: MIGHT THROW AN EXCEPTION, BUT IT'S FINE FOR NOW
                        .ToList(); // WORKS!!!
            }
            else
            {
                return defaultItemsToSpawn;
            }
        }

        [HarmonyPatch(typeof(Terminal), "Awake")]
        [HarmonyPostfix]
        private static void TerminalAwakePostFix() // runs when terminal is awake
        {
            if (itemsToSpawn == null)
            {
                // testing
                //GetDefaultItemsToSpawn();


                //LoggerInstance.LogDebug("configItemsToSpawn is not null. Setting to config values.");
                //return; // remove this line when implementing the patch
                // TODO: Set rarities based on configItemsToSpawn, DO NOT bind to configItemsToSpawn
                //throw new NotImplementedException(); // remove this line when implementing the patch
                try
                {
                    string itemsString = SpawnableItemsBase.configItemsToSpawn.Value;
                    if (itemsString == "") { throw new Exception(); }

                    string[] pairs = itemsString.Split(',');

                    foreach (string pair in pairs)
                    {
                        string[] parts = pair.Split(':');
                        Item tempSpawnableItem = StartOfRound.Instance.allItemsList.itemsList.Where((Item item) => item.itemName == parts[0]).First();   // TODO there may be an easier way of doing this.
                        itemsToSpawn.Add(new SpawnableItemWithRarity { spawnableItem = tempSpawnableItem, rarity = int.Parse(parts[1]) });
                    }

                    LoggerInstance.LogDebug($"List retrieved:\n{itemsToSpawnString}"); // idea convert to array with .ToArray() and see if it shows
                }
                catch (Exception)
                {
                    LoggerInstance.LogDebug("Error parsing config string, using default values...");
                    if (itemsToSpawn != null) { itemsToSpawn.Clear(); }
                    LoggerInstance.LogDebug("itemsToSpawn cleared");
                    itemsToSpawn = GetDefaultItemsToSpawn();
                    LoggerInstance.LogDebug($"Default list: {itemsToSpawnString}");
                }
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

        private static SpawnableItemWithRarity GetSpawnableItemWithRarity(Item item) // TODO: main function is to convert item to spawnableitemwithrarity
        {
            //throw new NotImplementedException(); // remove this line when implementing the patch
            
            if (item.itemName == "Mapper" || item.itemName == "Binoculars" || item.itemName == "Key" ) // check if items are mapper or binoculars
            {
                return null; // ERROR Might throw an exception, but it's fine for now
            }

            LoggerInstance.LogDebug($"Setting SpawnableItemWithRarity for item {item.itemName}");
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
