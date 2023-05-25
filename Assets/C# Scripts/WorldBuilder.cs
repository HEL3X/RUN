using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class WorldBuilder : MonoBehaviour
{
    [SerializeField] public int width = 10;  // Width of the maze (number of cells)
    [SerializeField] public int height = 10; // Height of the maze (number of cells)
    [SerializeField] public float cellSize = 1f; // Size of each cell
    [SerializeField] public GameObject tile;
    public List <NavMeshSurface> surfaces;
    public List<GameObject> tiles = new List<GameObject>();
    public Material matt;

    [SerializeField] public GameObject player;
    [SerializeField] public GameObject monster;
    [SerializeField] public GameObject key;
    [SerializeField] public GameObject hatch;

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
        CreateGrid();
        Build();
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
                if (y < height)
                {
                    directions.Add(0);
                }

                // Add east direction if within bounds
                if (x < width)
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
        wall.transform.localScale = new Vector3(cellSize, cellSize * 10f, 1f);

        // Add NavMeshObstacle component to make the wall not walkable
        NavMeshObstacle obstacle = wall.AddComponent<NavMeshObstacle>();
        obstacle.carving = true;
        wall.layer = 3;
        wall.GetComponent<MeshRenderer>().material = matt;

        return wall;
    }

    GameObject CreateEastWall(int x1, int y1, int x2, int y2)
    {
        Vector3 position = new Vector3((x1 + x2) * cellSize * 0.5f, 0f, (y1 + y2) * cellSize * 0.5f);
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = position;
        wall.transform.rotation = Quaternion.identity;
        wall.transform.localScale = new Vector3(1f, cellSize * 10f, cellSize);

        // Add NavMeshObstacle component to make the wall not walkable
        NavMeshObstacle obstacle = wall.AddComponent<NavMeshObstacle>();
        obstacle.carving = true;
        wall.layer = 3;
        wall.GetComponent<MeshRenderer>().material = matt;


        return wall;
    }

    public void Build()
    {
        for (int i = 0; i < surfaces.Count; i++)
        {
            surfaces[i].BuildNavMesh();
        }
    }


    void CreateGrid()
    {
        for (float i = 0; i < height; i++)
        {
            for (float j = 0; j < width; j++)
            {
                GameObject newTile = GameObject.Instantiate(tile, new Vector3(j*5+2.5f, 0f, i*5+2.5f), Quaternion.identity);
                tiles.Add(newTile);
                tile.active = false;
            }
        }
        spawnRandom();
    }

    public void spawnRandom()
    {
        if (tiles.Count > 0)
        {
            int randomPlayerIndex = Random.Range(0, tiles.Count);
            GameObject randomPlayerTile = tiles[randomPlayerIndex];
            player.transform.position = randomPlayerTile.transform.position;
            tiles.RemoveAt(randomPlayerIndex);

            int randomMonsterIndex = Random.Range(0, tiles.Count);
            GameObject randomMonsterTile = tiles[randomMonsterIndex];
            monster.transform.position = randomMonsterTile.transform.position;
            tiles.RemoveAt(randomMonsterIndex);

            int randomKeyIndex = Random.Range(0, tiles.Count);
            GameObject randomKeyTile = tiles[randomKeyIndex];
            key.transform.position = new Vector3(randomKeyTile.transform.position.x, 2, randomKeyTile.transform.position.z);
            tiles.RemoveAt(randomKeyIndex);

            int randomHatchIndex = Random.Range(0, tiles.Count);
            GameObject randomHatchTile = tiles[randomHatchIndex];
            hatch.transform.position = new Vector3(randomHatchTile.transform.position.x+0.85f, 0.6f, randomHatchTile.transform.position.z+0.85f);
            tiles.RemoveAt(randomHatchIndex);
        }
    }
}
