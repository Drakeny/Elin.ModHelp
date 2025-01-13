using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using System.Text;
using Cwl.Helper.FileUtil;
using Cwl.Helper.Unity;

namespace ModHelp;

[HarmonyPatch]
public static class ModHelpSetup
{
    public static Layer modc = null;
    public static UIText headerText = null;
    public static UIText topicsText = null;
    public static Dictionary<string, FileInfo> allHelpFilesDict = null;
    [HarmonyPostfix, HarmonyPatch(typeof(Core), "Update")]
    public static void Update(Core __instance)
    {
        if (Input.GetKeyDown(KeyCode.F1) && Input.GetKey(KeyCode.LeftShift))
        {
            //TODO: figure this out, right now using shift+f1 will open both help windows
            // MetadataHelper.LoadMetadata();
            // WritePackagesToFile();
            // CoroutineHelper.Deferred(() =>
            // {
            //     CallModHelp();
            // }, 5);
        }
    }

    public static void CallModHelp()
    {
        if (modc == null)
        {
            modc = Resources.Load<Layer>("UI/Layer/LayerHelp");
            modc = Object.Instantiate(modc);
        }
        if (!EClass.ui.layers.Contains(modc))
        {
            EClass.ui.AddLayer(modc);
        }
        (modc as LayerHelp).book.RefreshTopics();
        var loc = EClass.core.config.lang;
        switch (loc)
        {
            case "EN":
                (modc as LayerHelp).book.Show("drakenydev_elin_modhelp", "About");
                break;
            case "CN":
                (modc as LayerHelp).book.Show("drakenydev_elin_modhelp", "关于");
                break;
            case "JP":
                (modc as LayerHelp).book.Show("drakenydev_elin_modhelp", "概要");
                break;
        }

        // Changed window name for identification
        headerText = GameObject.Find("UI/Layers/LayerHelp(Clone)/MOD_UIBookHelp/Caption/GameObject/text caption").GetComponent<UIText>();
        headerText.text = "Mod Help";

        //Prebuild list of existing help files for search
        allHelpFilesDict = GetRelocatedFilesDictFromPackage("Text/Help/help.txt");
        UIBook.searchContext.Init();


        CoroutineHelper.Deferred(() =>
        {
            topicsText = GameObject.Find("UI/Layers/LayerHelp(Clone)/MOD_UIBookHelp/Content View/UIBook Help/Scrollview parchment with Header (1)/Header Top Parchment/UIText").GetComponent<UIText>();
            topicsText.text = "Installed Mods";
        }, 5);
    }



    public static void WritePackagesToFile()
    {
        using StreamWriter file = new(Plugin.dir + "/LangMod/EN/Text/Help/modhelp.txt");
        file.WriteLine("drakenydev_elin_modhelp-,$【Mod Help】");
        string[] mhtopics = GetFileTopics("drakenydev_elin_modhelp");
        foreach (string topic in mhtopics)
        {
            file.WriteLine($"drakenydev_elin_modhelp-{topic},{topic}");
        }

        foreach (BaseModPackage package in ELayer.core.mods.packages)
        {
            if (package.builtin || !package.activated || package.id == "drakenydev_elin_modhelp") continue;
            //if (!package.dirInfo.GetFiles().Any(f => f.Name.EndsWith(".dll"))) continue;

            string[] topics = GetFileTopics(package.id);
            if (topics == null)
            {
                WriteDefaultHelp(package.id, package.title);
                StringBuilder defaultPackageListItem = new StringBuilder();
                defaultPackageListItem.Append(package.id.Replace("-", "_"));
                defaultPackageListItem.Append("-default,");
                defaultPackageListItem.Append("$" + package.title);
                file.WriteLine(defaultPackageListItem.ToString());
                continue;
            }

            StringBuilder packageListItem = new StringBuilder();
            packageListItem.Append(package.id.Replace("-", "_"));
            packageListItem.Append("-,");
            packageListItem.Append("$" + package.title);
            file.WriteLine(packageListItem.ToString());
            foreach (string topic in topics)
            {
                file.WriteLine($"{package.id.Replace("-", "_")}-{topic},{topic}");
            }
        }
    }

