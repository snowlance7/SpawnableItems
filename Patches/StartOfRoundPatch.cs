/*using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using GameNetcodeStuff;
using BepInEx;
using SpawnableItems;

namespace ForceTeleportAll
{
    [HarmonyPatch]
    internal static class StartOfRoundPatch
    {
        private static readonly ManualLogSource LoggerInstance = SpawnableItemsBase.LoggerInstance;
        private static Terminal Terminal;

        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        [HarmonyPostfix]
        private static void GetItems(StartOfRound __instance)
        {
            List<Item> items = __instance.allItemsList.itemsList;
            LoggerInstance.LogDebug($"Got {items.Count} items...");
            foreach (Item item in items)
            {
                LoggerInstance.LogDebug(item.itemName);
            }

            LoggerInstance.LogInfo($"Getting buyable items...");
            List<Item> buyableItems = Terminal.buyableItemsList.ToList();
            LoggerInstance.LogDebug($"Got {buyableItems.Count} buyable items...");

            foreach (Item item in buyableItems)
            {
                LoggerInstance.LogDebug(item.itemName);
            }

            for (int k = 0; k < Terminal.terminalNodes.allKeywords.Length; k++)
            {
                TerminalKeyword keyword = Terminal.terminalNodes.allKeywords[k];
                if (!(keyword != null) || keyword.compatibleNouns == null)
                {
                    continue;
                }
                for (int m = 0; m < keyword.compatibleNouns.Length; m++)
                {
                    CompatibleNoun noun = keyword.compatibleNouns[m];
                    if (!(noun.result != null))
                    {
                        continue;
                    }
                    if (noun.result.buyItemIndex >= 0)
                    {
                        if (noun.result.buyItemIndex < Terminal.buyableItemsList.Length)
                        {
                            Item item2 = Terminal.buyableItemsList[noun.result.buyItemIndex];
                            if (item2 == null)
                            {
                                Plugin.Log.LogWarning((object)("The item " + noun.noun.word + "(" + noun.result.name + ") is null? This is unexpected behaviour. Item wont be added."));
                                continue;
                            }
                            if (!BuyableItems.Contains(item2))
                            {
                                Plugin.Log.LogMessage((object)("Found buyable item " + item2.itemName));
                                BuyableItems.Add(item2);
                            }
                            if (!AllItems.Contains(item2))
                            {
                                AllItems.Add(item2);
                            }
                        }
                        else
                        {
                            Plugin.Log.LogWarning((object)("The item " + noun.noun.word + "(" + noun.result.name + ") wasn't added to buyableItemsList. This is unexpected behaviour. Item wont be added."));
                        }
                    }
                    else
                    {
                        if (noun.result.shipUnlockableID < 0 || StartOfRound.Instance.unlockablesList.unlockables.Count <= noun.result.shipUnlockableID)
                        {
                            continue;
                        }
                        UnlockableItem unlockable2 = StartOfRound.Instance.unlockablesList.unlockables[noun.result.shipUnlockableID];
                        UnlockablePrices[unlockable2] = noun.result.itemCost;
                        if (unlockable2.alwaysInStock)
                        {
                            if (!ShipUpgrades.Contains(unlockable2))
                            {
                                Plugin.Log.LogMessage((object)("Found unlockable ship upgrade " + unlockable2.unlockableName));
                                ShipUpgrades.Add(unlockable2);
                            }
                        }
                        else if (!Decorations.Contains(unlockable2))
                        {
                            Plugin.Log.LogMessage((object)("Found unlockable ship decoration " + unlockable2.unlockableName));
                            Decorations.Add(unlockable2);
                        }
                    }
                }
            }
        }
    }
}*/