using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace ModHelp.Patches;

[HarmonyPatch]
class RefreshTopicsPatch
{
    [HarmonyPrefix, HarmonyPatch(typeof(UIBook), "RefreshTopics")]
    public static bool RefreshTopics(UIBook __instance)
    {

        if (ModHelpSetup.modc == null)
        {
            return true;
        }
        __instance.window.name = "MOD_UIBookHelp";
        string[] array = IO.LoadTextArray(Plugin.dir + "/LangMod/EN/Text/Help/modhelp.txt");

        List<UIList> lists = [__instance.list];
        __instance.list.Clear();
        __instance.list.callbacks = new UIList.Callback<UIBook.Item, ButtonCategory>
        {
            onClick = delegate (UIBook.Item a, ButtonCategory b)
            {


                if (a.items.Count > 0)
                {
                    if (b.gameObject.transform.Find("Image (1)").gameObject.activeInHierarchy == true)
                    {
                        var index = MetadataHelper.currentlyLoadedMetadata.FindIndex(m => m.guid == a.idFile);
                        var metadata = MetadataHelper.currentlyLoadedMetadata[index];
                        b.gameObject.transform.Find("Image (1)").SetActive(false);
                        b.gameObject.transform.Find("Text").GetComponent<RectTransform>().anchoredPosition -= new Vector2(15, 0);
                        b.gameObject.transform.Find("Image (1)").GetComponent<RectTransform>().anchoredPosition -= new Vector2(15, 0);
                        metadata.updated = false;
                        MetadataHelper.currentlyLoadedMetadata[index] = metadata;
                        MetadataHelper.SaveMetadata();
                    }
                    b.buttonFold.onClick.Invoke();
                }
                else
                {
                    if (b.gameObject.transform.Find("Image (1)").gameObject.activeInHierarchy == true)
                    {
                        var index = MetadataHelper.currentlyLoadedMetadata.FindIndex(m => m.guid == a.idFile);
                        var metadata = MetadataHelper.currentlyLoadedMetadata[index];
                        metadata.updated = false;
                        MetadataHelper.currentlyLoadedMetadata[index] = metadata;
                        b.gameObject.transform.Find("Image (1)").SetActive(false);
                        MetadataHelper.SaveMetadata();
                    }

                    __instance.idFile = a.idFile;
                    __instance.idTopic = a.id;
                    __instance.textTitle.SetText(a.title);
                    __instance.BuildPages();
                    __instance.ShowPage();
                }
            },
            onInstantiate = delegate (UIBook.Item a, ButtonCategory b)
            {
                Metadata metadata = MetadataHelper.currentlyLoadedMetadata.FirstOrDefault(m => m.guid == a.idFile);
                b.mainText.text = a.title;
                if (metadata.updated)
                {
                    b.gameObject.transform.Find("Image (1)").SetActive(true);
                }
                bool flag = false;
                if (a.items.Count > 0)
                {
                    if (metadata.updated)
                    {
                        b.gameObject.transform.Find("Text").GetComponent<RectTransform>().anchoredPosition += new Vector2(15, 0);
                        b.gameObject.transform.Find("Image (1)").GetComponent<RectTransform>().anchoredPosition += new Vector2(15, 0);
                    }
                    foreach (UIBook.Item item4 in a.items)
                    {
                        if (item4.idFile == __instance.idFile && item4.id == __instance.idTopic)
                        {
                            flag = true;
                        }
                    }
                }

                b.SetFold(a.items.Count > 0, !flag, delegate (UIList l)
                {
                    lists.Add(l);
                    foreach (UIBook.Item item5 in a.items)
                    {
                        l.Add(item5);
                    }
                });
            },
            onRefresh = null
        };
        string oldValue = '\t'.ToString();
        UIBook.Item item = null;
        UIBook.Item item2 = null;
        UIBook.helpTitles.Clear();
        string[] array2 = array;
        for (int i = 0; i < array2.Length; i++)
        {
            string[] array3 = array2[i].Replace(oldValue, "").Split(',', (char)StringSplitOptions.None);
            string[] array4 = array3[0].Split('-', (char)StringSplitOptions.None);
            UIBook.Item item3 = new UIBook.Item
            {
                idFile = array4[0],
                id = array4[1],
                title = array3[1]
            };
            UIBook.helpTitles[array3[0]] = item3.title.Replace("$", "");
            if (item3.title.StartsWith("$"))
            {
                item = item3;
                item3.title = item3.title.TrimStart('$');
                __instance.list.Add(item3);
            }
            else if (item != null)
            {
                item.items.Add(item3);
            }
            else
            {
                __instance.list.Add(item3);
            }

            if (item3.idFile == __instance.idFirstFile && item3.id == __instance.idFirstTopic)
            {
                item2 = item3;
                __instance.idFirstFile = null;
            }
        }

        __instance.list.Refresh();
        if (item2 != null)
        {
            foreach (UIList item6 in lists)
            {
                item6.Select(item2, invoke: true);
            }
        }
        else if (__instance.list.children.Count > 0)
        {
            __instance.list.children.FirstItem().Select(0, invoke: true);
        }
        else
        {
            __instance.list.Select(0, invoke: true);
        }

        List<object> items = __instance.list.items.ToList();
        for (int i = 1; i < items.Count; i++)
        {
            UIBook.Item bitem = items[i] as UIBook.Item;
            var metadata = MetadataHelper.currentlyLoadedMetadata.FirstOrDefault(m => ModHelpSetup.SanitizeId(m.guid) == bitem.idFile);
            if (metadata.updated)
            {
                items.RemoveAt(i);
                items.Insert(1, bitem);
            }
        }

        __instance.list.items = items;

        __instance.list.Refresh();

        SkinManager.tempSkin = null;
        return false;
    }
}
