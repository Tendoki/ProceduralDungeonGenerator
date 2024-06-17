using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WallsCreator
{
	public static HashSet<Vector2Int> CreateWalls(HashSet<Vector2Int> floor, TilemapVisualizer tilemapVisualizer, bool withDiagonals)
	{
		HashSet<Vector2Int> walls = new HashSet<Vector2Int>();
		if (withDiagonals)
		{
			walls = FindWalls(floor, Directions.allDirectionsList);
		}
		else
		{
			walls = FindWalls(floor, Directions.mainDirectionsList);
		}
		
		foreach (var position in walls)
		{
			tilemapVisualizer.PaintSingleWall(position);
		}

		return walls;
	}

	private static HashSet<Vector2Int> FindWalls(HashSet<Vector2Int> floor, List<Vector2Int> directionsList)
	{
		HashSet<Vector2Int> walls = new HashSet<Vector2Int>();
		foreach (var position in floor)
		{
			foreach (var direction in directionsList)
			{
				var neighbourPosition = position + direction;
				if (!floor.Contains(neighbourPosition))
					walls.Add(neighbourPosition);
			}
		}

		return walls;
	}
}