    private static void WriteDefaultHelp(string id, string title)
    {
        string loc = EClass.core.config.lang;
        Metadata metadata = MetadataHelper.currentlyLoadedMetadata.FirstOrDefault(m => m.guid == id);
        using StreamWriter file = new(Plugin.dir + "/LangMod/" + loc + "/Text/Help/DefaultFiles/" + id.Replace("-", "_") + ".txt");
        file.WriteLine("$default");
        switch (loc)
        {
            case "EN":
                file.WriteLine("");
                file.WriteLine("{topic|" + title + " - Updates}");
                file.WriteLine("{pair|Latest Update|" + metadata.newestUpdateDate + "}");
                file.WriteLine("{pair|Previous Update|" + (metadata.lastUpdatedDate ?? "Never updated previously") + "}");
                file.WriteLine("<size=12><i>*Dates are based on when the mod files were modified on your pc, they don't reflect the actual date of the updates. Manually modifying the mod files will also trigger updates to these dates.*</i></size>");
                file.WriteLine("");
                file.WriteLine("");
                file.WriteLine("");
                file.WriteLine("{layout,NoteHelp2}");
                file.WriteLine("<color=#00BFFF>This is a default page created by Mod Help. \nThis means that there is no help file provided for this mod.</color>");
                file.WriteLine("{layout}");
                file.WriteLine("");
                file.WriteLine("");
                file.WriteLine("");
                file.WriteLine("{link|View on Steam Workshop| https://steamcommunity.com/sharedfiles/filedetails/?id=" + metadata.name + "}");
                break;

            case "CN":
                file.WriteLine("");
                file.WriteLine("{topic|" + title + " - 更新}");
                file.WriteLine("{pair|最新更新|" + metadata.newestUpdateDate + "}");
                file.WriteLine("{pair|上次更新|" + (metadata.lastUpdatedDate ?? "之前从未更新") + "}");
                file.WriteLine("<size=12><i>*日期基于您的电脑上修改模组文件的时间。这些日期不反映实际更新日期。手动修改模组文件也会触发这些日期的更新。*</i></size>");
                file.WriteLine("");
                file.WriteLine("");
                file.WriteLine("");
                file.WriteLine("{layout,NoteHelp2}");
                file.WriteLine("<color=#00BFFF>这是由 Mod Help 创建的默认页面。\n这意味着此模组没有提供帮助文件。</color>");
                file.WriteLine("{layout}");
                file.WriteLine("");
                file.WriteLine("");
                file.WriteLine("");
                file.WriteLine("{link|在 Steam 创意工坊查看| https://steamcommunity.com/sharedfiles/filedetails/?id=" + metadata.name + "}");
                break;

            case "JP":
                file.WriteLine("");
                file.WriteLine("{topic|" + title + " - 更新履歴}");
                file.WriteLine("{pair|最新の更新|" + metadata.newestUpdateDate + "}");
                file.WriteLine("{pair|以前の更新|" + (metadata.lastUpdatedDate ?? "これまで更新されていません") + "}");
                file.WriteLine("<size=12><i>*日付はお使いのPCでモッドファイルが変更された日時に基づいています。実際の更新日時を反映するものではありません。手動でモッドファイルを変更すると、これらの日付も更新されます。*</i></size>");
                file.WriteLine("");
                file.WriteLine("");
                file.WriteLine("");
                file.WriteLine("{layout,NoteHelp2}");
                file.WriteLine("<color=#00BFFF>これは Mod Help によって作成されたデフォルトのページです。\nこのモッドにはヘルプファイルが提供されていません。</color>");
                file.WriteLine("{layout}");
                file.WriteLine("");
                file.WriteLine("");
                file.WriteLine("");
                file.WriteLine("{link|Steam ワークショップで表示| https://steamcommunity.com/sharedfiles/filedetails/?id=" + metadata.name + "}");
                break;

            default:
                file.WriteLine("Hello I'm default for " + id);
                break;
        }
    }

    public static string[] GetFileTopics(string packageId)
    {
        var help = PackageIterator.GetRelocatedFileFromPackage("Text/Help/help.txt", packageId);

        if (help == null) return null;

        var lines = File.ReadAllLines(help.FullName);
        var topics = lines.Where(l => l.StartsWith("$")).Select(l => l.TrimStart('$')).ToArray();

        return topics;
    }

    public static Dictionary<string, FileInfo> GetRelocatedFilesDictFromPackage(string relativePath)
    {
        string relativePath2 = relativePath;
        return (from p in BaseModManager.Instance.packages
                where p.activated && !p.builtin
                let file = PackageIterator.GetRelocatedFileFromPackage(relativePath2, p.id)
                where file?.FullName != null
                select new { PackageId = p.id, FileInfo = file })
                .ToDictionary(x => x.PackageId, x => x.FileInfo);
    }

    public static Dictionary<string, DirectoryInfo> GetLoadedPackagesDict()
    {

        return (from p in BaseModManager.Instance.packages
                where p.activated && !p.builtin
                let dir = PackageIterator.GetLoadedPackages(p.id)
                where dir?.FirstOrDefault() != null
                select new { PackageId = p.id, DirectoryInfo = dir.FirstOrDefault() })
                .ToDictionary(x => x.PackageId, x => x.DirectoryInfo);
    }

