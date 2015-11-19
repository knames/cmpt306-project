﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Procedurally generates a level from the given sets of rooms to the specified size.
 */
public class LevelGenerator : MonoBehaviour {

	/**
	 * Basically an index pair for the room and door arrays, makes the code a bit cleaner.
	 */ 
	public class Vector2I {
		public int x, y;
		public Vector2I(int x, int y) {
			this.x = x;
			this.y = y;
		}
	}

	public int numRooms = 10;
	int currNumRooms = 0;

	public GameObject[,] rooms;
	public Doors[,] doors;
	
	public GameObject[] SpawnRooms;
	/** Probability that a puzzle room is generated. */
	public float pPuzzleRoom = 0.0f;
	public GameObject[] PuzzleRooms;
	/** Probability that an obstacle room is generated. */
	public float pObstacleRoom = 0.5f;
	public GameObject[] ObstacleRooms;
	/** Probability that a battle room is generated. */
	public float pBattleRoom = 0.3f;
	public GameObject[] BattleRooms;
	/** Probability that a rest room is generated. */
	public float pRestRoom = 0.2f;
	public GameObject[] RestRooms;
	public GameObject[] BossRooms;

	public GameObject door;
	public GameObject wall;

	public int roomWidth = 27;
	public int roomHeight = 17;

	int direction = 0;
	
	public HashSet<Vector2I> usedPositions = new HashSet<Vector2I> ();
	public HashSet<Vector2I> availablePositions = new HashSet<Vector2I> ();

	// Use this for initialization
	void Start () {
		rooms = new GameObject[numRooms * 2 + 1, numRooms * 2 + 1];
		doors = new Doors[numRooms * 2 + 1, numRooms * 2 + 1];
		Random.seed = System.Environment.TickCount;
		Generate ();
	}

	/**
	 * Randomly generates a set of connected rooms.
	 */
	void Generate() {
		int currNumPuzzleRooms = 0;
		int currNumObstacleRooms = 0;
		int currNumBattleRooms = 0;
		int currNumRestRooms = 0;

		Vector2I center = new Vector2I (numRooms / 2, numRooms / 2);
		SetRoom (center, RandomRoom (SpawnRooms));
		AddAvailableRooms (center);
		SetDoors (center, new Doors());
		currNumRooms++;

		while (currNumRooms < numRooms) {
			AddRooms(RandomDirection (), Random.Range (1, (int) Mathf.Sqrt(numRooms)));
		}

		Vector2I position = RandomUsedPosition ();
		Direction direction = RandomDirection ();
		do {
			position = GetNextPosition (direction, position);
		} while (GetRoom (position) != null);
		SetRoom (position, RandomRoom (BossRooms));
		AddAvailableRooms (position);
		SetDoors (position, new Doors());
		GetDoors (GetPrevPosition(direction, position)).AddDoor (direction);
		GetDoors (position).AddDoor (DirectionMethods.OppositeDirection(direction));

		InstantiateRooms ();
	}

	/** Adds a line of rooms off of a randomly selected room in the given direction. */
	void AddRooms(Direction direction, int length) {
		Vector2I position = RandomUsedPosition ();
		int addedRooms = 0;
		while (addedRooms < length) {
			position = GetNextPosition (direction, position);
			if (!RoomInRange (position)) {
				break;
			} else if (GetRoom (position) == null) {
				GameObject room;
				float rand = Random.Range (0f, 1f);
				if (rand < pPuzzleRoom) {
					room = RandomRoom (PuzzleRooms);
				} else if (rand < pPuzzleRoom + pObstacleRoom) {
					room = RandomRoom (ObstacleRooms);
				} else if (rand < pPuzzleRoom + pObstacleRoom + pBattleRoom) {
					room = RandomRoom (BattleRooms);
				} else {
					room = RandomRoom (RestRooms);
				}
				SetRoom (position, room);
				AddAvailableRooms (position);

				SetDoors (position, new Doors());
				GetDoors (GetPrevPosition(direction, position)).AddDoor (direction);
				GetDoors (position).AddDoor (DirectionMethods.OppositeDirection(direction));

				currNumRooms++;
				addedRooms++;
			}
		}
	}
	
