using System;
using System.Linq;
using HarmonyLib;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;

namespace NoMoreDupe;

public class Core : Plugin
{
    public override string Name => "NoMoreDupe";

    public override string Description => "Fixes SCP-914 dupe";

    public override string Author => "CosmosZvezdochkin";

    public override Version Version => new(1, 0, 0);

    public override Version RequiredApiVersion => new(1, 1, 1);
    
    private Harmony _harmony;
    
    public override void Enable()
    {
        PatchAll();
    }

    public override void Disable()
    {
        UnpatchAll();
    }

    private void PatchAll()
    {
        _harmony ??= new Harmony($"{Author}.{Name}.{DateTime.Now.Ticks}");
        
        _harmony.PatchAll();
        
        Logger.Info($"Successfully patched {_harmony.GetPatchedMethods()?.Count() ?? 0} method(-s)");
    }

    private void UnpatchAll()
    {
        _harmony?.UnpatchAll();
        
        _harmony = null;
    }
}