using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    [SerializeField] private List<EnvironmentTile> AccessibleTiles = null;
    [SerializeField] private List<EnvironmentTile> InaccessibleTiles = null;
    [SerializeField] private EnvironmentTile WaterTile = null;
    [SerializeField] private List<EnvironmentTile> WaterTilesStraight = null;
    [SerializeField] private List<EnvironmentTile> WaterTilesCorners = null;
    [SerializeField] private List<EnvironmentTile> WaterTilesInner = null;
    [SerializeField] private float AccessiblePercentage = 0.0f;
    [SerializeField] private Texture2D HeightMap = null;

    private EnvironmentTile[][] mMap;
    private List<EnvironmentTile> mAll;
    private List<EnvironmentTile> mToBeTested;
    private List<EnvironmentTile> mLastSolution;
    private List<EnvironmentTile> mEdges;

    private enum ENbr { Edge, Land, Sea };
    private readonly Vector3 NodeSize = Vector3.one * 9.0f; 
    private const float TileSize = 10.0f;
    private const float TileHeight = 2.5f;
    private Vector2Int Size;

    public EnvironmentTile Start { get; private set; }

    private void Awake()
    {
        mAll = new List<EnvironmentTile>();
        mToBeTested = new List<EnvironmentTile>();
        mEdges = new List<EnvironmentTile>();

        Size = new Vector2Int(HeightMap.width, HeightMap.height);
        Debug.Log(Size);
    }

    // Get all accessible tiles on the outer edges
    public List<EnvironmentTile> GetAvailableEdgeTiles()
    {
        var edges = new List<EnvironmentTile>();

        foreach (var edge in mEdges)
        {
            foreach (var con in edge.Connections)
            {
                if (con.IsAccessible)
                    edges.Add(con);
            }
        }
        
        return edges;
    }

    private void OnDrawGizmos()
    {
        // Draw the environment nodes and connections if we have them
        if (mMap != null)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                for (int y = 0; y < Size.y; ++y)
                {
                    if (mMap[x][y].Connections != null)
                    {
                        for (int n = 0; n < mMap[x][y].Connections.Count; ++n)
                        {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine(mMap[x][y].Position, mMap[x][y].Connections[n].Position);
                        }
                    }

                    // Use different colours to represent the state of the nodes
                    Color c = Color.white;

                    if (!mMap[x][y].IsAccessible)
                    {
                        c = Color.red;
                    }
                    else
                    {
                        if (mLastSolution != null && mLastSolution.Contains( mMap[x][y] ))
                        {
                            c = Color.green;
                        }
                        else if (mMap[x][y].Visited)
                        {
                            c = Color.yellow;
                        }
                    }

                    Gizmos.color = c;
                    Gizmos.DrawWireCube(mMap[x][y].Position, NodeSize);
                }
            }
        }
    }

    private void Generate()
    {
        // Setup the map of the environment tiles according to the specified width and height
        // Generate tiles from the list of accessible and inaccessible prefabs using a random
        // and the specified accessible percentage
        mMap = new EnvironmentTile[Size.x][];

        int halfWidth = Size.x / 2;
        int halfHeight = Size.y / 2;
        Vector3 position = new Vector3( -(halfWidth * TileSize), 0.0f, -(halfHeight * TileSize) );
        Vector2Int startPos = new Vector2Int(halfWidth - 1, halfHeight - 1);

        for (int x = 0; x < Size.x; ++x)
        {
            mMap[x] = new EnvironmentTile[Size.y];

            for (int y = 0; y < Size.y; ++y)
            {
                var pixel = HeightMap.GetPixel(x, y);
                bool start = (x == startPos.x && y == startPos.y);
                bool isAccessible = start || Random.value < AccessiblePercentage;
                bool isWater = pixel.g < 0.5f;
                bool isEdge = pixel.b > 0.5f;

                isAccessible = isWater || isEdge ? false : isAccessible;

                List<EnvironmentTile> tiles = isAccessible ? AccessibleTiles : InaccessibleTiles;
                EnvironmentTile prefab = null;

                if (isEdge)
                {
                    ENbr[][] nbrs = new ENbr[3][];

                    for (int i = -1; i <= 1; ++i)
                    {
                        nbrs[i + 1] = new ENbr[3];

                        for (int j = -1; j <= 1; ++j)
                        {
                            if (HeightMap.GetPixel(x + i, y + j).b > 0.5f)
                                nbrs[i + 1][j + 1] = ENbr.Edge;
                            else if (HeightMap.GetPixel(x + i, y + j).g > 0.5)
                                nbrs[i + 1][j + 1] = ENbr.Land;
                            else
                                nbrs[i + 1][j + 1] = ENbr.Sea;
                        }
                    }

                    if (nbrs[0][1] == ENbr.Edge && nbrs[2][1] == ENbr.Edge && nbrs[1][0] == ENbr.Land)      // Top
                        prefab = WaterTilesStraight[0];
                    else if (nbrs[0][1] == ENbr.Edge && nbrs[2][1] == ENbr.Edge && nbrs[1][2] == ENbr.Land) // Bottom
                        prefab = WaterTilesStraight[2];
                    else if (nbrs[1][0] == ENbr.Edge && nbrs[1][2] == ENbr.Edge && nbrs[0][1] == ENbr.Land) // Right
                        prefab = WaterTilesStraight[1];
                    else if (nbrs[1][0] == ENbr.Edge && nbrs[1][2] == ENbr.Edge && nbrs[2][1] == ENbr.Land) // Left
                        prefab = WaterTilesStraight[3];
                    else if (nbrs[0][1] == ENbr.Edge && nbrs[1][2] == ENbr.Edge && nbrs[1][0] == ENbr.Sea)  // Outer NE
                        prefab = WaterTilesCorners[1];
                    else if (nbrs[0][1] == ENbr.Edge && nbrs[1][0] == ENbr.Edge && nbrs[1][2] == ENbr.Sea)  // Outer SE
                        prefab = WaterTilesCorners[0];
                    else if (nbrs[1][2] == ENbr.Edge && nbrs[2][1] == ENbr.Edge && nbrs[1][0] == ENbr.Sea)  // Outer SW
                        prefab = WaterTilesCorners[2];
                    else if (nbrs[1][0] == ENbr.Edge && nbrs[2][1] == ENbr.Edge && nbrs[1][2] == ENbr.Sea)  // Outer NW
                        prefab = WaterTilesCorners[3];
                    else if (nbrs[0][1] == ENbr.Edge && nbrs[1][2] == ENbr.Edge) // Inner NE
                        prefab = WaterTilesInner[0];
                    else if (nbrs[1][2] == ENbr.Edge && nbrs[2][1] == ENbr.Edge) // Inner NW
                        prefab = WaterTilesInner[1];
                    else if (nbrs[0][1] == ENbr.Edge && nbrs[1][0] == ENbr.Edge) // Inner SW
                        prefab = WaterTilesInner[2];
                    else if (nbrs[1][0] == ENbr.Edge && nbrs[2][1] == ENbr.Edge) // Inner SE
                        prefab = WaterTilesInner[3];
                }
                else
                {
                    prefab = isWater ? WaterTile : tiles[Random.Range(0, tiles.Count)];
                }

                EnvironmentTile tile = Instantiate(prefab, position, Quaternion.identity, transform);

                tile.Position = new Vector3( position.x + (TileSize / 2), TileHeight, position.z + (TileSize / 2));
                tile.IsAccessible = isAccessible;
                tile.gameObject.name = string.Format("Tile({0}, {1})", x, y);

                mMap[x][y] = tile;
                mAll.Add(tile);

                if (isEdge)
                    mEdges.Add(tile);

                if (start)
                {
                    Start = tile;
                }

                position.z += TileSize;
            }

            position.x += TileSize;
            position.z = -(halfHeight * TileSize);
        }
    }

    private void SetupConnections()
    {
        // Currently we are only setting up connections between adjacent nodes
        for (int x = 0; x < Size.x; ++x)
        {
            for (int y = 0; y < Size.y; ++y)
            {
                EnvironmentTile tile = mMap[x][y];
                tile.Connections = new List<EnvironmentTile>();

                if (x > 0)
                {
                    tile.Connections.Add(mMap[x - 1][y]);
                }

                if (x < Size.x - 1)
                {
                    tile.Connections.Add(mMap[x + 1][y]);
                }

                if (y > 0)
                {
                    tile.Connections.Add(mMap[x][y - 1]);
                }

                if (y < Size.y - 1)
                {
                    tile.Connections.Add(mMap[x][y + 1]);
                }
            }
        }
    }

    private float Distance(EnvironmentTile a, EnvironmentTile b)
    {
        // Use the length of the connection between these two nodes to find the distance, this 
        // is used to calculate the local goal during the search for a path to a location
        float result = float.MaxValue;
        EnvironmentTile directConnection = a.Connections.Find(c => c == b);

        if (directConnection != null)
        {
            result = TileSize;
        }

        return result;
    }

    private float Heuristic(EnvironmentTile a, EnvironmentTile b)
    {
        // Use the locations of the node to estimate how close they are by line of sight
        // experiment here with better ways of estimating the distance. This is used  to
        // calculate the global goal and work out the best order to prossess nodes in
        return Vector3.Distance(a.Position, b.Position);
    }

    public void GenerateWorld()
    {
        Generate();
        SetupConnections();
    }

    public void CleanUpWorld()
    {
        if (mMap != null)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                for (int y = 0; y < Size.y; ++y)
                {
                    Destroy(mMap[x][y].gameObject);
                }
            }
        }
    }

    public List<EnvironmentTile> Solve(EnvironmentTile begin, EnvironmentTile destination)
    {
        List<EnvironmentTile> result = null;

        if (begin != null && destination != null)
        {
            // Nothing to solve if there is a direct connection between these two locations
            EnvironmentTile directConnection = begin.Connections.Find(c => c == destination);
            
            if (directConnection == null)
            {
                // Set all the state to its starting values
                mToBeTested.Clear();

                for (int count = 0; count < mAll.Count; ++count)
                {
                    mAll[count].Parent = null;
                    mAll[count].Global = float.MaxValue;
                    mAll[count].Local = float.MaxValue;
                    mAll[count].Visited = false;
                }

                // Setup the start node to be zero away from start and estimate distance to target
                EnvironmentTile currentNode = begin;
                currentNode.Local = 0.0f;
                currentNode.Global = Heuristic(begin, destination);

                // Maintain a list of nodes to be tested and begin with the start node, keep going
                // as long as we still have nodes to test and we haven't reached the destination
                mToBeTested.Add(currentNode);

                while (mToBeTested.Count > 0 && currentNode != destination)
                {
                    // Begin by sorting the list each time by the heuristic
                    mToBeTested.Sort((a, b) => (int)(a.Global - b.Global));

                    // Remove any tiles that have already been visited
                    mToBeTested.RemoveAll(n => n.Visited);

                    // Check that we still have locations to visit
                    if (mToBeTested.Count > 0)
                    {
                        // Mark this note visited and then process it
                        currentNode = mToBeTested[0];
                        currentNode.Visited = true;

                        // Check each neighbour, if it is accessible and hasn't already been 
                        // processed then add it to the list to be tested 
                        for (int count = 0; count < currentNode.Connections.Count; ++count)
                        {
                            EnvironmentTile neighbour = currentNode.Connections[count];

                            if (!neighbour.Visited && neighbour.IsAccessible)
                            {
                                mToBeTested.Add(neighbour);
                            }

                            // Calculate the local goal of this location from our current location and 
                            // test if it is lower than the local goal it currently holds, if so then
                            // we can update it to be owned by the current node instead 
                            float possibleLocalGoal = currentNode.Local + Distance(currentNode, neighbour);
                            if (possibleLocalGoal < neighbour.Local)
                            {
                                neighbour.Parent = currentNode;
                                neighbour.Local = possibleLocalGoal;
                                neighbour.Global = neighbour.Local + Heuristic(neighbour, destination);
                            }
                        }
                    }
                }

                // Build path if we found one, by checking if the destination was visited, if so then 
                // we have a solution, trace it back through the parents and return the reverse route
                if (destination.Visited)
                {
                    result = new List<EnvironmentTile>();
                    EnvironmentTile routeNode = destination;

                    while (routeNode.Parent != null)
                    {
                        result.Add(routeNode);
                        routeNode = routeNode.Parent;
                    }
                    result.Add(routeNode);
                    result.Reverse();

                    Debug.LogFormat("Path Found: {0} steps {1} long", result.Count, destination.Local);
                }
                else
                {
                    Debug.LogWarning("Path Not Found");
                }
            }
            else
            {
                result = new List<EnvironmentTile>();
                result.Add(begin);
                result.Add(destination);
                Debug.LogFormat("Direct Connection: {0} <-> {1} {2} long", begin, destination, TileSize);
            }
        }
        else
        {
            Debug.LogWarning("Cannot find path for invalid nodes");
        }

        mLastSolution = result;

        return result;
    }

    public EnvironmentTile FindNextDirectTile(EnvironmentTile current, EnvironmentTile destination)
    {
        EnvironmentTile closest = null;
        float minDist = float.MaxValue;

        foreach(EnvironmentTile tile in current.Connections)
        {
            float dist = Vector3.Distance(tile.Position, destination.Position);
            
            if (dist < minDist)
            {
                closest = tile;
                minDist = dist;
            }
        }

        return closest;
    }

    public void Harvest(Harvestable tile)
    {
        if (tile == null)
            return;

        var envTile = tile.GetComponent<EnvironmentTile>();
        envTile.IsAccessible = true;

        // Destroy all children to give just environment tile
        for(int i = 0; i < tile.transform.childCount; ++i)
        {
            Destroy(tile.transform.GetChild(i).gameObject);
        }

        // Destroy the harvestable component
        Destroy(tile);
    }
}
