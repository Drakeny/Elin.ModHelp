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
    //FreshCloth made this.
    [HarmonyPrefix, HarmonyPatch(typeof(UINote), "AddImage", [typeof(string)])]
    public static bool AddImage(UINote __instance, string idFile)
    {
        if (ModHelpSetup.modc == null) return true;

        if (!SpriteReplacer.dictModItems.TryGetValue(idFile, out var path))
        {
            return false;
        }

        var image = Util.Instantiate<UIItem>("UI/Element/Deco/ImageNote", __instance.layout).image1;
        image.sprite = $"{path}.png".LoadSprite();
        if (image.sprite == null)
        {
            image.transform.parent.SetActive(enable: false);
            return false;
        }

        image.preserveAspect = true;
        image.SetNativeSize();

        var layout = __instance.layout;
        var available = layout.preferredWidth - (layout.padding.left + layout.padding.right) * 4;
        var ratio = image.sprite.texture.width / available;
        if (ratio < 1f)
        {
            return false;
        }

        var size = image.rectTransform.sizeDelta;
        image.rectTransform.sizeDelta = size with { x = size.x / ratio };

        return false;
    }
}