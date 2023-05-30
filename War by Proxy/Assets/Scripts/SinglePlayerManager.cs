using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SinglePlayerManager : MonoBehaviour
{
    public TileType[] tileTypes;
    public Unit[] unitTypes;
    public List<Unit> militaryBaseRecruits;
    public List<Unit> airportRecruits;
    public List<Unit> portRecruits;
    public List<City> cityUpgrades;
    public Material[] teamColours;
    public Transform map;
    public TurnSystem localTurnSystem = new TurnSystem();
    public GameObject canvas;
    public GameObject selectedTile;
    public GameObject mapButtonPrefab;
    public GameObject visibleGridPrefab;
    public GameObject turnIndicator;
    public int[,] damageMatrix;
    int mapSizeX;
    int mapSizeZ;
    int x;
    int z;
    int movementx;
    int movementz;
    int movedx;
    int movedz;
    int unloadindex;
    Transform tile;
    [SerializeField] Transform mainCamera;

    private GameGrid<int> grid;
    private Transform lastSelected = null;
    private string currentguiside = "left";
    private int builder = -1;
    private int unitSetter = -1;
    private int currentPlacer = 0;
    public int turnCounter = 0;
    private int turnsPassed = 1;
    private int creatorSelector = 0;
    private string unitSelected = "false";
    private bool menuUp = false;
    public PathMaking pathmaking;
    public PathFinding pathfinding;
    public Tilemap tilemap;
    public Unitmap unitmap;
    List<DijkstraNode> graph = new List<DijkstraNode>();
    List<GameObject> selectedTiles = new List<GameObject>();
    List<Unit> targetables = new List<Unit>();
    List<Unit> supplyTargetables = new List<Unit>();
    public List<Player> playersInMatch = new List<Player>();
    public AIQuirks AI;

    public int debugMode = 2;
    private int localPlayerID;

    public void PrintListUnits(List<Unit> array, int x, int z, int team)
    {
        canvas.GetComponent<GameGUI>().SPShowRecruitInfo(array, x, z, team);
    }

    public void PrintListCityUpgrades(List<City> array, int x, int z, Transform tile, int team)
    {
        canvas.GetComponent<GameGUI>().SPShowBuildingUpgrades(array, x, z, tile, team);
    }

    public void AddRecruitListeners(GameObject button, Unit unit, int x, int z, int team)
    {
        button.GetComponent<Button>().onClick.AddListener(() => RecruitUnit(unit.GetUnitType(), x, z, team));
        button.GetComponent<Button>().onClick.AddListener(() => CloseMenu());
        button.GetComponent<Button>().onClick.AddListener(() => canvas.GetComponent<GameGUI>().HideRecruitInfo());
        RecruitButtonHover component = button.AddComponent<RecruitButtonHover>();
        component.canvas = canvas;
        component.unit = unit;
        if(playersInMatch[localPlayerID].GetFunds() < unit.GetCost()) button.GetComponent<Button>().interactable = false;
    }

    public void AddBuildingListeners(GameObject button, City cityupgrade, int x, int z, Transform tile, int team)
    {
        button.GetComponent<Button>().onClick.AddListener(() => UpgradeCity(cityupgrade.GetTilemapSprite(), x, z, tile.position, team));
        button.GetComponent<Button>().onClick.AddListener(() => RPCDestroy(tile.gameObject.name));
        button.GetComponent<Button>().onClick.AddListener(() => CloseMenu());
        button.GetComponent<Button>().onClick.AddListener(() => canvas.GetComponent<GameGUI>().HideBuildingUpgrades());
        BuildingButtonHover component = button.AddComponent<BuildingButtonHover>();
        component.canvas = canvas;
        component.cityupgrade = cityupgrade;
        if(playersInMatch[localPlayerID].GetFunds() < ((City)cityupgrade).GetCost()) button.GetComponent<Button>().interactable = false;
    }

    public void RPCDestroy(string objectname)
    {
        Destroy(GameObject.Find(objectname));
    }

    public void RecruitUnit(Unit.UnitType type, int x, int z, int team)
    {
        Unit un = unitTypes[(int)type];
        un.SetUnitType(un.GetUnitType(), team);
        unitmap.InsertUnit(x, z, type, team, un.GetAmmo(), un.GetFuel(), null, 0);
        UnitInstantiate(un, x, z, team);
        playersInMatch[turnCounter].AddUnit(unitmap.GetGrid().GetGridObject(x, z));
        playersInMatch[turnCounter].ChangeFunds(-(unitmap.GetGrid().GetGridObject(x, z).GetCost()));
    }

    public void UpgradeCity(TileType.TilemapSprite upgrade, int x, int z, Vector3 pos, int team)
    {
        playersInMatch[turnCounter].RemoveBuilding((Building)tilemap.GetGrid().GetGridObject(x, z));
        tilemap.SetTilemapSprite(pos, upgrade, team);
        TileType tileObject = tilemap.GetGrid().GetGridObject(x, z);
        TileType ttm = tileTypes[(int)upgrade];
        GameObject tileInstance = Instantiate(ttm.tileVisualPrefab, pos, Quaternion.identity, map);
        tileInstance.name = tileObject.ToString() + x + z;
        tileObject.setTileVisual(tileInstance);
        ((Building)tileObject).Visualize(teamColours[((Building)tileObject).GetTeam()], tileInstance);
        playersInMatch[turnCounter].AddBuilding((Building)tilemap.GetGrid().GetGridObject(x, z));
        playersInMatch[turnCounter].ChangeFunds(-(((City)tileObject).GetCost()));
    }

    public void InitPlayers()
    {
        playersInMatch.Add(new Player(1, tilemap, unitmap, AI));
        playersInMatch.Add(new Player(2, tilemap, unitmap, AI));
        localTurnSystem.TurnInit(playersInMatch[turnCounter]);
    }

    private bool MouseClickDetector(ref int x, ref int z)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Transform tile = hit.transform;
            tilemap.GetGrid().GetXZ(tile.position, out x, out z);
            return true;
        }
        else
        {
            x = -1;
            z = -1;
            return false;
        }
    }

    private bool MouseClickDetector(ref int x, ref int z, ref Transform tile)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            tile = hit.transform;
            if(tile.parent != null && tile.parent.name != "Map")
            {
                tile = tile.parent;
            }
            tilemap.GetGrid().GetXZ(tile.position, out x, out z);
            return true;
        }
        else
        {
            tile = null;
            x = -1;
            z = -1;
            return false;
        }
    }

    public void CloseMenu()
    {
        menuUp = false;
    }

    public void UnitInstantiate(Unit un, int x, int z, int colorID)
    {
        GameObject unitInstance;
        if(un.unitVisualPrefab != null) 
        {
            unitInstance = Instantiate(un.unitVisualPrefab, new Vector3(x * 2, 0.7f, z * 2), Quaternion.identity, map);
        }
        else
        {
            unitInstance = Instantiate(unitTypes[un.GetIntFromUnit()].unitVisualPrefab, new Vector3(x * 2, 0.7f, z * 2), Quaternion.identity, map);
        }
        unitInstance.name = un.ToString() + x + z;
        unitmap.GetGrid().GetGridObject(x, z).SetUnitVisual(unitInstance);
        un.Visualize(teamColours[colorID], unitInstance);
    }

    private void Start()
    {
        damageMatrix = JSONHandler.ReadDamageMatrix("damageChart");
        localPlayerID = 0;
        mapSizeX = 16;
        mapSizeZ = 14;
        mainCamera.position = new Vector3(mapSizeX - 1, 18, mapSizeZ - 16);
        if(debugMode == 3)
        {
            unitmap = new Unitmap(mapSizeX, mapSizeZ, 2f, Vector3.zero);
            tilemap = new Tilemap(mapSizeX, mapSizeZ, 2f, Vector3.zero);
            GenerateMapVisual();
        }
        else if(debugMode == 4)
        {
            unitmap = new Unitmap(mapSizeX, mapSizeZ, 2f, Vector3.zero);
            tilemap = new Tilemap(mapSizeX, mapSizeZ, 2f, Vector3.zero);
            pathmaking = new PathMaking(mapSizeX, mapSizeZ);
            pathfinding = new PathFinding(mapSizeX, mapSizeZ);
            GenerateMapVisual();
        }
        InitPlayers();
        canvas.GetComponent<GameGUI>().quickmenu.transform.Find("QuickMenuOverView").GetComponent<Button>().onClick.AddListener(() => canvas.GetComponent<GameGUI>().SPShowMatchOverview(teamColours));
        canvas.GetComponent<GameGUI>().yieldconfirmation.transform.Find("ConfirmButton").GetComponent<Button>().onClick.AddListener(() => GameLost(localPlayerID));
    }

    void GenerateMapVisual()
    {
        if (debugMode == 3 || debugMode == 4)
        {
            tilemap.Load("Islander_Tiles");
            unitmap.Load("Islander_Units");
            for (int z = 0; z < mapSizeZ; z++)
            {
                for (int x = 0; x < mapSizeX; x++)
                {
                    /*if((bool)PhotonNetwork.LocalPlayer.CustomProperties["ShowGrid"])
                    {
                        Instantiate(visibleGridPrefab, new Vector3(x * 2 - 1, 0, z * 2 - 1), Quaternion.identity, map);
                    }*/
                    TileType tileObject = tilemap.GetGrid().GetGridObject(x, z);
                    TileType tt = tileTypes[tilemap.GetIntFromSprite(x, z)];
                    GameObject tileInstance = Instantiate(tt.tileVisualPrefab, new Vector3(x * 2, 0, z * 2), Quaternion.identity, map);
                    tileInstance.name = tileObject.ToString() + x + z;
                    tileObject.setTileVisual(tileInstance);
                    if(tileObject.GetType().IsSubclassOf(typeof(Building)))
                    {
                        ((Building)tileObject).Visualize(teamColours[((Building)tileObject).GetTeam()], tileInstance);
                    }
                    if(unitmap.GetGrid().GetGridObject(x, z) != null)
                    {
                        Unit un = unitTypes[unitmap.GetGrid().GetGridObject(x, z).GetIntFromUnit()];
                        UnitInstantiate(un, x, z, unitmap.GetGrid().GetGridObject(x, z).GetTeam());
                    }
                }
            }
        }
    }

    public void ActionMove(int x, int z)
    {
        GameObject movableUnit = GameObject.Find(unitmap.GetGrid().GetGridObject(x, z).ToString() + graph[0].x + graph[0].z);
        if(movableUnit)
        {
            movableUnit.transform.position = new Vector3(x * 2, 0.7f, z * 2);
            movableUnit.name = unitmap.GetGrid().GetGridObject(x, z).ToString() + x + z;
            unitmap.GetGrid().GetGridObject(x, z).SetIsActive(false);
            unitmap.GetGrid().GetGridObject(x, z).VisualDeactivation();
            unitmap.GetGrid().GetGridObject(x, z).FuelCost(Mathf.Abs(graph[0].x - x) + Mathf.Abs(graph[0].z - z));
            localTurnSystem.MoveUnitAfterOrder(unitmap.GetGrid().GetGridObject(x, z));
        }
        unitSelected = "false";
        foreach(GameObject selected in selectedTiles)
        {
            Destroy(selected);
        }
        canvas.GetComponent<GameGUI>().HideActionInfo();
    }

    public void ActionAttack(int x, int z)
    {
        GameObject movableUnit = GameObject.Find(unitmap.GetGrid().GetGridObject(x, z).ToString() + graph[0].x + graph[0].z);
        if(movableUnit)
        {
            movableUnit.transform.position = new Vector3(x * 2, 0.7f, z * 2);
            movableUnit.name = unitmap.GetGrid().GetGridObject(x, z).ToString() + x + z;
            unitmap.GetGrid().GetGridObject(x, z).FuelCost(Mathf.Abs(graph[0].x - x) + Mathf.Abs(graph[0].z - z));
        }
        foreach(GameObject selected in selectedTiles)
        {
            Destroy(selected);
        }
        for (int i = 1; i < targetables.Count; i++)
        {
            selectedTiles.Add(Instantiate(selectedTile, new Vector3(targetables[i].GetX() * 2, 0.1f, targetables[i].GetZ() * 2), Quaternion.identity, map));
        }
        unitSelected = "fire";
        movementx = x;
        movementz = z;
        canvas.GetComponent<GameGUI>().HideActionInfo();
    }

    public void ActionCapture(int x, int z)
    {
        GameObject movableUnit = GameObject.Find(unitmap.GetGrid().GetGridObject(x, z).ToString() + graph[0].x + graph[0].z);
        if(movableUnit)
        {
            movableUnit.transform.position = new Vector3(x * 2, 0.7f, z * 2);
            movableUnit.name = unitmap.GetGrid().GetGridObject(x, z).ToString() + x + z;
            unitmap.GetGrid().GetGridObject(x, z).SetIsActive(false);
            unitmap.GetGrid().GetGridObject(x, z).VisualDeactivation();
            unitmap.GetGrid().GetGridObject(x, z).FuelCost(Mathf.Abs(graph[0].x - x) + Mathf.Abs(graph[0].z - z));
            localTurnSystem.MoveUnitAfterOrder(unitmap.GetGrid().GetGridObject(x, z));
        }
        foreach(GameObject selected in selectedTiles)
        {
            Destroy(selected);
        }

        Transform localtile = GameObject.Find(tilemap.GetGrid().GetGridObject(x, z).ToString() + x + z).transform;
        TileType tileChecker = tilemap.GetGrid().GetGridObject(x, z);
        Unit unitChecker = unitmap.GetGrid().GetGridObject(x, z);
        int possibleloser = -1;
        string oldtile = tileChecker.ToString() + x + z;
        if(unitChecker != null && !(tileChecker.GetType().Equals(typeof(TileType))) && unitChecker.GetTeam() != ((Building)tileChecker).GetTeam())
        {
            Building helper = (Building)tileChecker;
            helper.SetHealth(helper.GetHealth() - unitChecker.GetHealth());
            if(helper.GetHealth() <= 0)
            {
                if(helper.GetTeam() != 0) playersInMatch[helper.GetTeam()-1].RemoveBuilding(helper);
                if(tileChecker.GetType().Equals(typeof(HQ)))
                {
                    helper.SetTilemapSprite(TileType.TilemapSprite.City);
                    possibleloser = helper.GetTeam()-1;
                }
                helper.SetTeam(unitChecker.GetTeam());
                TileType ttm = tileTypes[tilemap.GetIntFromSprite(x, z)];
                GameObject tileInstance = Instantiate(ttm.tileVisualPrefab, localtile.position, Quaternion.identity, map);
                tileInstance.name = tileChecker.ToString() + x + z;
                tileChecker.setTileVisual(tileInstance);
                playersInMatch[turnCounter].AddBuilding(helper);
                helper.Visualize(teamColours[unitChecker.GetTeam()], tileInstance);
                Destroy(localtile.gameObject);
            }
            if(possibleloser != -1)
            {
                GameLost(possibleloser);
            }
        }
        unitSelected = "false";
        canvas.GetComponent<GameGUI>().HideActionInfo();
    }

    public void ActionSupply(int x, int z)
    {
        GameObject movableUnit = GameObject.Find(unitmap.GetGrid().GetGridObject(x, z).ToString() + graph[0].x + graph[0].z);
        if(movableUnit)
        {
            movableUnit.transform.position = new Vector3(x * 2, 0.7f, z * 2);
            movableUnit.name = unitmap.GetGrid().GetGridObject(x, z).ToString() + x + z;
            unitmap.GetGrid().GetGridObject(x, z).SetIsActive(false);
            unitmap.GetGrid().GetGridObject(x, z).VisualDeactivation();
            unitmap.GetGrid().GetGridObject(x, z).FuelCost(Mathf.Abs(graph[0].x - x) + Mathf.Abs(graph[0].z - z));
            localTurnSystem.MoveUnitAfterOrder(unitmap.GetGrid().GetGridObject(x, z));
        }
        foreach(GameObject selected in selectedTiles)
        {
            Destroy(selected);
        }
        Unit supplier = unitmap.GetGrid().GetGridObject(supplyTargetables[0].GetX(), supplyTargetables[0].GetZ());
        supplier.SetIsActive(false);
        supplier.VisualDeactivation();
        localTurnSystem.MoveUnitAfterOrder(supplier);
        SynchronizeSupply(x, z);
        unitSelected = "false";
        canvas.GetComponent<GameGUI>().HideActionInfo();
    }

    public void ActionLoad(int x, int z)
    {
        GameObject movableUnit = GameObject.Find(unitmap.GetGrid().GetGridObject(x, z).ToString() + graph[0].x + graph[0].z);
        if(movableUnit)
        {
            movableUnit.transform.position = new Vector3(x * 2, 0.7f, z * 2);
            movableUnit.name = unitmap.GetGrid().GetGridObject(x, z).ToString() + x + z;
            unitmap.GetGrid().GetGridObject(x, z).FuelCost(Mathf.Abs(graph[0].x - x) + Mathf.Abs(graph[0].z - z));
        }
        foreach(GameObject selected in selectedTiles)
        {
            Destroy(selected);
        }
        foreach(Unit potentialTransport in unitmap.GetFriendlyUnitsInRange(x, z, unitmap.GetGrid().GetGridObject(x, z)))
        {
            if(potentialTransport.GetLoadCapacity() != 0 && potentialTransport.GetLoadedUnits()[potentialTransport.GetLoadCapacity()-1] == null)
            {
                selectedTiles.Add(Instantiate(selectedTile, new Vector3(potentialTransport.GetX() * 2, 0.1f, potentialTransport.GetZ() * 2), Quaternion.identity, map));
            }
        }
        unitSelected = "load";
        movementx = x;
        movementz = z;
        canvas.GetComponent<GameGUI>().HideActionInfo();
    }

    public bool GetTransportCompatibility(Unit transport, int loadCandidateX, int loadCandidateZ)
    {
        Unit loadCandidate = unitmap.GetGrid().GetGridObject(loadCandidateX, loadCandidateZ);
        if(transport.GetUnitType() == Unit.UnitType.APC && loadCandidate.GetStringFromMovementType() == "Foot") return true;
        if(transport.GetUnitType() == Unit.UnitType.Theli && loadCandidate.GetStringFromMovementType() == "Foot") return true;
        if(transport.GetUnitType() == Unit.UnitType.Tship && (loadCandidate.GetStringFromMovementType() == "Foot" || loadCandidate.GetStringFromMovementType() == "Tires" || loadCandidate.GetStringFromMovementType() == "Threads")) return true;
        return false;
    }

    public void ActionUnload(int x, int z, int index)
    {
        GameObject movableUnit = GameObject.Find(unitmap.GetGrid().GetGridObject(x, z).ToString() + graph[0].x + graph[0].z);
        if(movableUnit)
        {
            movableUnit.transform.position = new Vector3(x * 2, 0.7f, z * 2);
            movableUnit.name = unitmap.GetGrid().GetGridObject(x, z).ToString() + x + z;
            unitmap.GetGrid().GetGridObject(x, z).FuelCost(Mathf.Abs(graph[0].x - x) + Mathf.Abs(graph[0].z - z));
        }
        foreach(GameObject selected in selectedTiles)
        {
            Destroy(selected);
        }
        foreach(TileType tile in tilemap.GetNeighbouringTiles(x, z, unitmap.GetGrid().GetGridObject(x, z).GetLoadedUnits()[index], unitmap))
        {
            selectedTiles.Add(Instantiate(selectedTile, new Vector3(tile.GetX() * 2, 0.1f, tile.GetZ() * 2), Quaternion.identity, map));
        }
        unitSelected = "unload";
        movementx = x;
        movementz = z;
        unloadindex = index;
        canvas.GetComponent<GameGUI>().HideActionInfo();
    }

    public Unit[] UnitArrayShiftLeft(Unit[] array, int startindex)
    {
        string arraychecker = "Current array: ";
        for(int i = 0; i < array.Length; i++)
        {
            if(array[i] != null)
            {
                arraychecker += array[i].ToString() + "-";
            }
            else
            {
                arraychecker += "Null" + "-";
            }
        }
        Array.Copy(array, startindex, array, startindex-1, array.Length - startindex);
        Array.Clear(array, array.Length - 1, 1);
        arraychecker = "Array result: ";
        for(int i = 0; i < array.Length; i++)
        {
            if(array[i] != null)
            {
                arraychecker += array[i].ToString() + "-";
            }
            else
            {
                arraychecker += "Null" + "-";
            }
        }
        return array;
    }

    public void ActionUpgrade(int x, int z)
    {
        GameObject movableUnit = GameObject.Find(unitmap.GetGrid().GetGridObject(x, z).ToString() + graph[0].x + graph[0].z);
        if(movableUnit)
        {
            movableUnit.transform.position = new Vector3(x * 2, 0.7f, z * 2);
            movableUnit.name = unitmap.GetGrid().GetGridObject(x, z).ToString() + x + z;
            unitmap.GetGrid().GetGridObject(x, z).SetIsActive(false);
            unitmap.GetGrid().GetGridObject(x, z).VisualDeactivation();
            unitmap.GetGrid().GetGridObject(x, z).FuelCost(Mathf.Abs(graph[0].x - x) + Mathf.Abs(graph[0].z - z));
            localTurnSystem.MoveUnitAfterOrder(unitmap.GetGrid().GetGridObject(x, z));
        }
        foreach(GameObject selected in selectedTiles)
        {
            Destroy(selected);
        }
        SynchronizeUpgrade(x, z);
        unitSelected = "false";
        canvas.GetComponent<GameGUI>().HideActionInfo();
    }

    public void EndTurn()
    {
        pathmaking.ClearGrid();
        targetables.Clear();
        foreach(Unit usedUnit in localTurnSystem.GetUnitsOrdersReceived())
        {
            usedUnit.SetIsActive(true);
            usedUnit.VisualActivation();
        }
        menuUp = false;
        canvas.GetComponent<GameGUI>().quickmenu.SetActive(false);
        SynchronizeTurn();
    }

    public void GameLost(int playerID)
    {
        RemovePlayerFromGame(playerID);
        //photonView.RPC("CheckVictoryConditions", RpcTarget.Others);
        if(playerID == localPlayerID)
        {
            canvas.GetComponent<GameGUI>().ShowGameEndDialog(false, true);
            menuUp = true;
        }
        else
        {
            CheckVictoryConditions(localPlayerID);
        }
    }

    public void RemovePlayerFromGame(int PlayerID)
    {
        foreach(Unit destroyUnit in playersInMatch[PlayerID].GetUnloadedUnits())
        {
            GameObject destroyable = GameObject.Find(unitmap.GetGrid().GetGridObject(destroyUnit.GetX(), destroyUnit.GetZ()).ToString() + destroyUnit.GetX() + destroyUnit.GetZ());
            Destroy(destroyable);
            unitmap.GetGrid().SetGridObject(destroyUnit.GetX(), destroyUnit.GetZ(), null);
        }
        foreach(Building destroyBuilding in playersInMatch[PlayerID].GetBuildings())
        {
            destroyBuilding.SetTeam(0);
            RPCDestroy(destroyBuilding.ToString() + destroyBuilding.GetX() + destroyBuilding.GetZ());
            if(destroyBuilding.GetTilemapSprite() == TileType.TilemapSprite.HQ || destroyBuilding.GetType().IsSubclassOf(typeof(City)))
            {
                destroyBuilding.SetTilemapSprite(TileType.TilemapSprite.City);
            }
            TileType ttm = tileTypes[tilemap.GetIntFromSprite(destroyBuilding.GetX(), destroyBuilding.GetZ())];
            GameObject tileInstance = Instantiate(ttm.tileVisualPrefab, tilemap.GetGrid().GetWorldPosition(destroyBuilding.GetX(), destroyBuilding.GetZ()), Quaternion.identity, map);
            tileInstance.name = destroyBuilding.ToString() + destroyBuilding.GetX() + destroyBuilding.GetZ();
            tilemap.GetGrid().GetGridObject(destroyBuilding.GetX(), destroyBuilding.GetZ()).setTileVisual(tileInstance);
            destroyBuilding.Visualize(teamColours[0], tileInstance);
        }
        playersInMatch.RemoveAt(PlayerID);
    }

    public void CheckVictoryConditions(int playerID)
    {
        bool victory = true;
        for (int z = 0; z < mapSizeZ; z++)
        {
            for (int x = 0; x < mapSizeX; x++)
            {
                if(tilemap.GetGrid().GetGridObject(x, z).GetTilemapSprite() == TileType.TilemapSprite.HQ && ((Building)tilemap.GetGrid().GetGridObject(x, z)).GetTeam() != playerID + 1)
                {
                    victory = false;
                }
            }
        }
        if(victory && playerID == localPlayerID)
        {
            canvas.GetComponent<GameGUI>().ShowGameEndDialog(victory, true);
            foreach(Player playa in playersInMatch)
            {
                if(playa.GetTeam() == localPlayerID + 1) SynchronizeGrantWin(localPlayerID + 1);
            }
            menuUp = true;
        }
        else if(victory && playerID != localPlayerID)
        {
            //AI evaluation best there
        }
    }

    /*public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        int leavingPlayerTeam = (int)otherPlayer.CustomProperties["Index"] + 1;
        bool viable = false;
        foreach(Player playa in playersInMatch)
        {
            if(playa.GetTeam() == leavingPlayerTeam && !playa.GetVictoryStatus()) viable = true;
        }
        if(viable)
        {
            RemovePlayerFromGame((int)otherPlayer.CustomProperties["Index"]);
            CheckVictoryConditions();
        }
    }*/

    public bool CheckAlliance(int player1, int player2)
    {
        if(player1 == 0 || player2 == 0) return false;
        int team1 = 1;
        int team2 = 2;
        if(team1 != 0 && team2 != 0 && team1 == team2) return true;
        return false;
    }

    public void SynchronizeSupply(int x, int z)
    {
        List<Unit> supplytargets = new List<Unit>(unitmap.GetFriendlyUnitsInRange(x, z, unitmap.GetGrid().GetGridObject(x, z)));
        foreach(Unit target in supplytargets)
        {
            target.Refuel();
            target.Reammo();
        }
    }

    private void SynchronizeUpgrade(int x, int z)
    {
        unitmap.GetGrid().GetGridObject(x, z).Upgrade();
    }

    private void SynchronizeTurn()
    {
        if(turnCounter + 1 >= playersInMatch.Count)
        {
            turnCounter = 0;
        }
        else
        {
            turnCounter++;
        }
        localTurnSystem.TurnInit(playersInMatch[turnCounter]);
        if(turnCounter == 1)
        {
            List<Unit> unitsToMove = AI.sortUnitMovementList(new List<Unit>(playersInMatch[turnCounter].GetUnloadedUnits()), this);
            //AI logic
            Debug.Log("List of AI unloaded units this turn:");
            string list = "";
            foreach(Unit unit in unitsToMove)
            {
                list += unit.ToString() + unit.GetX() + unit.GetZ() + " - ";
            }
            Debug.Log(list);

            foreach(Unit unit in unitsToMove)
            {
                switch(unit.GetAIbehaviour())
                {
                    case 0: AI.StationaryBehaviour(unit); break;
                    case 1: AI.OccupyProductionBehaviour(unit); break;
                    case 2: AI.VIPCoverBehaviour(unit); break;
                    case 3: AI.SecureBuildingBehaviour(unit); break;
                    case 4: AI.AggressiveBehaviour(unit); break;
                    case 5: AI.DefensiveBehaviour(unit); break;
                    case 6: AI.InfantryBehaviour(unit); break;
                    case 7: AI.TransportLandBehaviour(unit); break;
                    case 8: AI.TransportAirBehaviour(unit); break;
                }
            }
            foreach(Building building in playersInMatch[turnCounter].GetBuildings())
            {
                if(unitmap.GetGrid().GetGridObject(building.GetX(), building.GetZ()) == null)
                {
                    if(building.GetType().Equals(typeof(MilitaryBase)))
                    {
                        AI.AIRecruitUnit(building.GetX(), building.GetZ(), 1, this);
                    }
                    else if(building.GetType().Equals(typeof(Airport)))
                    {
                        AI.AIRecruitUnit(building.GetX(), building.GetZ(), 2, this);
                    }
                    else if(building.GetType().Equals(typeof(Port)))
                    {
                        AI.AIRecruitUnit(building.GetX(), building.GetZ(), 3, this);
                    }
                }
            }

            Debug.Log("List of AI unloaded units after the turn:");
            list = "";
            foreach(Unit unit in playersInMatch[turnCounter].GetUnloadedUnits())
            {
                list += unit.ToString() + unit.GetX() + unit.GetZ() + " - ";
            }
            Debug.Log(list);

            SynchronizeTurn();
        }
    }

    private void SynchronizeGrantWin(int team)
    {
        foreach(Player playa in playersInMatch)
        {
            if(playa.GetTeam() == team) playa.GrantWin();
        }
    }

    //AI helper functions below this command and above Update()
    public List<DijkstraNode> getUnitMovementGraph(int localx, int localz)
    {
        return pathmaking.CreateReachableGraph(localx, localz, unitmap.GetGrid().GetGridObject(localx, localz), tilemap, unitmap, false, false);
    }

    public List<DijkstraNode> getUnitMovementGraph(int localx, int localz, bool infantryCheck)
    {
        return pathmaking.CreateReachableGraph(localx, localz, unitmap.GetGrid().GetGridObject(localx, localz), Unit.UnitType.Infantry, tilemap, unitmap, false, false);
    }

    public List<DijkstraNode> getMapReachabilityGraph(int localx, int localz)
    {
        return pathmaking.CreateReachableGraph(localx, localz, unitmap.GetGrid().GetGridObject(localx, localz), tilemap, unitmap, false, true);
    }

    public int getPotentialAttackValue(Unit attacker, Unit defender)
    {
        unitmap.SimulateAttack(attacker, defender, damageMatrix, tilemap.GetGrid().GetGridObject(defender.GetX(), defender.GetZ()).GetDefence(), tilemap.GetGrid().GetGridObject(attacker.GetX(), attacker.GetZ()).GetDefence(), out int attack, out int counterattack);
        return (int)(defender.GetCost() * ((float)attack / 100) - attacker.GetCost() * ((float)counterattack / 100));
    }

    public int getPotentialCounterattack(Unit attacker, Unit defender)
    {
        unitmap.SimulateAttack(attacker, defender, damageMatrix, tilemap.GetGrid().GetGridObject(defender.GetX(), defender.GetZ()).GetDefence(), tilemap.GetGrid().GetGridObject(attacker.GetX(), attacker.GetZ()).GetDefence(), out int attack, out int counterattack);
        return counterattack;
    }

    private void Update()
    {
        if(debugMode == 3)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) { creatorSelector = 1; Debug.Log("Terrain"); }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { creatorSelector = 2; Debug.Log("Neutral Buildings"); }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { creatorSelector = 3; Debug.Log("Red Buildings"); }
            if (Input.GetKeyDown(KeyCode.Alpha4)) { creatorSelector = 4; Debug.Log("Blue Buildings"); }
            if (Input.GetKeyDown(KeyCode.Alpha5)) { creatorSelector = 5; Debug.Log("Neutral Units"); }
            if (Input.GetKeyDown(KeyCode.Alpha6)) { creatorSelector = 6; Debug.Log("Red Units"); }
            if (Input.GetKeyDown(KeyCode.Alpha7)) { creatorSelector = 7; Debug.Log("Blue Units"); }

            switch(creatorSelector)
            {
                case 1:
                    if (Input.GetKeyDown(KeyCode.Q)) { builder = 0; currentPlacer = 0; Debug.Log("Plains"); }
                    if (Input.GetKeyDown(KeyCode.W)) { builder = 1; currentPlacer = 0; Debug.Log("Mountains"); }
                    if (Input.GetKeyDown(KeyCode.E)) { builder = 2; currentPlacer = 0; Debug.Log("Forest"); }
                    if (Input.GetKeyDown(KeyCode.R)) { builder = 3; currentPlacer = 0; Debug.Log("River"); }
                    if (Input.GetKeyDown(KeyCode.T)) { builder = 4; currentPlacer = 0; Debug.Log("Road"); }
                    if (Input.GetKeyDown(KeyCode.Y)) { builder = 5; currentPlacer = 0; Debug.Log("Sea"); }
                    if (Input.GetKeyDown(KeyCode.U)) { builder = 6; currentPlacer = 0; Debug.Log("Shore"); }
                    if (Input.GetKeyDown(KeyCode.I)) { builder = 7; currentPlacer = 0; Debug.Log("Reef"); }
                    break;
                case 2:
                    if (Input.GetKeyDown(KeyCode.Q)) { builder = 8; currentPlacer = 0; Debug.Log("Neutral City"); }
                    if (Input.GetKeyDown(KeyCode.W)) { builder = 9; currentPlacer = 0; Debug.Log("Neutral MilitaryBase"); }
                    if (Input.GetKeyDown(KeyCode.E)) { builder = 10; currentPlacer = 0; Debug.Log("Neutral Airport"); }
                    if (Input.GetKeyDown(KeyCode.R)) { builder = 11; currentPlacer = 0; Debug.Log("Neutral Port"); }
                    if (Input.GetKeyDown(KeyCode.T)) { builder = 12; currentPlacer = 0; Debug.Log("Neutral HQ"); }
                    if (Input.GetKeyDown(KeyCode.Y)) { builder = 13; currentPlacer = 0; Debug.Log("Neutral Radio"); }
                    if (Input.GetKeyDown(KeyCode.U)) { builder = 14; currentPlacer = 0; Debug.Log("Neutral Lab"); }
                    if (Input.GetKeyDown(KeyCode.I)) { builder = 15; currentPlacer = 0; Debug.Log("Neutral Outpost"); }
                    break;
                case 3:
                    if (Input.GetKeyDown(KeyCode.Q)) { builder = 8; currentPlacer = 1; Debug.Log("Red City"); }
                    if (Input.GetKeyDown(KeyCode.W)) { builder = 9; currentPlacer = 1; Debug.Log("Red MilitaryBase"); }
                    if (Input.GetKeyDown(KeyCode.E)) { builder = 10; currentPlacer = 1; Debug.Log("Red Airport"); }
                    if (Input.GetKeyDown(KeyCode.R)) { builder = 11; currentPlacer = 1; Debug.Log("Red Port"); }
                    if (Input.GetKeyDown(KeyCode.T)) { builder = 12; currentPlacer = 1; Debug.Log("Red HQ"); }
                    if (Input.GetKeyDown(KeyCode.Y)) { builder = 13; currentPlacer = 1; Debug.Log("Red Radio"); }
                    if (Input.GetKeyDown(KeyCode.U)) { builder = 14; currentPlacer = 1; Debug.Log("Red Lab"); }
                    if (Input.GetKeyDown(KeyCode.I)) { builder = 15; currentPlacer = 1; Debug.Log("Red Outpost"); }
                    break;
                case 4:
                    if (Input.GetKeyDown(KeyCode.Q)) { builder = 8; currentPlacer = 2; Debug.Log("Blue City"); }
                    if (Input.GetKeyDown(KeyCode.W)) { builder = 9; currentPlacer = 2; Debug.Log("Blue MilitaryBase"); }
                    if (Input.GetKeyDown(KeyCode.E)) { builder = 10; currentPlacer = 2; Debug.Log("Blue Airport"); }
                    if (Input.GetKeyDown(KeyCode.R)) { builder = 11; currentPlacer = 2; Debug.Log("Blue Port"); }
                    if (Input.GetKeyDown(KeyCode.T)) { builder = 12; currentPlacer = 2; Debug.Log("Blue HQ"); }
                    if (Input.GetKeyDown(KeyCode.Y)) { builder = 13; currentPlacer = 2; Debug.Log("Blue Radio"); }
                    if (Input.GetKeyDown(KeyCode.U)) { builder = 14; currentPlacer = 2; Debug.Log("Blue Lab"); }
                    if (Input.GetKeyDown(KeyCode.I)) { builder = 15; currentPlacer = 2; Debug.Log("Blue Outpost"); }
                    break;
                case 5:
                    if (Input.GetKeyDown(KeyCode.Q)) { builder = 16; currentPlacer = 0; Debug.Log("Neutral AATank"); }
                    if (Input.GetKeyDown(KeyCode.W)) { builder = 17; currentPlacer = 0; Debug.Log("Neutral APC"); }
                    if (Input.GetKeyDown(KeyCode.E)) { builder = 18; currentPlacer = 0; Debug.Log("Neutral Artillery"); }
                    if (Input.GetKeyDown(KeyCode.R)) { builder = 19; currentPlacer = 0; Debug.Log("Neutral Heli"); }
                    if (Input.GetKeyDown(KeyCode.T)) { builder = 20; currentPlacer = 0; Debug.Log("Neutral Battleship"); }
                    if (Input.GetKeyDown(KeyCode.Y)) { builder = 21; currentPlacer = 0; Debug.Log("Neutral Bomber"); }
                    if (Input.GetKeyDown(KeyCode.U)) { builder = 22; currentPlacer = 0; Debug.Log("Neutral Carrier"); }
                    if (Input.GetKeyDown(KeyCode.I)) { builder = 23; currentPlacer = 0; Debug.Log("Neutral Cruiser"); }
                    if (Input.GetKeyDown(KeyCode.O)) { builder = 24; currentPlacer = 0; Debug.Log("Neutral Fighter"); }
                    if (Input.GetKeyDown(KeyCode.P)) { builder = 25; currentPlacer = 0; Debug.Log("Neutral Infantry"); }
                    if (Input.GetKeyDown(KeyCode.A)) { builder = 26; currentPlacer = 0; Debug.Log("Neutral Tship"); }
                    if (Input.GetKeyDown(KeyCode.S)) { builder = 27; currentPlacer = 0; Debug.Log("Neutral Midtank"); }
                    if (Input.GetKeyDown(KeyCode.D)) { builder = 28; currentPlacer = 0; Debug.Log("Neutral Mech"); }
                    if (Input.GetKeyDown(KeyCode.F)) { builder = 29; currentPlacer = 0; Debug.Log("Neutral Heavytank"); }
                    if (Input.GetKeyDown(KeyCode.G)) { builder = 30; currentPlacer = 0; Debug.Log("Neutral Missile"); }
                    if (Input.GetKeyDown(KeyCode.H)) { builder = 31; currentPlacer = 0; Debug.Log("Neutral Recon"); }
                    if (Input.GetKeyDown(KeyCode.J)) { builder = 32; currentPlacer = 0; Debug.Log("Neutral Rocket"); }
                    if (Input.GetKeyDown(KeyCode.K)) { builder = 33; currentPlacer = 0; Debug.Log("Neutral Sub"); }
                    if (Input.GetKeyDown(KeyCode.L)) { builder = 34; currentPlacer = 0; Debug.Log("Neutral Theli"); }
                    if (Input.GetKeyDown(KeyCode.Z)) { builder = 35; currentPlacer = 0; Debug.Log("Neutral Tank"); }
                    break;
                case 6:
                    if (Input.GetKeyDown(KeyCode.Q)) { builder = 16; currentPlacer = 1; Debug.Log("Red AATank"); }
                    if (Input.GetKeyDown(KeyCode.W)) { builder = 17; currentPlacer = 1; Debug.Log("Red APC"); }
                    if (Input.GetKeyDown(KeyCode.E)) { builder = 18; currentPlacer = 1; Debug.Log("Red Artillery"); }
                    if (Input.GetKeyDown(KeyCode.R)) { builder = 19; currentPlacer = 1; Debug.Log("Red Heli"); }
                    if (Input.GetKeyDown(KeyCode.T)) { builder = 20; currentPlacer = 1; Debug.Log("Red Battleship"); }
                    if (Input.GetKeyDown(KeyCode.Y)) { builder = 21; currentPlacer = 1; Debug.Log("Red Bomber"); }
                    if (Input.GetKeyDown(KeyCode.U)) { builder = 22; currentPlacer = 1; Debug.Log("Red Carrier"); }
                    if (Input.GetKeyDown(KeyCode.I)) { builder = 23; currentPlacer = 1; Debug.Log("Red Cruiser"); }
                    if (Input.GetKeyDown(KeyCode.O)) { builder = 24; currentPlacer = 1; Debug.Log("Red Fighter"); }
                    if (Input.GetKeyDown(KeyCode.P)) { builder = 25; currentPlacer = 1; Debug.Log("Red Infantry"); }
                    if (Input.GetKeyDown(KeyCode.A)) { builder = 26; currentPlacer = 1; Debug.Log("Red Tship"); }
                    if (Input.GetKeyDown(KeyCode.S)) { builder = 27; currentPlacer = 1; Debug.Log("Red Midtank"); }
                    if (Input.GetKeyDown(KeyCode.D)) { builder = 28; currentPlacer = 1; Debug.Log("Red Mech"); }
                    if (Input.GetKeyDown(KeyCode.F)) { builder = 29; currentPlacer = 1; Debug.Log("Red Heavytank"); }
                    if (Input.GetKeyDown(KeyCode.G)) { builder = 30; currentPlacer = 1; Debug.Log("Red Missile"); }
                    if (Input.GetKeyDown(KeyCode.H)) { builder = 31; currentPlacer = 1; Debug.Log("Red Recon"); }
                    if (Input.GetKeyDown(KeyCode.J)) { builder = 32; currentPlacer = 1; Debug.Log("Red Rocket"); }
                    if (Input.GetKeyDown(KeyCode.K)) { builder = 33; currentPlacer = 1; Debug.Log("Red Sub"); }
                    if (Input.GetKeyDown(KeyCode.L)) { builder = 34; currentPlacer = 1; Debug.Log("Red Theli"); }
                    if (Input.GetKeyDown(KeyCode.Z)) { builder = 35; currentPlacer = 1; Debug.Log("Red Tank"); }
                    break;
                case 7:
                    if (Input.GetKeyDown(KeyCode.Q)) { builder = 16; currentPlacer = 2; Debug.Log("Blue AATank"); }
                    if (Input.GetKeyDown(KeyCode.W)) { builder = 17; currentPlacer = 2; Debug.Log("Blue APC"); }
                    if (Input.GetKeyDown(KeyCode.E)) { builder = 18; currentPlacer = 2; Debug.Log("Blue Artillery"); }
                    if (Input.GetKeyDown(KeyCode.R)) { builder = 19; currentPlacer = 2; Debug.Log("Blue Heli"); }
                    if (Input.GetKeyDown(KeyCode.T)) { builder = 20; currentPlacer = 2; Debug.Log("Blue Battleship"); }
                    if (Input.GetKeyDown(KeyCode.Y)) { builder = 21; currentPlacer = 2; Debug.Log("Blue Bomber"); }
                    if (Input.GetKeyDown(KeyCode.U)) { builder = 22; currentPlacer = 2; Debug.Log("Blue Carrier"); }
                    if (Input.GetKeyDown(KeyCode.I)) { builder = 23; currentPlacer = 2; Debug.Log("Blue Cruiser"); }
                    if (Input.GetKeyDown(KeyCode.O)) { builder = 24; currentPlacer = 2; Debug.Log("Blue Fighter"); }
                    if (Input.GetKeyDown(KeyCode.P)) { builder = 25; currentPlacer = 2; Debug.Log("Blue Infantry"); }
                    if (Input.GetKeyDown(KeyCode.A)) { builder = 26; currentPlacer = 2; Debug.Log("Blue Tship"); }
                    if (Input.GetKeyDown(KeyCode.S)) { builder = 27; currentPlacer = 2; Debug.Log("Blue Midtank"); }
                    if (Input.GetKeyDown(KeyCode.D)) { builder = 28; currentPlacer = 2; Debug.Log("Blue Mech"); }
                    if (Input.GetKeyDown(KeyCode.F)) { builder = 29; currentPlacer = 2; Debug.Log("Blue Heavytank"); }
                    if (Input.GetKeyDown(KeyCode.G)) { builder = 30; currentPlacer = 2; Debug.Log("Blue Missile"); }
                    if (Input.GetKeyDown(KeyCode.H)) { builder = 31; currentPlacer = 2; Debug.Log("Blue Recon"); }
                    if (Input.GetKeyDown(KeyCode.J)) { builder = 32; currentPlacer = 2; Debug.Log("Blue Rocket"); }
                    if (Input.GetKeyDown(KeyCode.K)) { builder = 33; currentPlacer = 2; Debug.Log("Blue Sub"); }
                    if (Input.GetKeyDown(KeyCode.L)) { builder = 34; currentPlacer = 2; Debug.Log("Blue Theli"); }
                    if (Input.GetKeyDown(KeyCode.Z)) { builder = 35; currentPlacer = 2; Debug.Log("Blue Tank"); }
                    break;
            }

            if (Input.GetMouseButton(1))
            {
                if (builder == -1) { builder = 0; Debug.Log("Plains"); }
                if (unitSetter == -1) { unitSetter = 0;}
                if (MouseClickDetector(ref x, ref z, ref tile))
                {
                    if (0 <= builder && builder <= 7)
                    {
                        tilemap.SetTilemapSprite(tile.position, builder, currentPlacer);
                        TileType ttm = tileTypes[builder];
                        GameObject tileInstance = Instantiate(ttm.tileVisualPrefab, tile.position, Quaternion.identity, map);
                        tileInstance.name = tilemap.GetGrid().GetGridObject(x, z).ToString() + x + z;
                        tilemap.GetGrid().GetGridObject(x, z).setTileVisual(tileInstance);
                        Destroy(tile.gameObject);
                    } 
                    else if (8 <= builder && builder <= 15)
                    {
                        tilemap.SetTilemapSprite(tile.position, builder, currentPlacer);
                        TileType tileObject = tilemap.GetGrid().GetGridObject(x, z);
                        TileType ttm = tileTypes[builder];
                        GameObject tileInstance = Instantiate(ttm.tileVisualPrefab, tile.position, Quaternion.identity, map);
                        tileInstance.name = tileObject.ToString() + x + z;
                        tileObject.setTileVisual(tileInstance);
                        ((Building)tileObject).Visualize(teamColours[((Building)tileObject).GetTeam()], tileInstance);
                        Destroy(tile.gameObject);
                    } 
                    else if (16 <= builder && builder <= 35)
                    {
                        if(unitmap.GetGrid().GetGridObject(tile.position) == null)
                        {
                            Unit un = unitTypes[builder-16];
                            unitmap.InsertUnit(x, z, (Unit.UnitType)(builder-13), currentPlacer, un.GetAmmo(), un.GetFuel(), null, 0);
                            UnitInstantiate(un, x, z, currentPlacer);
                        }
                        else
                        {
                            Destroy(GameObject.Find(unitTypes[builder-16].ToString()+x+z));
                            Unit un = unitTypes[builder-16];
                            unitmap.InsertUnit(x, z, (Unit.UnitType)(builder-16), currentPlacer, un.GetAmmo(), un.GetFuel(), null, 0);
                            UnitInstantiate(un, x, z, currentPlacer);
                        }
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                for (int z = 0; z < mapSizeZ; z++)
                {
                    for (int x = 0; x < mapSizeX; x++)
                    {
                        if(unitmap.GetGrid().GetGridObject(x, z) != null)
                        {
                            Destroy(GameObject.Find(unitmap.GetGrid().GetGridObject(x, z).ToString()+x+z));
                            unitmap.GetGrid().SetGridObject(x, z, null);
                        }
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                foreach (Transform child in map)
                {
                    GameObject.Destroy(child.gameObject);
                }
                GenerateMapVisual();
                Debug.Log("Loaded!");
            }

            if(Input.mousePosition.x < Screen.width / 4 && mainCamera.position.x > 14)
            {
                mainCamera.position = mainCamera.position + new Vector3(-0.04f, 0, 0);
            }
            if(Input.mousePosition.x > Screen.width - (Screen.width / 4) && mainCamera.position.x < mapSizeX * 2 - 16)
            {
                mainCamera.position = mainCamera.position + new Vector3(0.04f, 0, 0);
            }
            if(Input.mousePosition.y < Screen.height / 4 && mainCamera.position.z > -2)
            {
                mainCamera.position = mainCamera.position + new Vector3(0, 0, -0.04f);
            }
            if(Input.mousePosition.y > Screen.height - (Screen.height / 4) && mainCamera.position.z < mapSizeZ * 2 - 30)
            {
                mainCamera.position = mainCamera.position + new Vector3(0, 0, 0.04f);
            }
        } else if(debugMode == 4)
        {
            if (localPlayerID == turnCounter)
            {
                if (turnIndicator.activeSelf == false) turnIndicator.SetActive(true);
                if (Input.GetMouseButtonDown(0) && unitSelected == "move")
                {
                    if (MouseClickDetector(ref x, ref z, ref tile))
                    {
                        if(tile.gameObject.tag == "Selected" && localTurnSystem.GetUnitsAwaitingOrders().Contains(unitmap.GetGrid().GetGridObject(graph[0].x, graph[0].z)))
                        {
                            if((graph[0].x == x && graph[0].z == z) || unitmap.GetGrid().GetGridObject(x, z) == null)
                            {
                                unitmap.MoveUnit(graph[0].x, graph[0].z, x, z);
                                movedx = x;
                                movedz = z;
                                bool actionmove = true;
                                bool actionfire = false;
                                bool actioncapture = false;
                                bool actionsupply = false;
                                bool actionload = false;
                                bool[] unloadactions;
                                bool actionupgrade = false;
                                if(unitmap.GetGrid().GetGridObject(x, z).GetMaxRange() == 0 && unitmap.GetGrid().GetGridObject(x, z).GetCurrentAmmo() > 0)
                                {
                                    targetables.Add(unitmap.GetGrid().GetGridObject(x, z));
                                    targetables.AddRange(unitmap.GetEnemyUnitsInRange(x, z, tilemap.GetGrid().GetGridObject(x, z).GetType().Equals(typeof(Radio)), this));
                                    if(targetables.Count > 1) actionfire = true;
                                }
                                else if(graph[0].x == x && graph[0].z == z && unitmap.GetGrid().GetGridObject(x, z).GetMaxRange() > 0 && unitmap.GetGrid().GetGridObject(x, z).GetCurrentAmmo() > 0)
                                {
                                    targetables.Add(unitmap.GetGrid().GetGridObject(x, z));
                                    targetables.AddRange(unitmap.GetEnemyUnitsInRange(x, z, tilemap.GetGrid().GetGridObject(x, z).GetType().Equals(typeof(Radio)), this));
                                    if(targetables.Count > 1) actionfire = true;
                                }
                                if(tilemap.GetGrid().GetGridObject(x, z).GetType().IsSubclassOf(typeof(Building)) 
                                && ((Building)tilemap.GetGrid().GetGridObject(x, z)).GetTeam() != unitmap.GetGrid().GetGridObject(x, z).GetTeam()
                                && !CheckAlliance(((Building)tilemap.GetGrid().GetGridObject(x, z)).GetTeam(), unitmap.GetGrid().GetGridObject(x, z).GetTeam())
                                && unitmap.GetGrid().GetGridObject(x, z).GetMovementType() == 0)
                                {
                                    actioncapture = true;
                                }
                                if(unitmap.GetGrid().GetGridObject(x, z).GetUnitType() == Unit.UnitType.APC && unitmap.GetFriendlyUnitsInRange(x, z, unitmap.GetGrid().GetGridObject(x, z)).Count > 0)
                                {
                                    supplyTargetables.Add(unitmap.GetGrid().GetGridObject(x, z));
                                    supplyTargetables.AddRange(unitmap.GetFriendlyUnitsInRange(x, z, unitmap.GetGrid().GetGridObject(x, z)));
                                    if(supplyTargetables.Count > 1) actionsupply = true;
                                }
                                if(unitmap.GetFriendlyUnitsInRange(x, z, unitmap.GetGrid().GetGridObject(x, z)).Count > 0)
                                {
                                    foreach(Unit potentialTransport in unitmap.GetFriendlyUnitsInRange(x, z, unitmap.GetGrid().GetGridObject(x, z)))
                                    {
                                        if(potentialTransport.GetLoadCapacity() != 0 && potentialTransport.GetLoadedUnits()[potentialTransport.GetLoadCapacity()-1] == null)
                                        {
                                            if(GetTransportCompatibility(potentialTransport, x, z)) actionload = true;
                                        }
                                    }
                                }
                                if(unitmap.GetGrid().GetGridObject(x, z).GetLoadCapacity() != 0)
                                {
                                    unloadactions = new bool[unitmap.GetGrid().GetGridObject(x, z).GetLoadCapacity()];
                                    for(int i = 0; i < unitmap.GetGrid().GetGridObject(x, z).GetLoadCapacity(); i++)
                                    {
                                        if(unitmap.GetGrid().GetGridObject(x, z).GetLoadedUnits()[i] != null)
                                        {
                                            unloadactions[i] = true;
                                        }
                                    }
                                }
                                else
                                {
                                    unloadactions = new bool[0];
                                }
                                if(tilemap.GetGrid().GetGridObject(x, z).GetType() == typeof(Lab)
                                && ((Building)tilemap.GetGrid().GetGridObject(x, z)).GetTeam() == unitmap.GetGrid().GetGridObject(x, z).GetTeam()
                                && unitmap.GetGrid().GetGridObject(x, z).GetUpgradeCounter() < 3)
                                {
                                    actionupgrade = true;
                                }
                                unitSelected = "moved";

                                canvas.GetComponent<GameGUI>().SPShowActionInfo(actionmove, actionfire, actioncapture, actionsupply, actionload, unloadactions, actionupgrade, x, z);
                            }
                        }
                    }
                } 
                else if (Input.GetMouseButtonDown(0) && unitSelected == "fire")
                {
                    if (MouseClickDetector(ref x, ref z, ref tile))
                    {
                        if(tile.gameObject.tag == "Selected" && localTurnSystem.GetUnitsAwaitingOrders().Contains(unitmap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ())))
                        {
                            canvas.GetComponent<GameGUI>().HideAttackInfo();
                            bool isDead = (unitmap.AttackUnit(unitmap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ()), unitmap.GetGrid().GetGridObject(x, z), damageMatrix, tilemap.GetGrid().GetGridObject(x, z).GetDefence()));
                            unitSelected = "false";
                            int team = unitmap.GetGrid().GetGridObject(x, z).GetTeam()-1;
                            foreach(GameObject selected in selectedTiles)
                            {
                                Destroy(selected);
                            }
                            Unit attacker = unitmap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ());
                            attacker.SetIsActive(false);
                            attacker.VisualDeactivation();
                            attacker.AmmoCost(1);
                            localTurnSystem.MoveUnitAfterOrder(attacker);
                            if(isDead)
                            {
                                GameObject destroyable = GameObject.Find(unitmap.GetGrid().GetGridObject(x, z).ToString() + x + z);
                                Destroy(destroyable);
                                for(int i = 0; i < unitmap.GetGrid().GetGridObject(x, z).GetLoadCapacity(); i++)
                                {
                                    if(unitmap.GetGrid().GetGridObject(x, z).GetLoadedUnits()[i] != null)
                                    {
                                        playersInMatch[unitmap.GetGrid().GetGridObject(x, z).GetTeam()-1].RemoveLoadedUnit(unitmap.GetGrid().GetGridObject(x, z).GetLoadedUnits()[i]);
                                    }
                                }
                                playersInMatch[unitmap.GetGrid().GetGridObject(x, z).GetTeam()-1].RemoveUnit(unitmap.GetGrid().GetGridObject(x, z));
                                unitmap.GetGrid().SetGridObject(x, z, null);
                                if(playersInMatch[team].GetUnloadedUnits().Count == 0)
                                {
                                    GameLost(team);
                                }
                            }
                            else if(Mathf.Abs((float)(x - targetables[0].GetX())) + Mathf.Abs((float)(z - targetables[0].GetZ())) == 1 && unitmap.GetGrid().GetGridObject(x, z).GetMaxRange() == 0)
                            {
                                bool isDeadAttacker = (unitmap.AttackUnit(unitmap.GetGrid().GetGridObject(x, z), unitmap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ()), damageMatrix, tilemap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ()).GetDefence()));
                                Unit defender = unitmap.GetGrid().GetGridObject(x, z);
                                team = unitmap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ()).GetTeam()-1;
                                defender.AmmoCost(1);
                                if(isDeadAttacker)
                                {
                                    GameObject destroyableAttacker = GameObject.Find(unitmap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ()).ToString() + targetables[0].GetX() + targetables[0].GetZ());
                                    Destroy(destroyableAttacker);
                                    for(int i = 0; i < unitmap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ()).GetLoadCapacity(); i++)
                                    {
                                        if(unitmap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ()).GetLoadedUnits()[i] != null)
                                        {
                                            playersInMatch[unitmap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ()).GetTeam()-1].RemoveLoadedUnit(unitmap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ()).GetLoadedUnits()[i]);
                                        }
                                    }
                                    playersInMatch[unitmap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ()).GetTeam()-1].RemoveUnit(unitmap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ()));
                                    unitmap.GetGrid().SetGridObject(targetables[0].GetX(), targetables[0].GetZ(), null);
                                    if(playersInMatch[team].GetUnloadedUnits().Count == 0)
                                    {
                                        GameLost(team);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (Input.GetMouseButtonDown(0) && unitSelected == "load")
                {
                    if (MouseClickDetector(ref x, ref z, ref tile))
                    {
                        if(tile.gameObject.tag == "Selected")
                        {
                            unitSelected = "false";
                            foreach(GameObject selected in selectedTiles)
                            {
                                Destroy(selected);
                            }
                            Unit loaded = unitmap.GetGrid().GetGridObject(movementx, movementz);
                            loaded.SetIsActive(false);
                            localTurnSystem.unitsAwaitingOrders.Remove(loaded);
                            for(int i = 0; i < unitmap.GetGrid().GetGridObject(x, z).GetLoadCapacity(); i++)
                            {
                                if(unitmap.GetGrid().GetGridObject(x, z).GetLoadedUnits()[i] == null)
                                {
                                    unitmap.GetGrid().GetGridObject(x, z).LoadUnit(i, loaded);
                                    playersInMatch[loaded.GetTeam()-1].RemoveUnit(loaded);
                                    playersInMatch[loaded.GetTeam()-1].AddLoadedUnit(loaded);
                                    GameObject destroyable = GameObject.Find(unitmap.GetGrid().GetGridObject(movementx, movementz).ToString() + movementx + movementz);
                                    Destroy(destroyable);
                                    unitmap.GetGrid().SetGridObject(movementx, movementz, null);
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (Input.GetMouseButtonDown(0) && unitSelected == "unload")
                {
                    if (MouseClickDetector(ref x, ref z, ref tile))
                    {
                        if(tile.gameObject.tag == "Selected")
                        {
                            unitSelected = "false";
                            foreach(GameObject selected in selectedTiles)
                            {
                                Destroy(selected);
                            }
                            Unit unloader = unitmap.GetGrid().GetGridObject(movementx, movementz);
                            Unit unloaded = unloader.GetLoadedUnits()[unloadindex];
                            playersInMatch[unloaded.GetTeam()-1].RemoveLoadedUnit(unloaded);
                            unitmap.InsertUnit(x, z, unloaded.GetUnitType(), unloaded.GetTeam(), unloaded.GetCurrentAmmo(), unloaded.GetCurrentFuel(), unloaded.GetLoadedUnits(), unloader.GetUpgradeCounter());
                            unloader.LoadUnit(unloadindex, null);
                            unloaded = unitmap.GetGrid().GetGridObject(x, z);
                            UnitInstantiate(unloaded, x, z, unloaded.GetTeam());
                            UnitArrayShiftLeft(unloader.GetLoadedUnits(), unloadindex+1);
                            localTurnSystem.unitsOrdersReceived.Add(unloaded);
                            unloader.SetIsActive(false);
                            localTurnSystem.MoveUnitAfterOrder(unloader);
                            playersInMatch[unloaded.GetTeam()-1].AddUnit(unloaded);
                            unloaded.VisualDeactivation();
                            unloader.VisualDeactivation();
                        }
                    }
                }
                else if (Input.GetMouseButtonDown(0) && menuUp == false && unitSelected == "false")
                {
                    pathmaking.ClearGrid();
                    targetables.Clear();
                    if (MouseClickDetector(ref x, ref z))
                    {
                        TileType tileChecker = tilemap.GetGrid().GetGridObject(x, z);
                        if(localTurnSystem.GetUnitsAwaitingOrders().Contains(unitmap.GetGrid().GetGridObject(x, z)))
                        {
                            graph = pathmaking.CreateReachableGraph(x, z, unitmap.GetGrid().GetGridObject(x, z), tilemap, unitmap, false, false);
                            if (graph != null)
                            {
                                for (int i = 0; i < graph.Count; i++)
                                {
                                    //Debug.Log(graph[i].nodePosition() + " = " + graph[i].moveCost);
                                    selectedTiles.Add(Instantiate(selectedTile, new Vector3(graph[i].x * 2, 0.1f, graph[i].z * 2), Quaternion.identity, map));
                                }
                                unitSelected = "move";
                            }
                        }
                        else if(tileChecker.GetType().Equals(typeof(MilitaryBase)) && unitmap.GetGrid().GetGridObject(x, z) == null && ((MilitaryBase)tileChecker).GetTeam() == playersInMatch[localPlayerID].GetTeam())
                        {
                            MilitaryBase helper = (MilitaryBase)tileChecker;
                            menuUp = true;
                            PrintListUnits(militaryBaseRecruits, x, z, helper.GetTeam());
                        }
                        else if(tileChecker.GetType().Equals(typeof(Airport)) && unitmap.GetGrid().GetGridObject(x, z) == null && ((Airport)tileChecker).GetTeam() == playersInMatch[localPlayerID].GetTeam())
                        {
                            Airport helper = (Airport)tileChecker;
                            menuUp = true;
                            PrintListUnits(airportRecruits, x, z, helper.GetTeam());
                        }
                        else if(tileChecker.GetType().Equals(typeof(Port)) && unitmap.GetGrid().GetGridObject(x, z) == null && ((Port)tileChecker).GetTeam() == playersInMatch[localPlayerID].GetTeam())
                        {
                            Port helper = (Port)tileChecker;
                            menuUp = true;
                            PrintListUnits(portRecruits, x, z, helper.GetTeam());
                        }
                        else if(tileChecker.GetType().Equals(typeof(City)) && unitmap.GetGrid().GetGridObject(x, z) == null && ((City)tileChecker).GetTeam() == playersInMatch[localPlayerID].GetTeam())
                        {
                            City helper = (City)tileChecker;
                            menuUp = true;
                            PrintListCityUpgrades(cityUpgrades, x, z, tile, helper.GetTeam());
                        }
                        else
                        {
                            canvas.GetComponent<GameGUI>().quickmenu.transform.Find("QuickMenuEndTurn").GetComponent<Button>().interactable = true;
                            canvas.GetComponent<GameGUI>().quickmenu.SetActive(true);
                            menuUp = true;
                        }
                    }
                }
                else if (Input.GetMouseButtonDown(1) && menuUp == false)
                {
                    if (MouseClickDetector(ref x, ref z, ref tile))
                    {
                        if (unitSelected == "false")
                        {
                            if(unitmap.GetGrid().GetGridObject(x, z) != null)
                            {
                                canvas.GetComponent<GameGUI>().ShowExtendedInfo(tilemap.GetGrid().GetGridObject(x, z), tile.gameObject, unitmap.GetGrid().GetGridObject(x, z), unitmap.GetGrid().GetGridObject(x, z).GetUnitInstance());
                            }
                            else
                            {
                                canvas.GetComponent<GameGUI>().ShowExtendedInfo(tilemap.GetGrid().GetGridObject(x, z), tile.gameObject, null, null);
                            }
                            menuUp = true;
                        }
                        else if (unitSelected == "move")
                        {
                            unitSelected = "false";
                            foreach(GameObject selected in selectedTiles)
                            {
                                Destroy(selected);
                            }
                        }
                        else if (unitSelected == "moved")
                        {
                            unitSelected = "move";
                            targetables.Clear();
                            unitmap.MoveUnit(movedx, movedz, graph[0].x, graph[0].z);
                            canvas.GetComponent<GameGUI>().HideActionInfo();
                        }
                        else if (unitSelected == "fire" || unitSelected == "load" || unitSelected == "unload")
                        {
                            unitSelected = "move";
                            targetables.Clear();
                            GameObject movableUnit = GameObject.Find(unitmap.GetGrid().GetGridObject(movementx, movementz).ToString() + movementx + movementz);
                            if (movableUnit)
                            {
                                movableUnit.transform.position = new Vector3(graph[0].x * 2, 0.7f, graph[0].z * 2);
                                movableUnit.name = unitmap.GetGrid().GetGridObject(movementx, movementz).ToString() + graph[0].x + graph[0].z;
                                unitmap.GetGrid().GetGridObject(movementx, movementz).FuelCost(-(Mathf.Abs(movementx - graph[0].x) + Mathf.Abs(movementz - graph[0].z)));
                            }
                            unitmap.MoveUnit(movementx, movementz, graph[0].x, graph[0].z);
                            foreach (GameObject selected in selectedTiles)
                            {
                                Destroy(selected);
                            }
                            for (int i = 0; i < graph.Count; i++)
                            {
                                selectedTiles.Add(Instantiate(selectedTile, new Vector3(graph[i].x * 2, 0.1f, graph[i].z * 2), Quaternion.identity, map));
                            }
                            canvas.GetComponent<GameGUI>().HideAttackInfo();
                        }
                    }
                }
            }
            else
            {
                if (turnIndicator.activeSelf == true) turnIndicator.SetActive(false);
                if(Input.GetMouseButtonDown(0) && menuUp == false && unitSelected == "false")
                {
                    canvas.GetComponent<GameGUI>().quickmenu.transform.Find("QuickMenuEndTurn").GetComponent<Button>().interactable = false;
                    canvas.GetComponent<GameGUI>().quickmenu.SetActive(true);
                    menuUp = true;
                }
            }

            if((lastSelected == null || lastSelected != tile) && menuUp == false && MouseClickDetector(ref x, ref z, ref tile))
            {
                if(Input.mousePosition.x < Screen.width / 4 && currentguiside == "left")
                {
                    canvas.GetComponent<GameGUI>().ChangeDirection("right");
                    currentguiside = "right";
                }
                else if(Input.mousePosition.x > Screen.width - (Screen.width / 4) && currentguiside == "right")
                {
                    canvas.GetComponent<GameGUI>().ChangeDirection("left");
                    currentguiside = "left";
                }
                canvas.GetComponent<GameGUI>().ShowPlayerInfo(currentguiside, teamColours, playersInMatch[localPlayerID], turnsPassed);
                TileType tileObject = tilemap.GetGrid().GetGridObject(x, z);
                canvas.GetComponent<GameGUI>().ShowTileInfo(currentguiside, tileObject.GetDefence(), tileObject.GetTileVisual());
                if(unitmap.GetGrid().GetGridObject(x, z) != null)
                {
                    Unit unit = unitmap.GetGrid().GetGridObject(x, z);
                    canvas.GetComponent<GameGUI>().ShowUnitInfo(currentguiside, unit.GetHealth(), unit.GetUnitInstance());
                }
                else
                {
                    canvas.GetComponent<GameGUI>().HideUnitInfo();
                }
            }

            if(Input.GetKeyDown(KeyCode.Z) && unitSelected == "move" && MouseClickDetector(ref x, ref z, ref tile))
            {
                Debug.Log("Attempting to find path from " + graph[0].x + "," + graph[0].z + " to " + x + "," + z);
                List<PathNode> path = pathfinding.FindPath(graph[0].x, graph[0].z, x, z, unitmap.GetGrid().GetGridObject(graph[0].x, graph[0].z), tilemap, unitmap);
                if(path != null)
                {
                    Debug.Log("Path found!");
                    for(int i = 0; i < path.Count - 1; i++)
                    {
                        Debug.Log(path[i].nodePosition());
                        Debug.DrawLine(new Vector3(path[i].x * 2, 0.7f ,path[i].z * 2), new Vector3(path[i+1].x * 2, 0.7f, path[i+1].z * 2), Color.magenta, 2.0f);
                    }
                }
            }

            if(unitSelected == "fire" && MouseClickDetector(ref x, ref z, ref tile) && menuUp == false)
            {
                if(tile.gameObject.tag == "Selected" && unitmap.GetGrid().GetGridObject(x, z) != null && targetables.Contains(unitmap.GetGrid().GetGridObject(x, z)) && unitmap.GetGrid().GetGridObject(x, z) != targetables[0])
                {
                    unitmap.SimulateAttack(targetables[0], unitmap.GetGrid().GetGridObject(x, z), damageMatrix, tilemap.GetGrid().GetGridObject(x, z).GetDefence(), tilemap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ()).GetDefence(), out int attack, out int counterattack);
                    canvas.GetComponent<GameGUI>().ShowAttackInfo(Input.mousePosition, targetables[0].GetUnitInstance(), unitmap.GetGrid().GetGridObject(x, z).GetUnitInstance(), attack, counterattack);
                }
                else
                {
                    canvas.GetComponent<GameGUI>().HideAttackInfo();
                }
            }

            if(menuUp == false)
            {
                if(Input.mousePosition.x < Screen.width / 4 && mainCamera.position.x > 14)
                {
                    mainCamera.position = mainCamera.position + new Vector3(-0.2f, 0, 0);
                }
                if(Input.mousePosition.x > Screen.width - (Screen.width / 4) && mainCamera.position.x < mapSizeX * 2 - 16)
                {
                    mainCamera.position = mainCamera.position + new Vector3(0.2f, 0, 0);
                }
                if(Input.mousePosition.y < Screen.height / 4 && mainCamera.position.z > -2)
                {
                    mainCamera.position = mainCamera.position + new Vector3(0, 0, -0.2f);
                }
                if(Input.mousePosition.y > Screen.height - (Screen.height / 4) && mainCamera.position.z < mapSizeZ * 2 - 30)
                {
                    mainCamera.position = mainCamera.position + new Vector3(0, 0, 0.2f);
                }
            }
        }
    }
}