	/**
	 * Adds adjacent positions as potential room locations, and
	 * marks the given position as a used position.
	 */
	void AddAvailableRooms(Vector2I position) {
		usedPositions.Add (position);
		Vector2I up = GetNextPosition (Direction.UP, position);
		if (RoomInRange (up) && GetRoom (up) == null) {
			availablePositions.Add(up);
		}
		Vector2I down = GetNextPosition (Direction.DOWN, position);
		if (RoomInRange (down) && GetRoom (down) == null) {
			availablePositions.Add (down);
		}
		Vector2I left = GetNextPosition (Direction.LEFT, position);
		if (RoomInRange (left) && GetRoom (left) == null) {
			availablePositions.Add (left);
		}
		Vector2I right = GetNextPosition (Direction.RIGHT, position);
		if (RoomInRange (right) && GetRoom (right) == null) {
			availablePositions.Add (right);
		}
	}

	/**
	 * Given a direciton and position, returns the next position in that direction.
	 */ 
	Vector2I GetNextPosition(Direction direction, Vector2I position) {
		switch (direction) {
		case Direction.UP:
			return new Vector2I(position.x, position.y+1);
		case Direction.DOWN:
			return new Vector2I(position.x, position.y-1);
		case Direction.LEFT:
			return new Vector2I(position.x-1, position.y);
		case Direction.RIGHT:
			return new Vector2I(position.x+1, position.y);
		}
		return null;
	}

	/**
	 * Given a direciton and position, returns the previous position in that direction.
	 */
	Vector2I GetPrevPosition(Direction direction, Vector2I position) {
		return GetNextPosition(DirectionMethods.OppositeDirection(direction), position);
	}

	bool RoomInRange(Vector2I position) {
		return position.x >= 0 && position.x < rooms.GetLength (0) &&
			position.y >= 0 && position.y < rooms.GetLength (1);
	}

	GameObject GetRoom(Vector2I position) {
		return rooms [position.x, position.y];
	}

	void SetRoom(Vector2I position, GameObject room) {
		rooms [position.x, position.y] = room;
	}

	Doors GetDoors(Vector2I position) {
		return this.doors [position.x, position.y];
	}

	void SetDoors(Vector2I position, Doors doors) {
		this.doors [position.x, position.y] = doors;
	}

	/**
	 * Randomly selects a used position.
	 */
	Vector2I RandomUsedPosition() {
		Vector2I[] usedPositionArray = new Vector2I[usedPositions.Count];
		usedPositions.CopyTo (usedPositionArray);
		return usedPositionArray [Random.Range (0, usedPositionArray.Length)];
	}

	/**
	 * Randomly selects an available position.
	 */
	Vector2I RandomAvailablePosition() {
		Vector2I[] availablePositionArray = new Vector2I[availablePositions.Count];
		availablePositions.CopyTo (availablePositionArray);
		return availablePositionArray [Random.Range (0, availablePositionArray.Length)];
	}

	/**
	 * Given a set of rooms, returns a random room from the set.
	 */
	GameObject RandomRoom(GameObject[] rooms) {
		int i = Random.Range (0, rooms.Length);
		print (i);
		return rooms[i];
	}

	/**
	 * Returns a direction. I found that predictable rotation gave better
	 * variety, so it isn't currently random.
	 */ 
	Direction RandomDirection() {
		direction++;
		return (Direction) (direction % 4);
	}

	/**
	 * Instantiates all of the rooms, with central room placed at (0,0,0).
	 */
	void InstantiateRooms() {
		for (int i=0; i<rooms.GetLength (0); i++) {
			for (int j=0; j<rooms.GetLength (1); j++) {
				if (rooms[i, j] != null) {
					GameObject room = Instantiate (rooms[i, j]);
					Vector3 position = new Vector3((i-numRooms/2)*(roomWidth), (j-numRooms/2)*(roomHeight), 0);
					room.transform.position = position;
					Doors doors = this.doors [i, j];
					room.GetComponent<RoomController>().SetDoors(doors);
				}
			}
		}
	}
}
