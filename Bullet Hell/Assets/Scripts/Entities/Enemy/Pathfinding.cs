using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private static List<PathInformation> pathingUnits;

    private void Start()
    {
        //FindPathJob findPathJob = new FindPathJob { startPosition = new int2(0, 0), endPosition = new int2(19, 19) };
        //findPathJob.Schedule();

        pathingUnits = new List<PathInformation>();
    }

    private void Update()
    {
        for (int i = pathingUnits.Count - 1; i >= 0; i--)
        {
            if (pathingUnits[i].jobHandle.IsCompleted)
            {
                pathingUnits[i].jobHandle.Complete();
                pathingUnits[i].unit.SetPath(pathingUnits[i].savedPath);
                pathingUnits.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Helper function to calculate the shortest path
    /// </summary>
    /// <param name="startPos">The starting position</param>
    /// <param name="endPos">The ending position</param>
    /// <param name="gridDimensions">The dimensions of the room</param>
    /// <param name="walkabilityGrid">Array created by the room containing the weights of all the tiles</param>
    /// <param name="path">The path reference of the object moving along the path</param>
    public static void CalculatePath(int2 startPos, int2 endPos, int2 gridDimensions, NativeArray<int> walkabilityGrid, BaseEnemy enemy)
    {
        List<NativeList<int2>> pathList = new List<NativeList<int2>>();
        pathList.Add(new NativeList<int2>(Allocator.Persistent));
        FindPathJob findPathJob = new FindPathJob { startPosition = endPos, endPosition = startPos, gridDimensions = gridDimensions, walkabilityGrid = walkabilityGrid, path = pathList[0] };

        PathInformation newUnit = new PathInformation
        {
            savedPath = pathList[0],
            unit = enemy,
            jobHandle = findPathJob.Schedule()
        };
        pathingUnits.Add(newUnit);
    }

    //[BurstCompile]
    private struct FindPathJob : IJob
    {
        public int2 startPosition;
        public int2 endPosition;
        public int2 gridDimensions;
        [ReadOnly] public NativeArray<int> walkabilityGrid;
        public NativeList<int2> path;

        public void Execute()
        {
            // Initialize the grid, will need to be adjusted to calculate off of Justin's prefabs later
            int2 gridSize = gridDimensions;
            NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);

            for (int x = -10; x < gridSize.x - 10; x++)
            {
                for (int y = -10; y < gridSize.y - 10; y++)
                {
                    PathNode pathNode = new PathNode();
                    pathNode.x = x;
                    pathNode.y = y;
                    pathNode.index = CalculateIndex(x, y, gridSize.x);
                    pathNode.gCost = int.MaxValue;
                    pathNode.hCost = CalculateDistanceCost(new int2(x, y), endPosition);
                    pathNode.CalculateFCost();

                    pathNode.isWalkable = walkabilityGrid[CalculateIndex(x, y, gridSize.x)] == 1 ? true : false;
                    pathNode.cameFromNodeIndex = -1;

                    pathNodeArray[pathNode.index] = pathNode;
                }
            }

            NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Temp);
            neighbourOffsetArray[0] = new int2(-1, 0); // Left
            neighbourOffsetArray[1] = new int2(1, 0); // Right
            neighbourOffsetArray[2] = new int2(0, -1); // Down
            neighbourOffsetArray[3] = new int2(0, 1); // Up
            neighbourOffsetArray[4] = new int2(-1, -1); // Left Down
            neighbourOffsetArray[5] = new int2(-1, 1); // Left Up
            neighbourOffsetArray[6] = new int2(1, -1); // Right Down
            neighbourOffsetArray[7] = new int2(1, 1); // Right Up

            PathNode startNode = pathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridSize.x)];
            startNode.gCost = 0;
            startNode.CalculateFCost();
            pathNodeArray[startNode.index] = startNode;

            int endNodeIndex = CalculateIndex(endPosition.x, endPosition.y, gridSize.x);

            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            NativeList<int> closedList = new NativeList<int>(Allocator.Temp);
            openList.Add(startNode.index);

            while (openList.Length > 0)
            {
                int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodeArray);
                PathNode currentNode = pathNodeArray[currentNodeIndex];

                if (currentNodeIndex == endNodeIndex)
                {
                    // Reached end node
                    break;
                }

                // Remvoe current node from open list
                for (int i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == currentNodeIndex)
                    {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }
                closedList.Add(currentNodeIndex);

                for (int i = 0; i < neighbourOffsetArray.Length; i++)
                {
                    int2 neighbourOffset = neighbourOffsetArray[i];
                    int2 neighbourPosition = new int2(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y);

                    if (!IsPositionInsideGrid(neighbourPosition, gridSize))
                    {
                        // Neighbour not valid position
                        continue;
                    }

                    int neighbourNodeIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y, gridSize.x);

                    if (closedList.Contains(neighbourNodeIndex))
                    {
                        // Already searched this node
                        continue;
                    }

                    PathNode neighbourNode = pathNodeArray[neighbourNodeIndex];
                    if (!neighbourNode.isWalkable)
                    {
                        // Not walkable
                        continue;
                    }

                    int2 currentNodePosition = new int2(currentNode.x, currentNode.y);

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePosition, neighbourPosition);
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.cameFromNodeIndex = currentNodeIndex;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.CalculateFCost();
                        pathNodeArray[neighbourNodeIndex] = neighbourNode;

                        if (!openList.Contains(neighbourNode.index))
                        {
                            openList.Add(neighbourNode.index);
                        }
                    }
                }
            }

            PathNode endNode = pathNodeArray[endNodeIndex];
            if (endNode.cameFromNodeIndex == -1)
            {
                // Didn't find a path
            }
            else
            {
                // Found a path
                CalculatePath(pathNodeArray, endNode, path);
            }

            pathNodeArray.Dispose();
            neighbourOffsetArray.Dispose();
            openList.Dispose();
            closedList.Dispose();
        }

        /// <summary>
        /// Helper function to calculate the 1D index of the node on the grid
        /// </summary>
        /// <param name="x">X position of the node on the grid</param>
        /// <param name="y">Y position of the node on the grid</param>
        /// <param name="gridWidth">Width of the grid</param>
        /// <returns></returns>
        private int CalculateIndex(int x, int y, int gridWidth)
        {
            return (x + 10) + (y + 10) * gridWidth;
        }

        /// <summary>
        /// Calculate's the h cost of one node to another
        /// </summary>
        /// <param name="aPosition">The starting position</param>
        /// <param name="bPosition">The ending position</param>
        /// <returns></returns>
        private int CalculateDistanceCost(int2 aPosition, int2 bPosition)
        {
            int xDistance = math.abs(aPosition.x - bPosition.x);
            int yDistance = math.abs(aPosition.y - bPosition.y);
            int remaining = math.abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        /// <summary>
        /// Returns the node with the lowest f cost in the open nodes list
        /// </summary>
        /// <param name="openList">List of open nodes</param>
        /// <param name="pathNodeArray">Array of nodes</param>
        /// <returns>Index of the node with the lowest f cost</returns>
        private int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
        {
            PathNode lowestCostPathNode = pathNodeArray[openList[0]];
            for (int i = 1; i < openList.Length; i++)
            {
                PathNode testPathNode = pathNodeArray[openList[i]];
                if (testPathNode.fCost < lowestCostPathNode.fCost)
                {
                    lowestCostPathNode = testPathNode;
                }
            }

            return lowestCostPathNode.index;
        }

        /// <summary>
        /// Helper function to determine if the grid position is within the grid bounds
        /// </summary>
        /// <param name="gridPosition">Position of the grid</param>
        /// <param name="gridSize">Bounds of the grid being checked</param>
        /// <returns></returns>
        private bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize)
        {
            return
                gridPosition.x >= -9 &&
                gridPosition.y >= -9 &&
                gridPosition.x < gridSize.x - 10 &&
                gridPosition.y < gridSize.y - 10;
        }

        private void CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode, NativeList<int2> outputPath)
        {
            if (endNode.cameFromNodeIndex != -1)

                // Found a path
                outputPath.Add(new int2(endNode.x, endNode.y));

            PathNode currentNode = endNode;
            while (currentNode.cameFromNodeIndex != -1)
            {
                PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                outputPath.Add(new int2(cameFromNode.x, cameFromNode.y));
                currentNode = cameFromNode;
            }
        }
    }

    /// <summary>
    /// Struct to contain all of the information about a path node
    /// </summary>
    private struct PathNode
    {
        public int x;
        public int y;

        public int index;

        public int gCost;
        public int hCost;
        public int fCost;

        public bool isWalkable;

        public int cameFromNodeIndex;

        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }
    }

    /// <summary>
    /// Data container for path information to be stored across multiple frames
    /// </summary>
    private struct PathInformation
    {
        public NativeList<int2> savedPath;
        public BaseEnemy unit;
        public JobHandle jobHandle;
    }
}



