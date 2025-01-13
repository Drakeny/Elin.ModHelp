using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Cwl.Helper.FileUtil;
using Cwl.Helper.Unity;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ModHelp.Patches;

[HarmonyPatch]
class UINote_AddImage
{
    [HarmonyPrefix, HarmonyPatch(typeof(UINote), "AddImage", [typeof(string)])]
    public static bool AddImage(UINote __instance, string idFile)
    {
        var methods = __instance.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

        var refImage = methods.SingleOrDefault(m => m.Name == "Load" && m.ReturnType == typeof(UIItem) && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));

        UIItem utem = (UIItem)refImage.Invoke(__instance, ["UI/Element/Deco/ImageNote"]);
        Image image = utem.image1;

        string path = PackageIterator.GetLoadedPackages(BuildPagesPatch.currentlyBuildingFile).Select(d => d.GetFiles(idFile + ".png", SearchOption.AllDirectories)).FirstOrDefault()?.FirstOrDefault()?.FullName;

        Sprite sprite = image.sprite = path.LoadSprite();
        image.SetNativeSize();
        image.transform.parent.Rect().sizeDelta = image.Rect().sizeDelta;

        if (sprite == null)
        {
            image.transform.parent.SetActive(enable: false);
        }

        return false;
    }
}