using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlacerItems : MonoBehaviour
{
	[SerializeField] private List<ItemParam> defaultItems;
	[SerializeField] private List<ItemParam> ordinaryItems;
	[SerializeField] private List<ItemParam> playerRoomItems;
	[SerializeField] private List<ItemParam> enemiesRoomItems;
	[SerializeField] private List<ItemParam> resourcesRoomItems;
	[SerializeField] private List<ItemParam> treasureRoomItems;
	[SerializeField] private List<ItemParam> keyRoomItems;
	[SerializeField] private List<ItemParam> exitRoomItems;
	[SerializeField] private List<Item> items;
	private int roomNumberIter;

	public List<Item> PlaceItemsInRooms(List<Room> rooms, HashSet<Vector2Int> corridorPositions)
	{
		items = new List<Item>();
		roomNumberIter = 0;
		foreach (var room in rooms)
		{
			roomNumberIter++;
			HashSet<Vector2Int> roomFloorNoCorridor = new HashSet<Vector2Int>(room.floorPosition);
			roomFloorNoCorridor.ExceptWith(corridorPositions);
			Dictionary<PlacementType, HashSet<Vector2Int>> tileByType =
				CreateDictionaryTileByType(room.floorPosition, roomFloorNoCorridor);

			PlaceItemsInRoom(tileByType, defaultItems, room.floorPosition);
			switch (room.type)
			{
				case RoomType.Player:
					PlaceItemsInRoom(tileByType, ordinaryItems, room.floorPosition);
					break;
				case RoomType.Exit:
					break;
				case RoomType.Key:
					PlaceItemsInRoom(tileByType, keyRoomItems, room.floorPosition);
					break;
				case RoomType.Treasure:
					PlaceItemsInRoom(tileByType, treasureRoomItems, room.floorPosition);
					break;
				case RoomType.Enemies:
					PlaceItemsInRoom(tileByType, enemiesRoomItems, room.floorPosition);
					break;
				case RoomType.Resources:
					PlaceItemsInRoom(tileByType, resourcesRoomItems, room.floorPosition);
					break;
				
			}
		}
		return items;
	}

	private void PlaceItemsInRoom(Dictionary<PlacementType, HashSet<Vector2Int>> tileByType, List<ItemParam> typeItems, HashSet<Vector2Int> roomFloor)
	{
		foreach (var item in typeItems)
		{
			int numberItems = Random.Range(item.minNumber, item.maxNumber + 1);
			for (int i = 0; i < numberItems; i++)
			{
				Vector2Int? position = GetItemPlacementPosition(item.itemData.type, item.itemData.size, 50, tileByType);
				if (position != null)
				{
					items.Add(new Item((Vector2Int)position, item.itemData, roomNumberIter));
					List<Vector2Int> neighbors = GetNeighbours8Directions((Vector2Int)position, roomFloor);
					foreach (var neighbor in neighbors)
					{
						tileByType[item.itemData.type].Remove(neighbor);
					}
				}
			}
		}
	}

	private Dictionary<PlacementType, HashSet<Vector2Int>> CreateDictionaryTileByType(HashSet<Vector2Int> roomFloor,
		HashSet<Vector2Int> roomFloorNoCorridor)
	{
		Dictionary<PlacementType, HashSet<Vector2Int>>
			tileByType = new Dictionary<PlacementType, HashSet<Vector2Int>>();
		List<Vector2Int> floorList = new List<Vector2Int>(roomFloorNoCorridor);
		foreach (var position in floorList)
		{
			int neighboursCount8Dir = GetNeighbours8Directions(position, roomFloor).Count;
			PlacementType type = neighboursCount8Dir < 8 ? PlacementType.NearWall : PlacementType.OpenSpace;

			if (tileByType.ContainsKey(type) == false)
				tileByType[type] = new HashSet<Vector2Int>();
			
			if (type == PlacementType.NearWall && GetNeighbours4Directions(position, roomFloor).Count < 3)
				continue;
			tileByType[type].Add(position);
		}

		return tileByType;
	}

	private List<Vector2Int> GetNeighbours8Directions(Vector2Int startPosition, HashSet<Vector2Int> floor)
	{
		return GetNeighbours(startPosition, floor, Directions.allDirectionsList);
	}

	private List<Vector2Int> GetNeighbours4Directions(Vector2Int startPosition, HashSet<Vector2Int> floor)
	{
		return GetNeighbours(startPosition, floor, Directions.mainDirectionsList);
	}

	private List<Vector2Int> GetNeighbours(Vector2Int startPosition, HashSet<Vector2Int> floor, List<Vector2Int> neighboursOffsetList)
	{
		List<Vector2Int> neighbours = new List<Vector2Int>();
		foreach (var neighbourDirection in neighboursOffsetList)
		{
			Vector2Int potentialNeighbour = startPosition + neighbourDirection;
			if (floor.Contains(potentialNeighbour))
				neighbours.Add(potentialNeighbour);
		}

		return neighbours;
	}

	private Vector2Int? GetItemPlacementPosition(PlacementType placementType, Vector2Int size, int iterationsMax,
		Dictionary<PlacementType, HashSet<Vector2Int>> tileByType)
	{
		int itemArea = size.x * size.y;
		if (tileByType[placementType].Count < itemArea)
			return null;

		int iteration = 0;
		while (iteration < iterationsMax)
		{
			iteration++;
			int index = Random.Range(0, tileByType[placementType].Count);
			Vector2Int position = tileByType[placementType].ElementAt(index);

			if (itemArea > 1)
			{
				//var (result, placementPositions) = PlaceBigItem(position, size, addOffset);

			}
			else
			{
				tileByType[placementType].Remove(position);
			}

			return position;
		}

		return null;
	}
}

[System.Serializable]
public struct ItemParam
{
	public int minNumber;
	public int maxNumber;
	public ItemDataSO itemData;
}

[System.Serializable]
public class Item
{
	public ItemDataSO itemData;
	public Vector2Int center;
	public int roomNumber;

	public Item(Vector2Int center, ItemDataSO itemData, int roomNumber)
	{
		this.center = center;
		this.itemData = itemData;
		this.roomNumber = roomNumber;
	}
}
