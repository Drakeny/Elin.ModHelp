using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ModHelp;

//TODO: Patch UINote::AddButtonLink to add a distinctive button background
//TODO: Patch UIBook::BuildNote to a reversal for the {center} tag

[BepInPlugin("DrakenyDev.Elin.ModHelp", "ModHelp", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log;
    private static Harmony harmony;

    public static string dir;
    public static string id = "drakenydev_elin_modhelp";
    public void OnStartCore()
    {
        dir = Path.GetDirectoryName(Info.Location);
    }

    private void Start()
    {

        Log = base.Logger;
        harmony = new Harmony("DrakenyDev.Elin.ModHelp");
        harmony.PatchAll();
        Log.LogInfo("DrakenyDev.Elin.ModHelp loaded");
    }
}

