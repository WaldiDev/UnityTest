using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scenemanager : MonoBehaviour {
    
    int seed;
    const int mazeSize = 20;

    // Prefabs for room types
    public GameObject deadend;
    public GameObject passage;
    public GameObject curve;
    public GameObject tpiece;
    public GameObject cross;

    // Connection list contains information about the connections for each room.
    // This is computed once upon initializing this object.
    NodeConnections[,] connectionList = new NodeConnections[mazeSize, mazeSize];
    GameObject[,] tiles = new GameObject[mazeSize, mazeSize];

    public GameObject playerPrefab;
    GameObject player;
    Vector3 previousPosition;
    Vector2 currentPosition;

    enum NodeState
    {
        FRONTIER,
        IN,
        OUT
    }

    struct Node
    {
        public Vector2 position;
        public Vector2 visitedBy;
        public NodeState state;
    }

    struct NodeConnections
    {
        public bool l, r, u, d;
    }


    // Use this for initialization
    void Start () {
        seed = StaticObjectScript.seed;
	    if(seed < 0)
        {
            seed = Random.Range(mazeSize* mazeSize, mazeSize* mazeSize*10);
        }
        else if(seed < (mazeSize* mazeSize))
        {
            seed = (seed + 1) * (mazeSize* mazeSize);
        }

        GenerateMaze();

        player = (GameObject)Instantiate(playerPrefab, new Vector3(0.0f, 0.5f, 0.0f), Quaternion.Euler(0.0f, 0.0f, 0.0f));
        previousPosition = tiles[0, 0].transform.position;
        currentPosition = Vector2.zero;
	}

    List<Vector2> GetNeighbors(int x, int y)
    {
        List<Vector2> res = new List<Vector2>();
        if (x - 1 >= 0) res.Add(new Vector2(x - 1, y));
        if (x + 1 < mazeSize) res.Add(new Vector2(x + 1, y));
        if (y - 1 >= 0) res.Add(new Vector2(x, y - 1));
        if (y + 1 < mazeSize) res.Add(new Vector2(x, y + 1));
        return res;
    }

    void Mark(int x, int y, ref List<Node> frontier, ref Node[,] maze)
    {
        maze[x, y].state = NodeState.IN;

        var neighbors = GetNeighbors(x, y);
        foreach(var neighbor in neighbors)
        {
            if(maze[(int)neighbor.x, (int)neighbor.y].state == NodeState.OUT)
            {
                maze[(int)neighbor.x, (int)neighbor.y].state = NodeState.FRONTIER;
                frontier.Add(maze[(int)neighbor.x, (int)neighbor.y]);
            }
        }
    }

    void GenerateMaze()
    {
        // Array of Nodes for building the maze with Prim's algorithm
        Node[,] maze = new Node[mazeSize, mazeSize];

        for (int i = 0; i < mazeSize; ++i)
        {
            for(int j = 0; j < mazeSize; ++j)
            {
                maze[i, j].position = new Vector2(i, j);
                maze[i, j].state = NodeState.OUT;
                maze[i, j].visitedBy = new Vector2(-1, -1);
            }
        }

        int start = seed % (mazeSize* mazeSize);
        int startx = (int)Mathf.Floor(start / mazeSize);
        int starty = start % mazeSize;
        
        List<Node> frontier = new List<Node>();

        Mark(startx, starty, ref frontier, ref maze);

        List<Vector2> tempNeighbors = new List<Vector2>();
        while(frontier.Count > 0)
        {
            tempNeighbors.Clear();
            int seedVal = seed % frontier.Count;
            Node currentNode = frontier[seedVal];
            // mark one frontier node as IN, mark neighbors as frontier if out
            Mark((int)currentNode.position.x, (int)currentNode.position.y, ref frontier, ref maze);

            // get connection
            var neighbors = GetNeighbors((int)currentNode.position.x, (int)currentNode.position.y);
            foreach(var neighbor in neighbors)
            {
                if (maze[(int)neighbor.x, (int)neighbor.y].state == NodeState.IN)
                {
                    tempNeighbors.Add(neighbor);
                }
            }

            maze[(int)currentNode.position.x, (int)currentNode.position.y].visitedBy = tempNeighbors[seedVal % tempNeighbors.Count];

            // remove from frontier
            frontier.RemoveAt(seedVal);
        }
        

        for (int i = 0; i < mazeSize; ++i)
        {
            for (int j = 0; j < mazeSize; ++j)
            {
                connectionList[i, j].u = false;
                connectionList[i, j].d = false;
                connectionList[i, j].l = false;
                connectionList[i, j].r = false;
            }
        }

        // Process information of each Node to create the connection list, which persists through the object lifetime.
        foreach (Node node in maze)
        {
            Vector2 pos = node.position;
            Vector2 visitedBy = node.visitedBy;

            // start has negative visitedby value
            if(visitedBy.x < 0)
            {
                continue;
            }

            Vector2 dif = pos - visitedBy;

            if(dif.x == 1)
            {
                //up
                connectionList[(int)pos.x, (int)pos.y].u = true;
                connectionList[(int)visitedBy.x, (int)visitedBy.y].d = true;
            }
            else if(dif.x == -1)
            {
                //down
                connectionList[(int)pos.x, (int)pos.y].d = true;
                connectionList[(int)visitedBy.x, (int)visitedBy.y].u = true;
            }
            else if(dif.y == 1)
            {
                //left
                connectionList[(int)pos.x, (int)pos.y].l = true;
                connectionList[(int)visitedBy.x, (int)visitedBy.y].r = true;
            }
            else if(dif.y == -1)
            {
                //right
                connectionList[(int)pos.x, (int)pos.y].r = true;
                connectionList[(int)visitedBy.x, (int)visitedBy.y].l = true;
            }
        }
        
        // Create first 3 tiles in the bottom left corner, which is the default player start point.
        InstantiateTile(new Vector2(0, 0));
        InstantiateTile(new Vector2(0, 1));
        InstantiateTile(new Vector2(1, 0));
    }

    // Method used to spawn a tile. Decides which kind of tile and what orientation it has.
    void InstantiateTile(Vector2 pos)
    {
        int i = (int)pos.x;
        int j = (int)pos.y;
        var c = connectionList[i, j];

        int count = 0;
        if (c.u) { ++count; }
        if (c.d) { ++count; }
        if (c.l) { ++count; }
        if (c.r) { ++count; }

        // filter which kind of room the currently processed node
        switch (count)
        {
            case 4:
                //crossing                    
                tiles[i, j] = Instantiate(cross);
                break;
            case 3:
                //t crossing -> get direction
                tiles[i, j] = Instantiate(tpiece);
                if (!c.u)
                {
                    tiles[i, j].transform.Rotate(new Vector3(0, 0, 0));
                }
                else if (!c.r)
                {
                    tiles[i, j].transform.Rotate(new Vector3(0, 90, 0));
                }
                else if (!c.d)
                {
                    tiles[i, j].transform.Rotate(new Vector3(0, 180, 0));
                }
                else if (!c.l)
                {
                    tiles[i, j].transform.Rotate(new Vector3(0, 270, 0));
                }
                break;
            case 2:
                //curve or passage
                if (c.d && c.u)
                {
                    // vertical passage
                    tiles[i, j] = Instantiate(passage);
                    tiles[i, j].transform.Rotate(new Vector3(0, 90, 0));
                }
                else if (c.r && c.l)
                {
                    // horizontal passage
                    tiles[i, j] = Instantiate(passage);
                }
                else if (c.r && c.u)
                {
                    tiles[i, j] = Instantiate(curve);
                    tiles[i, j].transform.Rotate(new Vector3(0, 270, 0));
                }
                else if (c.r && c.r)
                {
                    tiles[i, j] = Instantiate(curve);
                    tiles[i, j].transform.Rotate(new Vector3(0, 0, 0));
                }
                else if (c.l && c.d)
                {
                    tiles[i, j] = Instantiate(curve);
                    tiles[i, j].transform.Rotate(new Vector3(0, 90, 0));
                }
                else if (c.l && c.l)
                {
                    tiles[i, j] = Instantiate(curve);
                    tiles[i, j].transform.Rotate(new Vector3(0, 180, 0));
                }
                break;
            case 1:
                //deadend
                tiles[i, j] = Instantiate(deadend);
                if (c.u)
                {
                    tiles[i, j].transform.Rotate(new Vector3(0, 270, 0));
                }
                else if (c.r)
                {
                    tiles[i, j].transform.Rotate(new Vector3(0, 0, 0));
                }
                else if (c.d)
                {
                    tiles[i, j].transform.Rotate(new Vector3(0, 90, 0));
                }
                else if (c.l)
                {
                    tiles[i, j].transform.Rotate(new Vector3(0, 180, 0));
                }
                break;
            default:
                Debug.Log("Error: no open borders.");
                break;
        }
        tiles[i, j].transform.position = new Vector3(i * cross.GetComponent<Renderer>().bounds.size.x, 0, j * cross.GetComponent<Renderer>().bounds.size.z);
    }

    // Method is called by triggers attached to rooms when a room is entered.
    void TileTriggered()
    {
        Vector3 dif = player.transform.position - previousPosition;

        int curx = (int)currentPosition.x, cury = (int)currentPosition.y;

        // Check in which direction the player crossed from one tile to another.
        if (dif.x <= -4.5)
        {
            // Destroy distant tiles
            if(curx+1 < mazeSize)
            {
                Destroy(tiles[curx + 1, cury]);
            }
            if(cury+1 < mazeSize)
            {
                Destroy(tiles[curx, cury + 1]);
            }
            if(cury-1 >= 0)
            {
                Destroy(tiles[curx, cury - 1]);
            }
            --currentPosition.x;
            --curx;
            // Instantiate new close tiles
            if (curx - 1 >= 0)
            {
                InstantiateTile(new Vector2(curx - 1, cury));
            }
            if (cury + 1 < mazeSize)
            {
                InstantiateTile(new Vector2(curx, cury + 1));
            }
            if (cury - 1 >= 0)
            {
                InstantiateTile(new Vector2(curx, cury - 1));
            }
        }
        else if (dif.x >= 4.5)
        {

            // Destroy distant tiles
            if (curx - 1 >= 0)
            {
                Destroy(tiles[curx - 1, cury]);
            }
            if (cury + 1 < mazeSize)
            {
                Destroy(tiles[curx, cury + 1]);
            }
            if (cury - 1 >= 0)
            {
                Destroy(tiles[curx, cury - 1]);
            }
            ++currentPosition.x;
            ++curx;
            // Instantiate new close tiles
            if (curx + 1 < mazeSize)
            {
                InstantiateTile(new Vector2(curx + 1, cury));
            }
            if (cury + 1 < mazeSize)
            {
                InstantiateTile(new Vector2(curx, cury + 1));
            }
            if (cury - 1 >= 0)
            {
                InstantiateTile(new Vector2(curx, cury - 1));
            }
        }
        else if (dif.z <= -4.5)
        {

            // Destroy distant tiles
            if (cury + 1 < mazeSize)
            {
                Destroy(tiles[curx, cury + 1]);
            }
            if (curx + 1 < mazeSize)
            {
                Destroy(tiles[curx + 1, cury]);
            }
            if (curx - 1 >= 0)
            {
                Destroy(tiles[curx - 1, cury]);
            }
            --currentPosition.y;
            --cury;
            // Instantiate new close tiles
            if (cury - 1 >= 0)
            {
                InstantiateTile(new Vector2(curx, cury - 1));
            }
            if (curx + 1 < mazeSize)
            {
                InstantiateTile(new Vector2(curx + 1, cury));
            }
            if (curx - 1 >= 0)
            {
                InstantiateTile(new Vector2(curx - 1, cury));
            }
        }
        else if (dif.z >= 4.5)
        {

            // Destroy distant tiles
            if (cury - 1 >= 0)
            {
                Destroy(tiles[curx, cury - 1]);
            }
            if (curx + 1 < mazeSize)
            {
                Destroy(tiles[curx + 1, cury]);
            }
            if (curx - 1 >= 0)
            {
                Destroy(tiles[curx - 1, cury]);
            }
            ++currentPosition.y;
            ++cury;
            // Instantiate new close tiles
            if (cury + 1 < mazeSize)
            {
                InstantiateTile(new Vector2(curx, cury + 1));
            }
            if (curx + 1 < mazeSize)
            {
                InstantiateTile(new Vector2(curx + 1, cury));
            }
            if (curx - 1 >= 0)
            {
                InstantiateTile(new Vector2(curx - 1, cury));
            }
        }
        previousPosition = tiles[(int)currentPosition.x, (int)currentPosition.y].transform.position;
    }
	
}
