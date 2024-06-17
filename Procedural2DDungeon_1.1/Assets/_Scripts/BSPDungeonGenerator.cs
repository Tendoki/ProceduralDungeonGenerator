using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class BSPDungeonGenerator : RandomWalkRoomGenerator
{
	[SerializeField]
	private PlacerItems placerItems;

	[SerializeField]
	private int minRoomWidth = 4, minRoomHeight = 4;
	[SerializeField]
	private int dungeonWidth = 20, dungeonHeight = 20;
	[SerializeField]
	[Range(0,10)]
	private int offset = 1;
	[SerializeField]
	private bool randomWalkRooms = false;
	[SerializeField]
	[Range(1, 3)]
	private int sizeCorridor = 2;
	[SerializeField]
	private bool isBinaryTreeGenerate;
	[SerializeField]
	private bool divideLessMin;
	[SerializeField]
	private bool sizeRoomCanLessMin;
	[SerializeField]
	private bool isStochastic;
	[SerializeField]
	private float enimiesRoomPercent = 0.8f;
	[SerializeField] 
	private int countTreasureRoom = 2;

	private Dictionary<Vector2Int, HashSet<Vector2Int>> roomsDictionary;
	//private List<HashSet<Vector2Int>> roomsPosition;
	private HashSet<Vector2Int> floor, corridorPositions, floorInRoomsPositions;
	private List<BoundsInt> roomsBounds;

	protected override void RunProceduralGeneration()
	{
		GenerateBSPDungeon();
	}

	private void GenerateBSPDungeon()
	{
		roomsDictionary = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
		roomsBounds = new List<BoundsInt>();
		
		floor = new HashSet<Vector2Int>(); //все клетки пола
		corridorPositions = new HashSet<Vector2Int>(); //клетки коридоров
		floorInRoomsPositions = new HashSet<Vector2Int>(); //клетки в комнатах
		//roomsPosition = new List<HashSet<Vector2Int>>();

		BoundsInt spaceToSplit = new BoundsInt((Vector3Int)startPosition,
			new Vector3Int(dungeonWidth, dungeonHeight, 0));

		//бинарное дерево пространств для соединения
		BinaryTree tree = new BinaryTree(spaceToSplit);

		roomsBounds = ProceduralGenerationAlgoritms.BinarySpacePartitioningBinaryTree(tree, minRoomWidth,
			minRoomHeight, offset, divideLessMin, sizeRoomCanLessMin, isStochastic);

		if (randomWalkRooms)
			floorInRoomsPositions = CreateRoomsRandomly(roomsBounds); //создание комнат случайным блужданием
		else
			floorInRoomsPositions = CreateSimpleRooms(roomsBounds); //создание обычных прямоугольных комнат
		floor.UnionWith(floorInRoomsPositions);

		List<List<Vector2Int>> corridors; //список коридоров
		if (isBinaryTreeGenerate)
			corridors = BinaryTreeGenerationCorridors(tree);
		else
			corridors = NearestNeighborsGenerationCorridors(roomsBounds);

		if (sizeCorridor > 1)
			IncreaseSizeCorridors(corridors, floor, sizeCorridor);

		(List<Room> rooms, HashSet< Tuple<Vector2Int, int> > corridorPositionsWithDistance) = ItemPlacementAlgoritms.DetermineTypesRooms(roomsDictionary, roomsBounds, corridorPositions, enimiesRoomPercent,
			countTreasureRoom);

		List<Item> items = placerItems.PlaceItemsInRooms(rooms, corridorPositions);
		VisualizeTiles(rooms, items, corridorPositionsWithDistance);
	}

	private void VisualizeTiles(List<Room> rooms, List<Item> items, HashSet<Tuple<Vector2Int, int>> corridorPositionsWithDistance)
	{
		tilemapVisualizer.PaintFloorTiles(floorInRoomsPositions);
		tilemapVisualizer.PaintCorridorTiles(corridorPositions);
		HashSet<Vector2Int> walls = WallsCreator.CreateWalls(floor, tilemapVisualizer, true);
		HashSet<Vector2Int> allPositions = new HashSet<Vector2Int>();
		allPositions.UnionWith(walls);
		allPositions.UnionWith(floor);
		tilemapVisualizer.PaintAllTilesWithRule(allPositions);
		tilemapVisualizer.PaintTypeRooms(rooms);

		tilemapVisualizer.PaintHeatMap(corridorPositionsWithDistance, ItemPlacementAlgoritms.maxDistance);
		tilemapVisualizer.PaintItemsTiles(items);
	}

	private List<List<Vector2Int>> BinaryTreeGenerationCorridors(BinaryTree tree)
	{
		List<List<Vector2Int>> corridors = new List<List<Vector2Int>>();
		Queue<Node> queue = new Queue<Node>();
		queue.Enqueue(tree.root);

		while (queue.Count > 0)
		{
			Node space = queue.Dequeue();
			if (space.LeftNode != null && space.RightNode != null)
			{
				List<Vector2Int> centers = FindTwoRooms(space.LeftNode, space.RightNode);
				List<Vector2Int> newCorridor = CreateCorridor(centers[0], centers[1]);

				corridorPositions.UnionWith(newCorridor);
				corridors.Add(newCorridor);
				floor.UnionWith(newCorridor);

				queue.Enqueue(space.LeftNode);
				queue.Enqueue(space.RightNode);
			}
		}
		return corridors;
	}	

	private List<List<Vector2Int>> NearestNeighborsGenerationCorridors(List<BoundsInt> roomsBounds)
	{
		List<Vector2Int> roomCenters = new List<Vector2Int>(); //центры всех комнат
		foreach (var room in roomsBounds)
		{
			roomCenters.Add(Vector2Int.RoundToInt(room.center));
		}

		return ConnectRooms(roomCenters);
	}

	private List<Vector2Int> FindTwoRooms(Node leftNode, Node rightNode)
	{
		List<BoundsInt> rooms_1 = new List<BoundsInt>();
		List<BoundsInt> rooms_2 = new List<BoundsInt>();
		Queue<Node> q1 = new Queue<Node>();
		q1.Enqueue(leftNode);
		while (q1.Count > 0)
		{
			Node node = q1.Dequeue();
			if (node.LeftNode == null && node.RightNode == null)
			{
				rooms_1.Add(node.area);
			}
			else
			{
				q1.Enqueue(node.LeftNode);
				q1.Enqueue(node.RightNode);
			}
		}

		Queue<Node> q2 = new Queue<Node>();
		q2.Enqueue(rightNode);
		while (q2.Count > 0)
		{
			Node node = q2.Dequeue();
			if (node.LeftNode == null && node.RightNode == null)
			{
				rooms_2.Add(node.area);
			}
			else
			{
				q2.Enqueue(node.LeftNode);
				q2.Enqueue(node.RightNode);
			}
		}
		BoundsInt firstRoom = new BoundsInt();
		BoundsInt secondRoom = new BoundsInt();
		float min = float.MaxValue;

		foreach (var room_1 in rooms_1)
		{
			foreach (var room_2 in rooms_2)
			{
				if (Vector2.Distance(room_1.center, room_2.center) < min)
				{
					min = Vector2.Distance(room_1.center, room_2.center);
					firstRoom = room_1;
					secondRoom = room_2;
				}
			}
		}

		List<Vector2Int> centers = new List<Vector2Int>();
		centers.Add(Vector2Int.RoundToInt(firstRoom.center));
		centers.Add(Vector2Int.RoundToInt(secondRoom.center));
		return centers;
	}

	private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsBounds)
	{
		HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
		for (int i = 0; i < roomsBounds.Count; i++)
		{
			var roomBounds = roomsBounds[i];
			var roomCenter =
				new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
			var roomFloor = GenerateRandomWalkRoom(randomWalkRoomParameters, roomCenter);
			HashSet<Vector2Int> newRoomFloor = new HashSet<Vector2Int>();
			foreach (var position in roomFloor)
			{
				if (position.x >= (roomBounds.xMin + offset) && position.x <= (roomBounds.xMax - offset) &&
				    position.y >= (roomBounds.yMin - offset) && position.y <= (roomBounds.yMax - offset))
				{
					newRoomFloor.Add(position);
					floor.Add(position);
				}
				roomsDictionary[roomCenter] = newRoomFloor;
			}
		}

		return floor;
	}

	private List<List<Vector2Int>> ConnectRooms(List<Vector2Int> roomCenters)
	{
		List<List<Vector2Int>> corridors = new List<List<Vector2Int>>();
		var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
		roomCenters.Remove(currentRoomCenter);

		while (roomCenters.Count > 0)
		{
			Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
			roomCenters.Remove(closest);
			List<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
			currentRoomCenter = closest;

			corridors.Add(newCorridor);
			corridorPositions.UnionWith(newCorridor);
			floor.UnionWith(newCorridor);
		}

		return corridors;
	}

	private List<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
	{
		List<Vector2Int> corridor = new List<Vector2Int>();
		var position = currentRoomCenter;
		corridor.Add(position);

		if (Random.value < 0.5f)
		{
			ProjectionСorridorOnX(corridor, currentRoomCenter, destination, ref position);
			ProjectionСorridorOnY(corridor, currentRoomCenter, destination, ref position);
		}
		else
		{
			ProjectionСorridorOnY(corridor, currentRoomCenter, destination, ref position);
			ProjectionСorridorOnX(corridor, currentRoomCenter, destination, ref position);
		}
		
		return corridor;
	}

	private void ProjectionСorridorOnY(List<Vector2Int> corridor, Vector2Int currentRoomCenter, Vector2Int destination, ref Vector2Int position)
	{
		int yLength = Mathf.Abs(currentRoomCenter.y - destination.y);
		if (yLength != 0)
		{
			Vector2Int yDirection = new Vector2Int(0, (destination.y - currentRoomCenter.y) / yLength);
			for (int i = 0; i < yLength; i++)
			{
				position += yDirection;
				corridor.Add(position);
			}
		}
	}

	private void ProjectionСorridorOnX(List<Vector2Int> corridor, Vector2Int currentRoomCenter, Vector2Int destination, ref Vector2Int position)
	{
		int xLength = Mathf.Abs(currentRoomCenter.x - destination.x);
		if (xLength != 0)
		{
			Vector2Int xDirection = new Vector2Int((destination.x - currentRoomCenter.x) / xLength, 0);
			for (int i = 0; i < xLength; i++)
			{
				position += xDirection;
				corridor.Add(position);
			}
		}
	}

	private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
	{
		Vector2Int closest = Vector2Int.zero;
		float distance = float.MaxValue;
		foreach (var position in roomCenters)
		{
			float currentDistance = Vector2.Distance(position, currentRoomCenter);
			if (currentDistance < distance)
			{
				distance = currentDistance;
				closest = position;
			}
		}

		return closest;
	}

	private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsBounds)
	{
		HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
		foreach (var room in roomsBounds)
		{
			HashSet<Vector2Int> newRoomFloor = new HashSet<Vector2Int>();
			var roomCenter = new Vector2Int(Mathf.RoundToInt(room.center.x), Mathf.RoundToInt(room.center.y));
			for (int col = offset; col < room.size.x - offset; col++)
			{
				for (int row = offset; row < room.size.y - offset; row++)
				{
					Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
					newRoomFloor.Add(position);
					floor.Add(position);
				}
			}
			roomsDictionary[roomCenter] = newRoomFloor;
		}

		return floor;
	}
}
