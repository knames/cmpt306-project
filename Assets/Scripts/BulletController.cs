﻿using UnityEngine;
using System.Collections;

/**
 * A simple bullet that destroys itself on collision, and damages an
 * enemy if it collided with one. This is kind of redundant with
 * DamageOnCollision, and shoulde eventually be removed.
 */ 
public class BulletController : MonoBehaviour {

	public int damage = 10;

	void OnCollisionEnter2D(Collision2D col) {
		EnemyHealth enemyHealth = col.gameObject.GetComponent<EnemyHealth> ();
		if (enemyHealth != null) {
			print ("zombie collided with bullet");
		 	enemyHealth.TakeDamage (damage);
		}
		Destroy (gameObject);
	}
}
