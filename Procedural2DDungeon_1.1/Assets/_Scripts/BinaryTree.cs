using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
	public Node LeftNode { get; set; }
	public Node RightNode { get; set; }
	public Node ParentNode { get; set; }
	public BoundsInt area;
	public int count;
	public bool isRightChildren;

	public Node(BoundsInt value, Node parentNode, bool rightChildren)
	{
		count = 0;
		LeftNode = null;
		RightNode = null;
		ParentNode = parentNode;
		isRightChildren = rightChildren;
		area = value;
	}
}
public class BinaryTree
{
	public Node root;

	public BinaryTree(BoundsInt area)
	{
		root = new Node(area , null, true);
	}
}
