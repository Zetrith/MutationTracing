using HarmonyLib;
using Verse;

namespace MutationTracing;

public class MutationTracing : Mod
{
    public MutationTracing(ModContentPack content) : base(content)
    {
        new Harmony("mutation_tracing").PatchAll();
    }
}
