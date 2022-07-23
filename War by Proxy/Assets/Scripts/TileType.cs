using UnityEngine;

[System.Serializable]
public class TilemapObject
{
    public enum TilemapSprite
    {
        Plains,
        Mountains,
        Forest,
        River,
        Road,
        Sea,
        Shore,
        Reef,
        City,
        MilitaryBase,
        Airport,
        Port,
        HQ,
        Radio,
        Lab,
        Outpost
    }

    protected internal GameGrid<TilemapObject> grid;
    protected internal int x;
    protected internal int z;
    [SerializeField] protected TilemapSprite tilemapSprite;
    public GameObject tileVisualPrefab;
    private GameObject tileVisual;
    protected int defenceRating;

    //Change to dictionary later
    protected int movementPenaltyFoot;
    protected int movementPenaltyTires;
    protected int movementPenaltyThreads;
    protected int movementPenaltyShip;
    protected int movementPenaltyLander;
    protected int movementPenaltyAir;

    public TilemapObject()
    {
        this.grid = null;
        this.x = 0;
        this.z = 0;
    }

    public TilemapObject(GameGrid<TilemapObject> grid, int x, int z)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
    }

    public TilemapObject(TilemapObject tmo)
    {
        this.grid = tmo.grid;
        this.x = tmo.x;
        this.z = tmo.z;
    }

    public void SetTilemapSprite(TilemapSprite tilemapSprite)
    {
        this.tilemapSprite = tilemapSprite;
        JSONHandler.TerrainArray tilemapSpriteValues = JSONHandler.ReadTerrainChart("terrainChart", tilemapSprite);
        this.movementPenaltyFoot = tilemapSpriteValues.movementPenaltyFoot;
        this.movementPenaltyThreads = tilemapSpriteValues.movementPenaltyThreads;
        this.movementPenaltyTires = tilemapSpriteValues.movementPenaltyTires;
        this.movementPenaltyAir = tilemapSpriteValues.movementPenaltyAir;
        this.movementPenaltyShip = tilemapSpriteValues.movementPenaltyShip;
        this.movementPenaltyLander = tilemapSpriteValues.movementPenaltyLander;
        this.defenceRating = tilemapSpriteValues.defenceRating;
        grid.TriggerGenericGridChanged(x, z);
    }

    public int GetX()
    {
        return x;
    }

    public int GetZ()
    {
        return z;
    }

    public GameObject GetTileVisual()
    {
        return tileVisual;
    }

    public void setTileVisual(GameObject instance)
    {
        tileVisual = instance;
    }

    public TilemapSprite GetTilemapSprite()
    {
        return tilemapSprite;
    }

    public int GetIntFromSprite()
    {
        return (int)tilemapSprite;
    }

    public int GetMovementPenaltyType(Unit unit)
    {
        switch(unit.GetMovementType())
        {
            case 0: return movementPenaltyFoot;
            case 1: return movementPenaltyTires;
            case 2: return movementPenaltyThreads;
            case 3: return movementPenaltyShip;
            case 4: return movementPenaltyLander;
            case 5: return movementPenaltyAir;
            default: return -1;
        }
    }

    public int GetMovementPenaltyType(int checker)
    {
        switch(checker)
        {
            case 0: return movementPenaltyFoot;
            case 1: return movementPenaltyTires;
            case 2: return movementPenaltyThreads;
            case 3: return movementPenaltyShip;
            case 4: return movementPenaltyLander;
            case 5: return movementPenaltyAir;
            default: return -1;
        }
    }

    public int GetDefence()
    {
        return defenceRating;
    }

    public override string ToString()
    {
        return tilemapSprite.ToString();
    }

    [System.Serializable]
    public class SaveObject
    {
        public string type;
        public TilemapSprite tilemapSprite;
        public int x;
        public int z;
        public int team;
        public int health;
        public int cost;

        public override string ToString()
        {
            return tilemapSprite + " " + x + " " + z + " " + team + " " + health;
        }
    }

    public virtual SaveObject Save()
    {
        return new SaveObject
        {
            type = "TilemapObject",
            tilemapSprite = tilemapSprite,
            x = x,
            z = z,
            team = 0,
            health = 0,
            cost = 0
        };
    }

    public void Load(SaveObject saveObject)
    {
        this.SetTilemapSprite(saveObject.tilemapSprite);
    }
}

[System.Serializable]
public class Building : TilemapObject
{
    protected internal int team;
    protected internal int health;

    public Building()
    {
        this.grid = null;
        this.x = 0;
        this.z = 0;
        this.team = 0;
        this.health = 0;
    }

    public Building(GameGrid<TilemapObject> grid, int x, int z, int team, int health)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
        this.team = team;
        this.health = health;
    }

    public Building(TilemapObject tmo, int team, int health)
    {
        this.grid = tmo.grid;
        this.x = tmo.x;
        this.z = tmo.z;
        this.team = team;
        this.health = health;
    }

    public int GetTeam()
    {
        return team;
    }

    public void SetTeam(int newTeam)
    {
        team = newTeam;
    }

    public int GetHealth()
    {
        return health;
    }

    public void SetHealth(int newHealth)
    {
        health = newHealth;
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

    public override SaveObject Save()
    {
        return new SaveObject
        {
            type = "Building",
            tilemapSprite = tilemapSprite,
            x = x,
            z = z,
            team = team,
            health = health,
            cost = 0
        };
    }
}

[System.Serializable]
public class City : Building
{
    [SerializeField] protected internal int cost;

