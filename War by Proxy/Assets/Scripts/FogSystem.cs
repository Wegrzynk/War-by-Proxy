using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogSystem : MonoBehaviour
{
    private GameGrid<FogNode> grid;

    public FogSystem(int width, int height)
    {
        grid = new GameGrid<FogNode>(width, height, 10f, Vector3.zero, false, (GameGrid<FogNode> g, int x, int z) => new FogNode(g, x, z, true));
    }

    public void ObscureFully()
    {
        for (int z = 0; z < grid.GetHeight(); z++)
        {
            for (int x = 0; x < grid.GetWidth(); x++)
            {
                grid.GetGridObject(x, z).SetIsFogged(true);
            }
        }
    }

    public void RevealLocally(Unit revealer)
    {
        int x = revealer.GetX();
        int z = revealer.GetZ();
        int visibility = revealer.GetVision();
        Debug.Log("Revealing for unit " + revealer + x + z);
        int counter = 0;
        for (int i = visibility; i >= -visibility; i--)
        {
            for (int j = counter; j >= -counter; j--)
            {
                if (grid.GetGridObject(x + i, z + j) != null)
                {
                    grid.GetGridObject(x + i, z + j).SetIsFogged(false);
                }
            }
            if (i > 0)
            {
                counter++;
            }
            else
            {
                counter--;
            }
        }
    }

    public GameGrid<FogNode> GetGrid()
    {
        return grid;
    }
}
