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
    internal static class PatchHandler
    {
        private static SpawnableItemsBase Instance = SpawnableItemsBase.Instance;
        private static readonly ManualLogSource LoggerInstance = SpawnableItemsBase.LoggerInstance;

        private static float totalInverseItemsValue;

        private static List<SpawnableItemWithRarity> defaultItemsToSpawn;
        private static List<SpawnableItemWithRarity> itemsToSpawn;
        private static string defaultItemsToSpawnString { get { return string.Join(",", GetDefaultItemsToSpawn().Select((SpawnableItemWithRarity itemWithRarity) => $"{itemWithRarity.spawnableItem.itemName}:{itemWithRarity.rarity}")); } } // defaultItemsToSpawn should not be null TODO: add null handling
        private static string itemsToSpawnString { get { return string.Join(",", itemsToSpawn.Select((SpawnableItemWithRarity itemWithRarity) => $"{itemWithRarity.spawnableItem.itemName}:{itemWithRarity.rarity}")); } } // itemsToSpawn should not be null TODO: add null handling

        private static List<SpawnableItemWithRarity> GetDefaultItemsToSpawn()
        {
            if (defaultItemsToSpawn == null)
            {
                List<Item> tempItemList =  StartOfRound.Instance.allItemsList.itemsList
                        .Where((Item item) => (!item.isScrap && (bool)item.spawnPrefab) || (SpawnableItemsBase.configIncludeDefensiveItems.Value && item.isDefensiveWeapon))
                        .ToList();

                // TODO: Figure out a good formula for calculating default rarities
                // fill itemsvalues list for use in GetSpawnableItemWithRarity
                /*List<float> itemsValues = new List<float>();
                foreach (Item item in tempItemList)
                {
                    if (item.creditsWorth != 0)
                    {
                        itemsValues.Add(item.creditsWorth);
                    }
                    else
                    {
                        itemsValues.Add((item.minValue + item.maxValue) / 2);
                    }
                    LoggerInstance.LogDebug($"tempItemList: {item.itemName}");
                }
                
                //totalInverseItemsValue = itemsValues.Select(value => 1 / value).ToList().Sum();
                //LoggerInstance.LogDebug($"totalInversItemsValue = {totalInverseItemsValue}");*/

                return tempItemList.Select(item => GetSpawnableItemWithRarity(item))
                        .Where(item => item != null) // Filter out null values
                        .ToList();
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
                try
                {
                    string itemsString = SpawnableItemsBase.configItemsToSpawn.Value;
                    if (itemsString == "") { throw new Exception(); }
                    itemsToSpawn = new List<SpawnableItemWithRarity>();

                    string[] pairs = itemsString.Split(',');

                    foreach (string pair in pairs)
                    {
                        string[] parts = pair.Split(':');
                        Item tempSpawnableItem = StartOfRound.Instance.allItemsList.itemsList.Where((Item item) => item.itemName == parts[0]).First();   // TODO there may be an easier way of doing this.
                        LoggerInstance.LogDebug($"tempSpawnableItem: {tempSpawnableItem.itemName}:{parts[1]}");
                        itemsToSpawn.Add(GetSpawnableItemWithRarity(tempSpawnableItem, int.Parse(parts[1])));
                    }

                    LoggerInstance.LogDebug($"List retrieved:\n{itemsToSpawnString}");
                }
                catch (Exception)
                {

                    LoggerInstance.LogDebug("Error parsing config string, using default values...");

                    if (itemsToSpawn != null) { itemsToSpawn.Clear(); LoggerInstance.LogDebug("itemsToSpawn cleared"); }
                    itemsToSpawn = GetDefaultItemsToSpawn();
                    if (SpawnableItemsBase.configItemsToSpawn.Value == "") { SpawnableItemsBase.configItemsToSpawn.Value = itemsToSpawnString; }
                    LoggerInstance.LogDebug($"Default list: {itemsToSpawnString}");
                }
            }
        }

        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        [HarmonyPrefix]
        private static void SpawnScrapInLevelPreFix(RoundManager __instance) // runs before scrap is spawned
        {
            if (!SpawnableItemsBase.configShouldScrapSpawn.Value) // if configShouldScrapSpawn is false, clear the spawnablescrap list
            {
                __instance.currentLevel.spawnableScrap.Clear();
            }

            if (SpawnableItemsBase.configItemSpawnSequence.Value == "WithScrap" || SpawnableItemsBase.configMaxItemsToSpawn.Value == -1)
            {
                // TODO ERROR: minmax arguement out of range exception
                // add items to spawnablescrap
                __instance.currentLevel.spawnableScrap.AddRange(itemsToSpawn);
            }
            else if (SpawnableItemsBase.configItemSpawnSequence.Value == "BeforeScrap")
            {
                // spawn number of items before scrap is spawned
                SpawnItemsInLevel();
            }
            

            // if configItemSpawnSequence is "BeforeScrap", then spawn items before scrap is spawned
            //__instance.currentLevel.spawnableScrap.Add(); // use this to add items to spawnablescrap in foreach loop, only use if configItemSpawnSequence is "WithScrap"
            // if configItemSpawnSequence is "AfterScrap", then spawn items after scrap is spawned in spawnscrapinlevelpostfix(should be automatic)
        }

        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        [HarmonyPostfix]
        public static void SpawnScrapInLevelPostFix() // runs after scrap is spawned
        {
            if (SpawnableItemsBase.configItemSpawnSequence.Value == "AfterScrap" && SpawnableItemsBase.configMaxItemsToSpawn.Value != -1)
            {
                SpawnItemsInLevel();
            }
        }

        private static void SpawnItemsInLevel()
        {

            // TODO ERROR: minmax arguement out of range exception
            try
            {
                if (SpawnableItemsBase.configMaxItemsToSpawn.Value != -1 && SpawnableItemsBase.configItemSpawnSequence.Value != "WithScrap")
                {
                    SpawnableItemWithRarity[] array = itemsToSpawn.ToArray();
                    int[] weights = array.Select((SpawnableItemWithRarity f) => f.rarity).ToArray();

                    List<RandomScrapSpawn> list = (from s in UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>()
                                                   where !s.spawnUsed
                                                   select s).ToList();
                    System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed - 7);
                    int num = random.Next(SpawnableItemsBase.configMinItemsToSpawn.Value, SpawnableItemsBase.configMaxItemsToSpawn.Value); // ERROR: 
                    LoggerInstance.LogDebug($"Spawning {num} items in level");
                    for (int i = 0; i < num; i++) // spawn items in level based on config for min and max items to spawn
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
                        // spawn item at random position
                        int randomWeightedIndex = RoundManager.Instance.GetRandomWeightedIndex(weights, random);
                        GameObject gameObject = UnityEngine.Object.Instantiate(array[randomWeightedIndex].spawnableItem.spawnPrefab, vector + Vector3.up * 0.5f, Quaternion.identity, StartOfRound.Instance.propsContainer); // TODO: ERROR IS HERE
                        gameObject.GetComponent<GrabbableObject>().fallTime = 0f;
                        gameObject.GetComponent<NetworkObject>().Spawn();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.LogError("Unable to spawn all items, ERROR: " + ex);
            }
        }

        private static SpawnableItemWithRarity GetSpawnableItemWithRarity(Item item) // TODO: main function is to convert item to spawnableitemwithrarity
        {
            if (item.itemName == "Mapper" || item.itemName == "Binoculars" || item.itemName == "Key" ) // check if items are mapper or binoculars
            {
                return null; // ERROR Might throw an exception, but it's fine for now
            }

            LoggerInstance.LogDebug($"Setting SpawnableItemWithRarity for item {item.itemName}");

            /*float _rarity;
            if (item.creditsWorth != 0)
            {
                _rarity = item.creditsWorth; //1 / (float)item.creditsWorth;
            }
            else if (item.maxValue != 0)
            {
                _rarity = ((item.minValue + item.maxValue) / 2);
            }
            else
            {
                _rarity = 25;
            }

            _rarity = (_rarity / totalInverseItemsValue) * 100; // TODO: if less than 5, times by 10

            // TODO: should check configs and set rarity accordingly otherwise do below
            // should check if it has a price or value and set rarity accordingly, otherwise set to average of all item rarities in that level?
            LoggerInstance.LogDebug($"Set rarity value for {item.itemName}:{(int)_rarity}");
            return new SpawnableItemWithRarity
            {
                rarity = (int)_rarity,
                spawnableItem = item
            };*/

            return new SpawnableItemWithRarity
            {
                rarity = 50,
                spawnableItem = item
            };
        }

        private static SpawnableItemWithRarity GetSpawnableItemWithRarity(Item _item, int _rarity) // TODO: main function is to convert item to spawnableitemwithrarity
        {
            LoggerInstance.LogDebug($"Setting SpawnableItemWithRarity for item {_item.itemName}:{_rarity}");
            
            return new SpawnableItemWithRarity
            {
                rarity = _rarity,
                spawnableItem = _item
            };
        }
    }
}
