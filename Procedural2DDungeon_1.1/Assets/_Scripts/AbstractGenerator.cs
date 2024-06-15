using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractGenerator : MonoBehaviour
{
	[SerializeField] protected TilemapVisualizer tilemapVisualizer = null;
	[SerializeField] protected Vector2Int startPosition = Vector2Int.zero;

	public void Generate()
	{
		tilemapVisualizer.Clear();
		RunProceduralGeneration();
	}

	protected abstract void RunProceduralGeneration();

	protected void IncreaseSizeCorridors(List<List<Vector2Int>> corridors, HashSet<Vector2Int> floorPositions, int size)
	{
		Vector2Int GetDirection(Vector2Int direction)
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

		List<Vector2Int> IncreaseSizeCorridorToTwo(List<Vector2Int> corridor, ref Vector2Int previousDirection)
		{
			List<Vector2Int> newCorridor = new List<Vector2Int>();
			Vector2Int direction = corridor[1] - corridor[0];

			var currentAngle = Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x);
			if (currentAngle < 0)
				currentAngle += 360;
			var previousAngle = Mathf.Rad2Deg * Mathf.Atan2(previousDirection.y, previousDirection.x);
			if (previousAngle < 0)
				previousAngle += 360;
			Vector2Int newCorridorTileOffset = GetDirection(direction);
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

		List<Vector2Int> IncreaseSizeCorridorToThree(List<Vector2Int> corridor)
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
			switch (size)
			{
				case 2:
					corridors[i] = IncreaseSizeCorridorToTwo(corridors[i], ref previousDirection);
					break;
				case 3:
					corridors[i] = IncreaseSizeCorridorToThree(corridors[i]);
					break;
			}

			floorPositions.UnionWith(corridors[i]);
		}
	}
}
