using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    private static string saveFolder = "/saves/";
    private static string saveFormat = ".data";

    private static void CreateIfNotExist(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static void Save(PlayerRecord record)
    {
        string path = Application.persistentDataPath + saveFolder + saveFormat;
        BinaryFormatter formatter = new BinaryFormatter();

        CreateIfNotExist(Application.persistentDataPath + saveFolder);

        var playerData = Load();
        var records = new List<PlayerRecord>();
        if (playerData.records != null)
        {
            records.AddRange(playerData.records);
        }
        records.Add(record);
        playerData.records = records.ToArray();

        FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
        formatter.Serialize(stream, playerData);
        stream.Close();
    }

    public static void Save()
    {
        string path = Application.persistentDataPath + saveFolder + saveFormat;
        BinaryFormatter formatter = new BinaryFormatter();

        CreateIfNotExist(Application.persistentDataPath + saveFolder);

        var playerData = Load();

        playerData.isNew = false;

        FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
        formatter.Serialize(stream, playerData);
        stream.Close();
    }

    public static PlayerData Load()
    {
        string path = Application.persistentDataPath + saveFolder + saveFormat;

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();

            FileStream stream = new FileStream(path, FileMode.Open);

            PlayerData data = (PlayerData)formatter.Deserialize(stream);
            stream.Close();
            return data;
        }
        else
        {
            Debug.LogWarning("Player data not found!!");
            return new PlayerData();
        }
    }

    public static void Remove()
    {
        string path = Application.persistentDataPath + saveFolder + saveFormat;

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
