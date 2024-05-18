using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimpleRandomWalkDungeonGenerator : AbstractDungeonGenerator
{
	[SerializeField] protected SimpleRandomWalkSO randomWalkParameters;

	protected override void RunProceduralGeneration()
	{
		HashSet<Vector2Int> floorPositions = RunRandomWalk(randomWalkParameters, startPosition);
		tilemapVisualizer.PaintFloorTiles(floorPositions);
		HashSet<Vector2Int> walls = WallGenerator.CreateWalls(floorPositions, tilemapVisualizer, true);
		HashSet<Vector2Int> allPositions = new HashSet<Vector2Int>();
		allPositions.UnionWith(walls);
		allPositions.UnionWith(floorPositions);
		tilemapVisualizer.PaintAllTilesWithRule(allPositions);
	}

	protected HashSet<Vector2Int> RunRandomWalk(SimpleRandomWalkSO parameters, Vector2Int position)
	{
		var currentPosition = position;
		HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
		for (int i = 0; i < parameters.iterations; i++)
		{
			var path = ProceduralGenerationAlgoritms.SimpleRandomWalk(currentPosition, parameters.walkLength);
			floorPositions.UnionWith(path);
			if (parameters.startRandomlyEachIteration)
				currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
		}

		return floorPositions;
	}

	protected void IncreaseCorridorsSize(List<List<Vector2Int>> corridors, HashSet<Vector2Int> floorPositions, int size)
	{
		Vector2Int GetDirection90From(Vector2Int direction)
		{
			if (direction == Vector2Int.up)
				return Vector2Int.right;
			if (direction == Vector2Int.right)
				return Vector2Int.down;
			if (direction == Vector2Int.down)
				return Vector2Int.left;
			if (direction == Vector2Int.left)
				return Vector2Int.up;
			return Vector2Int.zero;
		}

		List<Vector2Int> IncreaseCorridorSizeByOne(List<Vector2Int> corridor, ref Vector2Int previousDirection)
		{
			List<Vector2Int> newCorridor = new List<Vector2Int>();
			Vector2Int direction = corridor[1] - corridor[0];

			var currentAngle = Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x);
			if (currentAngle < 0)
				currentAngle += 360;
			var previousAngle = Mathf.Rad2Deg * Mathf.Atan2(previousDirection.y, previousDirection.x);
			if (previousAngle < 0)
				previousAngle += 360;
			Vector2Int newCorridorTileOffset = GetDirection90From(direction);
			for (int i = 0; i < corridor.Count; i++)
			{
				newCorridor.Add(corridor[i]);
				Vector2Int offsetTile = corridor[i] + newCorridorTileOffset;
				newCorridor.Add(offsetTile);
			}

			var angle = currentAngle - previousAngle;
			if (angle == 90 || angle == -270)
			{
				newCorridor.Add(corridor[0] + newCorridorTileOffset - direction);
			}

			previousDirection = direction;
			return newCorridor;
		}

		List<Vector2Int> IncreaseCorridorBrush3by3(List<Vector2Int> corridor)
		{
			List<Vector2Int> newCorridor = new List<Vector2Int>();
			for (int i = 1; i < corridor.Count; i++)
			{
				for (int x = -1; x < 2; x++)
				{
					for (int y = -1; y < 2; y++)
					{
						newCorridor.Add(corridor[i - 1] + new Vector2Int(x, y));
					}
				}
			}

			return newCorridor;
		}

		Vector2Int previousDirection = Vector2Int.zero;
		for (int i = 0; i < corridors.Count; i++)
		{
			if (size == 3)
				corridors[i] = IncreaseCorridorBrush3by3(corridors[i]);
			else if (size == 2)
				corridors[i] = IncreaseCorridorSizeByOne(corridors[i], ref previousDirection);
			floorPositions.UnionWith(corridors[i]);
		}
	}
}
