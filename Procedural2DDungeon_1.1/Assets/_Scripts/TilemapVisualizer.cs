using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Tile = UnityEngine.Tilemaps.Tile;

public class TilemapVisualizer : MonoBehaviour
{
	[Header("Floor")]
	[SerializeField] private Tilemap floorTilemap;
	[SerializeField] private TileBase floorTile;

	[Header("Walls")]
	[SerializeField] private Tilemap wallTilemap;
	[SerializeField] private TileBase wallTop;

	[Header("RuleTile")]
	[SerializeField] private Tilemap ruleTilemap;
	[SerializeField] private TileBase ruleTile;

	[Header("Corridor")]
	[SerializeField] private Tilemap corridorTilemap;

	[Header("TypeRoom")]
	[SerializeField] private Tilemap typeRoomTilemap;
	[SerializeField] private TileBase TileForPlayerRoom;
	[SerializeField] private TileBase TileForEnemiesRoom;
	[SerializeField] private TileBase TileForResourcesRoom;
	[SerializeField] private TileBase TileForTreasureRoom;
	[SerializeField] private TileBase TileForKeyRoom;
	[SerializeField] private TileBase TileForExitRoom;

	[Header("Heat map")]
	[SerializeField] private Tilemap heatTilemap;
	[SerializeField] private TileBase heatMapTile;

	[Header("Items")]
	[SerializeField] private Tilemap itemsTilemap;

	private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
	{
		foreach (var position in positions)
		{
			PaintSingleTile(tilemap, tile, position);
		}
	}

	private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
	{
		var tilePosition = tilemap.WorldToCell((Vector3Int)position);
		tilemap.SetTile(tilePosition, tile);
	}

	public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
	{
		PaintTiles(floorPositions, floorTilemap, floorTile);
	}

	public void PaintCorridorTiles(IEnumerable<Vector2Int> corridorPositions)
	{
		PaintTiles(corridorPositions, corridorTilemap, floorTile);
	}

	public void PaintAllTilesWithRule(IEnumerable<Vector2Int> positions)
	{
		PaintTiles(positions, ruleTilemap, ruleTile);
	}

	public void PaintHeatMap(IEnumerable<Tuple<Vector2Int, int>> corridorPositionsWithDistance, int maxDistance)
	{
		foreach (var position in corridorPositionsWithDistance)
		{
			var tilePosition = heatTilemap.WorldToCell((Vector3Int)position.Item1);
			heatTilemap.SetTile(tilePosition, heatMapTile);
			heatTilemap.SetTileFlags(tilePosition, TileFlags.None);
			//heatTilemap.SetColor(tilePosition, new Color(1, 0, 0));
			heatTilemap.SetColor(tilePosition, new Color(position.Item2 / (float)maxDistance, 1 - (position.Item2 / (float)maxDistance), 0));
		}
	}

	public void Clear()
	{
		floorTilemap.ClearAllTiles();
		corridorTilemap.ClearAllTiles();
		wallTilemap.ClearAllTiles();
		ruleTilemap.ClearAllTiles();
		typeRoomTilemap.ClearAllTiles();
		heatTilemap.ClearAllTiles();
		itemsTilemap.ClearAllTiles();
	}

	public void PaintSingleBasicWall(Vector2Int position)
	{
		PaintSingleTile(wallTilemap, wallTop, position);
	}

	public void PaintTypeRooms(List<Room> rooms)
	{
		foreach (var room in rooms)
		{
			//print(room.type + " " + room.distanceFromStart);
			if (room.type == RoomType.Player)
				PaintSingleTile(typeRoomTilemap, TileForPlayerRoom, room.center);
			else if (room.type == RoomType.Exit)
				PaintSingleTile(typeRoomTilemap, TileForExitRoom, room.center);
			else if (room.type == RoomType.Key)
				PaintSingleTile(typeRoomTilemap, TileForKeyRoom, room.center);
			else if (room.type == RoomType.Treasure)
				PaintSingleTile(typeRoomTilemap, TileForTreasureRoom, room.center);
			else if (room.type == RoomType.Enemies)
				PaintSingleTile(typeRoomTilemap, TileForEnemiesRoom, room.center);
			else if (room.type == RoomType.Resources)
				PaintSingleTile(typeRoomTilemap, TileForResourcesRoom, room.center);
		}
	}

	public void PaintItemsTiles(List<Item> items)
	{
		foreach (var item in items)
		{
			PaintSingleTile(itemsTilemap, item.itemData.tile, item.center);
		}
	}
}
