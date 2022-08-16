using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tilemap
{
    public event EventHandler OnLoaded;

    private GameGrid<TileType> grid;

    public Tilemap(int width, int height, float cellSize, Vector3 originPosition)
    {
        grid = new GameGrid<TileType>(width, height, cellSize, originPosition, false, (GameGrid<TileType> g, int x, int z) => new TileType(g, x, z));
    }

    public void SetTilemapSprite(Vector3 worldPosition, TileType.TilemapSprite tilemapSprite, int team)
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
            default: grid.SetGridObject(worldPosition, new TileType(grid.GetGridObject(worldPosition))); break;
        }
        TileType TileType = grid.GetGridObject(worldPosition);
        if (TileType != null)
        {
            TileType.SetTilemapSprite(tilemapSprite);
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
            default: grid.SetGridObject(worldPosition, new TileType(grid.GetGridObject(worldPosition))); break;
        }
        TileType TileType = grid.GetGridObject(worldPosition);
        if (TileType != null)
        {
            TileType.SetTilemapSprite((TileType.TilemapSprite)spriteInt);
        }
    }

    public List<TileType> GetNeighbouringTiles(int x, int z, Unit checker, Unitmap unitmap)
    {
        int minRange = 1;
        int maxRange = 1;
        int counter = 0;
        List<TileType> neighbourtiles = new List<TileType>();

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
        TileType TileType = grid.GetGridObject(worldPosition);
        int intedSprite = -1;
        if (TileType != null)
        {
            intedSprite = TileType.GetIntFromSprite();
        }
        return intedSprite;
    }

    public int GetIntFromSprite(int x, int z)
    {
        TileType TileType = grid.GetGridObject(x, z);
        int intedSprite = -1;
        if (TileType != null)
        {
            intedSprite = TileType.GetIntFromSprite();
        }
        return intedSprite;
    }

    public class SaveObject
    {
        public TileType.SaveObject[] TileTypeSaveObjectArray;
    }

    public void Save(string filename)
    {
        List<TileType.SaveObject> TileTypeSaveObjectList = new List<TileType.SaveObject>();
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int z = 0; z < grid.GetHeight(); z++)
            {
                TileType TileType = grid.GetGridObject(x, z);
                switch(TileType)
                {
                    case Radio r:
                    TileTypeSaveObjectList.Add(((Radio)TileType).Save());
                    break;
                    case Lab l:
                    TileTypeSaveObjectList.Add(((Lab)TileType).Save());
                    break;
                    case Outpost o:
                    TileTypeSaveObjectList.Add(((Outpost)TileType).Save());
                    break;
                    case City c:
                    TileTypeSaveObjectList.Add(((City)TileType).Save());
                    break;
                    case MilitaryBase m:
                    TileTypeSaveObjectList.Add(((MilitaryBase)TileType).Save());
                    break;
                    case Airport a:
                    TileTypeSaveObjectList.Add(((Airport)TileType).Save());
                    break;
                    case Port p:
                    TileTypeSaveObjectList.Add(((Port)TileType).Save());
                    break;
                    case HQ h:
                    TileTypeSaveObjectList.Add(((HQ)TileType).Save());
                    break;
                    case Building b:
                    TileTypeSaveObjectList.Add(((Building)TileType).Save());
                    break;
                    default:
                    TileTypeSaveObjectList.Add(TileType.Save());
                    break;
                }
            }
        }

        SaveObject saveObject = new SaveObject { TileTypeSaveObjectArray = TileTypeSaveObjectList.ToArray() };

        SaveSystem saveSystem = new SaveSystem();
        saveSystem.SaveObject(filename, saveObject, true, false);
    }

    public void Load(string filename)
    {
        SaveSystem saveSystem = new SaveSystem();
        SaveObject saveObject = saveSystem.LoadObject<SaveObject>(filename);
        if(saveObject == null)
        {
            for(int z = 0; z < grid.GetHeight(); z++)
            {
                for(int x = 0; x < grid.GetWidth(); x++)
                {
                    TileType TileType = grid.GetGridObject(x, z);
                    TileType.SetTilemapSprite(TileType.TilemapSprite.Plains);
                    grid.TriggerGenericGridChanged(x, z);
                }
            }
            OnLoaded?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            foreach (TileType.SaveObject TileTypeSaveObject in saveObject.TileTypeSaveObjectArray)
            {
                switch(TileTypeSaveObject.type)
                {
                    case "Radio":
                    grid.SetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z, new Radio(grid.GetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z), TileTypeSaveObject.team, TileTypeSaveObject.health, TileTypeSaveObject.cost));
                    break;
                    case "Lab":
                    grid.SetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z, new Lab(grid.GetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z), TileTypeSaveObject.team, TileTypeSaveObject.health, TileTypeSaveObject.cost));
                    break;
                    case "Outpost":
                    grid.SetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z, new Outpost(grid.GetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z), TileTypeSaveObject.team, TileTypeSaveObject.health, TileTypeSaveObject.cost));
                    break;
                    case "City":
                    grid.SetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z, new City(grid.GetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z), TileTypeSaveObject.team, TileTypeSaveObject.health));
                    break;
                    case "MilitaryBase":
                    grid.SetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z, new MilitaryBase(grid.GetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z), TileTypeSaveObject.team, TileTypeSaveObject.health));
                    break;
                    case "Airport":
                    grid.SetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z, new Airport(grid.GetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z), TileTypeSaveObject.team, TileTypeSaveObject.health));
                    break;
                    case "Port":
                    grid.SetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z, new Port(grid.GetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z), TileTypeSaveObject.team, TileTypeSaveObject.health));
                    break;
                    case "HQ":
                    grid.SetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z, new HQ(grid.GetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z), TileTypeSaveObject.team, TileTypeSaveObject.health));
                    break;
                    case "Building":
                    grid.SetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z, new Building(grid.GetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z), TileTypeSaveObject.team, TileTypeSaveObject.health));
                    break;
                    default:
                    break;
                }
                TileType TileType = grid.GetGridObject(TileTypeSaveObject.x, TileTypeSaveObject.z);
                TileType.Load(TileTypeSaveObject);
                grid.TriggerGenericGridChanged(TileTypeSaveObject.x, TileTypeSaveObject.z);
            }
            OnLoaded?.Invoke(this, EventArgs.Empty);
        }
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

    public GameGrid<TileType> GetGrid()
    {
        return grid;
    }
}