    public City()
    {
        this.grid = null;
        this.x = 0;
        this.z = 0;
        this.team = 0;
        this.health = 100;
        this.cost = 0;
    }

    public City(GameGrid<TilemapObject> grid, int x, int z, int team, int health)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
        this.team = team;
        this.health = health;
        this.cost = 0;
    }

    public City(TilemapObject tmo, int team, int health)
    {
        this.grid = tmo.grid;
        this.x = tmo.x;
        this.z = tmo.z;
        this.team = team;
        this.health = health;
        this.cost = 0;
    }

    public int GetCost()
    {
        return cost;
    }

    public override SaveObject Save()
    {
        return new SaveObject
        {
            type = "City",
            tilemapSprite = tilemapSprite,
            x = x,
            z = z,
            team = team,
            health = health,
            cost = 0
        };
    }
}

[System.Serializable]
public class MilitaryBase : Building
{
    public MilitaryBase(GameGrid<TilemapObject> grid, int x, int z, int team, int health)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
        this.team = team;
        this.health = health;
    }

    public MilitaryBase(TilemapObject tmo, int team, int health)
    {
        this.grid = tmo.grid;
        this.x = tmo.x;
        this.z = tmo.z;
        this.team = team;
        this.health = health;
    }

    public override SaveObject Save()
    {
        return new SaveObject
        {
            type = "MilitaryBase",
            tilemapSprite = tilemapSprite,
            x = x,
            z = z,
            team = team,
            health = health,
            cost = 0
        };
    }
}

[System.Serializable]
public class Airport : Building
{
    public Airport(GameGrid<TilemapObject> grid, int x, int z, int team, int health)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
        this.team = team;
        this.health = health;
    }

    public Airport(TilemapObject tmo, int team, int health)
    {
        this.grid = tmo.grid;
        this.x = tmo.x;
        this.z = tmo.z;
        this.team = team;
        this.health = health;
    }

    public override SaveObject Save()
    {
        return new SaveObject
        {
            type = "Airport",
            tilemapSprite = tilemapSprite,
            x = x,
            z = z,
            team = team,
            health = health,
            cost = 0
        };
    }
}

[System.Serializable]
public class Port : Building
{
    public Port(GameGrid<TilemapObject> grid, int x, int z, int team, int health)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
        this.team = team;
        this.health = health;
    }

    public Port(TilemapObject tmo, int team, int health)
    {
        this.grid = tmo.grid;
        this.x = tmo.x;
        this.z = tmo.z;
        this.team = team;
        this.health = health;
    }

    public override SaveObject Save()
    {
        return new SaveObject
        {
            type = "Port",
            tilemapSprite = tilemapSprite,
            x = x,
            z = z,
            team = team,
            health = health,
            cost = 0
        };
    }
}

[System.Serializable]
public class HQ : Building
{
    public HQ(GameGrid<TilemapObject> grid, int x, int z, int team, int health)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
        this.team = team;
        this.health = health;
    }

    public HQ(TilemapObject tmo, int team, int health)
    {
        this.grid = tmo.grid;
        this.x = tmo.x;
        this.z = tmo.z;
        this.team = team;
        this.health = health;
    }

    public override SaveObject Save()
    {
        return new SaveObject
        {
            type = "HQ",
            tilemapSprite = tilemapSprite,
            x = x,
            z = z,
            team = team,
            health = health,
            cost = 0
        };
    }
}

[System.Serializable]
public class Radio : City
{
    public Radio(GameGrid<TilemapObject> grid, int x, int z, int team, int health, int cost)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
        this.team = team;
        this.health = health;
        this.cost = cost;
    }

    public Radio(TilemapObject tmo, int team, int health, int cost)
    {
        this.grid = tmo.grid;
        this.x = tmo.x;
        this.z = tmo.z;
        this.team = team;
        this.health = health;
        this.cost = cost;
    }

    public override SaveObject Save()
    {
        return new SaveObject
        {
            type = "Radio",
            tilemapSprite = tilemapSprite,
            x = x,
            z = z,
            team = team,
            health = health,
            cost = cost
        };
    }
}

[System.Serializable]
public class Lab : City
{
    public Lab(GameGrid<TilemapObject> grid, int x, int z, int team, int health, int cost)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
        this.team = team;
        this.health = health;
        this.cost = cost;
    }

    public Lab(TilemapObject tmo, int team, int health, int cost)
    {
        this.grid = tmo.grid;
        this.x = tmo.x;
        this.z = tmo.z;
        this.team = team;
        this.health = health;
        this.cost = cost;
    }

    public override SaveObject Save()
    {
        return new SaveObject
        {
            type = "Lab",
            tilemapSprite = tilemapSprite,
            x = x,
            z = z,
            team = team,
            health = health,
            cost = cost
        };
    }
}

[System.Serializable]
public class Outpost : City
{
    public Outpost(GameGrid<TilemapObject> grid, int x, int z, int team, int health, int cost)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
        this.team = team;
        this.health = health;
        this.cost = cost;
    }

    public Outpost(TilemapObject tmo, int team, int health, int cost)
    {
        this.grid = tmo.grid;
        this.x = tmo.x;
        this.z = tmo.z;
        this.team = team;
        this.health = health;
        this.cost = cost;
    }

    public override SaveObject Save()
    {
        return new SaveObject
        {
            type = "Outpost",
            tilemapSprite = tilemapSprite,
            x = x,
            z = z,
            team = team,
            health = health,
            cost = cost
        };
    }
}