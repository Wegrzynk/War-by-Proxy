using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;

public class GameGUI : MonoBehaviourPunCallbacks
{
    public GameObject tileiconleft;
    public GameObject tiledefenseleft;
    public GameObject uniticonleft;
    public GameObject unithealthleft;
    public GameObject mainpanelleft;
    public GameObject tileiconright;
    public GameObject tiledefenseright;
    public GameObject uniticonright;
    public GameObject unithealthright;
    public GameObject mainpanelright;

    public GameObject tileinfo;
    public GameObject quickmenu;
    public GameObject quickmenu2;
    public GameObject actionmenu;
    public GameObject matchoverview;
    public GameObject yieldconfirmation;
    public GameObject gameenddialog;
    public GameObject recruitmenu;
    public GameObject buildingupgrademenu;
    public GameObject attackinfo;

    public GameObject actionoptionprefab;
    public GameObject recruitoptionprefab;
    public GameObject loadedunitprefab;
    public GameObject matchoverviewprefab;
    public GameObject mainscriptholder;

    void Start()
    {
        RuntimePreviewGenerator.PreviewDirection = new Vector3(1, -1, 1);
    }

    public void ShowPlayerInfo(string direction, Material[] colours, Player player, int turnsPassed)
    {
        if(direction == "left")
        {
            mainpanelleft.transform.Find("PlayerColor").GetComponent<Image>().color = colours[player.GetTeam()].color;
            mainpanelleft.transform.Find("PlayerName").Find("PlayerNameText").GetComponent<TextMeshProUGUI>().text = PhotonNetwork.LocalPlayer.NickName;
            mainpanelleft.transform.Find("PlayerFunds").Find("PlayerFundsText").GetComponent<TextMeshProUGUI>().text = "Funds:" + player.GetFunds().ToString();
            mainpanelleft.transform.Find("PlayerIncome").Find("PlayerIncomeText").GetComponent<TextMeshProUGUI>().text = "Income:" + (player.GetBuildings().Count * 1000).ToString();
            mainpanelleft.transform.Find("PlayerTurn").Find("PlayerTurnText").GetComponent<TextMeshProUGUI>().text = "Turn:" + turnsPassed.ToString();
            mainpanelleft.SetActive(true);
        }
        else
        {
            mainpanelright.transform.Find("PlayerColor").GetComponent<Image>().color = colours[player.GetTeam()].color;
            mainpanelright.transform.Find("PlayerName").Find("PlayerNameText").GetComponent<TextMeshProUGUI>().text = PhotonNetwork.LocalPlayer.NickName;
            mainpanelright.transform.Find("PlayerFunds").Find("PlayerFundsText").GetComponent<TextMeshProUGUI>().text = "Funds:" + player.GetFunds().ToString();
            mainpanelright.transform.Find("PlayerIncome").Find("PlayerIncomeText").GetComponent<TextMeshProUGUI>().text = "Income:" + (player.GetBuildings().Count * 1000).ToString();
            mainpanelright.transform.Find("PlayerTurn").Find("PlayerTurnText").GetComponent<TextMeshProUGUI>().text = "Turn:" + turnsPassed.ToString();
            mainpanelright.SetActive(true);
        }
    }

    public void ShowTileInfo(string direction, int defense, GameObject tile)
    {
        Texture2D thumbnail = RuntimePreviewGenerator.GenerateModelPreview(tile.transform);
        if(direction == "left")
        {
            tileiconleft.transform.Find("Image").GetComponent<Image>().sprite = Sprite.Create(thumbnail, new Rect(0.0f, 0.0f, thumbnail.width, thumbnail.height), new Vector2(0.5f, 0.5f), 100f);
            tiledefenseleft.transform.Find("TileDefenseText").GetComponent<TextMeshProUGUI>().text = "Defense: " + defense.ToString();
            tileiconleft.SetActive(true);
            tiledefenseleft.SetActive(true);
        }
        else
        {
            tileiconright.transform.Find("Image").GetComponent<Image>().sprite = Sprite.Create(thumbnail, new Rect(0.0f, 0.0f, thumbnail.width, thumbnail.height), new Vector2(0.5f, 0.5f), 100f);
            tiledefenseright.transform.Find("TileDefenseText").GetComponent<TextMeshProUGUI>().text = "Defense: " + defense.ToString();
            tileiconright.SetActive(true);
            tiledefenseright.SetActive(true);
        }
    }

