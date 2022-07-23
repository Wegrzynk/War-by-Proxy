using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSONHandler
{
    private const string SAVE_EXTENSION = "json";

    private static readonly string SAVE_FOLDER = Application.streamingAssetsPath + "/Charts/";
    private static bool isInit = false;

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }

    [System.Serializable]
    public class DamageMatrix
    {
        public string Attacker;
        public int AATank;
        public int APC;
        public int Artillery;
        public int Heli;
        public int Battleship;
        public int Bomber;
        public int Carrier;
        public int Cruiser;
        public int Fighter;
        public int Infantry;
        public int Tship;
        public int Midtank;
        public int Mech;
        public int Heavytank;
        public int Missile;
        public int Recon;
        public int Rocket;
        public int Sub;
        public int Theli;
        public int Tank;
    }

    public class DamageMatrixArray
    {
        public DamageMatrix[] damageChartArray;
    }

    [System.Serializable]
    public class TerrainArray
    {
        public TilemapObject.TilemapSprite tilemapSprite;
        public int defenceRating;
        public int movementPenaltyFoot;
        public int movementPenaltyThreads;
        public int movementPenaltyTires;
        public int movementPenaltyAir;
        public int movementPenaltyShip;
        public int movementPenaltyLander;
    }

    public class TerrainArrayArray
    {
        public TerrainArray[] terrainChartArray;
    }

    [System.Serializable]
    public class UnitArray
    {
        public string unitType;
        public int movementDistance;
        public int ammo;
        public int fuel;
        public int fuelConsumption;
        public int vision;
        public int attackDistanceMin;
        public int attackDistanceMax;
        public string movementType;
        public int cost;
        public int loadCapacity;
    }

    public class UnitArrayArray
    {
        public UnitArray[] unitChartArray;
    }

    public static void Init()
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

    public static int[,] ReadDamageMatrix(string fileName)
    {
        Init();
        if (File.Exists(SAVE_FOLDER + fileName + "." + SAVE_EXTENSION))
        {
            int[,] convertedMatrix = new int[20,20];

            string jsonstring = File.ReadAllText(SAVE_FOLDER + fileName + "." + SAVE_EXTENSION);
            DamageMatrixArray arrayInstance = JsonUtility.FromJson<DamageMatrixArray>(jsonstring);
            foreach(DamageMatrix attacking in arrayInstance.damageChartArray)
            {
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 0] = attacking.AATank;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 1] = attacking.APC;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 2] = attacking.Artillery;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 3] = attacking.Heli;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 4] = attacking.Battleship;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 5] = attacking.Bomber;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 6] = attacking.Carrier;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 7] = attacking.Cruiser;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 8] = attacking.Fighter;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 9] = attacking.Infantry;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 10] = attacking.Tship;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 11] = attacking.Midtank;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 12] = attacking.Mech;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 13] = attacking.Heavytank;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 14] = attacking.Missile;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 15] = attacking.Recon;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 16] = attacking.Rocket;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 17] = attacking.Sub;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 18] = attacking.Theli;
                convertedMatrix[(int)System.Enum.Parse(typeof(Unit.UnitType), attacking.Attacker), 19] = attacking.Tank;
            }
            return convertedMatrix;
        } 
        else
        {
            return null;
        }
    }

    public static UnitArray ReadUnitChart(string fileName, Unit.UnitType typeSet)
    {
        Init();
        if (File.Exists(SAVE_FOLDER + fileName + "." + SAVE_EXTENSION))
        {
            string jsonstring = File.ReadAllText(SAVE_FOLDER + fileName + "." + SAVE_EXTENSION);
            UnitArrayArray arrayInstance = JsonUtility.FromJson<UnitArrayArray>(jsonstring);
            
            return arrayInstance.unitChartArray[(int)typeSet];
        }
        else
        {
            return null;
        }
    }

    public static TerrainArray ReadTerrainChart(string fileName, TilemapObject.TilemapSprite typeSet)
    {
        Init();
        if (File.Exists(SAVE_FOLDER + fileName + "." + SAVE_EXTENSION))
        {
            string jsonstring = File.ReadAllText(SAVE_FOLDER + fileName + "." + SAVE_EXTENSION);
            TerrainArrayArray arrayInstance = JsonUtility.FromJson<TerrainArrayArray>(jsonstring);
            
            return arrayInstance.terrainChartArray[(int)typeSet];
        }
        else
        {
            return null;
        }
    }
}