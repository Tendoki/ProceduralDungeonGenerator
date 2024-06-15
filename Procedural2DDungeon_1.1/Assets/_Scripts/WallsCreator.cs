using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WallsCreator
{
	public static HashSet<Vector2Int> CreateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer, bool withDiagonals)
	{
		HashSet<Vector2Int> basicWallPositions = new HashSet<Vector2Int>();
		if (withDiagonals)
		{
			basicWallPositions = FindWalls(floorPositions, Directions.allDirectionsList);
		}
		else
		{
			basicWallPositions = FindWalls(floorPositions, Directions.mainDirectionsList);
		}
		
		foreach (var position in basicWallPositions)
		{
			tilemapVisualizer.PaintSingleBasicWall(position);
		}

		return basicWallPositions;
	}

	private static HashSet<Vector2Int> FindWalls(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionsList)
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