    public void ShowUnitInfo(string direction, int health, GameObject unit)
    {
        Texture2D thumbnail = RuntimePreviewGenerator.GenerateModelPreview(unit.transform);
        if(direction == "left")
        {
            uniticonleft.transform.Find("Image").GetComponent<Image>().sprite = Sprite.Create(thumbnail, new Rect(0.0f, 0.0f, thumbnail.width, thumbnail.height), new Vector2(0.5f, 0.5f), 100f);
            unithealthleft.transform.Find("UnitHealthText").GetComponent<TextMeshProUGUI>().text = "Health: " + health.ToString();
            uniticonleft.SetActive(true);
            unithealthleft.SetActive(true);
        }
        else
        {
            uniticonright.transform.Find("Image").GetComponent<Image>().sprite = Sprite.Create(thumbnail, new Rect(0.0f, 0.0f, thumbnail.width, thumbnail.height), new Vector2(0.5f, 0.5f), 100f);
            unithealthright.transform.Find("UnitHealthText").GetComponent<TextMeshProUGUI>().text = "Health: " + health.ToString();
            uniticonright.SetActive(true);
            unithealthright.SetActive(true);
        }
    }

    public void HideUnitInfo()
    {
        if(uniticonleft.activeSelf) uniticonleft.SetActive(false);
        if(unithealthleft.activeSelf) unithealthleft.SetActive(false);
        if(uniticonright.activeSelf) uniticonright.SetActive(false);
        if(unithealthright.activeSelf) unithealthright.SetActive(false);
    }

