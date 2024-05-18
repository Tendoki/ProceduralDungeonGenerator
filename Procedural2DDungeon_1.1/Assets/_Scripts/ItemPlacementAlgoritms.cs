using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public static class ItemPlacementAlgoritms
{
	public static int maxDistance = 0;

	public static (List<Room>, HashSet<Tuple<Vector2Int, int>> corridorPositionsWithDistance) DetermineTypesRooms(
		Dictionary<Vector2Int, HashSet<Vector2Int>> roomsDictionary,
		List<BoundsInt> roomsList, HashSet<Vector2Int> corridorPositions, float enemiesRoomPercent, int countTreasureRoom)
	{
		HashSet<Tuple<Vector2Int, int>> corridorPositionsWithDistance = new HashSet<Tuple<Vector2Int, int>>();
		List<Room> rooms = CreateRoomsList(roomsDictionary, roomsList);
		Room startRoom = GetStartRoom(rooms);

		List<Room> deadEnds = FindDistanceAlgorithm(startRoom, rooms, corridorPositions, corridorPositionsWithDistance);
		Quicksort(rooms, 0, rooms.Count - 1);
		Quicksort(deadEnds, 0, deadEnds.Count - 1);

		List<Room> roomsWithoutType = new List<Room>();
		for (int i = 0; i < rooms.Count; i++)
		{
			roomsWithoutType.Add(rooms[i]);
		}

		int countOrdinaryRooms = rooms.Count - 3 - countTreasureRoom;
		int countEnemiesRooms = Mathf.RoundToInt((countOrdinaryRooms) * enemiesRoomPercent);

		rooms[0].type = RoomType.Player;
		roomsWithoutType.Remove(rooms[0]);
		deadEnds.Remove(rooms[0]);

		rooms[rooms.Count - 1].type = RoomType.Exit;
		roomsWithoutType.Remove(rooms[rooms.Count - 1]);
		deadEnds.Remove(rooms[rooms.Count - 1]);

		for (int i = 0; i < countTreasureRoom; i++)
		{
			if (deadEnds.Count > 0)
			{
				int index = Random.Range(Mathf.FloorToInt((float)deadEnds.Count / 2), deadEnds.Count);
				Room treasureRoom = deadEnds[index];
				treasureRoom.type = RoomType.Treasure;
				roomsWithoutType.Remove(treasureRoom);
				deadEnds.Remove(treasureRoom);
			}
			else
			{
				Room treasureRoom = roomsWithoutType[Random.Range(Mathf.FloorToInt((float)roomsWithoutType.Count / 2), roomsWithoutType.Count)];
				treasureRoom.type = RoomType.Treasure;
				roomsWithoutType.Remove(treasureRoom);
			}
		}

		Room keyRoom = roomsWithoutType[Random.Range(Mathf.FloorToInt((float)roomsWithoutType.Count / 2), roomsWithoutType.Count)];
		roomsWithoutType.Remove(keyRoom);
		keyRoom.type = RoomType.Key;

		for (int i = 0; i < countEnemiesRooms; i++)
		{
			Room enemiesRoom = roomsWithoutType[Random.Range(0, roomsWithoutType.Count)];
			enemiesRoom.type = RoomType.Enemies;
			roomsWithoutType.Remove(enemiesRoom);
		}

		for (int i = 0; i < roomsWithoutType.Count; i++)
		{
			roomsWithoutType[i].type = RoomType.Resources;
		}

		return (rooms, corridorPositionsWithDistance);
	}

	private static List<Room> CreateRoomsList(Dictionary<Vector2Int, HashSet<Vector2Int>> roomsDictionary, List<BoundsInt> roomsList)
    {
	    List<Room> rooms = new List<Room>();
        foreach (var roomArea in roomsList)
        {
            Vector2Int center = Vector2Int.RoundToInt(roomArea.center);
            Room room = new Room(center, roomsDictionary[center]);
            rooms.Add(room);
        }

        return rooms;
    } 

    private static List<Room> FindDistanceAlgorithm(Room startRoom, List<Room> rooms, HashSet<Vector2Int> corridorPositions,
	    HashSet<Tuple<Vector2Int, int>> corridorPositionsWithDistance)
    {
	    List<Room> deadEnds = new List<Room>();
	    Vector2Int startPosition = startRoom.center;
	    HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>();
	    HashSet<Room> visitedRooms = new HashSet<Room>();
	    visitedPositions.Add(startPosition);
	    Queue<Tuple<Vector2Int, int>> q = new Queue<Tuple<Vector2Int, int>>();
        q.Enqueue(new Tuple<Vector2Int, int>(startPosition, 0));
        while (q.Count > 0)
        {
	        Tuple<Vector2Int, int> current = q.Dequeue();
	        foreach (var room in rooms)
	        {
		        if (!visitedRooms.Contains(room) && room.floorPosition.Contains(current.Item1))
		        {
			        room.distanceFromStart = current.Item2;
			        visitedRooms.Add(room);
			        break;
		        }
	        }

	        List<Vector2Int> neighbors = GetNeighbors(current.Item1, corridorPositions, visitedPositions);
	        int distance = current.Item2 + 1;
	        if (distance > maxDistance)
		        maxDistance = distance;
	        if (neighbors.Count == 0)
	        {
		        foreach (var room in rooms)
		        {
			        if (room.floorPosition.Contains(current.Item1))
			        {
				        deadEnds.Add(room);
						break;
			        }
		        }
	        }
			foreach (var neighbor in neighbors)
			{
				Tuple<Vector2Int, int> tuple = new Tuple<Vector2Int, int>(neighbor, distance);
				corridorPositionsWithDistance.Add(tuple);
		        q.Enqueue(tuple);
		        visitedPositions.Add(neighbor);
	        }
        }

        return deadEnds;
    }

    private static List<Vector2Int> GetNeighbors(Vector2Int currentPosition, HashSet<Vector2Int> corridorPositions, HashSet<Vector2Int> visitedPositions)
    {
	    List<Vector2Int> allNeighbors = new List<Vector2Int>();
	    allNeighbors.Add(currentPosition + new Vector2Int(1, 0));
	    allNeighbors.Add(currentPosition + new Vector2Int(-1, 0));
	    allNeighbors.Add(currentPosition + new Vector2Int(0, 1));
	    allNeighbors.Add(currentPosition + new Vector2Int(0, -1));

		List<Vector2Int> neighbors = new List<Vector2Int>();
		foreach (var neighbor in allNeighbors)
        {
	        if (corridorPositions.Contains(neighbor) && !visitedPositions.Contains(neighbor))
	        {
		        neighbors.Add(neighbor);
	        }
        }

		return neighbors;
    }

    private static Room GetStartRoom(List<Room> rooms)
    {
	    Room room = rooms[Random.Range(0, rooms.Count)];
	    return room;
    }

	private static int Partition(List<Room> rooms, int start, int end)
	{
		int marker = start; // divides left and right subarrays
		for (int i = start; i < end; i++)
		{
			if (rooms[i].distanceFromStart < rooms[end].distanceFromStart) // array[end] is pivot
			{
				(rooms[marker], rooms[i]) = (rooms[i], rooms[marker]);
				marker += 1;
			}
		}
		// put pivot(array[end]) between left and right subarrays
		(rooms[marker], rooms[end]) = (rooms[end], rooms[marker]);
		return marker;
	}

	private static void Quicksort(List<Room> rooms, int start, int end)
	{
		if (start >= end)
			return;

		int pivot = Partition(rooms, start, end);
		Quicksort(rooms, start, pivot - 1);
		Quicksort(rooms, pivot + 1, end);
	}
}

public enum PlacementType
{
    OpenSpace,
    NearWall
}

public enum RoomType
{
	Player,
    Enemies,
    Resources,
	Treasure,
    Key,
	Exit
}

public class Room
{
    public Vector2Int center;
    public HashSet<Vector2Int> floorPosition;
    public RoomType type;
    public int distanceFromStart;

    public Room(Vector2Int center, HashSet<Vector2Int> floorPosition)
    {
        this.center = center;
        this.floorPosition = floorPosition;
        this.distanceFromStart = 0;
    }
}