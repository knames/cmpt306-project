﻿using UnityEngine;
using System.Collections;

public class shootObjectAt : MonoBehaviour {

	public bool useRigidbody2d = false; 
	public Transform projectileSpawn; //offset position for projectile to spawn(so it doesn't spawn in the turret)
	public GameObject projectile;
	int projectileLayer;
	public Vector3 projectileOffset = new Vector3 (0f, 0f, 0);
	public Transform target = null; // can be null, will just fire straight out
	public bool isHeatSeaking = false; // true if you want it to chase the player
	public bool isRotating = false; // true if you want this to rotate 
	public bool projectileFacingPlayer = false; // create the object pointing at the player instead of in front of the shooter

	public bool damage_player = true;
	public bool damage_enemy = false;
	public bool ignore_enemy_col = true;

	public int damage = 10;
	public float expireTime = 2.0f;
	public float speed = 3.0f;
	float cooldownTimer = 1;
	public float fireDelay = 5; // maybe this could be randomized over a range of time in the future?

	
	GameObject[] orbiters;
	
	// Use this for initialization
	void Start () {
		target = GameObject.FindGameObjectWithTag ("Player").transform;
		projectileLayer = gameObject.layer;
	}
	
	// Update is called once per frame
	void Update () {
		Quaternion newRot;
		if (isRotating) // rotate to the player
		{
			newRot = Quaternion.LookRotation(transform.position - target.position, Vector3.forward);
			newRot.x = 0;
			newRot.y = 0;
			transform.rotation = Quaternion.Lerp(transform.rotation, newRot, Time.deltaTime * 8);
		}
		cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0) {
			Vector3 offset = transform.rotation * projectileOffset;
			GameObject projectileGo;
            if (projectileFacingPlayer)
			{
				newRot = Quaternion.LookRotation(transform.position - target.position, Vector3.forward);
				newRot.x = 0;
				newRot.y = 0;
				if(this.projectileSpawn != null)
						projectileGo = (GameObject)Instantiate(projectile, new Vector3(this.projectileSpawn.position.x,this.projectileSpawn.position.y, this.projectileSpawn.position.z), newRot);
				else
					projectileGo = (GameObject)Instantiate(projectile, transform.position + offset, newRot);
				if(this.useRigidbody2d)
					projectileGo.AddComponent<Rigidbody2D>();
				
			}
			else
			{
				if(this.projectileSpawn != null)
						projectileGo = (GameObject)Instantiate(projectile, new Vector3(this.projectileSpawn.position.x,this.projectileSpawn.position.y, this.projectileSpawn.position.z), transform.rotation);
				else
					projectileGo = (GameObject)Instantiate(projectile, transform.position + offset, transform.rotation);
				if(this.useRigidbody2d)
					projectileGo.AddComponent<Rigidbody2D>();
				
			}
			projectileGo.layer = projectileLayer;
			projectileGo.AddComponent<projectile>();
			projectileGo.AddComponent<DestroySelfOnCollision>();
			projectileGo.AddComponent<damageOnCollission>();
			projectileGo.GetComponent<damageOnCollission>().setDamage(damage);
			projectileGo.GetComponent<damageOnCollission>().damage_enemy = damage_enemy;
			projectileGo.GetComponent<damageOnCollission>().damage_player = damage_player;
			projectile projectileController = projectileGo.GetComponent<projectile>();
			if (isHeatSeaking)
				projectileController.heatTarget = target;
			projectileController.maxSpeed = speed;
			if (projectile.GetComponent<MultiOrbitController>()){
				MultiOrbitController time = projectileGo.GetComponent<MultiOrbitController>();
				time.expire_time = expireTime;
			}
			Destroy (projectileGo,expireTime);
			cooldownTimer = fireDelay;
		}

	}
}
