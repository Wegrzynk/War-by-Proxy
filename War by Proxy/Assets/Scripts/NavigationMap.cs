using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NavigationMap : MonoBehaviour
{
    [System.Serializable]
    public class MapNode
    {
        public bool isComplete;
        public bool isSideMission;
        public bool isSelected;
        public string missionName;
        public string missionNumber;
        public string missionRating;
        public GameObject mapNodeVisual;
        public List<Neighbour> neighboursList = new List<Neighbour>();

        public class Neighbour
        {
            public MapNode neighbourNode;
            public bool isNext;
            public bool isUnlocked;
            public List<GameObject> pathToNeighbour;
            public char directionToNeighbour;

            public Neighbour(MapNode neighbour, bool isNext, bool isUnlocked, List<GameObject> path, char direction)
            {
                this.neighbourNode = neighbour;
                this.isNext = isNext;
                this.isUnlocked = isUnlocked;
                this.pathToNeighbour = path;
                this.directionToNeighbour = direction;
            }
        }

        public void AddNeighbour(MapNode neighbour, bool next, bool unlocked, List<GameObject> path, char direction)
        {
            this.neighboursList.Add(new Neighbour(neighbour, next, unlocked, path, direction));
        }
    }

    public List<MapNode> completeMap;
    public GameObject paths;
    public List<Color> colors = new List<Color>{ Color.green, Color.cyan, Color.red};
    public MapNode currentlySelected;
    public GameObject missionNamePanel;
    public GameObject missionNumberPanel;
    public GameObject missionRatingPanel;
    public GameObject confirmDialog;
    public GameObject campaignEscapeWindow;

    public void Reset()
    {
        foreach(MapNode node in completeMap)
        {
            node.neighboursList.Clear();
            node.isSelected = false;
        }
        currentlySelected = completeMap[0];
        completeMap[0].isSelected = true;
    }

    public void InitLoad()
    {
        completeMap[0].AddNeighbour(completeMap[1], true, true, GetPath(0, 1), 'E');
        completeMap[1].AddNeighbour(completeMap[0], false, true, GetPath(1, 0), 'W');
        completeMap[1].AddNeighbour(completeMap[2], true, true, GetPath(1, 2), 'E');
        completeMap[1].AddNeighbour(completeMap[6], true, true, GetPath(1, 6), 'N');
        completeMap[2].AddNeighbour(completeMap[1], false, true, GetPath(2, 1), 'W');
        completeMap[2].AddNeighbour(completeMap[3], true, true, GetPath(2, 3), 'E');
        completeMap[3].AddNeighbour(completeMap[2], false, true, GetPath(3, 2), 'W');
        completeMap[3].AddNeighbour(completeMap[4], true, true, GetPath(3, 4), 'N');
        completeMap[3].AddNeighbour(completeMap[7], true, true, GetPath(3, 7), 'S');
        completeMap[4].AddNeighbour(completeMap[3], false, true, GetPath(4, 3), 'W');
        completeMap[4].AddNeighbour(completeMap[5], true, true, GetPath(4, 5), 'E');
        completeMap[5].AddNeighbour(completeMap[4], false, true, GetPath(5, 4), 'N');
        completeMap[6].AddNeighbour(completeMap[1], false, true, GetPath(6, 1), 'E');
        completeMap[7].AddNeighbour(completeMap[3], false, true, GetPath(7, 3), 'N');
        currentlySelected = completeMap[0];
        Visualize();
    }

    public void InitNew()
    {
        completeMap[0].AddNeighbour(completeMap[1], true, false, GetPath(0, 1), 'E');
        completeMap[1].AddNeighbour(completeMap[0], false, true, GetPath(1, 0), 'W');
        completeMap[1].AddNeighbour(completeMap[2], true, false, GetPath(1, 2), 'E');
        completeMap[1].AddNeighbour(completeMap[6], true, false, GetPath(1, 6), 'N');
        completeMap[2].AddNeighbour(completeMap[1], false, true, GetPath(2, 1), 'W');
        completeMap[2].AddNeighbour(completeMap[3], true, false, GetPath(2, 3), 'E');
        completeMap[3].AddNeighbour(completeMap[2], false, true, GetPath(3, 2), 'W');
        completeMap[3].AddNeighbour(completeMap[4], true, false, GetPath(3, 4), 'N');
        completeMap[3].AddNeighbour(completeMap[7], true, false, GetPath(3, 7), 'S');
        completeMap[4].AddNeighbour(completeMap[3], false, true, GetPath(4, 3), 'W');
        completeMap[4].AddNeighbour(completeMap[5], true, false, GetPath(4, 5), 'E');
        completeMap[5].AddNeighbour(completeMap[4], false, true, GetPath(5, 4), 'N');
        completeMap[6].AddNeighbour(completeMap[1], false, true, GetPath(6, 1), 'E');
        completeMap[7].AddNeighbour(completeMap[3], false, true, GetPath(7, 3), 'N');
        currentlySelected = completeMap[0];
        Visualize();
    }

    public List<GameObject> GetPath(int start, int end)
    {
        List<GameObject> pathList = new List<GameObject>();
        for(int i = 0; i < paths.transform.childCount; i++)
        {
            if(paths.transform.GetChild(i).name == "Path_" + start + "_" + end)
            {
                pathList.Add(paths.transform.GetChild(i).gameObject);
            }
        }
        return pathList;
    }

    public void Visualize()
    {
        foreach(MapNode node in completeMap)
        {
            if(node.isComplete)
            {
                node.mapNodeVisual.transform.Find("Panel").GetComponent<Image>().color = colors[0];
            }
            else if(node.isSideMission)
            {
                node.mapNodeVisual.transform.Find("Panel").GetComponent<Image>().color = colors[1];
            }
            else
            {
                node.mapNodeVisual.transform.Find("Panel").GetComponent<Image>().color = colors[2];
            }

            if(node.isSelected)
            {
                node.mapNodeVisual.transform.Find("SelectedBorder").gameObject.SetActive(true);
            }
            else
            {
                node.mapNodeVisual.transform.Find("SelectedBorder").gameObject.SetActive(false);
            }

            foreach(MapNode.Neighbour neighbour in node.neighboursList)
            {
                if(!neighbour.isNext)
                {
                    continue;
                }
                else if(neighbour.isUnlocked)
                {
                    neighbour.neighbourNode.mapNodeVisual.SetActive(true);
                    foreach(GameObject path in neighbour.pathToNeighbour)
                    {
                        path.SetActive(true);
                    }
                }
                else
                {
                    neighbour.neighbourNode.mapNodeVisual.SetActive(false);
                    foreach(GameObject path in neighbour.pathToNeighbour)
                    {
                        path.SetActive(false);
                    }
                }
            }
        }

        missionNamePanel.GetComponent<TextMeshProUGUI>().text = currentlySelected.missionName;
        missionNumberPanel.GetComponent<TextMeshProUGUI>().text = currentlySelected.missionNumber;
        missionRatingPanel.GetComponent<TextMeshProUGUI>().text = currentlySelected.missionRating;
    }

    public void StartMission(MapNode selected)
    {
        Debug.Log("Starting Mission " + selected.missionNumber + ": " + selected.missionName);
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
            foreach(MapNode.Neighbour neighbour in currentlySelected.neighboursList)
            {
                if(neighbour.directionToNeighbour == 'N' && neighbour.isUnlocked)
                {
                    currentlySelected.isSelected = false;
                    currentlySelected = neighbour.neighbourNode;
                    currentlySelected.isSelected = true;
                    Visualize();
                }
            }
        }
        if(Input.GetKeyDown(KeyCode.A))
        {
            foreach(MapNode.Neighbour neighbour in currentlySelected.neighboursList)
            {
                if(neighbour.directionToNeighbour == 'W' && neighbour.isUnlocked)
                {
                    currentlySelected.isSelected = false;
                    currentlySelected = neighbour.neighbourNode;
                    currentlySelected.isSelected = true;
                    Visualize();
                }
            }
        }
        if(Input.GetKeyDown(KeyCode.S))
        {
            foreach(MapNode.Neighbour neighbour in currentlySelected.neighboursList)
            {
                if(neighbour.directionToNeighbour == 'S' && neighbour.isUnlocked)
                {
                    currentlySelected.isSelected = false;
                    currentlySelected = neighbour.neighbourNode;
                    currentlySelected.isSelected = true;
                    Visualize();
                }
            }
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            foreach(MapNode.Neighbour neighbour in currentlySelected.neighboursList)
            {
                if(neighbour.directionToNeighbour == 'E' && neighbour.isUnlocked)
                {
                    currentlySelected.isSelected = false;
                    currentlySelected = neighbour.neighbourNode;
                    currentlySelected.isSelected = true;
                    Visualize();
                }
            }
        }
        if(Input.GetKeyDown(KeyCode.Return))
        {
            confirmDialog.SetActive(true);
            confirmDialog.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = "Begin Mission " + currentlySelected.missionNumber + ": " + currentlySelected.missionName;
            confirmDialog.transform.Find("confirmButton").GetComponent<Button>().onClick.RemoveAllListeners();
            confirmDialog.transform.Find("confirmButton").GetComponent<Button>().onClick.AddListener(() => StartMission(currentlySelected));
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            campaignEscapeWindow.SetActive(true);
        }
    }
}
