using System;
using System.Collections.Generic;
using System.IO;
// using System.IO.Hashing;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Cwl.Helper.String;

namespace ModHelp;

struct Metadata
{
    public string name;
    public string guid;
    public string lastUpdatedDate;
    public string newestUpdateDate;
    public string fileHash;
    public bool updated;
}

class MetadataHelper
{
    public static List<Metadata> cachedMetadata = [];
    public static List<Metadata> currentlyLoadedMetadata = [];

    //Build metadata on first run
    public static void BuildMetadata()
    {
        var dirDict = ModHelpSetup.GetLoadedPackagesDict();
        if (!File.Exists(Plugin.dir + "/metadata.json"))
        {
            string json = JsonConvert.SerializeObject(cachedMetadata, Formatting.Indented);
            File.WriteAllText(Plugin.dir + "/metadata.json", json);
            cachedMetadata = currentlyLoadedMetadata;
            foreach (var entry in dirDict)
            {
                Metadata metadata = new Metadata();
                metadata.name = entry.Value.Name;
                metadata.guid = entry.Key;
                metadata.newestUpdateDate = GetLastModified(entry.Value.FullName).ToString("yyyy-MM-dd HH:mm:ss");
                metadata.fileHash = MakeMd5(entry.Value);
                metadata.updated = false;
                currentlyLoadedMetadata.Add(metadata);
            }
            return;
        }

        foreach (var entry in dirDict)
        {
            Metadata metadata = new Metadata();
            metadata.name = entry.Value.Name;
            metadata.guid = entry.Key;
            metadata.newestUpdateDate = GetLastModified(entry.Value.FullName).ToString("yyyy-MM-dd HH:mm:ss");
            metadata.fileHash = MakeMd5(entry.Value);
            var cmtd = cachedMetadata.Find(m => m.guid == metadata.guid);
            metadata.updated = cmtd.IsNull() ? false : cmtd.updated;
            currentlyLoadedMetadata.Add(metadata);
        }
    }

    public static void LoadMetadata()
    {
        if (File.Exists(Plugin.dir + "/metadata.json"))
        {
            cachedMetadata.Clear();
            string json = File.ReadAllText(Plugin.dir + "/metadata.json");
            cachedMetadata = JsonConvert.DeserializeObject<List<Metadata>>(json);
        }
        BuildMetadata();
        UpdateMetadata();
    }


    public static void UpdateMetadata()
    {
        var dirDict = ModHelpSetup.GetLoadedPackagesDict();
        foreach (var entry in dirDict)
        {
            //check if there are new metadata to add
            if (!currentlyLoadedMetadata.Any(m => m.name == entry.Value.Name))
            {
                Metadata metadata = new Metadata();
                metadata.name = entry.Value.Name;
                metadata.guid = entry.Key;
                metadata.lastUpdatedDate = GetLastModified(entry.Value.FullName).ToString("yyyy-MM-dd HH:mm:ss");
                metadata.newestUpdateDate = GetLastModified(entry.Value.FullName).ToString("yyyy-MM-dd HH:mm:ss");
                metadata.fileHash = MakeMd5(entry.Value);
                metadata.updated = false;
                currentlyLoadedMetadata.Add(metadata);
            }
        }

        //check if any need to be deleted
        foreach (var metadata in currentlyLoadedMetadata)
        {
            if (!dirDict.Any(d => d.Value.Name == metadata.name))
            {
                currentlyLoadedMetadata.Remove(metadata);
            }
        }

        // update existing
        for (int i = 0; i < currentlyLoadedMetadata.Count; i++)
        {
            var metadata = currentlyLoadedMetadata[i];
            var cached = cachedMetadata.FirstOrDefault(m => m.name == metadata.name);
            if (cached.fileHash != metadata.fileHash)
            {
                metadata.lastUpdatedDate = cached.newestUpdateDate;
                metadata.newestUpdateDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                metadata.updated = true;
            }
            currentlyLoadedMetadata[i] = metadata;
        }

        SaveMetadata();
    }

    public static void SaveMetadata()
    {
        cachedMetadata = currentlyLoadedMetadata;
        string json = JsonConvert.SerializeObject(currentlyLoadedMetadata, Formatting.Indented);
        File.WriteAllText(Plugin.dir + "/metadata.json", json);
    }

    public static DateTime GetLastModified(string path)
    {
        return File.GetLastWriteTime(path);
    }

    // public static string HashFolder(string folderPath)
    // {
    //     Crc32 crc32 = new();
    //     var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
    //                          .OrderBy(f => f) // Sort files for consistent order
    //                          .ToArray();

    //     foreach (var file in files)
    //     {
    //         var lastWriteTime = File.GetLastWriteTimeUtc(file).ToBinary();
    //         var filePathBytes = Encoding.UTF8.GetBytes(file);
    //         var lastWriteTimeBytes = BitConverter.GetBytes(lastWriteTime);

    //         crc32.Append(filePathBytes);
    //         crc32.Append(lastWriteTimeBytes);
    //     }

    //     byte[] hashBytes = crc32.GetCurrentHash();

    //     var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    //     return hash;
    // }


    //Hi Fresh, thanks for this, i've copied it uncerimoniously, because the CRC one would need to pack the dll along it seems.
    internal static string MakeMd5(DirectoryInfo dir)
    {
        var files = dir.GetFiles("*", SearchOption.AllDirectories)
            .OrderBy(f => f.LastWriteTime)
            .ToArray();

        using var md5 = MD5.Create();
        foreach (var file in files)
        {
            var pathBuf = Encoding.UTF8.GetBytes(file.ShortPath().ToLower());
            md5.TransformBlock(pathBuf, 0, pathBuf.Length, pathBuf, 0);

            var timeBuf = Encoding.UTF8.GetBytes(file.LastWriteTime.ToLongTimeString());
            md5.TransformBlock(timeBuf, 0, timeBuf.Length, timeBuf, 0);
        }

        var hashBuf = Encoding.UTF8.GetBytes(dir.FullName.ToLower());
        md5.TransformFinalBlock(hashBuf, 0, hashBuf.Length);

        return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
    }

    public static List<Metadata> GetRecentlyUpdatedMetadata()
    {
        return currentlyLoadedMetadata.Where(m => m.guid != Plugin.id).OrderByDescending(m => m.newestUpdateDate).Take(5).ToList();
    }


}
