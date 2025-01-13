using Cwl.Helper.FileUtil;
using HarmonyLib;

namespace ModHelp.Patches;

[HarmonyPatch]
public class BuildPagesPatch
{
    public static string currentlyBuildingFile = string.Empty;
    [HarmonyPrefix, HarmonyPatch(typeof(UIBook), "BuildPages")]
    public static bool BuildPages(UIBook __instance)
    {
        if (__instance.window.name != "MOD_UIBookHelp") return true;

        currentlyBuildingFile = __instance.idFile;

        __instance.pages.Clear();
        string[] array = null;

        var DefaultLocHelpFile = PackageIterator.GetRelocatedFileFromPackage($"Text/Help/DefaultFiles/{__instance.idFile}.txt", Plugin.id);
        var locHelpFile = PackageIterator.GetRelocatedFileFromPackage("Text/Help/help.txt", __instance.idFile);

        if (locHelpFile != null)
        {
            array = IO.LoadTextArray(locHelpFile.FullName);
        }
        else
        {
            array = IO.LoadTextArray(DefaultLocHelpFile.FullName);
        }

        //--- Original code
        UIBook.Page page = new();
        int num = 0;
        string[] array2 = array;
        foreach (string text in array2)
        {
            if (__instance.bookItem != null && num == 0)
            {
                num++;
                continue;
            }
            if (text == "{p}")
            {
                __instance.AddPage(page);
                page = new UIBook.Page();
            }
            page.lines.Add(text);
            num++;
        }
        __instance.AddPage(page);

        return false;
    }
}