    public void ShowExtendedInfo(TileType tile, GameObject tileobject, Unit unit, GameObject unitObject)
    {
        Texture2D tilethumbnail = RuntimePreviewGenerator.GenerateModelPreview(tileobject.transform);
        Transform tiletypeinfo = tileinfo.transform.Find("TileTypeInfo");
        tiletypeinfo.Find("TTI_Title").Find("Text").GetComponent<TextMeshProUGUI>().text = "Tile: " + tile.ToString();
        tiletypeinfo.Find("TTI_Icon").Find("Image").GetComponent<Image>().sprite = Sprite.Create(tilethumbnail, new Rect(0.0f, 0.0f, tilethumbnail.width, tilethumbnail.height), new Vector2(0.5f, 0.5f), 100f);
        tiletypeinfo.Find("TTI_Defense").Find("Text").GetComponent<TextMeshProUGUI>().text = "Defense: " + tile.GetDefence();
        tiletypeinfo.Find("TTI_Income").Find("Text").GetComponent<TextMeshProUGUI>().text = "Income: " + CheckIncome(tile);
        tiletypeinfo.Find("TTI_Repair").Find("Text").GetComponent<TextMeshProUGUI>().text = "Repairs: " + CheckRepair(tile);
        tiletypeinfo.Find("TTI_Penalties").Find("TextFoot").GetComponent<TextMeshProUGUI>().text = "Foot: " + tile.GetMovementPenaltyType(0);
        tiletypeinfo.Find("TTI_Penalties").Find("TextTires").GetComponent<TextMeshProUGUI>().text = "Tires: " + tile.GetMovementPenaltyType(1);
        tiletypeinfo.Find("TTI_Penalties").Find("TextThreads").GetComponent<TextMeshProUGUI>().text = "Threads: " + tile.GetMovementPenaltyType(2);
        tiletypeinfo.Find("TTI_Penalties").Find("TextAir").GetComponent<TextMeshProUGUI>().text = "Air: " + tile.GetMovementPenaltyType(3);
        tiletypeinfo.Find("TTI_Penalties").Find("TextShip").GetComponent<TextMeshProUGUI>().text = "Ship: " + tile.GetMovementPenaltyType(4);
        tiletypeinfo.Find("TTI_Penalties").Find("TextLander").GetComponent<TextMeshProUGUI>().text = "Lander: " + tile.GetMovementPenaltyType(5);

        if(unit == null)
        {
            tileinfo.transform.Find("UnitInfo").gameObject.SetActive(false);
            tileinfo.transform.Find("NoUnitInfo").gameObject.SetActive(true);
        }
        else
        {
            tileinfo.transform.Find("NoUnitInfo").gameObject.SetActive(false);
            Texture2D unitthumbnail = RuntimePreviewGenerator.GenerateModelPreview(unitObject.transform);
            Transform unitinfo = tileinfo.transform.Find("UnitInfo");
            unitinfo.Find("UI_Title").Find("Text").GetComponent<TextMeshProUGUI>().text = "Unit: " + unit.ToString() + " " + ShowLevel(unit);
            unitinfo.Find("UI_Icon").Find("Image").GetComponent<Image>().sprite = Sprite.Create(unitthumbnail, new Rect(0.0f, 0.0f, unitthumbnail.width, unitthumbnail.height), new Vector2(0.5f, 0.5f), 100f);
            unitinfo.Find("UI_Health").Find("Text").GetComponent<TextMeshProUGUI>().text = "Health: " + unit.GetHealth();
            unitinfo.Find("UI_Movement").Find("Text").GetComponent<TextMeshProUGUI>().text = "Movement: " + unit.GetStringFromMovementType() + " " + unit.GetMovementDistance();
            unitinfo.Find("UI_Vision").Find("Text").GetComponent<TextMeshProUGUI>().text = "Vision: " + unit.GetVision();
            unitinfo.Find("UI_Fuel").Find("Text").GetComponent<TextMeshProUGUI>().text = "Fuel/Rations: " + unit.GetCurrentFuel() + "/" + unit.GetFuel();
            unitinfo.Find("UI_Ammo").Find("Text").GetComponent<TextMeshProUGUI>().text = "Ammunition: " + unit.GetCurrentAmmo() + "/" + unit.GetAmmo();
            if(unit.GetLoadCapacity() == 0)
            {
                unitinfo.Find("UI_AttackRange").Find("Text").GetComponent<TextMeshProUGUI>().text = "Attack Range: " + unit.GetMinRange() + "-" + unit.GetMaxRange();
            }
            else
            {
                unitinfo.Find("UI_AttackRange").Find("Text").GetComponent<TextMeshProUGUI>().text = "Load Capacity: " + unit.GetLoadCapacity();
                foreach (Transform child in tileinfo.transform.Find("LoadedUnitsList"))
                {
                    GameObject.Destroy(child.gameObject);
                }
                bool isempty = true;
                for(int i = 0; i < unit.GetLoadCapacity(); i++)
                {
                    if(unit.GetLoadedUnits()[i] != null)
                    {
                        tileinfo.transform.Find("LoadedUnitsList").gameObject.SetActive(true);
                        isempty = false;
                        GameObject loadedunit = Instantiate(loadedunitprefab, tileinfo.transform.Find("LoadedUnitsList"));
                        loadedunit.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = unit.GetLoadedUnits()[i].ToString() + " (" + unit.GetLoadedUnits()[i].GetHealth() + ")";
                    }
                }
                if(isempty) tileinfo.transform.Find("LoadedUnitsList").gameObject.SetActive(false);
            }
            unitinfo.gameObject.SetActive(true);
        }

        tileinfo.SetActive(true);
    }

    private string ShowLevel(Unit unit)
    {
        if(unit.GetUpgradeCounter() == 0)
        {
            return "";
        }
        else
        {
            return "+" + unit.GetUpgradeCounter().ToString();
        }
    }

