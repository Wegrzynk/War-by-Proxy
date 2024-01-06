using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIQuirks : MonoBehaviour
{
    public GameObject mainScriptHolder;
    public System.Random rng = new System.Random();
    public int[,] AIPreset = new int[20,6]
    {
        { 10, 10, 40, 40, 0, 7},   // AATank,
        { 0, 0, 0, 0, 0, 5},   // APC,
        { 0, 0, 100, 0, 0, 10},   // Artillery,
        { 0, 10, 60, 30, 0, 10},   // Heli,
        { 20, 0, 50, 30, 0, 0},   // Battleship,
        { 10, 0, 50, 40, 0, 20},   // Bomber,
        { 0, 0, 30, 0, 70, 0},   // Carrier,
        { 0, 0, 60, 40, 0, 0},   // Cruiser,
        { 0, 0, 30, 70, 0, 10},   // Fighter,
        { 0, 0, 0, 0, 0, 16},   // Infantry,
        { 0, 0, 0, 0, 0, 0},   // Tship,
        { 10, 10, 30, 50, 0, 10},   // Midtank,
        { 0, 0, 0, 0, 0, 10},   // Mech,
        { 10, 10, 30, 50, 0, 5},   // Heavytank,
        { 0, 0, 90, 10, 0, 5},   // Missile,
        { 0, 0, 100, 0, 0, 9},   // Recon,
        { 0, 0, 90, 10, 0, 11},   // Rocket,
        { 0, 0, 60, 40, 0, 0},   // Sub,
        { 0, 0, 0, 0, 0, 5},   // Theli,
        { 0, 0, 100, 0, 0, 17}   // Tank
    };
    public int minFoot = 5;
    public int dangerThreshold = 90;
    public Dictionary<Tuple<int,int>,List<Unit>> heatmap;

    //---HELPER FUNCTIONS
    private void AIMove(Unit unit, Node bestAttackPosition, SinglePlayerManager mainManager)
    {
        int initialx = unit.GetX();
        int initialz = unit.GetZ();
        mainManager.unitmap.MoveUnit(unit.GetX(), unit.GetZ(), bestAttackPosition.x, bestAttackPosition.z);
        GameObject movableUnit = GameObject.Find(unit.ToString() + initialx + initialz);
        if(movableUnit)
        {
            movableUnit.transform.position = new Vector3(bestAttackPosition.x * 2, 0.7f, bestAttackPosition.z * 2);
            movableUnit.name = unit.ToString() + bestAttackPosition.x + bestAttackPosition.z;
            unit.FuelCost(Mathf.Abs(initialx - bestAttackPosition.x) + Mathf.Abs(initialz - bestAttackPosition.z));
        }
    }

    private void AIAttack(Unit unit, Unit bestAttackTarget, SinglePlayerManager mainManager)
    {
        bool isDead = (mainManager.unitmap.AttackUnit(unit, bestAttackTarget, mainManager.damageMatrix, mainManager.tilemap.GetGrid().GetGridObject(bestAttackTarget.GetX(), bestAttackTarget.GetZ()).GetDefence()));
        unit.AmmoCost(1);
        int team = bestAttackTarget.GetTeam()-1;
        if(isDead)
        {
            GameObject destroyable = GameObject.Find(bestAttackTarget.ToString() + bestAttackTarget.GetX() + bestAttackTarget.GetZ());
            Destroy(destroyable);
            for(int i = 0; i < bestAttackTarget.GetLoadCapacity(); i++)
            {
                if(bestAttackTarget.GetLoadedUnits()[i] != null)
                {
                    mainManager.playersInMatch[bestAttackTarget.GetTeam()-1].RemoveLoadedUnit(bestAttackTarget.GetLoadedUnits()[i]);
                }
            }
            mainManager.playersInMatch[bestAttackTarget.GetTeam()-1].RemoveUnit(bestAttackTarget);
            mainManager.unitmap.GetGrid().SetGridObject(bestAttackTarget.GetX(), bestAttackTarget.GetZ(), null);
            if(mainManager.playersInMatch[team].GetUnloadedUnits().Count == 0)
            {
                mainManager.GameLost(team);
            }
        }
        else if(Mathf.Abs((float)(bestAttackTarget.GetX() - unit.GetX())) + Mathf.Abs((float)(bestAttackTarget.GetZ() - unit.GetZ())) == 1 && bestAttackTarget.GetMaxRange() == 0)
        {
            bool isDeadAttacker = (mainManager.unitmap.AttackUnit(bestAttackTarget, unit, mainManager.damageMatrix, mainManager.tilemap.GetGrid().GetGridObject(unit.GetX(), unit.GetZ()).GetDefence()));
            bestAttackTarget.AmmoCost(1);
            team = unit.GetTeam()-1;
            if(isDeadAttacker)
            {
                GameObject destroyableAttacker = GameObject.Find(unit.ToString() + unit.GetX() + unit.GetZ());
                Destroy(destroyableAttacker);
                for(int i = 0; i < unit.GetLoadCapacity(); i++)
                {
                    if(unit.GetLoadedUnits()[i] != null)
                    {
                        mainManager.playersInMatch[unit.GetTeam()-1].RemoveLoadedUnit(unit.GetLoadedUnits()[i]);
                    }
                }
                mainManager.playersInMatch[unit.GetTeam()-1].RemoveUnit(unit);
                mainManager.unitmap.GetGrid().SetGridObject(unit.GetX(), unit.GetZ(), null);
                if(mainManager.playersInMatch[team].GetUnloadedUnits().Count == 0)
                {
                    mainManager.GameLost(team);
                }
            }
        }
    }

    private void AICapture(Unit unit, SinglePlayerManager mainManager)
    {
        int x = unit.GetX();
        int z = unit.GetZ();

        Transform localtile = GameObject.Find(mainManager.tilemap.GetGrid().GetGridObject(x, z).ToString() + x + z).transform;
        TileType tileChecker = mainManager.tilemap.GetGrid().GetGridObject(x, z);
        int possibleloser = -1;
        string oldtile = tileChecker.ToString() + x + z;
        if(unit != null && !(tileChecker.GetType().Equals(typeof(TileType))) && unit.GetTeam() != ((Building)tileChecker).GetTeam())
        {
            Building helper = (Building)tileChecker;
            helper.SetHealth(helper.GetHealth() - unit.GetHealth());
            if(helper.GetHealth() <= 0)
            {
                if(helper.GetTeam() != 0) mainManager.playersInMatch[helper.GetTeam()-1].RemoveBuilding(helper);
                if(tileChecker.GetType().Equals(typeof(HQ)))
                {
                    helper.SetTilemapSprite(TileType.TilemapSprite.City);
                    possibleloser = helper.GetTeam()-1;
                }
                helper.SetTeam(unit.GetTeam());
                TileType ttm = mainManager.tileTypes[mainManager.tilemap.GetIntFromSprite(x, z)];
                GameObject tileInstance = Instantiate(ttm.tileVisualPrefab, localtile.position, Quaternion.identity, mainManager.map);
                tileInstance.name = tileChecker.ToString() + x + z;
                tileChecker.setTileVisual(tileInstance);
                mainManager.playersInMatch[mainManager.turnCounter].AddBuilding(helper);
                helper.Visualize(mainManager.teamColours[unit.GetTeam()], tileInstance);
                Destroy(localtile.gameObject);
            }
            if(possibleloser != -1)
            {
                mainManager.GameLost(possibleloser);
            }
        }
    }

    private void AILoad(Unit unit, Unit transport, SinglePlayerManager mainManager)
    {
        Unit loaded = unit;
        for(int i = 0; i < transport.GetLoadCapacity(); i++)
        {
            if(transport.GetLoadedUnits()[i] == null)
            {
                transport.LoadUnit(i, loaded);
                mainManager.playersInMatch[loaded.GetTeam()-1].RemoveUnit(loaded);
                mainManager.playersInMatch[loaded.GetTeam()-1].AddLoadedUnit(loaded);
                GameObject destroyable = GameObject.Find(unit.ToString() + unit.GetX() + unit.GetZ());
                Destroy(destroyable);
                mainManager.unitmap.GetGrid().SetGridObject(unit.GetX(), unit.GetZ(), null);
                break;
            }
        }
    }

    private void AIUnload(Unit unit, TileType unloadPosition, SinglePlayerManager mainManager)
    {
        Unit unloader = unit;
        Unit unloaded = unloader.GetLoadedUnits()[0];
        mainManager.playersInMatch[unloaded.GetTeam()-1].RemoveLoadedUnit(unloaded);
        mainManager.unitmap.InsertUnit(unloadPosition.GetX(), unloadPosition.GetZ(), unloaded.GetUnitType(), unloaded.GetTeam(), unloaded.GetCurrentAmmo(), unloaded.GetCurrentFuel(), unloaded.GetLoadedUnits(), unloader.GetUpgradeCounter());
        unloader.LoadUnit(0, null);
        unloaded = mainManager.unitmap.GetGrid().GetGridObject(unloadPosition.GetX(), unloadPosition.GetZ());
        mainManager.UnitInstantiate(unloaded, unloadPosition.GetX(), unloadPosition.GetZ(), unloaded.GetTeam());
        mainManager.UnitArrayShiftLeft(unloader.GetLoadedUnits(), 1);
        mainManager.playersInMatch[unloaded.GetTeam()-1].AddUnit(unloaded);
        switch(unloaded.GetUnitType())
        {
            case Unit.UnitType.Infantry: unloaded.SetAIbehaviour(6); break;
            case Unit.UnitType.Mech: unloaded.SetAIbehaviour(6); break;
            case Unit.UnitType.APC: unloaded.SetAIbehaviour(7); break;
            case Unit.UnitType.Theli: unloaded.SetAIbehaviour(8); break;
            case Unit.UnitType.Tship: unloaded.SetAIbehaviour(9); break;
            default: unloaded.SetAIbehaviour(RNGbehaviour((int)unloaded.GetUnitType())); break;
        }
    }

    private void AISupply(Unit unit, SinglePlayerManager mainManager)
    {
        List<Unit> supplytargets = new List<Unit>(mainManager.unitmap.GetFriendlyUnitsInRange(unit.GetX(), unit.GetZ(), unit));
        foreach(Unit target in supplytargets)
        {
            target.Refuel();
            target.Reammo();
        }
    }

    private void AIRecruit(Unit.UnitType type, int x, int z, int team, SinglePlayerManager mainManager)
    {
        Unit un = mainManager.unitTypes[(int)type];
        un.SetUnitType(un.GetUnitType(), team+1);
        mainManager.unitmap.InsertUnit(x, z, type, team+1, un.GetAmmo(), un.GetFuel(), null, 0);
        switch(type)
        {
            case Unit.UnitType.Infantry: mainManager.unitmap.GetGrid().GetGridObject(x, z).SetAIbehaviour(6); break;
            case Unit.UnitType.Mech: mainManager.unitmap.GetGrid().GetGridObject(x, z).SetAIbehaviour(6); break;
            case Unit.UnitType.APC: mainManager.unitmap.GetGrid().GetGridObject(x, z).SetAIbehaviour(7); break;
            case Unit.UnitType.Theli: mainManager.unitmap.GetGrid().GetGridObject(x, z).SetAIbehaviour(8); break;
            case Unit.UnitType.Tship: mainManager.unitmap.GetGrid().GetGridObject(x, z).SetAIbehaviour(9); break;
            default: mainManager.unitmap.GetGrid().GetGridObject(x, z).SetAIbehaviour(RNGbehaviour((int)type)); break;
        }
        mainManager.UnitInstantiate(un, x, z, team+1);
        mainManager.playersInMatch[team].AddUnit(mainManager.unitmap.GetGrid().GetGridObject(x, z));
        mainManager.playersInMatch[team].ChangeFunds(-(mainManager.unitmap.GetGrid().GetGridObject(x, z).GetCost()));
    }

    public int RNGbehaviour(int type)
    {
        int percentage = rng.Next(1, 101);
        for(int i = 0; i < 5; i++)
        {
            percentage -= AIPreset[type, i];
            if(percentage <= 0) return i+1;
        }
        return 5;
    }

    private void CreateEnemyHeatMap(SinglePlayerManager mainManager, Unit unit)
    {
        heatmap = new Dictionary<Tuple<int, int>, List<Unit>>();
        for(int x = 0; x < mainManager.unitmap.GetGrid().GetWidth(); x++)
        {
            for(int z = 0; z < mainManager.unitmap.GetGrid().GetHeight(); z++)
            {
                heatmap.Add(new Tuple<int, int>(x,z), new List<Unit>());
            }
        }

        //Debug.Log("Considered unit is: " + unit.ToString() + unit.GetX() + unit.GetZ());
        foreach(Player enemyPlayer in mainManager.playersInMatch)
        {
            if(unit.GetTeam() == enemyPlayer.GetTeam()) continue;
            foreach(Unit enemyUnit in enemyPlayer.GetUnloadedUnits())
            {
                //Debug.Log("Currently checked unit is: " + enemyUnit.ToString() + enemyUnit.GetX() + enemyUnit.GetZ());
                if(!enemyUnit.isIndirect())
                {
                    List<DijkstraNode> unitMovementGraph = mainManager.getUnitMovementGraph(enemyUnit.GetX(), enemyUnit.GetZ());
                    foreach(DijkstraNode possibleMove in unitMovementGraph)
                    {
                        List<Unit> reference = heatmap[new Tuple<int, int>(possibleMove.x, possibleMove.z)];
                        if(!reference.Contains(enemyUnit)) reference.Add(enemyUnit);

                        //Left
                        if(possibleMove.x - 1 >= 0)
                        {
                            reference = heatmap[new Tuple<int, int>(possibleMove.x - 1, possibleMove.z)];
                            if(!reference.Contains(enemyUnit)) reference.Add(enemyUnit);
                        }
                        //Right
                        if(possibleMove.x + 1 < mainManager.unitmap.GetGrid().GetWidth())
                        {
                            reference = heatmap[new Tuple<int, int>(possibleMove.x + 1, possibleMove.z)];
                            if(!reference.Contains(enemyUnit)) reference.Add(enemyUnit);
                        }
                        //Down
                        if(possibleMove.z - 1 >= 0)
                        {
                            reference = heatmap[new Tuple<int, int>(possibleMove.x, possibleMove.z - 1)];
                            if(!reference.Contains(enemyUnit)) reference.Add(enemyUnit);
                        }
                        //Up
                        if(possibleMove.z + 1 < mainManager.unitmap.GetGrid().GetHeight())
                        {
                            reference = heatmap[new Tuple<int, int>(possibleMove.x, possibleMove.z + 1)];
                            if(!reference.Contains(enemyUnit)) reference.Add(enemyUnit);
                        }
                    }
                }
                else
                {
                    int minRange = enemyUnit.GetMinRange();
                    int maxRange = enemyUnit.GetMaxRange();
                    int counter = 0;
                    for(int i=maxRange; i>=-maxRange; i--)
                    {
                        for(int j=counter; j>=-counter; j--)
                        {
                            if(Mathf.Abs(i)+Mathf.Abs(j)>=minRange && enemyUnit.GetX()+i >= 0 && enemyUnit.GetZ()+j >= 0 && 
                            enemyUnit.GetX()+i < mainManager.unitmap.GetGrid().GetWidth() && enemyUnit.GetZ()+j < mainManager.unitmap.GetGrid().GetHeight())
                            {
                                //Debug.Log("Checking coordinates: " + (enemyUnit.GetX()+i) + "," + (enemyUnit.GetZ()+j));
                                List<Unit> reference = heatmap[new Tuple<int, int>(enemyUnit.GetX()+i, enemyUnit.GetZ()+j)];
                                if(!reference.Contains(enemyUnit)) reference.Add(enemyUnit);
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
                }
            }
        }
    }

    private bool evaluateDanger(int x, int z, Unit unit, SinglePlayerManager mainManager)
    {
        float danger = 0f;
        CreateEnemyHeatMap(mainManager, unit);
        foreach(Unit enemyUnit in heatmap[new Tuple<int, int>(x,z)])
        {
            danger += mainManager.damageMatrix[enemyUnit.GetIntFromUnit(),unit.GetIntFromUnit()];
        }
        if(unit.GetPureMovementType() != Unit.MovementType.Air)
        {
            danger *= mainManager.tilemap.GetGrid().GetGridObject(x,z).GetDefence();
        }
        if(danger > dangerThreshold) return false;
        return true;
    }

    private bool attackEnemiesInRangeStationary(Unit unit, SinglePlayerManager mainManager)
    {
        int maxProfit = 0;
        int helper;
        Unit bestAttackTarget = null;

        foreach(Unit enemyChecker in mainManager.unitmap.GetEnemyUnitsInRange(unit.GetX(), unit.GetZ(), false, mainManager))
        {
            helper = mainManager.getPotentialAttackValue(unit, enemyChecker);
            if(mainManager.getPotentialCounterattack(unit, enemyChecker) < 50 && helper > maxProfit)
            {
                maxProfit = helper;
                bestAttackTarget = enemyChecker;
            }
        }
        if(bestAttackTarget != null)
        {
            AIAttack(unit, bestAttackTarget, mainManager);
            return true;
        }
        return false;
    }

    private bool attackEnemiesInRangeMobile(Unit unit, SinglePlayerManager mainManager)
    {
        List<DijkstraNode> unitMovementGraph = mainManager.getUnitMovementGraph(unit.GetX(), unit.GetZ());
        int maxProfit = 0;
        int helper;
        DijkstraNode bestAttackPosition = null;
        Unit bestAttackTarget = null;

        foreach(DijkstraNode possibleMove in unitMovementGraph)
        {
            foreach(Unit enemyChecker in mainManager.unitmap.GetEnemyUnitsInRange(possibleMove.x, possibleMove.z, mainManager, unit))
            {
                helper = mainManager.getPotentialAttackValue(unit, enemyChecker);
                if(mainManager.getPotentialCounterattack(unit, enemyChecker) < 50 && evaluateDanger(possibleMove.x, possibleMove.z, unit, mainManager) && helper > maxProfit)
                {
                    maxProfit = helper;
                    bestAttackPosition = possibleMove;
                    bestAttackTarget = enemyChecker;
                }
            }
        }
        if(bestAttackPosition != null && bestAttackTarget != null)
        {
            AIMove(unit, bestAttackPosition, mainManager);
            AIAttack(unit, bestAttackTarget, mainManager);
            return true;
        }
        return false;
    }

    private bool moveToVIPsInRange(Unit unit, SinglePlayerManager mainManager, bool landVIPs, bool regularTroops)
    {
        List<DijkstraNode> unitMovementGraph = mainManager.getUnitMovementGraph(unit.GetX(), unit.GetZ());
        int maxVIPs = 0;
        int VIPcounter = 0;
        int maxProfit = 0;
        int helper;
        Unit bestAttackTarget = null;
        DijkstraNode bestCoverPosition = null;
        bool returnValue = false;

        foreach(DijkstraNode possibleMove in unitMovementGraph)
        {
            VIPcounter = 0;
            foreach(Unit friendlyChecker in mainManager.unitmap.GetFriendlyUnitsInRange(possibleMove.x, possibleMove.z, unit))
            {
                if(regularTroops || ((landVIPs && (friendlyChecker.GetUnitType() == Unit.UnitType.Infantry || friendlyChecker.GetUnitType() == Unit.UnitType.Mech))
                || (!landVIPs && (friendlyChecker.GetUnitType() == Unit.UnitType.Tship))))
                {
                    VIPcounter++;
                }
            }
            if(VIPcounter > maxVIPs && !unit.isIndirect())
            {
                foreach(Unit enemyChecker in mainManager.unitmap.GetEnemyUnitsInRange(possibleMove.x, possibleMove.z, mainManager, unit))
                {
                    helper = mainManager.getPotentialAttackValue(unit, enemyChecker);
                    if(mainManager.getPotentialCounterattack(unit, enemyChecker) < 50 && helper > maxProfit)
                    {
                        maxProfit = helper;
                        bestAttackTarget = enemyChecker;
                    }
                }
            }
            if(VIPcounter > maxVIPs)
            {
                maxVIPs = VIPcounter;
                bestCoverPosition = possibleMove;
            }
        }
        if(bestCoverPosition != null)
        {
            AIMove(unit, bestCoverPosition, mainManager);
            returnValue = true;
        }
        if(bestAttackTarget != null)
        {
            AIAttack(unit, bestAttackTarget, mainManager);
            returnValue = true;
        }
        return returnValue;
    }

    private bool moveToCriticalShortageInRange(Unit unit, SinglePlayerManager mainManager)
    {
        List<DijkstraNode> unitMovementGraph = mainManager.getUnitMovementGraph(unit.GetX(), unit.GetZ());
        int maxSupply = 0;
        int maxCriticalSupply = 0;
        int supplyCounter = 0;
        int criticalSupplyCounter = 0;
        DijkstraNode bestSupplyPosition = null;
        bool returnValue = false;

        foreach(DijkstraNode possibleMove in unitMovementGraph)
        {
            supplyCounter = 0;
            criticalSupplyCounter = 0;
            foreach(Unit friendlyChecker in mainManager.unitmap.GetFriendlyUnitsInRange(possibleMove.x, possibleMove.z, unit))
            {
                supplyCounter++;
                if(friendlyChecker.GetCurrentFuel() <= friendlyChecker.GetFuel() / 5)
                {
                    criticalSupplyCounter++;
                }
            }
            if(criticalSupplyCounter > maxCriticalSupply)
            {
                maxCriticalSupply = criticalSupplyCounter;
                maxSupply = supplyCounter;
                bestSupplyPosition = possibleMove;
            }
            else if(criticalSupplyCounter == maxCriticalSupply && criticalSupplyCounter != 0 && supplyCounter > maxSupply)
            {
                maxCriticalSupply = criticalSupplyCounter;
                maxSupply = supplyCounter;
                bestSupplyPosition = possibleMove;
            }
        }
        if(bestSupplyPosition != null)
        {
            AIMove(unit, bestSupplyPosition, mainManager);
            AISupply(unit, mainManager);
            returnValue = true;
        }
        return returnValue;
    }

    private bool moveOntoNonAlliedBuilding(Unit unit, SinglePlayerManager mainManager)
    {
        List<DijkstraNode> unitMovementGraph = mainManager.getUnitMovementGraph(unit.GetX(), unit.GetZ());
        DijkstraNode securedBuildingTile = null;
        int maxProfit = 0;
        int helper;
        Unit bestAttackTarget = null;
        bool attackFound = false;
        bool returnValue = false;

        foreach(DijkstraNode possibleMove in unitMovementGraph)
        {
            if(isNonAlliedBuilding(possibleMove.x, possibleMove.z, unit, mainManager) && isReplacementNotLessImportant(securedBuildingTile, mainManager.tilemap.GetGrid().GetGridObject(possibleMove.x, possibleMove.z), mainManager))
            {
                if(!unit.isIndirect())
                {
                    foreach(Unit enemyChecker in mainManager.unitmap.GetEnemyUnitsInRange(possibleMove.x, possibleMove.z, mainManager, unit))
                    {
                        helper = mainManager.getPotentialAttackValue(unit, enemyChecker);
                        if(mainManager.getPotentialCounterattack(unit, enemyChecker) < 50 && helper > maxProfit)
                        {
                            attackFound = true;
                            maxProfit = helper;
                            bestAttackTarget = enemyChecker;
                            securedBuildingTile = possibleMove;
                        }
                    }
                }
                if(!attackFound) securedBuildingTile = possibleMove;
            }
        }
        if(securedBuildingTile != null)
        {
            AIMove(unit, securedBuildingTile, mainManager);
            returnValue = true;
        }
        if(bestAttackTarget != null)
        {
            AIAttack(unit, bestAttackTarget, mainManager);
            returnValue = true;
        }
        return returnValue;
    }

    private bool moveNextToNonAlliedBuilding(Unit unit, SinglePlayerManager mainManager)
    {
        List<DijkstraNode> unitMovementGraph = mainManager.getUnitMovementGraph(unit.GetX(), unit.GetZ());
        DijkstraNode securedStopTile = null;
        TileType securedBuildingTile = null;
        bool returnValue = false;

        foreach(DijkstraNode possibleMove in unitMovementGraph)
        {
            foreach(TileType tile in GetNeighbourNonAlliedBuildings(possibleMove.x, possibleMove.z, unit, mainManager))
            {
                if(mainManager.unitmap.GetGrid().GetGridObject(tile.GetX(), tile.GetZ()) == null && isReplacementNotLessImportant(securedBuildingTile, tile, mainManager))
                {
                    securedStopTile = possibleMove;
                    securedBuildingTile = tile;
                }
            }
        }
        if(securedStopTile != null)
        {
            AIMove(unit, securedStopTile, mainManager);
            returnValue = true;
        }
        if(securedBuildingTile != null)
        {
            AIUnload(unit, securedBuildingTile, mainManager);
            returnValue = true;
        }
        return returnValue;
    }

    private bool getOffBuildingIfInfantryNearby(Unit unit, SinglePlayerManager mainManager)
    {
        List<DijkstraNode> infantryCheck = mainManager.getUnitMovementGraph(unit.GetX(), unit.GetZ(), true);
        List<DijkstraNode> unitMovementGraph = mainManager.getUnitMovementGraph(unit.GetX(), unit.GetZ());

        foreach(DijkstraNode checker in infantryCheck)
        {
            if(mainManager.unitmap.GetGrid().GetGridObject(checker.x, checker.z) != null && mainManager.unitmap.GetGrid().GetGridObject(checker.x, checker.z).GetTeam() == unit.GetTeam() && (mainManager.unitmap.GetGrid().GetGridObject(checker.x, checker.z).GetUnitType() == Unit.UnitType.Infantry || mainManager.unitmap.GetGrid().GetGridObject(checker.x, checker.z).GetUnitType() == Unit.UnitType.Mech))
            {
                foreach(DijkstraNode possibleMove in unitMovementGraph)
                {
                    if(mainManager.unitmap.GetGrid().GetGridObject(possibleMove.x, possibleMove.z) == null)
                    {
                        AIMove(unit, possibleMove, mainManager);
                        return true;
                    }
                }
                return false;
            }
        }
        return false;
    }

    private bool getOffBuildingIfInfantryNearbyCheck(Unit unit, SinglePlayerManager mainManager)
    {
        List<DijkstraNode> infantryCheck = mainManager.getUnitMovementGraph(unit.GetX(), unit.GetZ(), true);
        List<DijkstraNode> unitMovementGraph = mainManager.getUnitMovementGraph(unit.GetX(), unit.GetZ());

        foreach(DijkstraNode checker in infantryCheck)
        {
            if(mainManager.unitmap.GetGrid().GetGridObject(checker.x, checker.z) != null && mainManager.unitmap.GetGrid().GetGridObject(checker.x, checker.z).GetTeam() == unit.GetTeam() && (mainManager.unitmap.GetGrid().GetGridObject(checker.x, checker.z).GetUnitType() == Unit.UnitType.Infantry || mainManager.unitmap.GetGrid().GetGridObject(checker.x, checker.z).GetUnitType() == Unit.UnitType.Mech))
            {
                foreach(DijkstraNode possibleMove in unitMovementGraph)
                {
                    if(mainManager.unitmap.GetGrid().GetGridObject(possibleMove.x, possibleMove.z) == null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        return false;
    }

    private bool captureBuilding(Unit unit, SinglePlayerManager mainManager)
    {
        List<DijkstraNode> unitMovementGraph = mainManager.getUnitMovementGraph(unit.GetX(), unit.GetZ());
        DijkstraNode securedBuildingTile = null;
        bool returnValue = false;

        foreach(DijkstraNode possibleMove in unitMovementGraph)
        {
            if(isNonAlliedBuilding(possibleMove.x, possibleMove.z, unit, mainManager) && isReplacementNotLessImportant(securedBuildingTile, mainManager.tilemap.GetGrid().GetGridObject(possibleMove.x, possibleMove.z), mainManager))
            {
                securedBuildingTile = possibleMove;
            }
        }
        if(securedBuildingTile != null)
        {
            AIMove(unit, securedBuildingTile, mainManager);
            AICapture(unit, mainManager);
            returnValue = true;
        }
        return returnValue;
    }

    private bool moveOntoAlliedNonProductionBuilding(Unit unit, SinglePlayerManager mainManager)
    {
        List<DijkstraNode> unitMovementGraph = mainManager.getUnitMovementGraph(unit.GetX(), unit.GetZ());
        DijkstraNode securedBuildingTile = null;
        int maxProfit = 0;
        int helper;
        Unit bestAttackTarget = null;
        bool attackFound = false;
        bool returnValue = false;

        foreach(DijkstraNode possibleMove in unitMovementGraph)
        {
            if(isAlliedNonProductionBuilding(possibleMove.x, possibleMove.z, unit, mainManager))
            {
                if(!unit.isIndirect())
                {
                    foreach(Unit enemyChecker in mainManager.unitmap.GetEnemyUnitsInRange(possibleMove.x, possibleMove.z, mainManager, unit))
                    {
                        helper = mainManager.getPotentialAttackValue(unit, enemyChecker);
                        if(mainManager.getPotentialCounterattack(unit, enemyChecker) < 50 && helper > maxProfit)
                        {
                            attackFound = true;
                            maxProfit = helper;
                            bestAttackTarget = enemyChecker;
                            securedBuildingTile = possibleMove;
                        }
                    }
                }
                if(!attackFound) securedBuildingTile = possibleMove;
            }
        }
        if(securedBuildingTile != null)
        {
            AIMove(unit, securedBuildingTile, mainManager);
            returnValue = true;
        }
        if(bestAttackTarget != null)
        {
            AIAttack(unit, bestAttackTarget, mainManager);
            returnValue = true;
        }
        return returnValue;
    }

    private bool moveOntoEnemyProductionBuilding(Unit unit, SinglePlayerManager mainManager)
    {
        List<DijkstraNode> unitMovementGraph = mainManager.getUnitMovementGraph(unit.GetX(), unit.GetZ());
        DijkstraNode securedBuildingTile = null;
        int maxProfit = 0;
        int helper;
        Unit bestAttackTarget = null;
        bool attackFound = false;
        bool returnValue = false;

        foreach(DijkstraNode possibleMove in unitMovementGraph)
        {
            if(isEnemyProductionBuilding(possibleMove.x, possibleMove.z, unit, mainManager))
            {
                if(!unit.isIndirect())
                {
                    foreach(Unit enemyChecker in mainManager.unitmap.GetEnemyUnitsInRange(possibleMove.x, possibleMove.z, mainManager, unit))
                    {
                        helper = mainManager.getPotentialAttackValue(unit, enemyChecker);
                        if(mainManager.getPotentialCounterattack(unit, enemyChecker) < 50 && helper > maxProfit)
                        {
                            attackFound = true;
                            maxProfit = helper;
                            bestAttackTarget = enemyChecker;
                            securedBuildingTile = possibleMove;
                        }
                    }
                }
                if(!attackFound) securedBuildingTile = possibleMove;
            }
        }
        if(securedBuildingTile != null)
        {
            AIMove(unit, securedBuildingTile, mainManager);
            returnValue = true;
        }
        if(bestAttackTarget != null)
        {
            AIAttack(unit, bestAttackTarget, mainManager);
            returnValue = true;
        }
        return returnValue;
    }

    private bool moveTowardsEnemyProductionBuilding(Unit unit, SinglePlayerManager mainManager)
    {
        List<DijkstraNode> unitMovementGraph = mainManager.getMapReachabilityGraph(unit.GetX(), unit.GetZ());
        List<PathNode> path = new List<PathNode>();
        PathNode selectedTile = null;
        bool returnValue = false;

        foreach(DijkstraNode possibleMove in unitMovementGraph)
        {
            if(isEnemyProductionBuilding(possibleMove.x, possibleMove.z, unit, mainManager))
            {
                path = mainManager.pathfinding.FindPath(unit.GetX(), unit.GetZ(), possibleMove.x, possibleMove.z, unit, mainManager.tilemap, mainManager.unitmap);
                foreach(PathNode node in path)
                {
                    if(node.gCost <= unit.GetMovementDistance() && mainManager.unitmap.GetGrid().GetGridObject(node.x, node.z) == null)
                    {
                        selectedTile = node;
                    }
                }
                if(selectedTile != null) break;
            }
        }
        if(selectedTile != null)
        {
            AIMove(unit, selectedTile, mainManager);
            returnValue = true;
        }
        return returnValue;
    }

    private bool moveTowardsEnemyUnit(Unit unit, SinglePlayerManager mainManager)
    {
        List<DijkstraNode> unitMovementGraph = mainManager.getMapReachabilityGraph(unit.GetX(), unit.GetZ());
        List<PathNode> path = new List<PathNode>();
        PathNode selectedTile = null;
        int maxProfit = 0;
        int helper;
        bool attackFound = false;
        bool returnValue = false;

        foreach(DijkstraNode possibleMove in unitMovementGraph)
        {
            foreach(Unit enemyChecker in mainManager.unitmap.GetEnemyUnitsInRange(possibleMove.x, possibleMove.z, mainManager, unit))
            {
                helper = mainManager.getPotentialAttackValue(unit, enemyChecker);
                if(mainManager.getPotentialCounterattack(unit, enemyChecker) < 50 && helper > maxProfit)
                {
                    attackFound = true;
                    maxProfit = helper;
                }
            }
            if(attackFound)
            {
                path = mainManager.pathfinding.FindPath(unit.GetX(), unit.GetZ(), possibleMove.x, possibleMove.z, unit, mainManager.tilemap, mainManager.unitmap);
                foreach(PathNode node in path)
                {
                    if(node.gCost <= unit.GetMovementDistance() && mainManager.unitmap.GetGrid().GetGridObject(node.x, node.z) == null)
                    {
                        selectedTile = node;
                    }
                }
                if(selectedTile != null) break;
            }
        }
        if(selectedTile != null)
        {
            AIMove(unit, selectedTile, mainManager);
            returnValue = true;
        }
        return returnValue;
    }

    private bool moveTowardsAlliedUnit(Unit unit, SinglePlayerManager mainManager, bool transport)
    {
        List<DijkstraNode> unitMovementGraph = mainManager.getMapReachabilityGraph(unit.GetX(), unit.GetZ());
        List<PathNode> path = new List<PathNode>();
        PathNode selectedTile = null;
        int unitsCount = 0;
        bool returnValue = false;

        foreach(DijkstraNode possibleMove in unitMovementGraph)
        {
            foreach(Unit friendlyChecker in mainManager.unitmap.GetFriendlyUnitsInRange(possibleMove.x, possibleMove.z, unit))
            {
                if(!transport || (transport && (friendlyChecker.GetUnitType() == Unit.UnitType.Infantry || friendlyChecker.GetUnitType() == Unit.UnitType.Mech)))
                {
                    unitsCount++;
                }
            }
            if(unitsCount > 0)
            {
                path = mainManager.pathfinding.FindPath(unit.GetX(), unit.GetZ(), possibleMove.x, possibleMove.z, unit, mainManager.tilemap, mainManager.unitmap);
                foreach(PathNode node in path)
                {
                    if(node.gCost <= unit.GetMovementDistance() && mainManager.unitmap.GetGrid().GetGridObject(node.x, node.z) == null)
                    {
                        selectedTile = node;
                    }
                }
                if(selectedTile != null) break;
            }
        }
        if(selectedTile != null)
        {
            AIMove(unit, selectedTile, mainManager);
            returnValue = true;
        }
        return returnValue;
    }

    private bool moveTowardsNonAlliedBuilding(Unit unit, SinglePlayerManager mainManager)
    {
        //Debug.Log("Trying to move near a building on map with unit " + unit.ToString() + unit.GetX() + unit.GetZ());
        List<DijkstraNode> unitMovementGraph = mainManager.getMapReachabilityGraph(unit.GetX(), unit.GetZ());
        List<PathNode> path = new List<PathNode>();
        PathNode selectedTile = null;
        bool returnValue = false;

        foreach(DijkstraNode possibleMove in unitMovementGraph)
        {
            if(isNonAlliedBuilding(possibleMove.x, possibleMove.z, unit, mainManager))
            {
                path = mainManager.pathfinding.FindPath(unit.GetX(), unit.GetZ(), possibleMove.x, possibleMove.z, unit, mainManager.tilemap, mainManager.unitmap);
                foreach(PathNode node in path)
                {
                    if(node.gCost <= unit.GetMovementDistance() && mainManager.unitmap.GetGrid().GetGridObject(node.x, node.z) == null)
                    {
                        selectedTile = node;
                    }
                }
                if(selectedTile != null) break;
            }
        }
        if(selectedTile != null)
        {
            AIMove(unit, selectedTile, mainManager);
            returnValue = true;
        }
        return returnValue;
    }

    private bool moveTowardsGoalIsland(Unit unit, TileType tile, SinglePlayerManager mainManager, bool hasLoadedUnit)
    {
        List<DijkstraNode> islandTiles;
        if(hasLoadedUnit)
        {
            islandTiles = mainManager.getMapReachabilityFromGoalGraph(tile.GetX(), tile.GetZ(), unit.GetLoadedUnits()[0]);
        }
        else
        {
            islandTiles = mainManager.getMapReachabilityFromGoalGraph(tile.GetX(), tile.GetZ(), mainManager.unitmap.GetGrid().GetGridObject(tile.GetX(), tile.GetZ()));
        }
        List<DijkstraNode> landerMovementGraph = mainManager.getMapReachabilityGraph(unit.GetX(), unit.GetZ());
        List<DijkstraNode> validShores = new List<DijkstraNode>();
        List<PathNode> path = new List<PathNode>();
        PathNode selectedTile = null;
        bool returnValue = false;

        foreach(DijkstraNode possibleMove in landerMovementGraph)
        {
            //Left
            if(possibleMove.x - 1 >= 0 && islandTiles.Contains(mainManager.pathmaking.GetNode(possibleMove.x - 1, possibleMove.z)))
            {
                if(!validShores.Contains(possibleMove)) validShores.Add(possibleMove);
            }
            //Right
            if(possibleMove.x + 1 < mainManager.unitmap.GetGrid().GetWidth() && islandTiles.Contains(mainManager.pathmaking.GetNode(possibleMove.x + 1, possibleMove.z)))
            {
                if(!validShores.Contains(possibleMove)) validShores.Add(possibleMove);
            }
            //Down
            if(possibleMove.z - 1 >= 0 && islandTiles.Contains(mainManager.pathmaking.GetNode(possibleMove.x, possibleMove.z - 1)))
            {
                if(!validShores.Contains(possibleMove)) validShores.Add(possibleMove);
            }
            //Up
            if(possibleMove.z + 1 < mainManager.unitmap.GetGrid().GetHeight() && islandTiles.Contains(mainManager.pathmaking.GetNode(possibleMove.x, possibleMove.z + 1)))
            {
                if(!validShores.Contains(possibleMove)) validShores.Add(possibleMove);
            }
        }

        foreach(DijkstraNode possibleMove in landerMovementGraph)
        {
            if(validShores.Contains(possibleMove))
            {
                path = mainManager.pathfinding.FindPath(unit.GetX(), unit.GetZ(), possibleMove.x, possibleMove.z, unit, mainManager.tilemap, mainManager.unitmap);
                foreach(PathNode node in path)
                {
                    if(node.gCost <= unit.GetMovementDistance() && mainManager.unitmap.GetGrid().GetGridObject(node.x, node.z) == null)
                    {
                        selectedTile = node;
                    }
                }
                if(selectedTile != null) break;
            }
        }
        if(selectedTile != null)
        {
            AIMove(unit, selectedTile, mainManager);
            if(hasLoadedUnit && path.Last() == selectedTile)
            {
                //Left
                if(selectedTile.x - 1 >= 0 && islandTiles.Contains(mainManager.pathmaking.GetNode(selectedTile.x - 1, selectedTile.z)))
                {
                    AIUnload(unit, mainManager.tilemap.GetGrid().GetGridObject(selectedTile.x - 1, selectedTile.z), mainManager);
                }
                //Right
                else if(selectedTile.x + 1 < mainManager.unitmap.GetGrid().GetWidth() && islandTiles.Contains(mainManager.pathmaking.GetNode(selectedTile.x + 1, selectedTile.z)))
                {
                    AIUnload(unit, mainManager.tilemap.GetGrid().GetGridObject(selectedTile.x + 1, selectedTile.z), mainManager);
                }
                //Down
                else if(selectedTile.z - 1 >= 0 && islandTiles.Contains(mainManager.pathmaking.GetNode(selectedTile.x, selectedTile.z - 1)))
                {
                    AIUnload(unit, mainManager.tilemap.GetGrid().GetGridObject(selectedTile.x, selectedTile.z - 1), mainManager);
                }
                //Up
                else if(selectedTile.z + 1 < mainManager.unitmap.GetGrid().GetHeight() && islandTiles.Contains(mainManager.pathmaking.GetNode(selectedTile.x, selectedTile.z + 1)))
                {
                    AIUnload(unit, mainManager.tilemap.GetGrid().GetGridObject(selectedTile.x, selectedTile.z + 1), mainManager);
                }
            }
            returnValue = true;
        }
        return returnValue;
    }

    private TileType ScanForGoal(Unit unit, SinglePlayerManager mainManager)
    {
        TileType target = null;
        for(int x = 0; x < mainManager.tilemap.GetGrid().GetWidth(); x++)
        {
            for(int z = 0; z < mainManager.tilemap.GetGrid().GetHeight(); z++)
            {
                if(isNonAlliedBuilding(x, z, unit, mainManager))
                {
                    if(target == null || (target != null && Mathf.Abs(unit.GetX() - x) + Mathf.Abs(unit.GetZ() - z) < Mathf.Abs(unit.GetX() - target.GetX()) + Mathf.Abs(unit.GetZ() - target.GetZ())))
                    {
                        target = mainManager.tilemap.GetGrid().GetGridObject(x, z);
                    }
                }
            }
        }
        return target;
    }

    private TileType FindStrandedUnit(Unit unit, SinglePlayerManager mainManager)
    {
        Player owner = mainManager.playersInMatch[unit.GetTeam()-1];
        TileType target = null;
        foreach(Unit friendly in owner.GetUnloadedUnits())
        {
            //Debug.Log("Found unit: " + friendly.ToString() + friendly.GetX() + friendly.GetZ());
            if(friendly.GetAIstate() == "ISLAND_STRANDED")
            {
                target = mainManager.tilemap.GetGrid().GetGridObject(friendly.GetX(), friendly.GetZ());
            }
        }

        return target;
    }

    private bool moveToTransportInRange(Unit unit, SinglePlayerManager mainManager)
    {
        List<DijkstraNode> unitMovementGraph = mainManager.getUnitMovementGraph(unit.GetX(), unit.GetZ());
        DijkstraNode transportPosition = null;
        Unit transport = null;
        bool returnValue = false;
        bool loopBreak = false;

        foreach(DijkstraNode possibleMove in unitMovementGraph)
        {
            foreach(Unit friendlyChecker in mainManager.unitmap.GetFriendlyUnitsInRange(possibleMove.x, possibleMove.z, unit))
            {
                if(friendlyChecker.GetLoadCapacity() > 0 && friendlyChecker.GetLoadedUnits()[friendlyChecker.GetLoadCapacity() - 1] == null)
                {
                    transportPosition = possibleMove;
                    transport = friendlyChecker;
                    loopBreak = true;
                    break;
                }
            }
            if(loopBreak) break;
        }
        if(transportPosition != null)
        {
            AIMove(unit, transportPosition, mainManager);
            returnValue = true;
        }
        if(transport != null)
        {
            AILoad(unit, transport, mainManager);
            returnValue = true;
        }
        return returnValue;
    }

    private bool moveTowardsTransport(Unit unit, SinglePlayerManager mainManager)
    {
        List<DijkstraNode> unitMovementGraph = mainManager.getMapReachabilityGraph(unit.GetX(), unit.GetZ());
        List<PathNode> path = new List<PathNode>();
        PathNode selectedTile = null;
        bool returnValue = false;
        bool loopBreak = false;

        foreach(DijkstraNode possibleMove in unitMovementGraph)
        {
            foreach(Unit friendlyChecker in mainManager.unitmap.GetFriendlyUnitsInRange(possibleMove.x, possibleMove.z, unit))
            {
                if(friendlyChecker.GetLoadCapacity() > 0 && friendlyChecker.GetLoadedUnits()[friendlyChecker.GetLoadCapacity() - 1] == null)
                {
                    path = mainManager.pathfinding.FindPath(unit.GetX(), unit.GetZ(), possibleMove.x, possibleMove.z, unit, mainManager.tilemap, mainManager.unitmap);
                    foreach(PathNode node in path)
                    {
                        if(node.gCost <= unit.GetMovementDistance() && mainManager.unitmap.GetGrid().GetGridObject(node.x, node.z) == null)
                        {
                            selectedTile = node;
                        }
                    }
                    if(selectedTile != null)
                    {
                        loopBreak = true;
                        break;
                    }
                }
            }
            if(loopBreak) break;
        }
        if(selectedTile != null)
        {
            AIMove(unit, selectedTile, mainManager);
            returnValue = true;
        }
        return returnValue;
    }

    private bool infantryNonAlliedBuildingDistanceCheck(Unit unit, SinglePlayerManager mainManager)
    {
        List<DijkstraNode> unitMovementGraph = mainManager.getMapReachabilityGraph(unit.GetX(), unit.GetZ());
        List<PathNode> path = new List<PathNode>();

        foreach(DijkstraNode possibleMove in unitMovementGraph)
        {
            if(isNonAlliedBuilding(possibleMove.x, possibleMove.z, unit, mainManager))
            {
                path = mainManager.pathfinding.FindPath(unit.GetX(), unit.GetZ(), possibleMove.x, possibleMove.z, unit, mainManager.tilemap, mainManager.unitmap);
                if(path[path.Count-1].gCost > unit.GetMovementDistance() * 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        return true;
    }

    public List<Unit> sortUnitMovementList(List<Unit> unitsToMove, SinglePlayerManager mainManager)
    {
        List<Unit> sortedList = new List<Unit>();
        int index;
        foreach(Unit unit in unitsToMove)
        {
            index = 0;
            foreach(Unit processedUnit in sortedList)
            {
                if(getBasicPriority(unit, mainManager) > getBasicPriority(processedUnit, mainManager))
                {
                    index++;
                }
            }
            //Debug.Log("Inserting " + unit.getst)
            sortedList.Insert(index, unit);
        }
        return sortedList;
    }

    private int getBasicPriority(Unit unit, SinglePlayerManager mainManager)
    {
        if((unit.GetAIbehaviour() == 1 || unit.GetAIbehaviour() == 3) && isNonAlliedBuilding(unit.GetX(), unit.GetZ(), unit, mainManager) && getOffBuildingIfInfantryNearbyCheck(unit, mainManager))
        {
            return 1;
        }
        if(unit.GetUnitType() == Unit.UnitType.Infantry || unit.GetUnitType() == Unit.UnitType.Mech)
        {
            return 2;
        }
        if(unit.GetUnitType() == Unit.UnitType.APC || unit.GetUnitType() == Unit.UnitType.Theli || unit.GetUnitType() == Unit.UnitType.Tship)
        {
            return 3;
        }
        if(unit.GetMaxRange() > 0)
        {
            return 4;
        }
        return 5;
    }

    private bool isEnemyProductionBuilding(int x, int z, Unit unit, SinglePlayerManager mainManager)
    {
        if((mainManager.tilemap.GetGrid().GetGridObject(x, z).GetType().Equals(typeof(MilitaryBase))
        || mainManager.tilemap.GetGrid().GetGridObject(x, z).GetType().Equals(typeof(Airport))
        || mainManager.tilemap.GetGrid().GetGridObject(x, z).GetType().Equals(typeof(Port)))
        && ((Building)mainManager.tilemap.GetGrid().GetGridObject(x, z)).GetTeam() != unit.GetTeam()
        && !mainManager.CheckAlliance(((Building)mainManager.tilemap.GetGrid().GetGridObject(x, z)).GetTeam(), unit.GetTeam())) return true;
        return false;
    }

    private bool isNonAlliedBuilding(int x, int z, Unit unit, SinglePlayerManager mainManager)
    {
        if(mainManager.tilemap.GetGrid().GetGridObject(x, z).GetType().IsSubclassOf(typeof(Building))
        && ((Building)mainManager.tilemap.GetGrid().GetGridObject(x, z)).GetTeam() != unit.GetTeam()
        && !mainManager.CheckAlliance(((Building)mainManager.tilemap.GetGrid().GetGridObject(x, z)).GetTeam(), unit.GetTeam())) return true;
        return false;
    }

    private bool isAlliedNonProductionBuilding(int x, int z, Unit unit, SinglePlayerManager mainManager)
    {
        if(mainManager.tilemap.GetGrid().GetGridObject(x, z).GetType().IsSubclassOf(typeof(Building))
        && (!mainManager.tilemap.GetGrid().GetGridObject(x, z).GetType().Equals(typeof(MilitaryBase))
        || !mainManager.tilemap.GetGrid().GetGridObject(x, z).GetType().Equals(typeof(Airport))
        || !mainManager.tilemap.GetGrid().GetGridObject(x, z).GetType().Equals(typeof(Port)))
        && ((Building)mainManager.tilemap.GetGrid().GetGridObject(x, z)).GetTeam() == unit.GetTeam()) return true;
        return false;
    }

    private bool isReplacementNotLessImportant(DijkstraNode current, TileType replacement, SinglePlayerManager mainManager)
    {
        if(current == null) return true;
        TileType.TilemapSprite[] order = { TileType.TilemapSprite.Port, TileType.TilemapSprite.Airport, TileType.TilemapSprite.MilitaryBase, TileType.TilemapSprite.HQ };
        int currentInt = 0;
        int replacementInt = 0;
        for(int i = 0; i < order.Length; i++)
        {
            if(mainManager.tilemap.GetGrid().GetGridObject(current.x, current.z).GetTilemapSprite() == order[i]) currentInt = i+1;
            if(replacement.GetTilemapSprite() == order[i]) replacementInt = i+1;
        }
        return currentInt <= replacementInt;
    }

    private bool isReplacementNotLessImportant(TileType current, TileType replacement, SinglePlayerManager mainManager)
    {
        if(current == null) return true;
        TileType.TilemapSprite[] order = { TileType.TilemapSprite.Port, TileType.TilemapSprite.Airport, TileType.TilemapSprite.MilitaryBase, TileType.TilemapSprite.HQ };
        int currentInt = 0;
        int replacementInt = 0;
        for(int i = 0; i < order.Length; i++)
        {
            if(current.GetTilemapSprite() == order[i]) currentInt = i+1;
            if(replacement.GetTilemapSprite() == order[i]) replacementInt = i+1;
        }
        return currentInt <= replacementInt;
    }

    private List<TileType> GetNeighbourNonAlliedBuildings(int x, int z, Unit unit, SinglePlayerManager mainManager)
    {
        List<TileType> neighbourList = new List<TileType>();

        //Left
        if(x - 1 >= 0 && isNonAlliedBuilding(x - 1, z, unit, mainManager)) neighbourList.Add(mainManager.tilemap.GetGrid().GetGridObject(x - 1, z));
        //Right
        if(x + 1 < mainManager.tilemap.GetGrid().GetWidth() && isNonAlliedBuilding(x + 1, z, unit, mainManager)) neighbourList.Add(mainManager.tilemap.GetGrid().GetGridObject(x + 1, z));
        //Down
        if(z - 1 >= 0 && isNonAlliedBuilding(x, z - 1, unit, mainManager)) neighbourList.Add(mainManager.tilemap.GetGrid().GetGridObject(x, z - 1));
        //Up
        if(z + 1 < mainManager.tilemap.GetGrid().GetHeight() && isNonAlliedBuilding(x, z + 1, unit, mainManager)) neighbourList.Add(mainManager.tilemap.GetGrid().GetGridObject(x, z + 1));
    
        return neighbourList;
    }

    //---AI BEHAVIOUR STATES---
    public void StationaryBehaviour(Unit unit)
    {
        SinglePlayerManager mainManager = mainScriptHolder.GetComponent<SinglePlayerManager>();

        //Condition #1 - enemy unit is in range and attacking it is a favourable trade, will attack (and move if necessary)
        if(unit.GetAmmo() > 0)
        {
            if(!unit.isIndirect())
            {
                attackEnemiesInRangeMobile(unit, mainManager);
            }
            else
            {
                attackEnemiesInRangeStationary(unit, mainManager);
            }
        }

        //Condition #2 - Default, will not move or perform any actions
        //Yep, literally nothing
    }

    public void OccupyProductionBehaviour(Unit unit)
    {
        SinglePlayerManager mainManager = mainScriptHolder.GetComponent<SinglePlayerManager>();
        bool checker = false;

        //Condition #1 - Is already occupying an enemy production building, will move out of the way for nearby infantry or will attack without moving if target is available and no infantry found
        if(isEnemyProductionBuilding(unit.GetX(), unit.GetZ(), unit, mainManager))
        {
            checker = getOffBuildingIfInfantryNearby(unit, mainManager);
            if(checker) return;

            if(unit.GetAmmo() > 0)
            {
                attackEnemiesInRangeStationary(unit, mainManager);
            }
            return;
        }

        //Condition #2 - If has unoccupied enemy production building in range, will move on top of it and attack neighbouring enemies if possible
        checker = moveOntoEnemyProductionBuilding(unit, mainManager);
        if(checker) return;

        //Condition #3 - If has enemies in movement+attack range, will attack (and move if necessary)
        if(unit.GetAmmo() > 0)
        {
            if(!unit.isIndirect())
            {
                checker = attackEnemiesInRangeMobile(unit, mainManager);
            }
            else
            {
                checker = attackEnemiesInRangeStationary(unit, mainManager);
            }
            if(checker) return;
        }

        //Condition #4 - If an unoccupied enemy production building exists on the map, will move towards the closest available one
        checker = moveTowardsEnemyProductionBuilding(unit, mainManager);
        if(checker) return;

        //Condition #5 - No above condition met, will default to moving to closest enemy unit while zoning (avoiding getting first striked)
        moveTowardsEnemyUnit(unit, mainManager);
    }

    public void VIPCoverBehaviour(Unit unit)
    {
        SinglePlayerManager mainManager = mainScriptHolder.GetComponent<SinglePlayerManager>();
        bool condition1checker = false;

        //Condition #1 - if is a land or an air unit, will check for infantry/mech in range and move next to them. Prioritizes position with more neighbouring infantry/mech
        //if is a naval unit (easier to if check), will check for transport ships in range and move next to them. Prioritizes position with more neighbouring transport ships
        //Will attack enemy units if available from new position. Movement is prioritized, so indirect units won't attack unless stationary this turn
        if(unit.GetPureMovementType() == Unit.MovementType.Ship)
        {
            condition1checker = moveToVIPsInRange(unit, mainManager, false, false);
        }
        else
        {
            condition1checker = moveToVIPsInRange(unit, mainManager, true, false);
        }
        if(condition1checker) return;

        //Condition #2 - If no VIPs in range, will default to aggressive behaviour
        AggressiveBehaviour(unit);
    }

    public void SecureBuildingBehaviour(Unit unit)
    {
        SinglePlayerManager mainManager = mainScriptHolder.GetComponent<SinglePlayerManager>();
        bool checker = false;

        //Condition #1 - is already securing a non-allied building, will attack enemies in range without moving
        if(isNonAlliedBuilding(unit.GetX(), unit.GetZ(), unit, mainManager))
        {
            checker = getOffBuildingIfInfantryNearby(unit, mainManager);
            if(checker) return;
            
            if(unit.GetAmmo() > 0)
            {
                attackEnemiesInRangeStationary(unit, mainManager);
            }
            return;
        }

        //Condition #2 - if non-allied buildings are in range, will move onto one with best enemy engagement, otherwise the last checked one
        checker = moveOntoNonAlliedBuilding(unit, mainManager);
        if(checker) return;

        //Condition #3 - if non-allied non-secured buildings not found within range, will move to the nearest one outside of range using A*
        //It would be super sick if i could implement zoning function
        checker = moveTowardsNonAlliedBuilding(unit, mainManager);
        if(checker) return;

        //Condition #4 - if non-allied non-secured buildings not found on map, defaults to aggressive behaviour as a cleanup duty
        AggressiveBehaviour(unit);
    }

    public void AggressiveBehaviour(Unit unit)
    {
        SinglePlayerManager mainManager = mainScriptHolder.GetComponent<SinglePlayerManager>();
        bool checker = false;

        //Condition #1 - if enemy is within range, will attack unit which yields most profit
        if(unit.GetAmmo() > 0)
        {
            if(!unit.isIndirect())
            {
                checker = attackEnemiesInRangeMobile(unit, mainManager);
            }
            else
            {
                checker = attackEnemiesInRangeStationary(unit, mainManager);
            }
            if(checker) return;
        }

        //Condition #2 - if enemy is outside of range, will move towards nearest enemy unit on map using A*
        moveTowardsEnemyUnit(unit, mainManager);

        //Condition #3 - if no valid enemies found on map, will remain stationary
        //Yep, literally nothing
    }

    public void DefensiveBehaviour(Unit unit)
    {
        SinglePlayerManager mainManager = mainScriptHolder.GetComponent<SinglePlayerManager>();
        bool checker = false;

        //Condition #1 - if allied units in range, will move to a tile with most neighbouring allied units
        //if neighbouring allied units count is the same, will prioritize tiles with the best enemy engagement
        checker = moveToVIPsInRange(unit, mainManager, false, true);
        if(checker) return;

        //Condition #2 - if already on allied non-production building, will remain stationary and attack if possible
        if(isAlliedNonProductionBuilding(unit.GetX(), unit.GetZ(), unit, mainManager))
        {
            attackEnemiesInRangeStationary(unit, mainManager);
            return;
        }

        //Condition #3 - if allied non-production building is in range, will move onto one with best enemy engagement, otherwise the last checked one
        checker = moveOntoAlliedNonProductionBuilding(unit, mainManager);
        if(checker) return;

        //Condition #4 - Default defensive behaviour, will move towards nearest reachable unit on map with A*. If none exists, will remain stationary
        moveTowardsAlliedUnit(unit, mainManager, false);
    }

    public void InfantryBehaviour(Unit unit)
    {
        SinglePlayerManager mainManager = mainScriptHolder.GetComponent<SinglePlayerManager>();
        bool checker = false;

        //Condition #1 - if currently on capturable building (only possible if tried to capture previous turn and was damaged), will continue to capture
        if(isNonAlliedBuilding(unit.GetX(), unit.GetZ(), unit, mainManager))
        {
            AICapture(unit, mainManager);
            return;
        }

        //Condition #2 - if capturable building is in range, will move to it and attempt to capture it
        checker = captureBuilding(unit, mainManager);
        if(checker) return;

        //Condition #3 - if no capturable building in range, the nearest one is >2 turns away and transport is in range, load into the transport.
        if(infantryNonAlliedBuildingDistanceCheck(unit, mainManager))
        {
            checker = moveToTransportInRange(unit, mainManager);
            if(checker) return;
        }

        //Condition #4 - if no capturable building in range, the nearest one is 2 turns away, no transport in range but enemy units are in range, attack if favourable
        checker = attackEnemiesInRangeMobile(unit, mainManager);
        if(checker) return;

        //Condition #5 - if no previous condition was true, default to moving towards the nearest capturable building
        checker = moveTowardsNonAlliedBuilding(unit, mainManager);
        if(checker) return;

        unit.SetAIstate("ISLAND_STRANDED");
        moveTowardsTransport(unit, mainManager);
    }

    public void TransportLandBehaviour(Unit unit)
    {
        SinglePlayerManager mainManager = mainScriptHolder.GetComponent<SinglePlayerManager>();
        bool checker = false;

        //Condition #1 - if allied unit with critical ammo/fuel shortage is in range, will move to a tile neighbouring the critical unit and resupply it
        //Secondary objective - will attempt to resupply as many allied units as possible when selecting neighbouring tiles
        checker = moveToCriticalShortageInRange(unit, mainManager);
        if(checker) return;

        //Condition #2 - defaults to air transport behaviour while sticking to its own unit type and its limitations
        TransportAirBehaviour(unit);
    }

    public void TransportAirBehaviour(Unit unit)
    {
        SinglePlayerManager mainManager = mainScriptHolder.GetComponent<SinglePlayerManager>();
        bool checker = false;

        //Condition #1 - if transporting a unit (infantry/mech) and non-allied building in range, will move to a neighbouring tile to the building
        //With multiple buildings, standard type priority (HQ > MilBase > Airport > Port > other) and the furthest of the same type is picked.
        if(unit.GetLoadedUnits()[0] != null)
        {
            checker = moveNextToNonAlliedBuilding(unit, mainManager);
            if(checker) return;
        }

        //Condition #2 - if transporting a unit (infantry/mech) and no non-allied building in range, will move towards the nearest non-allied building on map
        if(unit.GetLoadedUnits()[0] != null)
        {
            checker = moveTowardsNonAlliedBuilding(unit, mainManager);
            if(checker) return;
        }

        //Condition #3 - if not transporting a unit and allied infantry/mech in range, will move to a neighbouring tile to the infantry/mech
        checker = moveToVIPsInRange(unit, mainManager, true, false);
        if(checker) return;

        //Condition #4 - if not transporting a unit and allied infantry/mech not in range, will move towards the nearest infantry/mech on map
        moveTowardsAlliedUnit(unit, mainManager, true);

        //Condition #5 - defaults to stationary if not transporting anything and no infantry/mech on the map
        //Yep, literally nothing
    }

    public void TransportSeaBehaviour(Unit unit)
    {
        SinglePlayerManager mainManager = mainScriptHolder.GetComponent<SinglePlayerManager>();
        bool checker = false;

        //Condition #1 - if transporting a unit (any land unit), will move towards an island that contains non-allied buildings
        if(unit.GetLoadedUnits()[0] != null)
        {
            checker = moveTowardsGoalIsland(unit, ScanForGoal(unit, mainManager), mainManager, true);
            if(checker) return;
        }

        //Condition #2 - if not transporting any unit, will move towards an island that contains stranded units
        TileType stranded = FindStrandedUnit(unit, mainManager);
        if(stranded != null)
        {
            checker = moveTowardsGoalIsland(unit, stranded, mainManager, false);
            if(checker) return;
        }

        //Condition #3 - defaults to stationary if not transporting anything and no land unit is stranded
        //Yep, literally nothing
    }

    //---RECRUIT FUNCTION
    public void AIRecruitUnit(int x, int z, int type, SinglePlayerManager mainManager)
    {
        //Debug.Log("Attempting to recruit from building on coordinates: " + x + "," + z);
        List<Unit> recruits = new List<Unit>();
        Unit currentlySelected = null;
        int team = ((Building)mainManager.tilemap.GetGrid().GetGridObject(x, z)).GetTeam() - 1;
        switch(type)
        {
            case 1:
                //Debug.Log("Selected building is a Military Base");
                recruits = mainManager.militaryBaseRecruits;
                break;
            case 2:
                //Debug.Log("Selected building is an Airport");
                recruits = mainManager.airportRecruits;
                break;
            case 3:
                //Debug.Log("Selected building is a Port");
                recruits = mainManager.portRecruits;
                break;
            default:
                //Debug.Log("Something went wrong when selecting building type");
                recruits = mainManager.militaryBaseRecruits;
                break;
        }

        //Debug.Log("Buying infantry if infantry count " + mainManager.playersInMatch[team].unitTypeCount[9] + " is less than " + minFoot);
        if(type == 1 && 1000 <= mainManager.playersInMatch[team].GetFunds() && mainManager.playersInMatch[team].unitTypeCount[9] < minFoot)
        {
            AIRecruit(Unit.UnitType.Infantry, x, z, team, mainManager);
            //Debug.Log("AI current funds: " + mainManager.playersInMatch[team].GetFunds());
            return;
        }

        foreach(Unit unit in recruits)
        {
            //Debug.Log("Currently checking unit: " + unit.ToString() + ", Current funds: " + mainManager.playersInMatch[team].GetFunds() + ", Current army count: " + mainManager.playersInMatch[team].GetUnits().Count);
            //Debug.Log("Checking if unit cost = " + unit.GetCost() + " is not greater than AI funds = " + mainManager.playersInMatch[team].GetFunds());
            if(unit.GetCost() <= mainManager.playersInMatch[team].GetFunds())
            {
                //Debug.Log("Count of units of this type in army: " + mainManager.playersInMatch[team].unitTypeCount[unit.GetIntFromUnit()] + ". Expected: " + (float)mainManager.playersInMatch[team].GetUnits().Count * ((float)AIPreset[unit.GetIntFromUnit(),5] / 100));
                if(mainManager.playersInMatch[team].unitTypeCount[unit.GetIntFromUnit()] == 0 && AIPreset[unit.GetIntFromUnit(),5] != 0)
                {
                    //Debug.Log("Both current unit type in army is 0 and this unit's rate is not 0");
                    currentlySelected = unit;
                }
                else if((float)mainManager.playersInMatch[team].GetUnits().Count * ((float)AIPreset[unit.GetIntFromUnit(),5] / 100) > mainManager.playersInMatch[team].unitTypeCount[unit.GetIntFromUnit()])
                {
                    //Debug.Log("More units of this type required");
                    if(currentlySelected != null && AIPreset[unit.GetIntFromUnit(),5] >= AIPreset[currentlySelected.GetIntFromUnit(),5])
                    {
                        //Debug.Log("Currently selected's rate: " + AIPreset[currentlySelected.GetIntFromUnit(),5] + ", Checked unit's rate: " + AIPreset[unit.GetIntFromUnit(),5]);
                        currentlySelected = unit;
                    }
                    else if(currentlySelected == null)
                    {
                        //Debug.Log("Currently no selected unit. Assigning the checked one.");
                        currentlySelected = unit;
                    }
                }
            }
            else
            {
                break;
            }
        }

        if(currentlySelected != null)
        {
            AIRecruit(currentlySelected.GetUnitType(), x, z, team, mainManager);
            //Debug.Log("AI current funds: " + mainManager.playersInMatch[team].GetFunds());
        }
    }
}
