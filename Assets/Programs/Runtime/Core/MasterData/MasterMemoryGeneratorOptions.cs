using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Game.Core.MasterData;
using Game.Core.MasterData.MemoryTables;
using MasterMemory;
using MessagePack;
using MessagePack.Resolvers;
using UnityEngine;

// WARN: namespaceの末尾をMasterMemoryにすると自動生成コードと衝突するヨ...
[assembly: MasterMemoryGeneratorOptions(
    Namespace = "Game.Core.MasterData",
    IsReturnNullIfKeyNotFound = false,
    PrefixClassName = ""
)]

namespace System.Runtime.CompilerServices
{
    internal sealed class IsExternalInit
    {
    }
}

#if UNITY_EDITOR || DEBUG

// public class _MasterMemoryGeneratorOptions : MonoBehaviour
// {
// }


// table definition marked by MemoryTableAttribute.
// database-table must be serializable by MessagePack-CSsharp
// [MemoryTable("SampleMaster"), MessagePackObject(true)]
// public sealed partial class SampleMaster
// {
//     // index definition by attributes.
//     [PrimaryKey]
//     public int Id { get; set; }
//
//     // secondary index can add multiple(discriminated by index-number).
//     [SecondaryKey(0), NonUnique]
//     [SecondaryKey(1, keyOrder: 1), NonUnique]
//     public int Age { get; set; }
//
//     [SecondaryKey(2), NonUnique]
//     [SecondaryKey(1, keyOrder: 0), NonUnique]
//     public int Gender { get; set; }
//
//     public string Name { get; set; }
// }

// public static class MasterMemoryResolverInitializer
// {
//     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
//     public static void SetupMessagePackResolver()
//     {
//         // Create CompositeResolver
//         StaticCompositeResolver.Instance.Register(new[]
//         {
//             MasterMemoryResolver.Instance, // set MasterMemory generated resolver
//             StandardResolver.Instance      // set default MessagePack resolver
//         });
//
//         // Set as default
//         var options = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
//         MessagePackSerializer.DefaultOptions = options;
//     }
// }

public static class MasterDataEditor
{
    // memo: 1～3が操作できるエディタ拡張を検討
    // 1.MemoryTable作成
    // 2.MemoryTableからTsv作成（仮データ）
    // 3.Tsvからバイナリ作成
    // 4.Addressableに自動登録
    // 5.Addressable経由でバイナリをロードしてデータベース構築（⇒MasterDataServiceを用意してアプリ起動時に読み込む）

    // EX
    // 1.MySql定義情報からMemoryTableクラスを自動生成できるツールを検討（やりすぎ感）

    [UnityEditor.MenuItem("Project/MasterMemory/Get MemoryTables")]
    private static void GetMemoryTables()
    {
        // Assembly assembly = Assembly.GetAssembly(typeof(SampleMaster));
        Assembly assembly = Assembly.GetExecutingAssembly();
        Type[] types = assembly.GetTypes();
        foreach (Type type in types)
        {
            if (!type.IsClass)
                continue;

            var attr = type.GetCustomAttribute<MemoryTableAttribute>();
            if (attr != null)
            {
                Debug.Log($"MemoryTable: {type.FullName}");
                // Console.WriteLine(type.FullName);
            }
        }
    }

    // 定義ファイルからTsv化する
    [UnityEditor.MenuItem("Project/MasterMemory/Generate Tsv")]
    private static void GenerateTsv()
    {
        #region SampleData

        var stageMasters = new[]
        {
            new GameStageMaster
            {
                Id = 1,
                Name = "Stage00",
                MaxPoint = 10,
                PlayerMaxHp = 5,
                NextStageId = 2,
            },
            new GameStageMaster
            {
                Id = 2,
                Name = "Stage01",
                MaxPoint = 15,
                PlayerMaxHp = 3,
                NextStageId = 3,
            },
            new GameStageMaster
            {
                Id = 3,
                Name = "Stage02",
                MaxPoint = 20,
                PlayerMaxHp = 1,
                NextStageId = null,
            },
        };

        #endregion

        // string path = "Assets/Programs/Runtime/Core/MasterMemory/Editor/Tsv/SampleMaster.tsv";
        string path = $"Assets/MasterData/Tsv/{nameof(GameStageMaster)}.tsv";

        var directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        // if (File.Exists(path))
        // {
        //     StreamReader reader = new StreamReader(path);
        //     var line = reader.ReadToEnd();
        //     string[] lines = line.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        //     reader.Dispose();
        // }

        using (StreamWriter writer = new StreamWriter(path))
        {
            var properties = typeof(GameStageMaster).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var columnNames = properties.Select(x => x.Name).ToList();
            var columnNameLine = string.Join("\t", columnNames);
            writer.WriteLine(columnNameLine);

            foreach (var master in stageMasters)
            {
                var row = properties.Select(x => x.GetValue(master)?.ToString()).ToList();
                var line = string.Join("\t", row);
                writer.WriteLine(line);
            }
        }
    }

