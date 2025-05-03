using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private void Start()
    {
        float startTime = Time.realtimeSinceStartup;
        FindPathJob findPathJob = new FindPathJob { startPosition = new int2(0, 0), endPosition = new int2(19, 19) };
        findPathJob.Schedule();

        Debug.Log("Time: " + ((Time.realtimeSinceStartup - startTime) * 1000f));
    }

    [BurstCompile]
    private struct FindPathJob : IJob
    {
        public int2 startPosition;
        public int2 endPosition;

        public void Execute()
        {
            // Initialize the grid, will need to be adjusted to calculate off of Justin's prefabs later
            int2 gridSize = new int2(20, 20);
            NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    PathNode pathNode = new PathNode();
                    pathNode.x = x;
                    pathNode.y = y;
                    pathNode.index = CalculateIndex(x, y, gridSize.x);
                    pathNode.gCost = int.MaxValue;
                    pathNode.hCost = CalculateDistanceCost(new int2(x, y), endPosition);
                    pathNode.CalculateFCost();

                    pathNode.isWalkable = true;
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
                Debug.Log("Didn't find a path");
            }
            else
            {
                // Found a path
                NativeList<int2> path = CalculatePath(pathNodeArray, endNode);

                foreach (int2 pathPosition in path)
                {
                    Debug.Log(pathPosition);
                }

                path.Dispose();
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
            return x + y * gridWidth;
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
                gridPosition.x >= 0 &&
                gridPosition.y >= 0 &&
                gridPosition.x < gridSize.x &&
                gridPosition.y < gridSize.y; ;
        }

        private NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
        {
            if (endNode.cameFromNodeIndex == -1)
            {
                // Couldn't find a path
                return new NativeList<int2>(Allocator.Temp);
            }
            else
            {
                // Found a path
                NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
                path.Add(new int2(endNode.x, endNode.y));

                PathNode currentNode = endNode;
                while (currentNode.cameFromNodeIndex != -1)
                {
                    PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                    path.Add(new int2(cameFromNode.x, cameFromNode.y));
                    currentNode = cameFromNode;
                }

                return path;
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
    }
}
