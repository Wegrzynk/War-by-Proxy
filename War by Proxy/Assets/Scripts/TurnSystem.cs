using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnSystem
{
    public List<Unit> unitsAwaitingOrders;
    public List<Unit> unitsOrdersReceived;
    public List<Building> buildingsAwaitingOrders;
    public List<Building> buildingsOrdersReceived;

    public TurnSystem()
    {
        this.unitsAwaitingOrders = new List<Unit>();
        this.unitsOrdersReceived = new List<Unit>();
        this.buildingsAwaitingOrders = new List<Building>();
        this.buildingsOrdersReceived = new List<Building>();
    }

    public void TurnInit(Player player)
    {
        unitsAwaitingOrders.Clear();
        unitsOrdersReceived.Clear();
        buildingsAwaitingOrders.Clear();
        buildingsOrdersReceived.Clear();

        unitsAwaitingOrders.AddRange(player.GetUnits());
        buildingsAwaitingOrders.AddRange(player.GetBuildings());
        player.ChangeFunds(buildingsAwaitingOrders.Count * 1000);
        player.SetIsActive(true);
    }

    public void MoveUnitAfterOrder(Unit unit)
    {
        unitsAwaitingOrders.Remove(unit);
        unitsOrdersReceived.Add(unit);
    }

    public void MoveBuildingAfterOrder(Building building)
    {
        buildingsAwaitingOrders.Remove(building);
        buildingsOrdersReceived.Add(building);
    }

    public List<Unit> GetUnitsAwaitingOrders()
    {
        return unitsAwaitingOrders;
    }

    public List<Building> GetBuildingsAwaitingOrders()
    {
        return buildingsAwaitingOrders;
    }

    public List<Unit> GetUnitsOrdersReceived()
    {
        return unitsOrdersReceived;
    }

    public List<Building> GetBuildingsOrdersReceived()
    {
        return buildingsOrdersReceived;
    }
}
