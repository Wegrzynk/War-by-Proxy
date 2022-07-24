using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unitmap
{
    public event EventHandler OnLoaded;

    private GameGrid<Unit> grid;

    public Unitmap(int width, int height, float cellSize, Vector3 originPosition)
    {
        grid = new GameGrid<Unit>(width, height, cellSize, originPosition, false, (GameGrid<Unit> g, int x, int z) => null);
    }

    public void SetUnitType(Vector3 worldPosition, Unit.UnitType UnitType, int team, int ammo, int fuel, Unit[] loadedUnits, int upgradeCounter)
    {
        Unit Unit = grid.GetGridObject(worldPosition);
        if (Unit != null)
        {
            Unit.SetUnitType(UnitType, team, ammo, fuel, loadedUnits, upgradeCounter);
        }
    }

    public void InsertUnit(int x, int z, Unit.UnitType UnitType, int team, int ammo, int fuel, Unit[] loadedUnits, int upgradeCounter)
    {
        grid.SetGridObject(x, z, new Unit(grid, x, z));
        grid.GetGridObject(x, z).SetUnitType(UnitType, team, ammo, fuel, loadedUnits, upgradeCounter);
    }

    public void MoveUnit(int startX, int startZ, int endX, int endZ)
    {
        if(startX == endX && startZ == endZ)
        {
            return;
        }
        grid.SetGridObject(endX, endZ, grid.GetGridObject(startX, startZ));
        grid.GetGridObject(endX, endZ).setXZ(endX, endZ);
        grid.TriggerGenericGridChanged(endX, endZ);
        grid.SetGridObject(startX, startZ, null);
        grid.TriggerGenericGridChanged(startX, startZ);
        grid.TriggerGenericGridChanged(endX, endZ);
    }

    public List<Unit> GetEnemyUnitsInRange(int x, int z)
    {
        int minRange = grid.GetGridObject(x, z).GetMinRange();
        int maxRange = grid.GetGridObject(x, z).GetMaxRange();
        if(minRange == 0 || maxRange == 0)
        {
            minRange = 1;
            maxRange = 1;
        }
        int counter = 0;
        List<Unit> targetableUnits = new List<Unit>();

        for(int i=maxRange; i>=-maxRange; i--)
        {
            for(int j=counter; j>=-counter; j--)
            {
                if(Mathf.Abs(i)+Mathf.Abs(j)>=minRange)
                {
                    if(grid.GetGridObject(x+i, z+j) != null && grid.GetGridObject(x+i, z+j).GetTeam() != grid.GetGridObject(x, z).GetTeam())
                    {
                        targetableUnits.Add(grid.GetGridObject(x+i, z+j));
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

        return targetableUnits;
    }

    public List<Unit> GetFriendlyUnitsInRange(int x, int z)
    {
        int minRange = 1;
        int maxRange = 1;
        int counter = 0;
        List<Unit> targetableUnits = new List<Unit>();

        for(int i=maxRange; i>=-maxRange; i--)
        {
            for(int j=counter; j>=-counter; j--)
            {
                if(Mathf.Abs(i)+Mathf.Abs(j)>=minRange)
                {
                    if(grid.GetGridObject(x+i, z+j) != null && grid.GetGridObject(x+i, z+j).GetTeam() == grid.GetGridObject(x, z).GetTeam())
                    {
                        targetableUnits.Add(grid.GetGridObject(x+i, z+j));
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

        return targetableUnits;
    }

    public bool AttackUnit(Unit attacker, Unit defender, int[,] damageMatrix, int defenceRating)
    {
        int damageFormula = Mathf.RoundToInt(damageMatrix[attacker.GetIntFromUnit(),defender.GetIntFromUnit()] * (1 + (float)attacker.GetUpgradeCounter() / 10) * ((float)attacker.GetHealth() / 100) * ((float)(1000 - (defenceRating * defender.GetHealth())) / 1000));

        bool isDead = defender.TakeDamage(damageFormula);
        return isDead;
    }

    public void ClearGrid()
    {
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int z = 0; z < grid.GetHeight(); z++)
            {
                grid.SetGridObject(x, z, null);
            }
        }
    }

    public void SetUnitType(Vector3 worldPosition, int spriteInt, int team, int ammo, int fuel, Unit[] loadedUnits, int upgradeCounter)
    {
        Unit Unit = grid.GetGridObject(worldPosition);
        if (Unit != null)
        {
            Unit.SetUnitType((Unit.UnitType)spriteInt, team, ammo, fuel, loadedUnits, upgradeCounter);
        }
    }

    public int GetIntFromUnit(Vector3 worldPosition)
    {
        Unit Unit = grid.GetGridObject(worldPosition);
        int intedSprite = -1;
        if (Unit != null)
        {
            intedSprite = Unit.GetIntFromUnit();
        }
        return intedSprite;
    }

    public int GetIntFromUnit(int x, int z)
    {
        Unit Unit = grid.GetGridObject(x, z);
        int intedSprite = -1;
        if (Unit != null)
        {
            intedSprite = Unit.GetIntFromUnit();
        }
        return intedSprite;
    }

    public class SaveObject
    {
        public Unit.SaveObject[] UnitSaveObjectArray;
    }

    public void Save(string filename)
    {
        List<Unit.SaveObject> UnitSaveObjectList = new List<Unit.SaveObject>();
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int z = 0; z < grid.GetHeight(); z++)
            {
                Unit Unit = grid.GetGridObject(x, z);
                if(Unit != null)
                {
                    UnitSaveObjectList.Add(Unit.Save());
                }
            }
        }

        SaveObject saveObject = new SaveObject { UnitSaveObjectArray = UnitSaveObjectList.ToArray() };

        SaveSystem saveSystem = new SaveSystem();
        saveSystem.SaveObject(filename, saveObject, true, false);
    }

    public void Load(string filename)
    {
        SaveSystem saveSystem = new SaveSystem();
        SaveObject saveObject = saveSystem.LoadObject<SaveObject>(filename);
        ClearGrid();
        if(saveObject != null)
        {
            foreach (Unit.SaveObject UnitSaveObject in saveObject.UnitSaveObjectArray)
            {
                grid.SetGridObject(UnitSaveObject.x, UnitSaveObject.z, new Unit(grid, UnitSaveObject.x, UnitSaveObject.z));
                grid.GetGridObject(UnitSaveObject.x, UnitSaveObject.z).SetUnitType(UnitSaveObject.unitType, UnitSaveObject.team, UnitSaveObject.ammo, UnitSaveObject.fuel, UnitSaveObject.loadedUnits, UnitSaveObject.upgradeCounter);
                grid.TriggerGenericGridChanged(UnitSaveObject.x, UnitSaveObject.z);
            }
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

    public GameGrid<Unit> GetGrid()
    {
        return grid;
    }
}