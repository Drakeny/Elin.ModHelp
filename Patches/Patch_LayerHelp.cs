using HarmonyLib;

namespace ModHelp;

[HarmonyPatch]
class Patch_LayerHelp
{

    [HarmonyPrefix, HarmonyPatch(typeof(LayerHelp), "Toggle")]
    public static bool Toggle(LayerHelp __instance)
    {
        //Making sure search context is cleared for normal help
        UIBook.searchContext.items.Clear();
        UIBook.searchContext.result.Clear();
        UIBook.searchContext.Init();
        return true;
    }
}