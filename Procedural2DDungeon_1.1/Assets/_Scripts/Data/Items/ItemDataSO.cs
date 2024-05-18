using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Item_", menuName = "PCG/ItemData")]
public class ItemDataSO : ScriptableObject
{
	public TileBase tile;
	public Vector2Int size;
	public PlacementType type;
}
