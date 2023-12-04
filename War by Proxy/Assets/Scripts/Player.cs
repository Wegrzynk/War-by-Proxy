using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player
{
    private int team;
    private int funds;
    private List<Unit> ownedUnits;
    private List<Building> ownedBuildings;
    private List<Unit> ownedLoadedUnits;
    private bool isActive;
    private bool achievedVictory;
    private int AIstatus;
    public int[] unitTypeCount = new int[20];
    AIQuirks RNGreference;

    public Player(int team, Tilemap tilemap, Unitmap unitmap, AIQuirks reference)
    {
        this.team = team;
        isActive = false;
        achievedVictory = false;
        ownedUnits = new List<Unit>();
        ownedLoadedUnits = new List<Unit>();
        ownedBuildings = new List<Building>();
        RNGreference = reference;
        for(int i = 0; i < unitTypeCount.Length; i++)
        {
            unitTypeCount[i] = 0;
        }
        for(int x = 0; x < tilemap.GetGrid().GetWidth(); x++)
        {
            for(int z = 0; z < tilemap.GetGrid().GetHeight(); z++)
            {
                TileType tileHelp = tilemap.GetGrid().GetGridObject(x, z);
                if(tileHelp.GetType().IsSubclassOf(typeof(Building)) && ((Building)tileHelp).GetTeam() == team)
                {
                    ownedBuildings.Add((Building)tileHelp);
                }
                Unit unitHelp = unitmap.GetGrid().GetGridObject(x, z);
                if(unitHelp != null && unitHelp.GetTeam() == team)
                {
                    if(RNGreference != null)
                    {
                        switch(unitHelp.GetUnitType())
                        {
                            case Unit.UnitType.Infantry: unitHelp.SetAIbehaviour(6); break;
                            case Unit.UnitType.Mech: unitHelp.SetAIbehaviour(6); break;
                            case Unit.UnitType.APC: unitHelp.SetAIbehaviour(7); break;
                            case Unit.UnitType.Theli: unitHelp.SetAIbehaviour(8); break;
                            case Unit.UnitType.Tship: unitHelp.SetAIbehaviour(0); break;
                            default: unitHelp.SetAIbehaviour(RNGreference.RNGbehaviour((int)unitHelp.GetUnitType())); break;
                        }
                    }
                    if(unitHelp.GetLoadedUnits().Length > 0)
                    {
                        foreach(Unit loaded in unitHelp.GetLoadedUnits())
                        {
                            if(loaded != null)
                            {
                                Debug.Log("Found unit in coordinates" + x + "." + z + "?");
                                unitTypeCount[unitHelp.GetIntFromUnit()]++;
                                ownedLoadedUnits.Add(loaded);
                            }
                        }
                    }
                    unitTypeCount[unitHelp.GetIntFromUnit()]++;
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

    public void GrantWin()
    {
        achievedVictory = true;
    }

    public void AddUnit(Unit newUnit)
    {
        ownedUnits.Add(newUnit);
        unitTypeCount[newUnit.GetIntFromUnit()]++;
    }

    public void AddLoadedUnit(Unit newUnit)
    {
        ownedLoadedUnits.Add(newUnit);
        unitTypeCount[newUnit.GetIntFromUnit()]++;
    }

    public void AddBuilding(Building newBuilding)
    {
        ownedBuildings.Add(newBuilding);
    }

    public void RemoveUnit(Unit newUnit)
    {
        ownedUnits.Remove(newUnit);
        unitTypeCount[newUnit.GetIntFromUnit()]--;
    }

    public void RemoveLoadedUnit(Unit newUnit)
    {
        ownedLoadedUnits.Remove(newUnit);
        unitTypeCount[newUnit.GetIntFromUnit()]--;
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

    public bool GetVictoryStatus()
    {
        return achievedVictory;
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
