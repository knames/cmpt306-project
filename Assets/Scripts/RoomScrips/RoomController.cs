﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomController : MonoBehaviour {

	public const int ROOM_WIDTH = 27;
	public const int ROOM_HEIGHT = 17;

	Player_Increase_Abilities player_skills;

	public GameObject door;
	public GameObject wall;

	bool isCompleted = false;

	delegate void StateDelegate();
	StateDelegate stateDelegate;


	Doors doors;

	List<GameObject> instantiatedDoors = new List<GameObject>();

	Transform enemies;
	
	// Use this for initialization
	void Start () {
		stateDelegate = WaitForPlayer;
		foreach (Transform child in transform) {
			if (child.name == "Enemies") {
				enemies = child;
			}
		}
		DisableEnemies ();
		
	}
	
	// Update is called once per frame
	void Update () {
		stateDelegate ();
	}

	public void Completed() {
		isCompleted = true;
	}

    public void Unfinished() {
        isCompleted = false;
    }

	void WaitForPlayer() {
		DisableEnemies ();
		if (playerInside ()) {
			GameObject camera = GameObject.FindGameObjectWithTag ("MainCamera");
			camera.GetComponent<smoothCam>().target = transform;
//			EnableChildren ();
			stateDelegate = PlayerEntered;
		}
	}

	void PlayerEntered () {
		if (!isCompleted) {
			print ("closing doors");
			InstantiateDoors ();
			stateDelegate = WaitForCompletion;
		} else {
			stateDelegate = WaitForPlayer;
		}
	}

	void WaitForCompletion () {
		print ("waiting for completion");
		EnableEnemies ();
		if (!playerInside ()) {
			stateDelegate = OpenDoors;
//			DisableChildren ();
		} else if (isCompleted) {
			player_skills = GameObject.Find("Player").GetComponent<Player_Increase_Abilities>();
			player_skills.increaseSkillPoints(1);
			stateDelegate = OpenDoors;
		}
	}

	void OpenDoors() {
		print ("opening doors");
		DestroyDoors ();
		stateDelegate = WaitForPlayer;
    }

	bool playerInside() {
		GameObject player = GameObject.FindGameObjectWithTag ("Player");
		Vector3 playerPosition = player.transform.position;
		Vector3 roomPosition = transform.position;
		return (playerPosition.x > roomPosition.x - ROOM_WIDTH / 2.0 + 1 &&
		        playerPosition.x < roomPosition.x + ROOM_WIDTH / 2.0 - 1 &&
		        playerPosition.y > roomPosition.y - ROOM_HEIGHT / 2.0 + 1 &&
		        playerPosition.y < roomPosition.y + ROOM_HEIGHT / 2.0 - 1);
	}

	public void SetDoors(Doors doors) {
		this.doors = doors;
		InstantiateWalls ();
	}

	void InstantiateWalls() {
		if (!doors.up) {
			GameObject wall = Instantiate (this.wall);
			Vector3 wallPosition = transform.position;
			wallPosition.y += ROOM_HEIGHT/2;
			wall.transform.position = wallPosition;
		}
		if (!doors.down) {
			GameObject wall = Instantiate (this.wall);
			Vector3 wallPosition = transform.position;
			wallPosition.y -= ROOM_HEIGHT/2;
			wall.transform.position = wallPosition;
		}
		if (!doors.left) {
			GameObject wall = Instantiate (this.wall);
			Vector3 wallPosition = transform.position;
			wallPosition.x -= ROOM_WIDTH/2;
			wall.transform.position = wallPosition;
		}
		if (!doors.right) {
			GameObject wall = Instantiate (this.wall);
			Vector3 wallPosition = transform.position;
			wallPosition.x += ROOM_WIDTH/2;
			wall.transform.position = wallPosition;
		}
	}

	void InstantiateDoors() {
		if (doors.up) {
			GameObject door = Instantiate (this.door);
			Vector3 doorPosition = transform.position;
			doorPosition.y += ROOM_HEIGHT/2;
			door.transform.position = doorPosition;
			instantiatedDoors.Add (door);
		}
		if (doors.down) {
			GameObject door = Instantiate (this.door);
			Vector3 doorPosition = transform.position;
			doorPosition.y -= ROOM_HEIGHT/2;
			door.transform.position = doorPosition;
			instantiatedDoors.Add (door);
		}
		if (doors.left) {
			GameObject door = Instantiate (this.door);
			Vector3 doorPosition = transform.position;
			doorPosition.x -= ROOM_WIDTH/2;
			door.transform.position = doorPosition;
			door.transform.Rotate(new Vector3(0,0,90));
			instantiatedDoors.Add (door);
		}
		if (doors.right) {
			GameObject door = Instantiate (this.door);
			Vector3 doorPosition = transform.position;
			doorPosition.x += ROOM_WIDTH/2;
			door.transform.position = doorPosition;
			door.transform.Rotate(new Vector3(0,0,90));
			instantiatedDoors.Add (door);
		}
	}

	void DestroyDoors() {
		while (instantiatedDoors.Count > 0) {
			GameObject door = instantiatedDoors[0];
			instantiatedDoors.RemoveAt (0);
			Destroy (door);
		}
	}

	void DisableEnemies() {
		if (enemies != null) {
			foreach (Transform child in enemies) {
				child.GetComponent<WalkToTarget> ().enabled = false;
			}
		}
	}

	void EnableEnemies() {
		if (enemies != null) {
			foreach (Transform child in enemies) {
				child.GetComponent<WalkToTarget> ().enabled = true;
			}
		}
	}
}
