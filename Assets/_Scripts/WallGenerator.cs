using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class WallGenerator
{
    public static HashSet<Vector2Int> CreateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer)
    {
        var basicWallPositions = FindWallsInDirections(floorPositions, Direction2D.cardinalDirections);
        var cornerWallPositions = FindWallsInDirections(floorPositions, Direction2D.diagonalDirections);
        
        HashSet<Vector2Int> torchPositions = createBasicWall(tilemapVisualizer, basicWallPositions, floorPositions);
        createCornerWalls(tilemapVisualizer, cornerWallPositions, floorPositions);

        return torchPositions;
    }

    private static void createCornerWalls(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> cornerWallPositions, HashSet<Vector2Int> floorPositions)
    {
        foreach (var position in cornerWallPositions)
        {
            string neighborsBinaryType = "";

            foreach (var direction in Direction2D.eightDirectionsList)
            {
                var neighborPosition = position + direction;

                if (floorPositions.Contains(neighborPosition))
                {
                    neighborsBinaryType += "1";
                }
                else
                {
                    neighborsBinaryType += "0";
                }
            }
            tilemapVisualizer.PaintSingleCornerWall(position, neighborsBinaryType);
        }
    }

    private static HashSet<Vector2Int> createBasicWall(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> basicWallPositions, HashSet<Vector2Int> floorPositions)
    {
        HashSet<Vector2Int> torchPositions = new HashSet<Vector2Int>();
        foreach (var position in basicWallPositions)
        {
            string neighborsBinaryType = "";
            foreach (var direction in Direction2D.cardinalDirections)
            {
                var neighborPosition = position + direction;
                if (floorPositions.Contains(neighborPosition))
                {
                    neighborsBinaryType += "1";
                }
                else
                {
                    neighborsBinaryType += "0";
                }
            }

            tilemapVisualizer.PaintSingleBasicWall(position, neighborsBinaryType);
            int typeAsInt = Convert.ToInt32(neighborsBinaryType, 2);
            if (WallTypesHelper.wallTop.Contains(typeAsInt))
            {
                // Place the torch on the floor tile below the wall
                var torchPosition = position + Vector2Int.down;
                if (floorPositions.Contains(torchPosition))
                {
                    torchPositions.Add(torchPosition);
                }
            }
        }
        return torchPositions;
    }

    private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionList)
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();

        foreach (var position in floorPositions) 
        {
            foreach (var direction in directionList)
            {
                var neighborPosition = position + direction;
                if (floorPositions.Contains(neighborPosition) == false)
                {
                    // Then it's a wall

                    wallPositions.Add(neighborPosition);
                }
            }
        }

        return wallPositions;
    }
}