    public void ShowActionInfo(bool move, bool attack, bool capture, bool supply, bool load, bool[] unloadlist, bool upgrade, int x, int z)
    {
        GameManager helper = mainscriptholder.GetComponent<GameManager>();
        if(move)
        {
            GameObject moveaction = Instantiate(actionoptionprefab, actionmenu.transform);
            moveaction.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Move";
            moveaction.GetComponent<Button>().onClick.AddListener(() => helper.ActionMove(x, z));
        }
        if(attack)
        {
            GameObject attackaction = Instantiate(actionoptionprefab, actionmenu.transform);
            attackaction.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Attack";
            attackaction.GetComponent<Button>().onClick.AddListener(() => helper.ActionAttack(x, z));
        }
        if(capture)
        {
            GameObject captureaction = Instantiate(actionoptionprefab, actionmenu.transform);
            captureaction.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Capture";
            captureaction.GetComponent<Button>().onClick.AddListener(() => helper.ActionCapture(x, z));
        }
        if(supply)
        {
            GameObject supplyaction = Instantiate(actionoptionprefab, actionmenu.transform);
            supplyaction.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Supply";
            supplyaction.GetComponent<Button>().onClick.AddListener(() => helper.ActionSupply(x, z));
        }
        if(load)
        {
            GameObject loadaction = Instantiate(actionoptionprefab, actionmenu.transform);
            loadaction.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Load";
            loadaction.GetComponent<Button>().onClick.AddListener(() => helper.ActionLoad(x, z));
        }
        for(int i = 0; i < unloadlist.Length; i++)
        {
            if(unloadlist[i])
            {
                GameObject unloadaction = Instantiate(actionoptionprefab, actionmenu.transform);
                unloadaction.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Unload" + (i+1);
                int indexparameter = i;
                unloadaction.GetComponent<Button>().onClick.AddListener(() => helper.ActionUnload(x, z, indexparameter));
            }
        }
        if(upgrade)
        {
            GameObject upgradeaction = Instantiate(actionoptionprefab, actionmenu.transform);
            upgradeaction.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Upgrade";
            upgradeaction.GetComponent<Button>().onClick.AddListener(() => helper.ActionUpgrade(x, z));
        }

        actionmenu.SetActive(true);
    }

    public void SPShowActionInfo(bool move, bool attack, bool capture, bool supply, bool load, bool[] unloadlist, bool upgrade, int x, int z)
    {
        SinglePlayerManager helper = mainscriptholder.GetComponent<SinglePlayerManager>();
        if(move)
        {
            GameObject moveaction = Instantiate(actionoptionprefab, actionmenu.transform);
            moveaction.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Move";
            moveaction.GetComponent<Button>().onClick.AddListener(() => helper.ActionMove(x, z));
        }
        if(attack)
        {
            GameObject attackaction = Instantiate(actionoptionprefab, actionmenu.transform);
            attackaction.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Attack";
            attackaction.GetComponent<Button>().onClick.AddListener(() => helper.ActionAttack(x, z));
        }
        if(capture)
        {
            GameObject captureaction = Instantiate(actionoptionprefab, actionmenu.transform);
            captureaction.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Capture";
            captureaction.GetComponent<Button>().onClick.AddListener(() => helper.ActionCapture(x, z));
        }
        if(supply)
        {
            GameObject supplyaction = Instantiate(actionoptionprefab, actionmenu.transform);
            supplyaction.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Supply";
            supplyaction.GetComponent<Button>().onClick.AddListener(() => helper.ActionSupply(x, z));
        }
        if(load)
        {
            GameObject loadaction = Instantiate(actionoptionprefab, actionmenu.transform);
            loadaction.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Load";
            loadaction.GetComponent<Button>().onClick.AddListener(() => helper.ActionLoad(x, z));
        }
        for(int i = 0; i < unloadlist.Length; i++)
        {
            if(unloadlist[i])
            {
                GameObject unloadaction = Instantiate(actionoptionprefab, actionmenu.transform);
                unloadaction.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Unload" + (i+1);
                int indexparameter = i;
                unloadaction.GetComponent<Button>().onClick.AddListener(() => helper.ActionUnload(x, z, indexparameter));
            }
        }
        if(upgrade)
        {
            GameObject upgradeaction = Instantiate(actionoptionprefab, actionmenu.transform);
            upgradeaction.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Upgrade";
            upgradeaction.GetComponent<Button>().onClick.AddListener(() => helper.ActionUpgrade(x, z));
        }

        actionmenu.SetActive(true);
    }

