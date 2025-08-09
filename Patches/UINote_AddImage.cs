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

        string path = PackageIterator.GetLoadedPackages(BuildPagesPatch.currentlyBuildingFile).FirstOrDefault()?.FullName + $"/Texture/{idFile}.png";
        Plugin.Log.LogDebug($"Current image path {path}");

        if (!File.Exists(path))
        {
            //return false;
        }


        var image = Util.Instantiate<UIItem>("UI/Element/Deco/ImageNote", __instance.layout).image1;
        image.sprite = path.LoadSprite();

        if (image.sprite == null)
        {
            image.transform.parent.SetActive(false);
            return false;
        }

        image.preserveAspect = true;

        var layout = __instance.layout;
        var layoutRect = layout.GetComponent<RectTransform>();

        CoroutineHelper.Deferred(() =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);

            float totalWidth = layoutRect.rect.width;
            float paddingX = layout.padding.left + layout.padding.right;

            float availableWidth = totalWidth - paddingX;

            // Fixed maximum height in pixels
            const float maxDisplayHeight = 400f;

            if (availableWidth <= 0)
                return;

            var texture = image.sprite.texture;
            float texWidth = texture.width;
            float texHeight = texture.height;

            // Calculate scale needed for width and height separately
            float scaleByWidth = texWidth / availableWidth;
            float scaleByHeight = texHeight / maxDisplayHeight;

            // Pick the larger scale (so image fits both width & height)
            float scale = Math.Max(scaleByWidth, scaleByHeight);

            float newWidth = texWidth;
            float newHeight = texHeight;

            if (scale > 1f)
            {
                newWidth /= scale;
                newHeight /= scale;
            }

            image.rectTransform.sizeDelta = new Vector2(newWidth, newHeight);

            var layoutElement = image.GetComponent<LayoutElement>() ?? image.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = newWidth;
            layoutElement.preferredHeight = newHeight;

            Plugin.Log.LogInfo($"Final image size: {newWidth}x{newHeight} (scale {scale})");

            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);

        }, 2);
        return false;
    }
}