using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "RandomWalkRoomParameters_",menuName = "PCG/RandomWalkRoomData")]
public class RandomWalkRoomSO : ScriptableObject
{
	public int iterations = 10, walkLength = 10;
	public bool startRandomlyEachIteration = true;
}
