using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveSystem 
{

    private const string SAVE_EXTENSION = "txt";

    private static readonly string SAVE_FOLDER = Application.streamingAssetsPath + "/Saves/";
    private static bool isInit = false;

    public void Init()
    {
        if (!isInit)
        {
            isInit = true;
            if (!Directory.Exists(SAVE_FOLDER))
            {
                Directory.CreateDirectory(SAVE_FOLDER);
            }
        }
    }

    public void Save(string fileName, string saveString, bool overwrite, bool append)
    {
        Init();
        string saveFileName = fileName;
        if (!overwrite)
        {
            int saveNumber = 1;
            while (File.Exists(SAVE_FOLDER + saveFileName + "." + SAVE_EXTENSION))
            {
                saveNumber++;
                saveFileName = fileName + "_" + saveNumber;
            }
        }
        if(append)
        {
            File.AppendAllText(SAVE_FOLDER + saveFileName + "." + SAVE_EXTENSION, saveString);
        } else
        {
            File.WriteAllText(SAVE_FOLDER + saveFileName + "." + SAVE_EXTENSION, saveString);
        }
    }

    public string Load(string fileName)
    {
        Init();
        if (File.Exists(SAVE_FOLDER + fileName + "." + SAVE_EXTENSION))
        {
            string saveString = File.ReadAllText(SAVE_FOLDER + fileName + "." + SAVE_EXTENSION);
            return saveString;
        } else
        {
            return null;
        }
    }

    public string LoadMostRecentFile()
    {
        Init();
        DirectoryInfo directoryInfo = new DirectoryInfo(SAVE_FOLDER);
        FileInfo[] saveFiles = directoryInfo.GetFiles("*." + SAVE_EXTENSION);
        FileInfo mostRecentFile = null;
        foreach (FileInfo fileInfo in saveFiles)
        {
            if (mostRecentFile == null)
            {
                mostRecentFile = fileInfo;
            } else
            {
                if (fileInfo.LastWriteTime > mostRecentFile.LastWriteTime)
                {
                    mostRecentFile = fileInfo;
                }
            }
        }

        if (mostRecentFile != null)
        {
            string saveString = File.ReadAllText(mostRecentFile.FullName);
            return saveString;
        } else
        {
            return null;
        }
    }

    public void SaveObject(object saveObject)
    {
        SaveObject("save", saveObject, true, true);
    }

    public void SaveObject(string fileName, object saveObject, bool overwrite, bool append)
    {
        Init();
        string json = JsonUtility.ToJson(saveObject, true);
        Save(fileName, json, overwrite, append);
    }

    public TSaveObject LoadMostRecentObject<TSaveObject>()
    {
        Init();
        string saveString = LoadMostRecentFile();
        if (saveString != null)
        {
            TSaveObject saveObject = JsonUtility.FromJson<TSaveObject>(saveString);
            return saveObject;
        } else
        {
            return default(TSaveObject);
        }
    }

    public TSaveObject LoadObject<TSaveObject>(string fileName)
    {
        Init();
        string saveString = Load(fileName);
        if (saveString != null)
        {
            TSaveObject saveObject = JsonUtility.FromJson<TSaveObject>(saveString);
            return saveObject;
        } else
        {
            return default(TSaveObject);
        }
    }
}
