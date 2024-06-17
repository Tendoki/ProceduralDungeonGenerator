using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomWalkDungeonGenerator : RandomWalkRoomGenerator
{
	[SerializeField]
	private int corridorLength = 14, corridorCount = 5;
	[SerializeField]
	[Range(0.1f, 1)]
	private float roomPercent = 0.8f;
	[SerializeField]
	[Range(1,3)]
	private int sizeCorridor = 2;
	[SerializeField]
	private bool consistentCorridors;
	[SerializeField]
	private bool withPrevDir;


	protected override void RunProceduralGeneration()
	{
		GenerateRandomWalkDungeon();
	}

	private void GenerateRandomWalkDungeon()
	{
		HashSet<Vector2Int> corridorsFloorPositions = new HashSet<Vector2Int>(); //позиции пола коридоров
		List<Vector2Int> possibleRoomPositions = new List<Vector2Int>(); //возможные позиции комнат

		//создаем коридоры
		List<List<Vector2Int>> corridors = CreateCorridors(corridorsFloorPositions, possibleRoomPositions);

		HashSet<Vector2Int> floorInRoomsPositions = CreateRooms(possibleRoomPositions); //создаем комнаты

		List<Vector2Int> deadEnds = FindDeadEnds(corridorsFloorPositions); //находим тупики
		CreateRoomsAtDeadEnds(deadEnds, floorInRoomsPositions); //добавляем комнаты в тупики

		HashSet<Vector2Int> floor = UnionPositions(corridorsFloorPositions, floorInRoomsPositions);

		//увеличение размера коридоров
		if (sizeCorridor > 1)
			IncreaseSizeCorridors(corridors, floor, sizeCorridor);

		//визуализация плиток
		VisualizeTiles(floor);
	}

	private HashSet<Vector2Int> UnionPositions(HashSet<Vector2Int> corridorsFloorPositions, HashSet<Vector2Int> roomsFloor)
	{
		HashSet<Vector2Int> floor = new HashSet<Vector2Int>(); //все позиции пола
		floor.UnionWith(corridorsFloorPositions); //объединяем c коридорами
		floor.UnionWith(roomsFloor); //объединяем с комнатами

		return floor;
	}

	private void VisualizeTiles(HashSet<Vector2Int> floor)
	{
		tilemapVisualizer.PaintFloorTiles(floor);
		HashSet<Vector2Int> walls = WallsCreator.CreateWalls(floor, tilemapVisualizer, true);
		HashSet<Vector2Int> allPositions = new HashSet<Vector2Int>();
		allPositions.UnionWith(walls);
		allPositions.UnionWith(floor);
		tilemapVisualizer.PaintAllTilesWithRule(allPositions);
	}

	private void CreateRoomsAtDeadEnds(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomsFloor)
	{
		foreach (var deadEnd in deadEnds)
		{
			if (roomsFloor.Contains(deadEnd) == false)
			{
				HashSet<Vector2Int> newRoom = GenerateRandomWalkRoom(randomWalkRoomParameters, deadEnd);
				roomsFloor.UnionWith(newRoom);
			}
		}
	}

	private List<Vector2Int> FindDeadEnds(HashSet<Vector2Int> corridorsFloorPositions)
	{
		List<Vector2Int> deadEnds = new List<Vector2Int>();
		foreach (var position in corridorsFloorPositions)
		{
			int neighboursCount = 0;
			foreach (var direction in Directions.mainDirectionsList)
			{
				if (corridorsFloorPositions.Contains(position + direction))
					neighboursCount++;
			}

			if (neighboursCount == 1)
				deadEnds.Add(position);
		}

		return deadEnds;
	}

	private HashSet<Vector2Int> CreateRooms(List<Vector2Int> possibleRoomPositions)
	{
		HashSet<Vector2Int> roomsFloor = new HashSet<Vector2Int>();
		int roomsToCreateCount = Mathf.RoundToInt(possibleRoomPositions.Count * roomPercent);

		for (int i = 0; i < roomsToCreateCount; i++)
		{
			Vector2Int roomCenter = possibleRoomPositions[Random.Range(0, possibleRoomPositions.Count)];
			possibleRoomPositions.Remove(roomCenter);
			var roomFloor = GenerateRandomWalkRoom(randomWalkRoomParameters, roomCenter);
			roomsFloor.UnionWith(roomFloor);
		}

		return roomsFloor;
	}

	private List<List<Vector2Int>> CreateCorridors(HashSet<Vector2Int> corridorsFloorPositions, List<Vector2Int> possibleRoomPositions)
	{
		List<List<Vector2Int>> corridors = new List<List<Vector2Int>>();
		if (consistentCorridors)
			corridors = CreateConsistentCorridors(corridorsFloorPositions, possibleRoomPositions);
		else
			corridors = CreateRandomlyCorridors(corridorsFloorPositions, possibleRoomPositions);

		return corridors;
	}

	private List<List<Vector2Int>> CreateConsistentCorridors(HashSet<Vector2Int> corridorsFloorPositions, List<Vector2Int> possibleRoomPositions)
	{
		var currentPosition = startPosition;
		possibleRoomPositions.Add(currentPosition);
		List<List<Vector2Int>> corridors = new List<List<Vector2Int>>();

		Vector2Int previousDirection;
		Vector2Int newDirection = Directions.GetRandomMainDirection();
		for (int i = 0; i < corridorCount; i++)
		{
			var corridor = ProceduralGenerationAlgoritms.RandomWalkCorridor(currentPosition, corridorLength, newDirection);
			corridors.Add(corridor);
			currentPosition = corridor[corridor.Count - 1];
			possibleRoomPositions.Add(currentPosition);
			corridorsFloorPositions.UnionWith(corridor);

			if (withPrevDir)
			{
				previousDirection = newDirection;
				newDirection = Directions.GetRandomMainDirectionWithPreviousDir(previousDirection);
			}
			else
			{
				newDirection = Directions.GetRandomMainDirection();
			}
		}

		return corridors;
	}

	private List<List<Vector2Int>> CreateRandomlyCorridors(HashSet<Vector2Int> corridorsFloorPositions, List<Vector2Int> possibleRoomPositions)
	{
		var currentPosition = startPosition;
		possibleRoomPositions.Add(currentPosition);
		List<List<Vector2Int>> corridors = new List<List<Vector2Int>>();

		Vector2Int newDirection = Directions.GetRandomMainDirection();
		for (int i = 0; i < corridorCount; i++)
		{
			var corridor = ProceduralGenerationAlgoritms.RandomWalkCorridor(currentPosition, corridorLength, newDirection);
			corridors.Add(corridor);
			possibleRoomPositions.Add(corridor[corridor.Count - 1]);
			currentPosition = possibleRoomPositions[Random.Range(0, possibleRoomPositions.Count)];
			corridorsFloorPositions.UnionWith(corridor);
			newDirection = Directions.GetRandomMainDirection();
		}

		return corridors;
	}
}
