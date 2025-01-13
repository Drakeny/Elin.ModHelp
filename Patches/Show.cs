using System;
using HarmonyLib;

namespace ModHelp.Patches;

[HarmonyPatch]
public class ShowPatch
{
    [HarmonyPrefix, HarmonyPatch(typeof(UIBook), "Show", new Type[] { typeof(string), typeof(string), typeof(string), typeof(BookList.Item) })]
    public static bool Show(UIBook __instance, string _idFile, string _idTopic, BookList.Item _bookItem)
    {
        if (__instance.window.name != "MOD_UIBookHelp") return true;
        __instance.idFile = _idFile;
        __instance.idTopic = _idTopic;
        __instance.bookItem = _bookItem;
        __instance.BuildPages();
        __instance.Show();
        return false;
    }

}