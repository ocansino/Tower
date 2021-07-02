using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
	[Serializable]
	public class RandomRange
	{
		public int minimum; 			
		public int maximum; 			
		
		public RandomRange(int min, int max)
		{
			minimum = min;
			maximum = max;
		}
	}
	
	public int columns;
	public int rows;
	public GameObject exit;											//Prefab to spawn for exit tile
	public GameObject shop;                                         //Shop prefab
	public GameObject[] floorTiles;									//Array of floor tiles
	public GameObject[] foodTiles;									//Array of food tiles
	public GameObject[] enemyTiles;									//Array of enemys
	public GameObject[] outerWallTiles;								//Array of outer wall tiles
	public GameObject[] itemTiles;                                  //Array of loot item tiles
	public GameObject[] moneyTiles;                                 //Array of tiles that have purely monetary value
	
	public RandomRange foodCount = new RandomRange(1, 5);
	public RandomRange moneyCount = new RandomRange(5, 20);
	
	private Transform boardHolder;									//transform of Board object
	private List <Vector3> gridPositions = new List <Vector3> ();	//all possible locations to spawn stuff

	private int[] architecture;
	public int minHallWidth = 2;
	
	bool PlaceWall(int doorIndex, bool vert)
	{
		int par = vert ? columns : 1;
		int perp = vert ? 1 : columns;
		
		if (architecture[doorIndex] != 0) return false;

		// Ensure appropriate distance from another wall
		for (int i = 1; i <= minHallWidth; i++)
		{
			if (architecture[doorIndex - i * perp] != 0) return false;
			if (architecture[doorIndex + i * perp] != 0) return false;
		}

		// Check in advance for doors to prevent wall from hitting a door
		// TODO: maybe instead we could just move the door if we bump into it
		for (int i = doorIndex + par; architecture[i] <= 0; i += par)
		{
			if (architecture[i] < 0) return false;
		}
		
		for (int i = doorIndex - par; architecture[i] <= 0; i -= par)
		{
			if (architecture[i] < 0) return false;
		}
		
		// Place down the door.  I'm representing doors with -1.
		// The idea is that if there are other things I don't want to bump into, they can be negative as well, but stuff
		// like different kinds of walls can be positive, where it's fine if we bump into them, they just stop the wall.
		// But probably we will only ever have walls and doors so it doesn't really matter how we do it.
		architecture[doorIndex] = -1;

		for (int i = doorIndex + par; architecture[i] == 0; i += par)
		{
			architecture[i] = 1;
		}

		for (int i = doorIndex - par; architecture[i] == 0; i -= par)
		{
			architecture[i] = 1;
		}
			
		return true;
	}
	
	void GenerateRooms(int maxPartitions)
	{
		architecture = new int[rows * columns];

		// Setup the outer walls
		for (int x = 0; x < columns; x++)
		{
			architecture[x] = 1;
			architecture[architecture.Length - x - 1] = 1;
		}

		for (int y = 0; y < rows; y++)
		{
			architecture[y * columns] = 1;
			architecture[(y + 1) * columns - 1] = 1;
		}
		
		// Generate partitioning walls, alternating horizontal/vertical walls.
		bool vert = false;

		for (int i = 0; i < maxPartitions; i++)
		{
			int x = Random.Range(1, columns - 1);
			int y = Random.Range(1, rows - 1);
			
			// Make up to 3 attempts to place the wall before giving up
			for (int attempt = 0; attempt < 3; attempt++) {
				if (PlaceWall(x + y * columns, vert)) break;
			}
			vert = !vert;
		}
	}

	void GenerateEntranceRoom(int cx, int cy)
	{
		architecture = new int[rows * columns];
		for (int i = 0; i < architecture.Length; i++)
		{
			architecture[i] = 1;
		}

		for (int y = 0; y <= cy; y++)
		{
			architecture[cx + y * columns] = 0;
		}
		
		for (int dx = -3; dx <= 3; dx++)
		{
			for (int dy = -3; dy <= 3; dy++)
			{
				architecture[cx+dx + (cy+dy) * columns] = 0;
			}
		}
	}


	void PlaceArchitecture()
	{
		boardHolder = new GameObject ("Board").transform;
		
		for(int x = 0; x < columns; x++)
		{
			for(int y = 0; y < rows; y++)
			{
				int tile = architecture[x + y * columns];
				
				//Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
				GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];;
				if (tile == 0) // It's a floor
				{
					// We already have the texture, but must add to gridPositions to mark as a free space
					gridPositions.Add(new Vector3(x, y, 0));
				}
				else if (tile == 1) // It's a wall
				{
					toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
				}
				else if (tile == -1) {}  // TODO: maybe texture for the door?
				else Debug.LogError("wtf");
				
				GameObject instance =
					Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity) as GameObject;
				
				//Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
				instance.transform.SetParent(boardHolder);
			}
		}
	}
	
	// TODO: do this differently, at least make gridPositions local
	Vector3 RandomPosition()
	{
		int randomIndex = Random.Range(0, gridPositions.Count);
		Vector3 randomPosition = gridPositions[randomIndex];
		gridPositions.RemoveAt(randomIndex);
		return randomPosition;
	}

	Vector3 UsePosition(int x, int y)
	{
		for (int i = 0; i < gridPositions.Count; i++)
		{
			if (gridPositions[i].x == x && gridPositions[i].y == y)
			{
				Vector3 result = gridPositions[i];
				gridPositions.RemoveAt(i);
				return result;
			}
		}

		throw new ArgumentException("Position is already in use");
	}
	
	
	void ArrangeRandomly(GameObject[] tileArray, int minimum, int maximum)
	{
		int numObjects = Random.Range (minimum, maximum+1);
		
		for(int i = 0; i < numObjects; i++)
		{
			Vector3 randomPosition = RandomPosition();
			GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
			Instantiate(tileChoice, randomPosition, Quaternion.identity);
		}
	}

	void ArrangeRandomly(GameObject[] tileArray, int count)
	{
		ArrangeRandomly(tileArray, count, count);
	}

	void ArrangeRandomly(GameObject[] tileArray, RandomRange range)
	{
		ArrangeRandomly(tileArray, range.minimum, range.maximum);
	}

	public void SetupScene (int level)
	{
		gridPositions.Clear();

		if (level == 1)
		{
			int cx = columns / 2;
			int cy = 10;
			GenerateEntranceRoom(cx, cy);
			PlaceArchitecture();
			GameObject.Find("Player").transform.position = UsePosition(cx, 1);
			Instantiate (exit, UsePosition(cx, cy), Quaternion.identity);
			// for testing enemies
			//Instantiate(enemyTiles[0], UsePosition(cx - 1, cy - 3), Quaternion.identity);
			//Instantiate(enemyTiles[0], UsePosition(cx + 1, cy - 3), Quaternion.identity);
			ArrangeRandomly(foodTiles, 1);
			ArrangeRandomly(moneyTiles, 3);
			return;
		}

		GenerateRooms(10);
		PlaceArchitecture();

		// Place player and exit
		GameObject.Find("Player").transform.position = RandomPosition();
		Instantiate(exit, RandomPosition(), Quaternion.identity);
		
		if (level % 5 == 0)
			Instantiate(shop, RandomPosition(), Quaternion.identity);
		
		ArrangeRandomly(foodTiles, foodCount);
		ArrangeRandomly(moneyTiles, moneyCount);
		
		if (Random.Range(0, 100) < 20 && level > 3 || level == 3)
			ArrangeRandomly(itemTiles, 1);
		
		// Linear progression up to level 3, then logarithmic with 5 enemies on level 4
		int enemyCount;
		if (level <= 3)
			enemyCount = level - 1;
		else
			enemyCount = (int)(5 * Mathf.Log(level - 2, 2));

		//Put the enemies at randomized positions.
		ArrangeRandomly(enemyTiles, enemyCount);
	}
}