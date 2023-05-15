using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBuilder : MonoBehaviour
{
    public int width = 10;  // Width of the maze (number of cells)
    public int height = 10; // Height of the maze (number of cells)
    public float cellSize = 1f; // Size of each cell

    private Cell[,] cells; // 2D array to store the cells

    // Cell class to represent each cell in the maze
    private class Cell
    {
        public bool visited;
        public GameObject northWall;
        public GameObject eastWall;
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateMaze();
    }

    // Generate the maze using the Binary Tree algorithm
    void GenerateMaze()
    {
        // Initialize the cell grid
        cells = new Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y] = new Cell();
            }
        }

        // Create the maze layout
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Create walls for each cell
                cells[x, y].northWall = CreateNorthWall(x, y, x + 1, y);
                cells[x, y].eastWall = CreateEastWall(x + 1, y, x + 1, y + 1);
  
            }
        }

        // Randomly remove walls using Binary Tree algorithm
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // List of possible directions to remove a wall
                List<int> directions = new List<int>();

                // Add north direction if within bounds
                if (y < height - 1)
                {
                    directions.Add(0);
                }

                // Add east direction if within bounds
                if (x < width - 1)
                {
                    directions.Add(1);
                }

                // Choose a random direction from the available directions
                int direction = directions[Random.Range(0, directions.Count)];

                // Remove the wall in the chosen direction
                if (direction == 0)
                {
                    Destroy(cells[x, y].northWall);
                }
                else if (direction == 1)
                {
                    Destroy(cells[x, y].eastWall);
                }
            }
        }
    }

    // Create a wall between two cells
    GameObject CreateNorthWall(int x1, int y1, int x2, int y2)
    {
        Vector3 position = new Vector3((x1 + x2) * cellSize * 0.5f, 0f, (y1 + y2) * cellSize * 0.5f);
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = position;
        wall.transform.localScale = new Vector3(cellSize, cellSize * 2f, 1f);
        return wall;
    }

    GameObject CreateEastWall(int x1, int y1, int x2, int y2)
    {
        Vector3 position = new Vector3((y1 + y2) * cellSize * 0.5f, 0f, (x1 + x2) * cellSize * 0.5f);
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = position;
        wall.transform.localScale = new Vector3(1f, cellSize * 2f, cellSize);
        return wall;
    }
}
