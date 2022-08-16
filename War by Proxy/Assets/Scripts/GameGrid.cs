using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGrid<GenericGrid>
{
    public event EventHandler<OnGenericGridChangedEventArgs> OnGenericGridChanged;
    public class OnGenericGridChangedEventArgs : EventArgs
    {
        public int x;
        public int z;
    }

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private bool showText;
    private GenericGrid[,] gridArray;
    private TextMesh[,] debugTextArray;

    public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
    {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        transform.Rotate(90.0f, 0.0f, 0.0f, Space.Self);
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return textMesh;
    }

    public GameGrid(int width, int height, float cellSize, Vector3 originPosition, bool showText, Func<GameGrid<GenericGrid>, int, int, GenericGrid> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;
        this.showText = showText;

        gridArray = new GenericGrid[width, height];
        debugTextArray = new TextMesh[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int z = 0; z < gridArray.GetLength(1); z++)
            {
                gridArray[x, z] = createGridObject(this, x, z);
            }
        }

        if (showText)
        {
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int z = 0; z < gridArray.GetLength(1); z++)
                {
                    debugTextArray[x, z] = CreateWorldText(null, gridArray[x, z]?.ToString(), GetDebugLinePosition(x, z) + new Vector3(cellSize, 0, cellSize) * .5f, 9, Color.white, TextAnchor.MiddleCenter, TextAlignment.Left, 5000);
                }
            }


            OnGenericGridChanged += (object sender, OnGenericGridChangedEventArgs eventArgs) =>
            {
                debugTextArray[eventArgs.x, eventArgs.z].text = gridArray[eventArgs.x, eventArgs.z]?.ToString();
            };
        }
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    public Vector3 GetDebugLinePosition(int x, int z)
    {
        return new Vector3(x - 0.5f, 0.25f, z - 0.5f) * cellSize + originPosition;
    }

    public Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x, 0, z) * cellSize + originPosition;
    }

    public void GetXZ(Vector3 worldPosition, out int x, out int z )
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        z = Mathf.FloorToInt((worldPosition - originPosition).z / cellSize);
    }

    public void SetGridObject(int x, int z, GenericGrid value)
    {
        if (x >= 0 && z >= 0 && x < width && z < height)
        {
            gridArray[x, z] = value;
            if (OnGenericGridChanged != null) OnGenericGridChanged(this, new OnGenericGridChangedEventArgs { x = x, z = z });
        }
    }

    public void TriggerGenericGridChanged(int x, int z)
    {
        if (OnGenericGridChanged != null) OnGenericGridChanged(this, new OnGenericGridChangedEventArgs { x = x, z = z });
    }

    public void SetGridObject(Vector3 worldPosition, GenericGrid value)
    {
        int x, z;
        GetXZ(worldPosition, out x, out z);
        SetGridObject(x, z, value);
    }

    public GenericGrid GetGridObject(int x, int z)
    {
        if (x >= 0 && z >= 0 && x < width && z < height)
        {
            return gridArray[x, z];
        }
        else
        {
            return default(GenericGrid);
        }
    }

    public GenericGrid GetGridObject(Vector3 worldPosition)
    {
        int x, z;
        GetXZ(worldPosition, out x, out z);
        return GetGridObject(x, z);
    }
}
