using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Unit
{
    public enum UnitType
    {
        AATank,
        APC,
        Artillery,
        Heli,
        Battleship,
        Bomber,
        Carrier,
        Cruiser,
        Fighter,
        Infantry,
        Tship,
        Midtank,
        Mech,
        Heavytank,
        Missile,
        Recon,
        Rocket,
        Sub,
        Theli,
        Tank
    }

    public enum MovementType
    {
        Foot,
        Tires,
        Threads,
        Ship,
        Lander,
        Air
    }

    private GameGrid<Unit> grid;
    private int x;
    private int z;
    private int health;
    private int team;
    [SerializeField] private UnitType unitType;
    public GameObject unitVisualPrefab;
    private GameObject unitVisual;
    private MovementType movementType;
    private int movementDistance;
    private int ammo;
    private int currentAmmo;
    private int attackDistanceMin;
    private int attackDistanceMax;
    private int fuel;
    private int currentFuel;
    private int fuelConsumption;
    private int vision;
    private int cost;
    private int loadCapacity;
    private Unit[] loadedUnits;
    private int upgradeCounter;
    private bool isActive;
    private int AIbehaviour;

    public Unit(GameGrid<Unit> grid, int x, int z)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
        this.health = 100;
        this.isActive = true;
        this.AIbehaviour = 0;
    }

    public void SetUnitType(UnitType unitType, int team, int currentAmmo, int currentFuel, Unit[] loadedUnits, int upgradeCounter)
    {
        this.unitType = unitType;
        JSONHandler.UnitArray unitTypeValues = JSONHandler.ReadUnitChart("unitChart", unitType);
        this.movementType = (MovementType)System.Enum.Parse(typeof(MovementType), unitTypeValues.movementType);
        this.movementDistance = unitTypeValues.movementDistance;
        this.ammo = unitTypeValues.ammo;
        this.currentAmmo = currentAmmo;
        this.attackDistanceMin = unitTypeValues.attackDistanceMin;
        this.attackDistanceMax = unitTypeValues.attackDistanceMax;
        this.fuel = unitTypeValues.fuel;
        this.currentFuel = currentFuel;
        this.fuelConsumption = unitTypeValues.fuelConsumption;
        this.vision = unitTypeValues.vision;
        this.cost = unitTypeValues.cost;
        this.loadCapacity = unitTypeValues.loadCapacity;
        this.loadedUnits = new Unit[unitTypeValues.loadCapacity];
        this.upgradeCounter = upgradeCounter;
        if(loadedUnits != null)
        {
            Array.Copy(loadedUnits, this.loadedUnits, unitTypeValues.loadCapacity);
        }
        this.team = team;
        grid.TriggerGenericGridChanged(x, z);
    }

    public void SetUnitType(UnitType unitType, int team)
    {
        this.unitType = unitType;
        JSONHandler.UnitArray unitTypeValues = JSONHandler.ReadUnitChart("unitChart", unitType);
        this.movementType = (MovementType)System.Enum.Parse(typeof(MovementType), unitTypeValues.movementType);
        this.movementDistance = unitTypeValues.movementDistance;
        this.ammo = unitTypeValues.ammo;
        this.currentAmmo = unitTypeValues.ammo;
        this.attackDistanceMin = unitTypeValues.attackDistanceMin;
        this.attackDistanceMax = unitTypeValues.attackDistanceMax;
        this.fuel = unitTypeValues.fuel;
        this.currentFuel = unitTypeValues.fuel;
        this.fuelConsumption = unitTypeValues.fuelConsumption;
        this.vision = unitTypeValues.vision;
        this.cost = unitTypeValues.cost;
        this.loadCapacity = unitTypeValues.loadCapacity;
        this.loadedUnits = new Unit[unitTypeValues.loadCapacity];
        this.upgradeCounter = 0;
        this.team = team;
    }

    public void SetUnitVisual(GameObject instance)
    {
        unitVisual = instance;
    }

    public void setXZ(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public GameObject GetUnitInstance()
    {
        return unitVisual;
    }

    public UnitType GetUnitType()
    {
        return unitType;
    }

    public bool isIndirect()
    {
        //if(unitType == UnitType.Artillery || unitType == UnitType.Battleship || unitType == UnitType.Missile || unitType == UnitType.Rocket) return true;
        if(attackDistanceMax > 0) return true;
        return false;
    }

    public int GetTeam()
    {
        return team;
    }

    public int GetX()
    {
        return x;
    }

    public int GetZ()
    {
        return z;
    }

    public bool GetIsActive()
    {
        return isActive;
    }

    public void SetIsActive(bool activation)
    {
        isActive = activation;
    }

    public void SetAIbehaviour(int behaviour)
    {
        AIbehaviour = behaviour;
    }

    public int GetIntFromUnit()
    {
        return (int)unitType;
    }

    public int GetMovementDistance()
    {
        return movementDistance;
    }

    public int GetMovementType()
    {
        return (int)movementType;
    }

    public MovementType GetPureMovementType()
    {
        return movementType;
    }

    public string GetStringFromMovementType()
    {
        return System.Enum.GetName(typeof(MovementType), movementType);
    }

    public int GetHealth()
    {
        return health;
    }

    public int GetMinRange()
    {
        return attackDistanceMin;
    }

    public int GetMaxRange()
    {
        return attackDistanceMax;
    }

    public int GetVision()
    {
        return vision;
    }

    public int GetAmmo()
    {
        return ammo;
    }

    public int GetFuel()
    {
        return fuel;
    }

    public void Reammo()
    {
        currentAmmo = ammo;
    }

    public void Refuel()
    {
        currentFuel = fuel;
    }

    public void AmmoCost(int reduction)
    {
        currentAmmo = currentAmmo - reduction;
    }

    public void FuelCost(int reduction)
    {
        currentFuel = currentFuel - reduction;
    }

    public int GetCost()
    {
        return cost;
    }

    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public int GetCurrentFuel()
    {
        return currentFuel;
    }

    public int GetLoadCapacity()
    {
        return loadCapacity;
    }

    public Unit[] GetLoadedUnits()
    {
        return loadedUnits;
    }

    public int GetUpgradeCounter()
    {
        return upgradeCounter;
    }

    public void LoadUnit(int index, Unit unittoload)
    {
        loadedUnits[index] = unittoload;
    }

    public bool TakeDamage(int damage)
    {
        health -= damage;
        if(health<=0){
            return true;
        }
        return false;
    }

    public void Upgrade()
    {
        upgradeCounter++;
    }

    public void Visualize(Material colour, GameObject instance)
    {
        foreach(Transform child in instance.transform)
        {
            if(child.tag == "ColorCode")
            {
                child.gameObject.GetComponent<MeshRenderer>().material = colour;
            }
        }
    }

    public void VisualDeactivation()
    {
        foreach(Transform child in unitVisual.transform)
        {
            Color c = child.gameObject.GetComponent<Renderer>().material.color;
            c.r *= 0.5f;
            c.g *= 0.5f;
            c.b *= 0.5f;
            child.gameObject.GetComponent<Renderer>().material.color = c;
        }
        this.isActive = false;
    }

    public void VisualActivation()
    {
        foreach(Transform child in unitVisual.transform)
        {
            Color c = child.gameObject.GetComponent<Renderer>().material.color;
            c.r *= 2;
            c.g *= 2;
            c.b *= 2;
            child.gameObject.GetComponent<Renderer>().material.color = c;
        }
        this.isActive = true;
    }

    public override string ToString()
    {
        return unitType.ToString();
    }

    [System.Serializable]
    public class SaveObject
    {
        public UnitType unitType;
        public int health;
        public int team;
        public int x;
        public int z;
        public int ammo;
        public int fuel;
        public Unit[] loadedUnits;
        public int upgradeCounter;
    }

    public SaveObject Save()
    {
        return new SaveObject
        {
            unitType = unitType,
            health = health,
            team = team,
            x = x,
            z = z,
            ammo = ammo,
            fuel = fuel,
            loadedUnits = loadedUnits,
            upgradeCounter = upgradeCounter
        };
    }

    public void Load(SaveObject saveObject)
    {
        unitType = saveObject.unitType;
    }
}