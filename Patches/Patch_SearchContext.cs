using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;

namespace ModHelp;

[HarmonyPatch]
class Patch_SearchContext
{
    [HarmonyPrefix, HarmonyPatch(typeof(SearchContext), "Init")]
    public static bool Init(SearchContext __instance)
    {
        if (ModHelpSetup.modc == null) return true;

        __instance.items.Clear();
        __instance.result.Clear();

        foreach (KeyValuePair<string, FileInfo> fileInfo in ModHelpSetup.allHelpFilesDict)
        {
            if (fileInfo.Value.Extension != ".txt" || fileInfo.Value.Name == "_topics.txt" || fileInfo.Value.Name == "include.txt")
            {
                continue;
            }
            string[] array = IO.LoadTextArray(fileInfo.Value.FullName);
            string idFile = fileInfo.Key.ToString();
            string idTopic = "";
            string[] array2 = array;
            foreach (string text in array2)
            {
                if (text.Length == 0 || text.IsEmpty() || text == Environment.NewLine)
                {
                    continue;
                }
                if (text[0] == '$')
                {
                    idTopic = text.Replace("$", "");
                    continue;
                }
                bool flag = false;
                if (text[0] == '{')
                {
                    if (text.Length < 6)
                    {
                        continue;
                    }
                    switch (text.Substring(0, 3))
                    {
                        case "{A|":
                        case "{Q|":
                            flag = true;
                            break;
                        case "{pa":
                            break;
                        default:
                            continue;
                    }
                }
                SearchContext.Item item = new SearchContext.Item();
                if (flag)
                {
                    item.textSearch = (item.text = text.Split('|')[1].Replace("}", "")).ToLower();
                }
                else
                {
                    item.text = text;
                    item.textSearch = text.ToLower();
                }
                item.idFile = idFile;
                item.idTopic = idTopic;
                __instance.items.Add(item);
            }
        }


        return false;
    }
}