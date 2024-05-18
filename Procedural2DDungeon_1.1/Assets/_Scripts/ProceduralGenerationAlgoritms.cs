using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.VersionControl.Asset;

public static class ProceduralGenerationAlgoritms
{
	public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startPosition, int walkLength)
	{
		HashSet<Vector2Int> path = new HashSet<Vector2Int>();

		path.Add(startPosition);
		var previousPosition = startPosition;

		for (int i = 0; i < walkLength; i++)
		{
			var newPosition = previousPosition + Direction2D.GetRandomCardinalDirection();
			path.Add(newPosition);
			previousPosition = newPosition;
		}

		return path;
	}

	public static List<Vector2Int> RandomWalkCorridor(Vector2Int startPosition, int corridorLength)
	{
		List<Vector2Int> corridor = new List<Vector2Int>();
		var direction = Direction2D.GetRandomCardinalDirection();
		var currentPosition = startPosition;
		corridor.Add(currentPosition);

		for (int i = 0; i < corridorLength; i++)
		{
			currentPosition += direction;
			corridor.Add(currentPosition);
		}

		return corridor;
	}

	public static List<BoundsInt> BinarySpacePartitioningBinaryTree(BinaryTree tree, int minWidth, int minHeight, int offset,
		bool divideLessMin, bool sizeRoomCanLessMin, bool isStochastic)
	{
		List<BoundsInt> roomsList = new List<BoundsInt>();
		if (isStochastic)
		{
			List<Node> spaces = new List<Node>();
			spaces.Add(tree.root);
			StochasticDivision(spaces, roomsList, minWidth, minHeight, offset, divideLessMin, sizeRoomCanLessMin);
		}
		else
		{
			Queue<Node> spacesQueue = new Queue<Node>();
			spacesQueue.Enqueue(tree.root);
			UniformDivision(spacesQueue, roomsList, minWidth, minHeight, offset, divideLessMin, sizeRoomCanLessMin);
		}
		
		return roomsList;
	}

	private static void StochasticDivision(List<Node> spaces, List<BoundsInt> roomsList, int minWidth, int minHeight, int offset,
		bool divideLessMin, bool sizeRoomCanLessMin)
	{
		while (spaces.Count > 0)
		{
			var space = spaces[Random.Range(0, spaces.Count)];
			spaces.Remove(space);
			if (Random.value < 0.5f)
			{
				if (space.area.size.y >= minHeight * 2)
				{
					List<BoundsInt> newRooms = SplitHorizontally(space.area, minHeight, offset, divideLessMin);
					CheckAfterSplitStochastic(newRooms, spaces, space, minWidth, minHeight, sizeRoomCanLessMin);
				}
				else if (space.area.size.x >= minWidth * 2)
				{
					List<BoundsInt> newRooms = SplitVertically(space.area, minWidth, offset, divideLessMin);
					CheckAfterSplitStochastic(newRooms, spaces, space, minWidth, minHeight, sizeRoomCanLessMin);
				}
				else
				{
					roomsList.Add(space.area);
				}
			}
			else
			{
				if (space.area.size.x >= minWidth * 2)
				{
					List<BoundsInt> newRooms = SplitVertically(space.area, minWidth, offset, divideLessMin);
					CheckAfterSplitStochastic(newRooms, spaces, space, minWidth, minHeight, sizeRoomCanLessMin);
				}
				else if (space.area.size.y >= minHeight * 2)
				{
					List<BoundsInt> newRooms = SplitHorizontally(space.area, minHeight, offset, divideLessMin);
					CheckAfterSplitStochastic(newRooms, spaces, space, minWidth, minHeight, sizeRoomCanLessMin);
				}
				else
				{
					roomsList.Add(space.area);
				}
			}
		}
	}

	private static void CheckAfterSplitStochastic(List<BoundsInt> newRooms, List<Node> spaces, Node space, int minWidth,
		int minHeight, bool sizeRoomCanLessMin)
	{
		if (sizeRoomCanLessMin)
		{
			Node leftNode = new Node(newRooms[0], space, false);
			Node rightNode = new Node(newRooms[1], space, true);
			space.LeftNode = leftNode;
			space.RightNode = rightNode;
			spaces.Add(leftNode);
			spaces.Add(rightNode);
		}
		else
		{
			if ((newRooms[0].size.x < minWidth || newRooms[0].size.y < minHeight) && newRooms[1].size.x >= minWidth &&
			    newRooms[1].size.y >= minHeight)
			{
				space.area = newRooms[1];
				spaces.Add(space);
			} 
			else if ((newRooms[1].size.x < minWidth || newRooms[1].size.y < minHeight) && newRooms[0].size.x >= minWidth &&
			      newRooms[0].size.y >= minHeight)
			{
				space.area = newRooms[0];
				spaces.Add(space);
			}
			else if (newRooms[1].size.x >= minWidth && newRooms[1].size.y >= minHeight && newRooms[0].size.x >= minWidth &&
			         newRooms[0].size.y >= minHeight)
			{
				Node leftNode = new Node(newRooms[0], space, false);
				Node rightNode = new Node(newRooms[1], space, true);
				space.LeftNode = leftNode;
				space.RightNode = rightNode;
				spaces.Add(leftNode);
				spaces.Add(rightNode);
			}
		}
	}

	private static void UniformDivision(Queue<Node> spacesQueue, List<BoundsInt> roomsList, int minWidth, int minHeight, int offset,
		bool divideLessMin, bool sizeRoomCanLessMin)
	{
		while (spacesQueue.Count > 0)
		{
			var space = spacesQueue.Dequeue();
			if (Random.value < 0.5f)
			{
				if (space.area.size.y >= minHeight * 2)
				{
					List<BoundsInt> newRooms = SplitHorizontally(space.area, minHeight, offset, divideLessMin);
					CheckAfterSplitUniform(newRooms, spacesQueue, space,minWidth, minHeight, sizeRoomCanLessMin);
				}
				else if (space.area.size.x >= minWidth * 2)
				{
					List<BoundsInt> newRooms = SplitVertically(space.area, minWidth, offset, divideLessMin);
					CheckAfterSplitUniform(newRooms, spacesQueue, space, minWidth, minHeight, sizeRoomCanLessMin);
				}
				else
				{
					roomsList.Add(space.area);
				}
			}
			else
			{
				if (space.area.size.x >= minWidth * 2)
				{
					List<BoundsInt> newRooms = SplitVertically(space.area, minWidth, offset, divideLessMin);
					CheckAfterSplitUniform(newRooms, spacesQueue, space, minWidth, minHeight, sizeRoomCanLessMin);

				}
				else if (space.area.size.y >= minHeight * 2)
				{
					List<BoundsInt> newRooms = SplitHorizontally(space.area, minHeight, offset, divideLessMin);
					CheckAfterSplitUniform(newRooms, spacesQueue, space, minWidth, minHeight, sizeRoomCanLessMin);
				}
				else
				{
					roomsList.Add(space.area);
				}
			}
		}
	}

	private static void CheckAfterSplitUniform(List<BoundsInt> newRooms, Queue<Node> spacesQueue, Node space, int minWidth,
		int minHeight, bool sizeRoomCanLessMin)
	{
		if (sizeRoomCanLessMin)
		{
			Node leftNode = new Node(newRooms[0], space, false);
			Node rightNode = new Node(newRooms[1], space, true);
			space.LeftNode = leftNode;
			space.RightNode = rightNode;
			spacesQueue.Enqueue(leftNode);
			spacesQueue.Enqueue(rightNode);
		}
		else
		{
			if ((newRooms[0].size.x < minWidth || newRooms[0].size.y < minHeight) && newRooms[1].size.x >= minWidth &&
			    newRooms[1].size.y >= minHeight)
			{
				space.area = newRooms[1];
				spacesQueue.Enqueue(space);
			}
			else if ((newRooms[1].size.x < minWidth || newRooms[1].size.y < minHeight) && newRooms[0].size.x >= minWidth &&
			         newRooms[0].size.y >= minHeight)
			{
				space.area = newRooms[0];
				spacesQueue.Enqueue(space);
			}
			else if (newRooms[1].size.x >= minWidth && newRooms[1].size.y >= minHeight && newRooms[0].size.x >= minWidth &&
			         newRooms[0].size.y >= minHeight)
			{
				Node leftNode = new Node(newRooms[0], space, false);
				Node rightNode = new Node(newRooms[1], space, true);
				space.LeftNode = leftNode;
				space.RightNode = rightNode;
				spacesQueue.Enqueue(leftNode);
				spacesQueue.Enqueue(rightNode);
			}
		}
	}

	public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int minHeight, 
		int offset, bool divideLessMin)
	{
		Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
		List<BoundsInt> roomsList = new List<BoundsInt>();
		roomsQueue.Enqueue(spaceToSplit);
		while (roomsQueue.Count > 0)
		{
			var room = roomsQueue.Dequeue();
			if (room.size.y >= minHeight && room.size.x >= minWidth)
			{
				if (Random.value < 0.5f)
				{
					if (room.size.y >= minHeight * 2)
					{
						List<BoundsInt> newRooms = SplitHorizontally(room, minHeight, offset, divideLessMin);
						roomsQueue.Enqueue(newRooms[0]);
						roomsQueue.Enqueue(newRooms[1]);
					}
					else if (room.size.x >= minWidth * 2)
					{
						List<BoundsInt> newRooms = SplitVertically(room, minWidth, offset, divideLessMin);
						roomsQueue.Enqueue(newRooms[0]);
						roomsQueue.Enqueue(newRooms[1]);
					}
					else
					{
						roomsList.Add(room);
					}
				}
				else
				{
					if (room.size.x >= minWidth * 2)
					{
						List<BoundsInt> newRooms = SplitVertically(room, minWidth, offset, divideLessMin);
						roomsQueue.Enqueue(newRooms[0]);
						roomsQueue.Enqueue(newRooms[1]);
					}
					else if (room.size.y >= minHeight * 2)
					{
						List<BoundsInt> newRooms = SplitHorizontally(room, minHeight, offset, divideLessMin);
						roomsQueue.Enqueue(newRooms[0]);
						roomsQueue.Enqueue(newRooms[1]);
					}
					else
					{
						roomsList.Add(room);
					}
				}
			}
		}

		return roomsList;
	}

	private static List<BoundsInt> SplitVertically(BoundsInt room, int minWidth, int offset, bool divideLessMin)
	{
		int xSplit;
		if (divideLessMin)
			xSplit = Random.Range(3 * offset, room.size.x - 3 * offset + 1);
		else
			xSplit = Random.Range(minWidth, room.size.x - minWidth + 1);

		BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(xSplit, room.size.y, room.size.z));
		BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z),
			new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));

		List<BoundsInt> newRooms = new List<BoundsInt>();
		newRooms.Add(room1);
		newRooms.Add(room2);
		return newRooms;
	}

	private static List<BoundsInt> SplitHorizontally(BoundsInt room, int minHeight, int offset, bool divideLessMin)
	{
		int ySplit;
		if (divideLessMin)
			ySplit = Random.Range(3 * offset, room.size.y - 3 * offset + 1);
		else
			ySplit = Random.Range(minHeight, room.size.y - minHeight + 1);


		BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
		BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z),
			new Vector3Int(room.size.x, room.size.y - ySplit, room.size.z));

		List<BoundsInt> newRooms = new List<BoundsInt>();
		newRooms.Add(room1);
		newRooms.Add(room2);
		return newRooms;
	}
}

public static class Direction2D
{
	public static List<Vector2Int> cardinalDirectionsList = new List<Vector2Int>
	{
		new Vector2Int(0, 1), //UP
		new Vector2Int(1, 0), //RIGHT
		new Vector2Int(0, -1), //DOWN
		new Vector2Int(-1, 0) //LEFT
	};

	public static List<Vector2Int> allDirectionsList = new List<Vector2Int>
	{
		new Vector2Int(0, 1), //UP
		new Vector2Int(1, 0), //RIGHT
		new Vector2Int(0, -1), //DOWN
		new Vector2Int(-1, 0), //LEFT
		new Vector2Int(1,1), //RIGHT-UP
		new Vector2Int(1, -1), //RIGHT-DOWN
		new Vector2Int(-1, -1), //LEFT-DOWN
		new Vector2Int(-1, 1) //LEFT-UP
	};

	public static Vector2Int GetRandomCardinalDirection()
	{
		return cardinalDirectionsList[Random.Range(0, cardinalDirectionsList.Count)];
	}
}
