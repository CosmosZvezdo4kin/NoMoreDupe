using HarmonyLib;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.ThrowableProjectiles;
using Mirror;
using Scp914;
using UnityEngine;

namespace NoMoreDupe.Patches;

[HarmonyPatch(typeof(Scp914.Processors.Scp914ItemProcessor), nameof(Scp914.Processors.Scp914ItemProcessor.UpgradeInventoryItem))]
public class Scp914ItemProcessor
{
    private static bool Prefix(Scp914.Processors.Scp914ItemProcessor __instance, Scp914KnobSetting setting, ItemBase sourceItem, ref Scp914Result __result)
    {
        if (sourceItem is ThrowableItem throwableItem && !throwableItem.AllowHolster)
        {
            __result = default;
            return false;
        }
        
        var sourcePickup = sourceItem.ServerDropItem(false);
        var scp914Result = __instance.UpgradePickup(setting, sourcePickup);
        
        if (scp914Result.ResultingPickups == null || scp914Result.ResultingPickups.Length == 0)
        {
            __result = scp914Result;
            return false;
        }
        
        var userInventory = sourceItem.OwnerInventory.UserInventory;
        
        __instance.ClearCombiner();
        
        foreach (var itemPickupBase in scp914Result.ResultingPickups)
        {
            if (itemPickupBase == null) 
                continue;
            
            if (itemPickupBase.GetSearchCompletor(sourceItem.Owner.searchCoordinator, 3.4028235E+38f).ValidateStart())
            {
                sourceItem.OwnerInventory.ServerAddItem(itemPickupBase.ItemId.TypeId, ItemAddReason.Scp914Upgrade, itemPickupBase.Info.Serial, itemPickupBase);
                    
                if (userInventory.Items.TryGetValue(itemPickupBase.Info.Serial, out var resultingItem))
                    __instance.AddResultToCombiner(resultingItem);
                
                if (!NetworkServer.spawned.ContainsKey(itemPickupBase.netId))
                    Object.Destroy(itemPickupBase.gameObject);
                else
                    itemPickupBase.DestroySelf();
            }
            else
            {
                __instance.AddResultToCombiner(itemPickupBase);
                itemPickupBase.Position = sourceItem.Owner.transform.position;
                
                if (!NetworkServer.spawned.ContainsKey(itemPickupBase.netId))
                    NetworkServer.Spawn(itemPickupBase.gameObject);
            }
        }
        
        __result = __instance.GenerateResultFromCombiner(sourceItem);
        return false;
    }
}