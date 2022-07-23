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
            // Sprawdzenie czy istnieje folder zapis�w
            if (!Directory.Exists(SAVE_FOLDER))
            {
                // Utworzenie folderu zapis�w
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
            // Nazwa pliku zapisu jest zawsze unikalna
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
        Debug.LogError("Checking if " + SAVE_FOLDER + fileName + "." + SAVE_EXTENSION + " exists");
        if (File.Exists(SAVE_FOLDER + fileName + "." + SAVE_EXTENSION))
        {
            string saveString = File.ReadAllText(SAVE_FOLDER + fileName + "." + SAVE_EXTENSION);
            return saveString;
        } else
        {
            Debug.LogError("File doesnt exist???");
            return null;
        }
    }

    public string LoadMostRecentFile()
    {
        Init();
        DirectoryInfo directoryInfo = new DirectoryInfo(SAVE_FOLDER);
        // Pobranie wszystkich plik�w zapisu
        FileInfo[] saveFiles = directoryInfo.GetFiles("*." + SAVE_EXTENSION);
        // Przej�cie przez wszystkie pliki zapisu, szukaj�c nadpisanego/zapisanego najp�niej
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

        // Je�li istnieje plik zapisu, za�adowanie go. W przeciwnym wypadku zwr�cenie null.
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
        Debug.LogError("Attempting to run this bloody thing");
        Init();
        string saveString = Load(fileName);
        if (saveString != null)
        {
            Debug.LogError("Saving the object");
            TSaveObject saveObject = JsonUtility.FromJson<TSaveObject>(saveString);
            return saveObject;
        } else
        {
            Debug.LogError("Returning Default");
            return default(TSaveObject);
        }
    }

    /*public static void ManualToJson(Tilemap.SaveObject saveObject, string filename)
    {
        Init();
        string resultsJson = "";
        resultsJson += "{\"tilemapObjectSaveObjectArray\":[";
        Debug.Log(JsonUtility.ToJson((Building.SaveObject2)saveObject.tilemapObjectSaveObjectArray[0]));
        for (int i = 0; i < saveObject.tilemapObjectSaveObjectArray.Length; i++)
        {
            if (saveObject.tilemapObjectSaveObjectArray[i].GetType() == typeof(Building))
            {
                resultsJson += JsonUtility.ToJson((Building.SaveObject2)saveObject.tilemapObjectSaveObjectArray[i]);
            } 
            else 
            {
                resultsJson += JsonUtility.ToJson(saveObject.tilemapObjectSaveObjectArray[i]);
            }
            if (i < saveObject.tilemapObjectSaveObjectArray.Length - 1)
            {
                resultsJson += ",";
            }
        }
        resultsJson += "]}";
        Save(filename, resultsJson, true, false);
    }*/
}