    public static void BuildAboutFile()
    {
        var lastUpdates = MetadataHelper.GetRecentlyUpdatedMetadata();
        var loc = EClass.core.config.lang;
        using StreamWriter file = new(Plugin.dir + "/LangMod/" + loc + "/Text/Help/about.txt");
        switch (loc)
        {
            case "EN":
                file.WriteLine("$About");
                file.WriteLine("{topic|Welcome to Mod Help!}");
                file.WriteLine("{pair|Tip:|Mods listed with a small crystal next to their names have been updated recently.}");
                file.WriteLine("{pair|Tip:|You can use the search bar to find information about mods that have provided help files.}");
                file.WriteLine("");
                file.WriteLine($"・ You have {MetadataHelper.currentlyLoadedMetadata.Count} mods loaded.");
                file.WriteLine("");
                file.WriteLine("・ If you're a mod creator, you can include your own help files, which will automatically be loaded here, keeping everything organized and accessible. Check the 'For creators' section for more information.");
                file.WriteLine("{topic|Recently Updated}");
                break;

            case "CN":
                file.WriteLine("$关于");
                file.WriteLine("{topic|欢迎使用 Mod Help！}");
                file.WriteLine("{pair|提示：|带有小水晶图标的模组表示最近已更新。}");
                file.WriteLine("{pair|提示：|您可以使用搜索栏查找提供帮助文件的模组信息。}");
                file.WriteLine("");
                file.WriteLine($"・ 您已加载 {MetadataHelper.currentlyLoadedMetadata.Count} 个模组。");
                file.WriteLine("");
                file.WriteLine("・ 如果您是模组作者，可以包含自己的帮助文件，它们会自动加载到这里，让一切井然有序且易于访问。请查看“适用于创作者”部分了解更多信息。");
                file.WriteLine("{topic|最近更新}");
                break;

            case "JP":
                file.WriteLine("$概要");
                file.WriteLine("{topic|Mod Help へようこそ！}");
                file.WriteLine("{pair|ヒント：|名前の横に小さなクリスタルが付いているモッドは、最近更新されています。}");
                file.WriteLine("{pair|ヒント：|検索バーを使用して、ヘルプファイルを提供しているモッドの情報を見つけることができます。}");
                file.WriteLine("");
                file.WriteLine($"・ 現在、{MetadataHelper.currentlyLoadedMetadata.Count} 個のモッドが読み込まれています。");
                file.WriteLine("");
                file.WriteLine("・ モッド制作者の場合、独自のヘルプファイルを追加することで、ここに自動的に読み込まれ、整理されて使いやすくなります。「クリエイター向け」のセクションをご覧ください。");
                file.WriteLine("{topic|最近更新}");
                break;

        }

        foreach (var lastUpdate in lastUpdates)
        {
            string title = (from p in BaseModManager.Instance.packages where p.id == lastUpdate.guid select p.title).FirstOrDefault();
            file.WriteLine("{pair|" + title + "|" + lastUpdate.newestUpdateDate + "}");
        }
        file.Close();

        BuildHelpFile();
    }

    public static void BuildHelpFile()
    {
        var lines = new List<string>();
        var about = PackageIterator.GetRelocatedFileFromPackage("Text/Help/about.txt", Plugin.id);
        var creators = PackageIterator.GetRelocatedFileFromPackage("Text/Help/Creators.txt", Plugin.id);
        var changelog = PackageIterator.GetRelocatedFileFromPackage("Text/Help/Changelog.txt", Plugin.id);
        if (about != null)
        {
            lines.AddRange(File.ReadAllLines(about.FullName));
        }
        if (creators != null)
        {
            lines.AddRange(File.ReadAllLines(creators.FullName));
        }
        if (changelog != null)
        {
            lines.AddRange(File.ReadAllLines(changelog.FullName));
        }
        File.WriteAllLines(Plugin.dir + "/LangMod/" + EClass.core.config.lang + "/Text/Help/help.txt", lines);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HotItemContext), "Show")]
    public static void Show(string id)
    {
        if (!(EClass.ui.contextMenu.currentMenu == null) && id == "system")
        {
            UIContextMenu uIContextMenu = EClass.ui.contextMenu.currentMenu;

            var a = uIContextMenu.AddButton("Mod Help", delegate
            {
                MetadataHelper.LoadMetadata();
                BuildAboutFile();

                WritePackagesToFile();

                CoroutineHelper.Deferred(() =>
                {
                    CallModHelp();
                }, 5);
            });

            a.gameObject.transform.SetSiblingIndex(11);
        }
    }


}

