using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    private int team;
    private int funds;
    private List<Unit> ownedUnits;
    private List<Building> ownedBuildings;
    private List<Unit> ownedLoadedUnits;
    private bool isActive;

    public Player(int team, Tilemap tilemap, Unitmap unitmap)
    {
        this.team = team;
        isActive = false;
        ownedUnits = new List<Unit>();
        ownedLoadedUnits = new List<Unit>();
        ownedBuildings = new List<Building>();
        for(int x = 0; x < tilemap.GetGrid().GetWidth(); x++)
        {
            for(int z = 0; z < tilemap.GetGrid().GetHeight(); z++)
            {
                TilemapObject tileHelp = tilemap.GetGrid().GetGridObject(x, z);
                if(tileHelp.GetType().IsSubclassOf(typeof(Building)) && ((Building)tileHelp).GetTeam() == team)
                {
                    ownedBuildings.Add((Building)tileHelp);
                }
                Unit unitHelp = unitmap.GetGrid().GetGridObject(x, z);
                if(unitHelp != null && unitHelp.GetTeam() == team)
                {
                    ownedUnits.Add(unitHelp);
                }
            }
        }
        this.funds = 0;
    }

    public void SetIsActive(bool activation)
    {
        isActive = activation;
    }

    public void AddUnit(Unit newUnit)
    {
        ownedUnits.Add(newUnit);
    }

    public void AddLoadedUnit(Unit newUnit)
    {
        ownedLoadedUnits.Add(newUnit);
    }

    public void AddBuilding(Building newBuilding)
    {
        ownedBuildings.Add(newBuilding);
    }

    public void RemoveUnit(Unit newUnit)
    {
        ownedUnits.Remove(newUnit);
    }

    public void RemoveLoadedUnit(Unit newUnit)
    {
        ownedLoadedUnits.Remove(newUnit);
    }

    public void RemoveBuilding(Building newBuilding)
    {
        ownedBuildings.Remove(newBuilding);
    }

    public void ChangeFunds(int change)
    {
        funds = funds + change;
    }

    public bool GetIsActive()
    {
        return isActive;
    }

    public int GetTeam()
    {
        return team;
    }

    public int GetFunds()
    {
        return funds;
    }

    public List<Unit> GetUnits()
    {
        List<Unit> resultList = new List<Unit>();
        resultList.AddRange(ownedUnits);
        resultList.AddRange(ownedLoadedUnits);
        return resultList;
    }

    public List<Unit> GetUnloadedUnits()
    {
        return ownedUnits;
    }

    public List<Unit> GetLoadedUnits()
    {
        return ownedLoadedUnits;
    }

    public int GetUnitsValue()
    {
        int totalvalue = 0;
        List<Unit> resultList = new List<Unit>();
        resultList.AddRange(ownedUnits);
        resultList.AddRange(ownedLoadedUnits);
        foreach(Unit unit in resultList)
        {
            totalvalue += Mathf.RoundToInt(unit.GetCost() * ((float)unit.GetHealth() / 100));
        }
        return totalvalue;
    }

    public List<Building> GetBuildings()
    {
        return ownedBuildings;
    }
}
