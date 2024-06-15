using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomWalkRoomGenerator : AbstractGenerator
{
	[SerializeField] protected RandomWalkRoomSO randomWalkRoomParameters;

	protected override void RunProceduralGeneration()
	{
		//генерация структуры пола комнаты
		HashSet<Vector2Int> floorPositions = GenerateRandomWalkRoom(randomWalkRoomParameters, startPosition);

		//визуализация плиток
		VisualizeTiles(floorPositions);
	}

	private void VisualizeTiles(HashSet<Vector2Int> floorPositions)
	{
		tilemapVisualizer.PaintFloorTiles(floorPositions);

		//создание стен
		HashSet<Vector2Int> walls = WallsCreator.CreateWalls(floorPositions, tilemapVisualizer, true);

		//рисование плиток по правилам
		HashSet<Vector2Int> allPositions = new HashSet<Vector2Int>();
		allPositions.UnionWith(walls);
		allPositions.UnionWith(floorPositions);
		tilemapVisualizer.PaintAllTilesWithRule(allPositions);
	}

	protected HashSet<Vector2Int> GenerateRandomWalkRoom(RandomWalkRoomSO parameters, Vector2Int position)
	{
		var currentPosition = position;
		HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
		for (int i = 0; i < parameters.iterations; i++)
		{
			var path = ProceduralGenerationAlgoritms.RandomWalk(currentPosition, parameters.walkLength);
			floorPositions.UnionWith(path);
			if (parameters.startRandomlyEachIteration)
				currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
		}

		return floorPositions;
	}
}
