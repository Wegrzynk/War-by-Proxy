using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tilemap
{
    public event EventHandler OnLoaded;

    private GameGrid<TilemapObject> grid;

    public Tilemap(int width, int height, float cellSize, Vector3 originPosition)
    {
        grid = new GameGrid<TilemapObject>(width, height, cellSize, originPosition, false, (GameGrid<TilemapObject> g, int x, int z) => new TilemapObject(g, x, z));
    }

    public void SetTilemapSprite(Vector3 worldPosition, TilemapObject.TilemapSprite tilemapSprite, int team)
    {
        switch((int)tilemapSprite)
        {
            case 8: grid.SetGridObject(worldPosition, new City(grid.GetGridObject(worldPosition), team, 100)); break;
            case 9: grid.SetGridObject(worldPosition, new MilitaryBase(grid.GetGridObject(worldPosition), team, 100)); break;
            case 10: grid.SetGridObject(worldPosition, new Airport(grid.GetGridObject(worldPosition), team, 100)); break;
            case 11: grid.SetGridObject(worldPosition, new Port(grid.GetGridObject(worldPosition), team, 100)); break;
            case 12: grid.SetGridObject(worldPosition, new HQ(grid.GetGridObject(worldPosition), team, 100)); break;
            case 13: grid.SetGridObject(worldPosition, new Radio(grid.GetGridObject(worldPosition), team, 100, 2000)); break;
            case 14: grid.SetGridObject(worldPosition, new Lab(grid.GetGridObject(worldPosition), team, 100, 4000)); break;
            case 15: grid.SetGridObject(worldPosition, new Outpost(grid.GetGridObject(worldPosition), team, 100, 3000)); break;
            default: grid.SetGridObject(worldPosition, new TilemapObject(grid.GetGridObject(worldPosition))); break;
        }
        TilemapObject tilemapObject = grid.GetGridObject(worldPosition);
        if (tilemapObject != null)
        {
            tilemapObject.SetTilemapSprite(tilemapSprite);
        }
    }

    public void SetTilemapSprite(Vector3 worldPosition, int spriteInt, int team)
    {
        switch(spriteInt)
        {
            case 8: grid.SetGridObject(worldPosition, new City(grid.GetGridObject(worldPosition), team, 100)); break;
            case 9: grid.SetGridObject(worldPosition, new MilitaryBase(grid.GetGridObject(worldPosition), team, 100)); break;
            case 10: grid.SetGridObject(worldPosition, new Airport(grid.GetGridObject(worldPosition), team, 100)); break;
            case 11: grid.SetGridObject(worldPosition, new Port(grid.GetGridObject(worldPosition), team, 100)); break;
            case 12: grid.SetGridObject(worldPosition, new HQ(grid.GetGridObject(worldPosition), team, 100)); break;
            case 13: grid.SetGridObject(worldPosition, new Radio(grid.GetGridObject(worldPosition), team, 100, 2000)); break;
            case 14: grid.SetGridObject(worldPosition, new Lab(grid.GetGridObject(worldPosition), team, 100, 4000)); break;
            case 15: grid.SetGridObject(worldPosition, new Outpost(grid.GetGridObject(worldPosition), team, 100, 3000)); break;
            default: grid.SetGridObject(worldPosition, new TilemapObject(grid.GetGridObject(worldPosition))); break;
        }
        TilemapObject tilemapObject = grid.GetGridObject(worldPosition);
        if (tilemapObject != null)
        {
            tilemapObject.SetTilemapSprite((TilemapObject.TilemapSprite)spriteInt);
        }
    }

    public List<TilemapObject> GetNeighbouringTiles(int x, int z, Unit checker, Unitmap unitmap)
    {
        int minRange = 1;
        int maxRange = 1;
        int counter = 0;
        List<TilemapObject> neighbourtiles = new List<TilemapObject>();

        for(int i=maxRange; i>=-maxRange; i--)
        {
            for(int j=counter; j>=-counter; j--)
            {
                if(Mathf.Abs(i)+Mathf.Abs(j)>=minRange)
                {
                    if(grid.GetGridObject(x+i, z+j) != null && grid.GetGridObject(x+i, z+j).GetMovementPenaltyType(checker) != 0 && unitmap.GetGrid().GetGridObject(x+i, z+j) == null)
                    {
                        neighbourtiles.Add(grid.GetGridObject(x+i, z+j));
                    }
                }
            }
            if(i>0)
            {
                counter++;
            }
            else
            {
                counter--;
            }
        }

        return neighbourtiles;
    }

    public int GetIntFromSprite(Vector3 worldPosition)
    {
        TilemapObject tilemapObject = grid.GetGridObject(worldPosition);
        int intedSprite = -1;
        if (tilemapObject != null)
        {
            intedSprite = tilemapObject.GetIntFromSprite();
        }
        return intedSprite;
    }

    public int GetIntFromSprite(int x, int z)
    {
        TilemapObject tilemapObject = grid.GetGridObject(x, z);
        int intedSprite = -1;
        if (tilemapObject != null)
        {
            intedSprite = tilemapObject.GetIntFromSprite();
        }
        return intedSprite;
    }

    public class SaveObject
    {
        public TilemapObject.SaveObject[] tilemapObjectSaveObjectArray;
    }

    public void Save(string filename)
    {
        List<TilemapObject.SaveObject> tilemapObjectSaveObjectList = new List<TilemapObject.SaveObject>();
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int z = 0; z < grid.GetHeight(); z++)
            {
                TilemapObject tilemapObject = grid.GetGridObject(x, z);
                switch(tilemapObject)
                {
                    case Radio r:
                    tilemapObjectSaveObjectList.Add(((Radio)tilemapObject).Save());
                    break;
                    case Lab l:
                    tilemapObjectSaveObjectList.Add(((Lab)tilemapObject).Save());
                    break;
                    case Outpost o:
                    tilemapObjectSaveObjectList.Add(((Outpost)tilemapObject).Save());
                    break;
                    case City c:
                    tilemapObjectSaveObjectList.Add(((City)tilemapObject).Save());
                    break;
                    case MilitaryBase m:
                    tilemapObjectSaveObjectList.Add(((MilitaryBase)tilemapObject).Save());
                    break;
                    case Airport a:
                    tilemapObjectSaveObjectList.Add(((Airport)tilemapObject).Save());
                    break;
                    case Port p:
                    tilemapObjectSaveObjectList.Add(((Port)tilemapObject).Save());
                    break;
                    case HQ h:
                    tilemapObjectSaveObjectList.Add(((HQ)tilemapObject).Save());
                    break;
                    case Building b:
                    tilemapObjectSaveObjectList.Add(((Building)tilemapObject).Save());
                    break;
                    default:
                    tilemapObjectSaveObjectList.Add(tilemapObject.Save());
                    break;
                }
            }
        }

        SaveObject saveObject = new SaveObject { tilemapObjectSaveObjectArray = tilemapObjectSaveObjectList.ToArray() };

        SaveSystem saveSystem = new SaveSystem();
        saveSystem.SaveObject(filename, saveObject, true, false);
    }

    public void Load(string filename)
    {
        SaveSystem saveSystem = new SaveSystem();
        SaveObject saveObject = saveSystem.LoadObject<SaveObject>(filename);
        foreach (TilemapObject.SaveObject tilemapObjectSaveObject in saveObject.tilemapObjectSaveObjectArray)
        {
            switch(tilemapObjectSaveObject.type)
            {
                case "Radio":
                grid.SetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z, new Radio(grid.GetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z), tilemapObjectSaveObject.team, tilemapObjectSaveObject.health, tilemapObjectSaveObject.cost));
                break;
                case "Lab":
                grid.SetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z, new Lab(grid.GetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z), tilemapObjectSaveObject.team, tilemapObjectSaveObject.health, tilemapObjectSaveObject.cost));
                break;
                case "Outpost":
                grid.SetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z, new Outpost(grid.GetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z), tilemapObjectSaveObject.team, tilemapObjectSaveObject.health, tilemapObjectSaveObject.cost));
                break;
                case "City":
                grid.SetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z, new City(grid.GetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z), tilemapObjectSaveObject.team, tilemapObjectSaveObject.health));
                break;
                case "MilitaryBase":
                grid.SetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z, new MilitaryBase(grid.GetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z), tilemapObjectSaveObject.team, tilemapObjectSaveObject.health));
                break;
                case "Airport":
                grid.SetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z, new Airport(grid.GetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z), tilemapObjectSaveObject.team, tilemapObjectSaveObject.health));
                break;
                case "Port":
                grid.SetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z, new Port(grid.GetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z), tilemapObjectSaveObject.team, tilemapObjectSaveObject.health));
                break;
                case "HQ":
                grid.SetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z, new HQ(grid.GetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z), tilemapObjectSaveObject.team, tilemapObjectSaveObject.health));
                break;
                case "Building":
                grid.SetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z, new Building(grid.GetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z), tilemapObjectSaveObject.team, tilemapObjectSaveObject.health));
                break;
                default:
                break;
            }
            TilemapObject tilemapObject = grid.GetGridObject(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z);
            tilemapObject.Load(tilemapObjectSaveObject);
            grid.TriggerGenericGridChanged(tilemapObjectSaveObject.x, tilemapObjectSaveObject.z);
        }
        OnLoaded?.Invoke(this, EventArgs.Empty);
    }

    public bool SaveExists(string filename)
    {
        SaveSystem saveSystem = new SaveSystem();
        SaveObject saveObject = saveSystem.LoadObject<SaveObject>(filename);
        if (saveObject == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public GameGrid<TilemapObject> GetGrid()
    {
        return grid;
    }
}