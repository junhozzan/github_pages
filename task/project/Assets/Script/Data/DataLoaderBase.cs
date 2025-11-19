using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;

public abstract class DataLoaderBase
{
#if UNITY_EDITOR
    private readonly List<DataBase> allDatas = new();
#endif

    public abstract void Load();

#if UNITY_EDITOR
    public void Verify(DataManager manager)
    {
        foreach (var data in allDatas)
        {
            data.Verify(manager);
        }
    }
#endif

    protected IEnumerable<T> LoadResources<T>(Func<SmartXmlNode, T> constructor, params string[] files) where T : DataBase
    {
        var byteDatas = new List<byte[]>();
        foreach (var file in files)
        {
            LoadFile(file, byteDatas);
        }

        var result = ByteDatasToNodes(byteDatas).Select(x => constructor(x));

#if UNITY_EDITOR
        allDatas.AddRange(result);
#endif

        return result;
    }

    private static void LoadFile(string fileName, List<byte[]> bytes)
    {
        TextAsset ta = FakeAddressableManager.Instance.LoadTextAsset(fileName);
        bytes.Add(ta.bytes);
    }

    private static List<SmartXmlNode> ByteDatasToNodes(List<byte[]> byteDatas)
    {
        var result = new List<SmartXmlNode>();
        if (byteDatas == null || byteDatas.Count == 0)
        {
            return result;
        }

        foreach (var byteData in byteDatas)
        {
            using var stream = new MemoryStream(byteData);
            using var reader = new XmlTextReader(stream);

            reader.ReadToFollowing("Data");
            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                result.Add(new SmartXmlNode(reader));
            }
        }

        return result;
    }
}
