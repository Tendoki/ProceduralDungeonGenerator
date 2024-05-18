using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorridorFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
	[SerializeField]
	private int corridorLength = 14, corridorCount = 5;
	[SerializeField]
	[Range(0.1f, 1)]
	private float roomPercent = 0.8f;
	[SerializeField]
	[Range(1,3)]
	private int sizeCorridor = 2;


	protected override void RunProceduralGeneration()
	{
		CorridorFirstGeneration();
	}

	private void CorridorFirstGeneration()
	{
		HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>(); //позиции пола
		HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>(); //возможные позиции комнат

		List<List<Vector2Int>> corridors = CreateCorridors(floorPositions, potentialRoomPositions); //создаем корридоры

		HashSet<Vector2Int> roomPositions = CreateRooms(potentialRoomPositions); //создаем комнаты

		List<Vector2Int> deadEnds = FindAllDeadEnds(floorPositions); //находим тупики
		CreateRoomsAtDeadEnd(deadEnds, roomPositions); //добавляем комнаты в тупики

		floorPositions.UnionWith(roomPositions); //объединяем корридоры с комнатами

		if (sizeCorridor > 1)
			IncreaseCorridorsSize(corridors, floorPositions, sizeCorridor);

		tilemapVisualizer.PaintFloorTiles(floorPositions);
		HashSet<Vector2Int> walls = WallGenerator.CreateWalls(floorPositions, tilemapVisualizer, true);
		HashSet<Vector2Int> allPositions = new HashSet<Vector2Int>();
		allPositions.UnionWith(walls);
		allPositions.UnionWith(floorPositions);
		tilemapVisualizer.PaintAllTilesWithRule(allPositions);
	}

	private void CreateRoomsAtDeadEnd(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomFloors)
	{
		foreach (var position in deadEnds)
		{
			if (roomFloors.Contains(position) == false)
			{
				var room = RunRandomWalk(randomWalkParameters, position);
				roomFloors.UnionWith(room);
			}
		}
	}

	private List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
	{
		List<Vector2Int> deadEnds = new List<Vector2Int>();
		foreach (var position in floorPositions)
		{
			int neighboursCount = 0;
			foreach (var direction in Direction2D.cardinalDirectionsList)
			{
				if (floorPositions.Contains(position + direction))
					neighboursCount++;
			}

			if (neighboursCount == 1)
				deadEnds.Add(position);
		}

		return deadEnds;
	}

	private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions)
	{
		HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>();
		int roomsToCreateCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);

		List<Vector2Int> roomsToCreate =
			potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomsToCreateCount).ToList();

		foreach (var roomPosition in roomsToCreate)
		{
			var roomFloor = RunRandomWalk(randomWalkParameters, roomPosition);
			roomPositions.UnionWith(roomFloor);
		}

		return roomPositions;
	}

	private List<List<Vector2Int>> CreateCorridors(HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> potentialRoomPositions)
	{
		var currentPosition = startPosition;
		potentialRoomPositions.Add(currentPosition);
		List<List<Vector2Int>> corridors = new List<List<Vector2Int>>();

		for (int i = 0; i < corridorCount; i++)
		{
			var corridor = ProceduralGenerationAlgoritms.RandomWalkCorridor(currentPosition, corridorLength);
			corridors.Add(corridor);
			currentPosition = corridor[corridor.Count - 1];
			potentialRoomPositions.Add(currentPosition);
			floorPositions.UnionWith(corridor);
		}

		return corridors;
	}
}