    public void HideActionInfo()
    {
        foreach (Transform child in actionmenu.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        actionmenu.SetActive(false);
    }

    public void ShowMatchOverview(Material[] colours)
    {
        List<Player> playersinmatch = new List<Player>(mainscriptholder.GetComponent<GameManager>().playersInMatch);
        foreach(Player playerinfo in playersinmatch)
        {
            GameObject matchplayer = Instantiate(matchoverviewprefab, matchoverview.transform.Find("OverviewPlayersList").Find("ScrollView").Find("Viewport").Find("Content"));
            matchplayer.transform.Find("PlayerColor").GetComponent<Image>().color = colours[playerinfo.GetTeam()].color;
            matchplayer.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>().text = PhotonNetwork.PlayerList[playerinfo.GetTeam()-1].NickName;
            matchplayer.transform.Find("PlayerFunds").Find("Text").GetComponent<TextMeshProUGUI>().text = "Funds: " + playerinfo.GetFunds().ToString();
            matchplayer.transform.Find("PlayerIncome").Find("Text").GetComponent<TextMeshProUGUI>().text = "Income: " + (playerinfo.GetBuildings().Count * 1000).ToString();
            matchplayer.transform.Find("PlayerUnits").Find("Text").GetComponent<TextMeshProUGUI>().text = "Units Owned: " + playerinfo.GetUnits().Count;
            matchplayer.transform.Find("PlayerUnitsValue").Find("Text").GetComponent<TextMeshProUGUI>().text = "Units Value: " + playerinfo.GetUnitsValue();
        }
        matchoverview.SetActive(true);
        quickmenu.SetActive(false);
    }

    public void SPShowMatchOverview(Material[] colours)
    {
        List<Player> playersinmatch = new List<Player>(mainscriptholder.GetComponent<SinglePlayerManager>().playersInMatch);
        foreach(Player playerinfo in playersinmatch)
        {
            GameObject matchplayer = Instantiate(matchoverviewprefab, matchoverview.transform.Find("OverviewPlayersList").Find("ScrollView").Find("Viewport").Find("Content"));
            matchplayer.transform.Find("PlayerColor").GetComponent<Image>().color = colours[playerinfo.GetTeam()].color;
            matchplayer.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>().text = PhotonNetwork.PlayerList[playerinfo.GetTeam()-1].NickName;
            matchplayer.transform.Find("PlayerFunds").Find("Text").GetComponent<TextMeshProUGUI>().text = "Funds: " + playerinfo.GetFunds().ToString();
            matchplayer.transform.Find("PlayerIncome").Find("Text").GetComponent<TextMeshProUGUI>().text = "Income: " + (playerinfo.GetBuildings().Count * 1000).ToString();
            matchplayer.transform.Find("PlayerUnits").Find("Text").GetComponent<TextMeshProUGUI>().text = "Units Owned: " + playerinfo.GetUnits().Count;
            matchplayer.transform.Find("PlayerUnitsValue").Find("Text").GetComponent<TextMeshProUGUI>().text = "Units Value: " + playerinfo.GetUnitsValue();
        }
        matchoverview.SetActive(true);
        quickmenu.SetActive(false);
    }

    public void HideMatchOverview()
    {
        foreach (Transform child in matchoverview.transform.Find("OverviewPlayersList").Find("ScrollView").Find("Viewport").Find("Content"))
        {
            GameObject.Destroy(child.gameObject);
        }
        matchoverview.SetActive(false);
    }

    public void ShowGameEndDialog(bool isvictory, bool singlePlayer)
    {
        if(isvictory)
        {
            gameenddialog.transform.Find("Description").Find("Text").GetComponent<TextMeshProUGUI>().text = "VICTORY!";
        }
        else
        {
            gameenddialog.transform.Find("Description").Find("Text").GetComponent<TextMeshProUGUI>().text = "DEFEAT!";
        }
        gameenddialog.transform.Find("ReturnButton").GetComponent<Button>().onClick.AddListener(() => LeaveGame(singlePlayer));
        gameenddialog.SetActive(true);
    }

    public void LeaveGame(bool singlePlayer)
    {
        if(singlePlayer)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            if(PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.Leaving) PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel(0);
    }

    public void ShowRecruitInfo(List<Unit> array, int x, int z, int team)
    {
        foreach(Unit item in array)
        {
            GameObject buyableUnit = Instantiate(recruitoptionprefab, recruitmenu.transform.Find("UnitShop").Find("ScrollView").Find("Viewport").Find("Content"));
            item.SetUnitType(item.GetUnitType(), team);
            item.SetUnitVisual(item.unitVisualPrefab);
            buyableUnit.transform.Find("UnitName").GetComponent<TextMeshProUGUI>().text = item.ToString();
            buyableUnit.transform.Find("UnitPrice").GetComponent<TextMeshProUGUI>().text = item.GetCost().ToString();
            mainscriptholder.GetComponent<GameManager>().AddRecruitListeners(buyableUnit, item, x, z, team);
        }
        recruitmenu.SetActive(true);
    }

    public void SPShowRecruitInfo(List<Unit> array, int x, int z, int team)
    {
        foreach(Unit item in array)
        {
            GameObject buyableUnit = Instantiate(recruitoptionprefab, recruitmenu.transform.Find("UnitShop").Find("ScrollView").Find("Viewport").Find("Content"));
            item.SetUnitType(item.GetUnitType(), team);
            item.SetUnitVisual(item.unitVisualPrefab);
            buyableUnit.transform.Find("UnitName").GetComponent<TextMeshProUGUI>().text = item.ToString();
            buyableUnit.transform.Find("UnitPrice").GetComponent<TextMeshProUGUI>().text = item.GetCost().ToString();
            mainscriptholder.GetComponent<SinglePlayerManager>().AddRecruitListeners(buyableUnit, item, x, z, team);
        }
        recruitmenu.SetActive(true);
    }
    
    public void HideRecruitInfo()
    {
        foreach (Transform child in recruitmenu.transform.Find("UnitShop").Find("ScrollView").Find("Viewport").Find("Content"))
        {
            GameObject.Destroy(child.gameObject);
        }
        recruitmenu.transform.Find("UnitInfo").gameObject.SetActive(false);
        recruitmenu.transform.Find("NoUnitInfo").gameObject.SetActive(true);
        recruitmenu.SetActive(false);
    }

    public void ShowBuildingUpgrades(List<City> array, int x, int z, Transform tile, int team)
    {
        foreach(City item in array)
        {
            GameObject buyableBuilding = Instantiate(recruitoptionprefab, buildingupgrademenu.transform.Find("BuildingShop").Find("ScrollView").Find("Viewport").Find("Content"));
            item.setTileVisual(item.tileVisualPrefab);
            buyableBuilding.transform.Find("UnitName").GetComponent<TextMeshProUGUI>().text = item.ToString();
            buyableBuilding.transform.Find("UnitPrice").GetComponent<TextMeshProUGUI>().text = item.GetCost().ToString();
            mainscriptholder.GetComponent<GameManager>().AddBuildingListeners(buyableBuilding, item, x, z, tile, team);
        }
        buildingupgrademenu.SetActive(true);
    }

    public void SPShowBuildingUpgrades(List<City> array, int x, int z, Transform tile, int team)
    {
        foreach(City item in array)
        {
            GameObject buyableBuilding = Instantiate(recruitoptionprefab, buildingupgrademenu.transform.Find("BuildingShop").Find("ScrollView").Find("Viewport").Find("Content"));
            item.setTileVisual(item.tileVisualPrefab);
            buyableBuilding.transform.Find("UnitName").GetComponent<TextMeshProUGUI>().text = item.ToString();
            buyableBuilding.transform.Find("UnitPrice").GetComponent<TextMeshProUGUI>().text = item.GetCost().ToString();
            mainscriptholder.GetComponent<SinglePlayerManager>().AddBuildingListeners(buyableBuilding, item, x, z, tile, team);
        }
        buildingupgrademenu.SetActive(true);
    }

    public void HideBuildingUpgrades()
    {
        foreach (Transform child in buildingupgrademenu.transform.Find("BuildingShop").Find("ScrollView").Find("Viewport").Find("Content"))
        {
            GameObject.Destroy(child.gameObject);
        }
        buildingupgrademenu.transform.Find("BuildingInfo").gameObject.SetActive(false);
        buildingupgrademenu.transform.Find("NoBuildingInfo").gameObject.SetActive(true);
        buildingupgrademenu.SetActive(false);
    }

    public void ShowAttackInfo(Vector3 pos, GameObject attackerUnit, GameObject defenderUnit, int simulatedAttack, int simulatedCounterattack)
    {
        Texture2D attackerthumbnail = RuntimePreviewGenerator.GenerateModelPreview(attackerUnit.transform);
        Texture2D defenderthumbnail = RuntimePreviewGenerator.GenerateModelPreview(defenderUnit.transform);
        attackinfo.transform.position = pos + new Vector3(0, -60, 0);
        attackinfo.transform.Find("AttackerIcon").Find("Image").GetComponent<Image>().sprite = Sprite.Create(attackerthumbnail, new Rect(0.0f, 0.0f, attackerthumbnail.width, attackerthumbnail.height), new Vector2(0.5f, 0.5f), 100f);
        attackinfo.transform.Find("DefenderIcon").Find("Image").GetComponent<Image>().sprite = Sprite.Create(defenderthumbnail, new Rect(0.0f, 0.0f, defenderthumbnail.width, defenderthumbnail.height), new Vector2(0.5f, 0.5f), 100f);
        attackinfo.transform.Find("AttackDescription").Find("Text").GetComponent<TextMeshProUGUI>().text = "Attack: " + simulatedAttack + "%";
        attackinfo.transform.Find("CounterattackDescription").Find("Text").GetComponent<TextMeshProUGUI>().text = "Counterattack: " + simulatedCounterattack + "%";
        attackinfo.SetActive(true);
    }

    public void HideAttackInfo()
    {
        if(attackinfo.activeSelf) attackinfo.SetActive(false);
    }

    public string CheckIncome(TileType tile)
    {
        if(tile.GetTilemapSprite() == TileType.TilemapSprite.HQ)
        {
            return "2000";
        }
        if(tile.GetType().IsSubclassOf(typeof(Building)))
        {
            return "1000";
        }
        return "None";
    }

    public string CheckRepair(TileType tile)
    {
        if(tile.GetType().IsSubclassOf(typeof(City)) || tile.GetTilemapSprite() == TileType.TilemapSprite.HQ || tile.GetTilemapSprite() == TileType.TilemapSprite.MilitaryBase)
        {
            return "Ground";
        }
        if(tile.GetTilemapSprite() == TileType.TilemapSprite.Airport)
        {
            return "Air";
        }
        if(tile.GetTilemapSprite() == TileType.TilemapSprite.Port)
        {
            return "Sea";
        }
        return "Nothing";
    }

    public void ChangeDirection(string direction)
    {
        if(direction == "left")
        {
            if(tileiconright.activeSelf)
            {
                tileiconright.SetActive(false);
                tileiconleft.SetActive(true);
            }
            if(tiledefenseright.activeSelf)
            {
                tiledefenseright.SetActive(false);
                tiledefenseleft.SetActive(true);
            }
            if(uniticonright.activeSelf)
            {
                uniticonright.SetActive(false);
                uniticonleft.SetActive(true);
            }
            if(unithealthright.activeSelf)
            {
                unithealthright.SetActive(false);
                unithealthleft.SetActive(true);
            }
            mainpanelright.SetActive(false);
            mainpanelleft.SetActive(true);
        }
        else
        {
            if(tileiconleft.activeSelf)
            {
                tileiconleft.SetActive(false);
                tileiconright.SetActive(true);
            }
            if(tiledefenseleft.activeSelf)
            {
                tiledefenseleft.SetActive(false);
                tiledefenseright.SetActive(true);
            }
            if(uniticonleft.activeSelf)
            {
                uniticonleft.SetActive(false);
                uniticonright.SetActive(true);
            }
            if(unithealthleft.activeSelf)
            {
                unithealthleft.SetActive(false);
                unithealthright.SetActive(true);
            }
            mainpanelleft.SetActive(false);
            mainpanelright.SetActive(true);
        }
    }
}