    // Tsvからバイナリデータ化する
    [UnityEditor.MenuItem("Project/MasterMemory/Generate Binary")]
    private static void GenerateBinary()
    {
        // MessagePackの初期化
        var messagePackResolvers = CompositeResolver.Create(
            MasterMemoryResolver.Instance, // 自動生成されたResolver
            StandardResolver.Instance      // MessagePackの標準Resolver
        );
        var options = MessagePackSerializerOptions.Standard.WithResolver(messagePackResolvers);
        MessagePackSerializer.DefaultOptions = options;

        // 本来はCSV/TSVなどを読み込む
        // string tsvPath = "Assets/Programs/Runtime/Core/MasterMemory/Editor/Tsv/SampleMaster.tsv";
        string tsvPath = $"Assets/MasterData/Tsv/{nameof(GameStageMaster)}.tsv";
        if (!File.Exists(tsvPath))
            return;
        var lines = File.ReadAllLines(tsvPath);
        char tsvSeparator = '\t';
        var columnNames = lines
            .First()
            .Split(tsvSeparator)
            .Select((name, index) => (name, index))
            .ToDictionary(x => x.name, x => x.index);

        var masterType = typeof(GameStageMaster);
        var properties = masterType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var instances = new List<object>();
        foreach (var line in lines.Skip(1))
        {
            var values = line.Split(tsvSeparator);
            var instance = Activator.CreateInstance(masterType);

            foreach (var property in properties)
            {
                if (columnNames.TryGetValue(property.Name, out var index))
                {
                    var value = ConvertToType(property.PropertyType, values[index]);
                    property.SetValue(instance, value);
                }
            }

            instances.Add(instance);
        }

        object ConvertToType(Type propertyType, string value)
        {
            var converter = TypeDescriptor.GetConverter(propertyType);
            return converter.ConvertFrom(value);
        }

        object[] elements = instances.ToArray();
        // try
        // {
        //     elements = instances.ToArray();
        // }
        // catch (Exception e)
        // {
        //     Debug.LogError($"TSV読み込み失敗: {masterType.Name} \n{e}");
        // }

        var masterElements = Array.CreateInstance(masterType, elements.Length);
        Array.Copy(elements, masterElements, elements.Length);

        // TODO: すべてのマスタを読み込んで一つのバイナリにする

        // DatabaseBuilderを使ってバイナリデータを生成する
        var databaseBuilder = new DatabaseBuilder();
        var appendMethods = databaseBuilder
            .GetType()
            .GetMethods()
            .Where(x => x.Name == "Append" && x.GetParameters().Length == 1)
            .ToDictionary(x => x.GetParameters().First().ParameterType.GetGenericArguments().First());
        if (appendMethods.TryGetValue(masterType, out var method))
        {
            method.Invoke(databaseBuilder, new[] { masterElements });
        }

        // databaseBuilder.Append(elements);
        var binary = databaseBuilder.Build();

        // バイナリは永続化
        // var path = "Assets/Programs/Runtime/Core/MasterMemory/Editor/Binary/SampleMaster.bytes";
        var path = $"Assets/MasterData/MasterDataBinary.bytes";
        var directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        File.WriteAllBytes(path, binary);
        UnityEditor.AssetDatabase.Refresh();
    }

    // バイナリデータをアセットとして読み込んでデータベース構築
    [UnityEditor.MenuItem("Project/MasterMemory/Load Binary")]
    private static void LoadBinary()
    {
        // MessagePackの初期化
        var messagePackResolvers = CompositeResolver.Create(
            MasterMemoryResolver.Instance, // 自動生成されたResolver
            StandardResolver.Instance      // MessagePackの標準Resolver
        );
        var options = MessagePackSerializerOptions.Standard.WithResolver(messagePackResolvers);
        MessagePackSerializer.DefaultOptions = options;

        // ロード
        // var path = "Assets/Programs/Runtime/Core/MasterMemory/Editor/Binary/SampleMaster.bytes";
        var path = $"Assets/MasterData/MasterDataBinary.bytes";
        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        var binary = asset.bytes;

        // MemoryDatabaseをバイナリから作成
        var memoryDatabase = new MemoryDatabase(binary);
        // テーブルからデータを検索
        var masters = memoryDatabase.GameStageMasterTable.All;
        foreach (var master in masters)
        {
            Debug.Log($"{nameof(GameStageMaster)}: {master.Id} {master.Name} {master.NextStageId}");
        }
    }
}
#endif