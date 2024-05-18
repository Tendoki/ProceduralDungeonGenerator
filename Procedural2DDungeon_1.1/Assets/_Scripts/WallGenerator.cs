using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WallGenerator
{
	public static HashSet<Vector2Int> CreateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer, bool withDiagonals)
	{
		HashSet<Vector2Int> basicWallPositions = new HashSet<Vector2Int>();
		if (withDiagonals)
		{
			basicWallPositions = FindWallsInDirections(floorPositions, Direction2D.allDirectionsList);
		}
		else
		{
			basicWallPositions = FindWallsInDirections(floorPositions, Direction2D.cardinalDirectionsList);
		}
		
		foreach (var position in basicWallPositions)
		{
			tilemapVisualizer.PaintSingleBasicWall(position);
		}

		return basicWallPositions;
	}

	private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionsList)
	{
		HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
		foreach (var position in floorPositions)
		{
			foreach (var direction in directionsList)
			{
				var neighbourPosition = position + direction;
				if (!floorPositions.Contains(neighbourPosition))
					wallPositions.Add(neighbourPosition);
			}
		}

		return wallPositions;
	}
}
