using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GameManager : MonoBehaviourPun
{
    public TilemapObject[] tileTypes;
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
    public GameObject list;
    public GameObject listContent;
    public GameObject mapButtonPrefab;
    public GameObject visibleGridPrefab;
    public int[,] damageMatrix;
    int mapSizeX;
    int mapSizeZ;
    int x;
    int z;
    int movementx;
    int movementz;
    int unloadindex;
    Transform tile;
    [SerializeField] Transform mainCamera;

    private GameGrid<int> grid;
    private Transform lastSelected = null;
    private string currentguiside = "left";
    private int builder = -1;
    private int unitSetter = -1;
    private int currentPlacer = 0;
    private int turnCounter = 0;
    private int turnsPassed = 1;
    private int creatorSelector = 0;
    private string unitSelected = "false";
    private bool menuUp = false;
    private PathFinding pathfinding;
    private PathMaking pathmaking;
    private Tilemap tilemap;
    private Unitmap unitmap;
    List<DijkstraNode> graph = new List<DijkstraNode>();
    List<GameObject> selectedTiles = new List<GameObject>();
    List<Unit> targetables = new List<Unit>();
    List<Unit> supplyTargetables = new List<Unit>();
    public List<Player> playersInMatch = new List<Player>();

    public int debugMode = 2;
    private int localPlayerID;

    public void PrintListUnits(List<Unit> array, int x, int z, int team)
    {
        canvas.GetComponent<GameGUI>().ShowRecruitInfo(array, x, z, team);
    }

    public void PrintListCityUpgrades(List<City> array, int x, int z, Transform tile, int team)
    {
        canvas.GetComponent<GameGUI>().ShowBuildingUpgrades(array, x, z, tile, team);
    }

    public void AddRecruitListeners(GameObject button, Unit unit, int x, int z, int team)
    {
        button.GetComponent<Button>().onClick.AddListener(() => photonView.RPC("RecruitUnit", RpcTarget.All, unit.GetUnitType(), x, z, team));
        button.GetComponent<Button>().onClick.AddListener(() => CloseMenu());
        button.GetComponent<Button>().onClick.AddListener(() => canvas.GetComponent<GameGUI>().HideRecruitInfo());
        RecruitButtonHover component = button.AddComponent<RecruitButtonHover>();
        component.canvas = canvas;
        component.unit = unit;
        if(playersInMatch[localPlayerID].GetFunds() < unit.GetCost()) button.GetComponent<Button>().interactable = false;
    }

    public void AddBuildingListeners(GameObject button, City cityupgrade, int x, int z, Transform tile, int team)
    {
        button.GetComponent<Button>().onClick.AddListener(() => photonView.RPC("UpgradeCity", RpcTarget.All, cityupgrade.GetTilemapSprite(), x, z, tile.position, team));
        button.GetComponent<Button>().onClick.AddListener(() => photonView.RPC("RPCDestroy", RpcTarget.All, tile.gameObject.name));
        button.GetComponent<Button>().onClick.AddListener(() => CloseMenu());
        button.GetComponent<Button>().onClick.AddListener(() => canvas.GetComponent<GameGUI>().HideBuildingUpgrades());
        BuildingButtonHover component = button.AddComponent<BuildingButtonHover>();
        component.canvas = canvas;
        component.cityupgrade = cityupgrade;
        if(playersInMatch[localPlayerID].GetFunds() < ((City)cityupgrade).GetCost()) button.GetComponent<Button>().interactable = false;
    }

    [PunRPC]
    public void RPCDestroy(string objectname)
    {
        Destroy(GameObject.Find(objectname));
    }

    [PunRPC]
    public void RecruitUnit(Unit.UnitType type, int x, int z, int team)
    {
        Unit un = unitTypes[(int)type];
        un.SetUnitType(un.GetUnitType(), team);
        unitmap.InsertUnit(x, z, type, team, un.GetAmmo(), un.GetFuel(), null, 0);
        UnitInstantiate(un, x, z, team);
        playersInMatch[turnCounter].AddUnit(unitmap.GetGrid().GetGridObject(x, z));
        playersInMatch[turnCounter].ChangeFunds(-(unitmap.GetGrid().GetGridObject(x, z).GetCost()));
    }

    [PunRPC]
    public void UpgradeCity(TilemapObject.TilemapSprite upgrade, int x, int z, Vector3 pos, int team)
    {
        playersInMatch[turnCounter].RemoveBuilding((Building)tilemap.GetGrid().GetGridObject(x, z));
        tilemap.SetTilemapSprite(pos, upgrade, team);
        TilemapObject tileObject = tilemap.GetGrid().GetGridObject(x, z);
        TilemapObject ttm = tileTypes[(int)upgrade];
        GameObject tileInstance = Instantiate(ttm.tileVisualPrefab, pos, Quaternion.identity, map);
        tileInstance.name = tileObject.ToString() + x + z;
        tileObject.setTileVisual(tileInstance);
        ((Building)tileObject).Visualize(teamColours[((Building)tileObject).GetTeam()], tileInstance);
        playersInMatch[turnCounter].AddBuilding((Building)tilemap.GetGrid().GetGridObject(x, z));
        playersInMatch[turnCounter].ChangeFunds(-(((City)tileObject).GetCost()));
    }

    public void InitPlayers()
    {
        playersInMatch.Add(new Player(1, tilemap, unitmap));
        playersInMatch.Add(new Player(2, tilemap, unitmap));
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

    private void UnitInstantiate(Unit un, int x, int z, int colorID)
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
        localPlayerID = (int)PhotonNetwork.LocalPlayer.CustomProperties["Index"];
        mapSizeX = (int)PhotonNetwork.CurrentRoom.CustomProperties["Width"];
        mapSizeZ = (int)PhotonNetwork.CurrentRoom.CustomProperties["Height"];
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
            GenerateMapVisual();
        }
        InitPlayers();
        canvas.GetComponent<GameGUI>().quickmenu.transform.Find("QuickMenuOverView").GetComponent<Button>().onClick.AddListener(() => canvas.GetComponent<GameGUI>().ShowMatchOverview(teamColours));
        canvas.GetComponent<GameGUI>().yieldconfirmation.transform.Find("ConfirmButton").GetComponent<Button>().onClick.AddListener(() => photonView.RPC("GameLost", PhotonNetwork.LocalPlayer, localPlayerID));
    }

    void GenerateMapVisual()
    {
        if (debugMode == 3 || debugMode == 4)
        {
            tilemap.Load((string)PhotonNetwork.CurrentRoom.CustomProperties["MapName"] + "_Tiles");
            unitmap.Load((string)PhotonNetwork.CurrentRoom.CustomProperties["MapName"] + "_Units");
            for (int z = 0; z < mapSizeZ; z++)
            {
                for (int x = 0; x < mapSizeX; x++)
                {
                    if((bool)PhotonNetwork.LocalPlayer.CustomProperties["ShowGrid"])
                    {
                        Instantiate(visibleGridPrefab, new Vector3(x * 2 - 1, 0, z * 2 - 1), Quaternion.identity, map);
                    }
                    TilemapObject tileObject = tilemap.GetGrid().GetGridObject(x, z);
                    TilemapObject tt = tileTypes[tilemap.GetIntFromSprite(x, z)];
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
        photonView.RPC("SynchronizeMove", RpcTarget.Others, graph[0].x, graph[0].z, x, z);
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
        photonView.RPC("SynchronizeMove", RpcTarget.Others, graph[0].x, graph[0].z, x, z);

        Transform localtile = GameObject.Find(tilemap.GetGrid().GetGridObject(x, z).ToString() + x + z).transform;
        TilemapObject tileChecker = tilemap.GetGrid().GetGridObject(x, z);
        Unit unitChecker = unitmap.GetGrid().GetGridObject(x, z);
        int possibleloser = -1;
        string oldtile = tileChecker.ToString() + x + z;
        if(unitChecker != null && !(tileChecker.GetType().Equals(typeof(TilemapObject))) && unitChecker.GetTeam() != ((Building)tileChecker).GetTeam())
        {
            Building helper = (Building)tileChecker;
            helper.SetHealth(helper.GetHealth() - unitChecker.GetHealth());
            if(helper.GetHealth() <= 0)
            {
                if(helper.GetTeam() != 0) playersInMatch[helper.GetTeam()-1].RemoveBuilding(helper);
                if(tileChecker.GetType().Equals(typeof(HQ)))
                {
                    helper.SetTilemapSprite(TilemapObject.TilemapSprite.City);
                    possibleloser = helper.GetTeam()-1;
                }
                helper.SetTeam(unitChecker.GetTeam());
                TilemapObject ttm = tileTypes[tilemap.GetIntFromSprite(x, z)];
                GameObject tileInstance = Instantiate(ttm.tileVisualPrefab, localtile.position, Quaternion.identity, map);
                tileInstance.name = tileChecker.ToString() + x + z;
                tileChecker.setTileVisual(tileInstance);
                playersInMatch[turnCounter].AddBuilding(helper);
                helper.Visualize(teamColours[unitChecker.GetTeam()], tileInstance);
                Destroy(localtile.gameObject);
            }
            photonView.RPC("SynchronizeCapture", RpcTarget.Others, x, z, localtile.position, oldtile);
            if(possibleloser != -1)
            {
                photonView.RPC("GameLost", PhotonNetwork.PlayerList[possibleloser], PhotonNetwork.PlayerList[possibleloser].CustomProperties["Index"]);
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
        photonView.RPC("SynchronizeMove", RpcTarget.Others, graph[0].x, graph[0].z, x, z);
        photonView.RPC("SynchronizeSupply", RpcTarget.All, x, z);
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
        foreach(Unit potentialTransport in unitmap.GetFriendlyUnitsInRange(x, z))
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
        foreach(TilemapObject tile in tilemap.GetNeighbouringTiles(x, z, unitmap.GetGrid().GetGridObject(x, z).GetLoadedUnits()[index], unitmap))
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
        photonView.RPC("SynchronizeMove", RpcTarget.Others, graph[0].x, graph[0].z, x, z);
        photonView.RPC("SynchronizeUpgrade", RpcTarget.All, x, z);
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
        photonView.RPC("SynchronizeTurn", RpcTarget.All);
    }

    [PunRPC]
    public void GameLost(int playerID)
    {
        photonView.RPC("RemovePlayerFromGame", RpcTarget.All, playerID);
        photonView.RPC("CheckVictoryConditions", RpcTarget.Others);
        canvas.GetComponent<GameGUI>().ShowGameEndDialog(false);
        menuUp = true;
    }

    [PunRPC]
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
            if(destroyBuilding.GetTilemapSprite() == TilemapObject.TilemapSprite.HQ || destroyBuilding.GetType().IsSubclassOf(typeof(City)))
            {
                destroyBuilding.SetTilemapSprite(TilemapObject.TilemapSprite.City);
            }
            TilemapObject ttm = tileTypes[tilemap.GetIntFromSprite(destroyBuilding.GetX(), destroyBuilding.GetZ())];
            GameObject tileInstance = Instantiate(ttm.tileVisualPrefab, tilemap.GetGrid().GetWorldPosition(destroyBuilding.GetX(), destroyBuilding.GetZ()), Quaternion.identity, map);
            tileInstance.name = destroyBuilding.ToString() + destroyBuilding.GetX() + destroyBuilding.GetZ();
            tilemap.GetGrid().GetGridObject(destroyBuilding.GetX(), destroyBuilding.GetZ()).setTileVisual(tileInstance);
            destroyBuilding.Visualize(teamColours[0], tileInstance);
        }
        playersInMatch.RemoveAt(PlayerID);
    }

    [PunRPC]
    public void CheckVictoryConditions()
    {
        bool victory = true;
        for (int z = 0; z < mapSizeZ; z++)
        {
            for (int x = 0; x < mapSizeX; x++)
            {
                if(tilemap.GetGrid().GetGridObject(x, z).GetTilemapSprite() == TilemapObject.TilemapSprite.HQ && ((Building)tilemap.GetGrid().GetGridObject(x, z)).GetTeam() != localPlayerID + 1)
                {
                    victory = false;
                }
            }
        }
        if(victory)
        {
            canvas.GetComponent<GameGUI>().ShowGameEndDialog(victory);
            menuUp = true;
        }
    }

    [PunRPC]
    private void SynchronizeMove(int startx, int startz, int endx, int endz)
    {
        unitmap.MoveUnit(startx, startz, endx, endz);
        GameObject movableUnit = GameObject.Find(unitmap.GetGrid().GetGridObject(endx, endz).ToString() + startx + startz);
        if(movableUnit)
        {
            movableUnit.transform.position = new Vector3(endx * 2, 0.7f, endz * 2);
            movableUnit.name = unitmap.GetGrid().GetGridObject(endx, endz).ToString() + endx + endz;
            unitmap.GetGrid().GetGridObject(endx, endz).FuelCost(Mathf.Abs(startx - endx) + Mathf.Abs(startz - endz));
        }
    }

    [PunRPC]
    private void SynchronizeFire(int attackerx, int attackerz, int defenderx, int defenderz)
    {
        bool isDead = (unitmap.AttackUnit(unitmap.GetGrid().GetGridObject(attackerx, attackerz), unitmap.GetGrid().GetGridObject(defenderx, defenderz), damageMatrix, tilemap.GetGrid().GetGridObject(defenderx, defenderz).GetDefence()));
        unitmap.GetGrid().GetGridObject(attackerx, attackerz).AmmoCost(1);
        if(isDead)
        {
            GameObject destroyable = GameObject.Find(unitmap.GetGrid().GetGridObject(defenderx, defenderz).ToString() + defenderx + defenderz);
            Destroy(destroyable);
            for(int i = 0; i < unitmap.GetGrid().GetGridObject(defenderx, defenderz).GetLoadCapacity(); i++)
            {
                if(unitmap.GetGrid().GetGridObject(defenderx, defenderz).GetLoadedUnits()[i] != null)
                {
                    playersInMatch[unitmap.GetGrid().GetGridObject(defenderx, defenderz).GetTeam()-1].RemoveLoadedUnit(unitmap.GetGrid().GetGridObject(defenderx, defenderz).GetLoadedUnits()[i]);
                }
            }
            playersInMatch[unitmap.GetGrid().GetGridObject(defenderx, defenderz).GetTeam()-1].RemoveUnit(unitmap.GetGrid().GetGridObject(defenderx, defenderz));
            unitmap.GetGrid().SetGridObject(defenderx, defenderz, null);
        }
    }

    [PunRPC]
    private void SynchronizeCapture(int x, int z, Vector3 pos, string tilename)
    {
        TilemapObject tileChecker = tilemap.GetGrid().GetGridObject(x, z);
        Unit unitChecker = unitmap.GetGrid().GetGridObject(x, z);
        Building helper = (Building)tileChecker;
        helper.SetHealth(helper.GetHealth() - unitChecker.GetHealth());
        if(helper.GetHealth() <= 0)
        {
            if(helper.GetTeam() != 0) playersInMatch[helper.GetTeam()-1].RemoveBuilding(helper);
            if(tileChecker.GetType().Equals(typeof(HQ)))
            {
                helper.SetTilemapSprite(TilemapObject.TilemapSprite.City);
            }
            helper.SetTeam(unitChecker.GetTeam());
            TilemapObject ttm = tileTypes[tilemap.GetIntFromSprite(x, z)];
            GameObject tileInstance = Instantiate(ttm.tileVisualPrefab, pos, Quaternion.identity, map);
            tileInstance.name = tileChecker.ToString() + x + z;
            tileChecker.setTileVisual(tileInstance);
            playersInMatch[turnCounter].AddBuilding(helper);
            helper.Visualize(teamColours[unitChecker.GetTeam()], tileInstance);
            RPCDestroy(tilename);
        }
    }

    [PunRPC]
    private void SynchronizeSupply(int x, int z)
    {
        List<Unit> supplytargets = new List<Unit>(unitmap.GetFriendlyUnitsInRange(x, z));
        foreach(Unit target in supplytargets)
        {
            target.Refuel();
            target.Reammo();
        }
    }

    [PunRPC]
    private void SynchronizeLoad(int loadedx, int loadedz, int loaderx, int loaderz, int index)
    {
        GameObject loadedunit = GameObject.Find(unitmap.GetGrid().GetGridObject(loadedx, loadedz).ToString() + loadedx + loadedz);
        Destroy(loadedunit);
        playersInMatch[unitmap.GetGrid().GetGridObject(loadedx, loadedz).GetTeam()-1].RemoveUnit(unitmap.GetGrid().GetGridObject(loadedx, loadedz));
        unitmap.GetGrid().GetGridObject(loaderx, loaderz).LoadUnit(index, unitmap.GetGrid().GetGridObject(loadedx, loadedz));
        playersInMatch[unitmap.GetGrid().GetGridObject(loadedx, loadedz).GetTeam()-1].AddLoadedUnit(unitmap.GetGrid().GetGridObject(loaderx, loaderz).GetLoadedUnits()[index]);
        unitmap.GetGrid().SetGridObject(loadedx, loadedz, null);
    }

    [PunRPC]
    private void SynchronizeUnload(int unloaderx, int unloaderz, int unloadedx, int unloadedz, int index)
    {
        Unit unloader = unitmap.GetGrid().GetGridObject(unloaderx, unloaderz);
        Unit unloaded = unloader.GetLoadedUnits()[index];
        unitmap.InsertUnit(unloadedx, unloadedz, unloaded.GetUnitType(), unloaded.GetTeam(), unloaded.GetCurrentAmmo(), unloaded.GetCurrentFuel(), unloaded.GetLoadedUnits(), unloaded.GetUpgradeCounter());
        UnitInstantiate(unloaded, unloadedx, unloadedz, unloaded.GetTeam());
        playersInMatch[unitmap.GetGrid().GetGridObject(unloadedx, unloadedz).GetTeam()-1].RemoveLoadedUnit(unitmap.GetGrid().GetGridObject(unloaderx, unloaderz).GetLoadedUnits()[index]);
        unloader.LoadUnit(index, null);
        playersInMatch[unitmap.GetGrid().GetGridObject(unloadedx, unloadedz).GetTeam()-1].AddUnit(unitmap.GetGrid().GetGridObject(unloadedx, unloadedz));
        UnitArrayShiftLeft(unloader.GetLoadedUnits(), index+1);
    }

    [PunRPC]
    private void SynchronizeUpgrade(int x, int z)
    {
        unitmap.GetGrid().GetGridObject(x, z).Upgrade();
    }

    [PunRPC]
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
                        TilemapObject ttm = tileTypes[builder];
                        GameObject tileInstance = Instantiate(ttm.tileVisualPrefab, tile.position, Quaternion.identity, map);
                        tileInstance.name = tilemap.GetGrid().GetGridObject(x, z).ToString() + x + z;
                        tilemap.GetGrid().GetGridObject(x, z).setTileVisual(tileInstance);
                        Destroy(tile.gameObject);
                    } 
                    else if (8 <= builder && builder <= 15)
                    {
                        tilemap.SetTilemapSprite(tile.position, builder, currentPlacer);
                        TilemapObject tileObject = tilemap.GetGrid().GetGridObject(x, z);
                        TilemapObject ttm = tileTypes[builder];
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

            if (Input.GetKeyDown(KeyCode.B))
            {
                tilemap.Save((string)PhotonNetwork.CurrentRoom.CustomProperties["MapName"] + "_Tiles");
                unitmap.Save((string)PhotonNetwork.CurrentRoom.CustomProperties["MapName"] + "_Units");
                Debug.Log("Saved!");
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
                if (Input.GetMouseButtonDown(0) && unitSelected == "move")
                {
                    if (MouseClickDetector(ref x, ref z, ref tile))
                    {
                        if(tile.gameObject.tag == "Selected" && localTurnSystem.GetUnitsAwaitingOrders().Contains(unitmap.GetGrid().GetGridObject(graph[0].x, graph[0].z)))
                        {
                            if((graph[0].x == x && graph[0].z == z) || unitmap.GetGrid().GetGridObject(x, z) == null)
                            {
                                unitmap.MoveUnit(graph[0].x, graph[0].z, x, z);
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
                                    targetables.AddRange(unitmap.GetEnemyUnitsInRange(x, z));
                                    if(targetables.Count > 1) actionfire = true;
                                }
                                else if(graph[0].x == x && graph[0].z == z && unitmap.GetGrid().GetGridObject(x, z).GetMaxRange() > 0)
                                {
                                    targetables.Add(unitmap.GetGrid().GetGridObject(x, z));
                                    targetables.AddRange(unitmap.GetEnemyUnitsInRange(x, z));
                                    if(targetables.Count > 1) actionfire = true;
                                }
                                if(tilemap.GetGrid().GetGridObject(x, z).GetType().IsSubclassOf(typeof(Building)) 
                                && ((Building)tilemap.GetGrid().GetGridObject(x, z)).GetTeam() != unitmap.GetGrid().GetGridObject(x, z).GetTeam()
                                && unitmap.GetGrid().GetGridObject(x, z).GetMovementType() == 0)
                                {
                                    actioncapture = true;
                                }
                                if(unitmap.GetGrid().GetGridObject(x, z).GetUnitType() == Unit.UnitType.APC && unitmap.GetFriendlyUnitsInRange(x, z).Count > 0)
                                {
                                    supplyTargetables.Add(unitmap.GetGrid().GetGridObject(x, z));
                                    supplyTargetables.AddRange(unitmap.GetFriendlyUnitsInRange(x, z));
                                    if(supplyTargetables.Count > 1) actionsupply = true;
                                }
                                if(unitmap.GetFriendlyUnitsInRange(x, z).Count > 0)
                                {
                                    foreach(Unit potentialTransport in unitmap.GetFriendlyUnitsInRange(x, z))
                                    {
                                        if(potentialTransport.GetLoadCapacity() != 0 && potentialTransport.GetLoadedUnits()[potentialTransport.GetLoadCapacity()-1] == null)
                                        {
                                            if(GetTransportCompatibility(potentialTransport, x, z)) actionload = true;
                                        }
                                    }
                                }
                                if(unitmap.GetGrid().GetGridObject(x, z).GetLoadCapacity() != 0 /*&& unitmap.GetGrid().GetGridObject(x, z).GetLoadedUnits()[0] != null*/)
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

                                canvas.GetComponent<GameGUI>().ShowActionInfo(actionmove, actionfire, actioncapture, actionsupply, actionload, unloadactions, actionupgrade, x, z);
                            }
                        }
                    }
                } 
                else if (Input.GetMouseButtonDown(1) && unitSelected == "fire")
                {
                    if (MouseClickDetector(ref x, ref z, ref tile))
                    {
                        if(tile.gameObject.tag == "Selected" && localTurnSystem.GetUnitsAwaitingOrders().Contains(unitmap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ())))
                        {
                            canvas.GetComponent<GameGUI>().HideAttackInfo();
                            bool isDead = (unitmap.AttackUnit(unitmap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ()), unitmap.GetGrid().GetGridObject(x, z), damageMatrix, tilemap.GetGrid().GetGridObject(x, z).GetDefence()));
                            unitSelected = "false";
                            foreach(GameObject selected in selectedTiles)
                            {
                                Destroy(selected);
                            }
                            Unit attacker = unitmap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ());
                            attacker.SetIsActive(false);
                            attacker.VisualDeactivation();
                            attacker.AmmoCost(1);
                            localTurnSystem.MoveUnitAfterOrder(attacker);
                            photonView.RPC("SynchronizeMove", RpcTarget.Others, graph[0].x, graph[0].z, movementx, movementz);
                            photonView.RPC("SynchronizeFire", RpcTarget.Others, targetables[0].GetX(), targetables[0].GetZ(), x, z);
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
                            }
                            else if(Mathf.Abs((float)(x - targetables[0].GetX())) + Mathf.Abs((float)(z - targetables[0].GetZ())) == 1 && unitmap.GetGrid().GetGridObject(x, z).GetMaxRange() == 0)
                            {
                                bool isDeadAttacker = (unitmap.AttackUnit(unitmap.GetGrid().GetGridObject(x, z), unitmap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ()), damageMatrix, tilemap.GetGrid().GetGridObject(targetables[0].GetX(), targetables[0].GetZ()).GetDefence()));
                                Unit defender = unitmap.GetGrid().GetGridObject(x, z);
                                defender.AmmoCost(1);
                                photonView.RPC("SynchronizeFire", RpcTarget.Others, x, z, targetables[0].GetX(), targetables[0].GetZ());
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
                                }
                            }
                        }
                    }
                }
                else if (Input.GetMouseButtonDown(1) && unitSelected == "load")
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
                                    photonView.RPC("SynchronizeMove", RpcTarget.Others, graph[0].x, graph[0].z, movementx, movementz);
                                    photonView.RPC("SynchronizeLoad", RpcTarget.Others, movementx, movementz, x, z, i);
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (Input.GetMouseButtonDown(1) && unitSelected == "unload")
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
                            photonView.RPC("SynchronizeMove", RpcTarget.Others, graph[0].x, graph[0].z, movementx, movementz);
                            photonView.RPC("SynchronizeUnload", RpcTarget.Others, movementx, movementz, x, z, unloadindex);
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
                        if(localTurnSystem.GetUnitsAwaitingOrders().Contains(unitmap.GetGrid().GetGridObject(x, z)))
                        {
                            graph = pathmaking.CreateReachableGraph(x, z, unitmap.GetGrid().GetGridObject(x, z), tilemap, unitmap, false);
                            if (graph != null)
                            {
                                for (int i = 0; i < graph.Count; i++)
                                {
                                    selectedTiles.Add(Instantiate(selectedTile, new Vector3(graph[i].x * 2, 0.1f, graph[i].z * 2), Quaternion.identity, map));
                                }
                                unitSelected = "move";
                            }
                        }
                        else
                        {
                            canvas.GetComponent<GameGUI>().quickmenu.SetActive(true);
                            menuUp = true;
                        }
                    }
                }
                else if (Input.GetMouseButtonDown(1) && menuUp == false)
                {
                    if (MouseClickDetector(ref x, ref z, ref tile))
                    {
                        if(unitmap.GetGrid().GetGridObject(x, z) != null)
                        {
                            canvas.GetComponent<GameGUI>().ShowExtendedInfo(tilemap.GetGrid().GetGridObject(x, z), tile.gameObject, unitmap.GetGrid().GetGridObject(x, z), unitmap.GetGrid().GetGridObject(x, z).GetUnitInstance());
                        }
                        else
                        {
                            canvas.GetComponent<GameGUI>().ShowExtendedInfo(tilemap.GetGrid().GetGridObject(x, z), tile.gameObject, null, null);
                        }
                    }
                    menuUp = true;
                }
                else if (Input.GetKeyDown(KeyCode.R))
                {
                    pathmaking.ClearGrid();
                    targetables.Clear();
                    if (MouseClickDetector(ref x, ref z, ref tile))
                    {
                        TilemapObject tileChecker = tilemap.GetGrid().GetGridObject(x, z);
                        if(tileChecker.GetType().Equals(typeof(MilitaryBase)) && menuUp == false && unitmap.GetGrid().GetGridObject(x, z) == null)
                        {
                            MilitaryBase helper = (MilitaryBase)tileChecker;
                            menuUp = true;
                            PrintListUnits(militaryBaseRecruits, x, z, helper.GetTeam());
                        }
                        else if(tileChecker.GetType().Equals(typeof(Airport)) && menuUp == false && unitmap.GetGrid().GetGridObject(x, z) == null)
                        {
                            Airport helper = (Airport)tileChecker;
                            menuUp = true;
                            PrintListUnits(airportRecruits, x, z, helper.GetTeam());
                        }
                        else if(tileChecker.GetType().Equals(typeof(Port)) && menuUp == false && unitmap.GetGrid().GetGridObject(x, z) == null)
                        {
                            Port helper = (Port)tileChecker;
                            menuUp = true;
                            PrintListUnits(portRecruits, x, z, helper.GetTeam());
                        }
                        else if(tileChecker.GetType().Equals(typeof(City)) && menuUp == false && unitmap.GetGrid().GetGridObject(x, z) == null && ((City)tileChecker).GetTeam() == playersInMatch[localPlayerID].GetTeam())
                        {
                            City helper = (City)tileChecker;
                            menuUp = true;
                            PrintListCityUpgrades(cityUpgrades, x, z, tile, helper.GetTeam());
                        }
                    }
                }
                else if (Input.GetKeyDown(KeyCode.C))
                {
                    pathmaking.ClearGrid();
                    targetables.Clear();
                    if (MouseClickDetector(ref x, ref z, ref tile))
                    {
                        TilemapObject tileChecker = tilemap.GetGrid().GetGridObject(x, z);
                        Unit unitChecker = unitmap.GetGrid().GetGridObject(x, z);
                        if(unitChecker != null && !(tileChecker.GetType().Equals(typeof(TilemapObject))) && unitChecker.GetTeam() != ((Building)tileChecker).GetTeam())
                        {
                            Building helper = (Building)tileChecker;
                            helper.SetHealth(helper.GetHealth() - unitChecker.GetHealth());
                            if(helper.GetHealth() <= 0)
                            {
                                helper.SetTeam(unitChecker.GetTeam());
                                TilemapObject ttm = tileTypes[tilemap.GetIntFromSprite(x, z)];
                                GameObject tileInstance = Instantiate(ttm.tileVisualPrefab, tile.position, Quaternion.identity, map);
                                tileInstance.name = tileChecker.ToString() + x + z;
                                tileChecker.setTileVisual(tileInstance);
                                helper.Visualize(teamColours[unitChecker.GetTeam()], tileInstance);
                                Destroy(tile.gameObject);
                            }
                            photonView.RPC("SynchronizeCapture", RpcTarget.Others, x, z, tile.position, tileChecker.ToString() + x + z);
                        }
                    }
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    pathmaking.ClearGrid();
                    targetables.Clear();
                    if (MouseClickDetector(ref x, ref z))
                    {
                        Unit unitChecker = unitmap.GetGrid().GetGridObject(x, z);
                        if(unitChecker != null && unitChecker.GetIsActive())
                        {
                            unitChecker.VisualDeactivation();
                        } 
                        else if(unitChecker != null && !unitChecker.GetIsActive())
                        {
                            unitChecker.VisualActivation();
                        }
                    }
                }
            }

            if(MouseClickDetector(ref x, ref z, ref tile) && (lastSelected == null || lastSelected != tile) && menuUp == false)
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
                TilemapObject tileObject = tilemap.GetGrid().GetGridObject(x, z);
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

            if(unitSelected == "fire" && MouseClickDetector(ref x, ref z, ref tile) && menuUp == false)
            {
                if(tile.gameObject.tag == "Selected")
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
            }
        }
    }
}
