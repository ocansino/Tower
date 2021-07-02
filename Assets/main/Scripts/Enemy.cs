using UnityEngine;
using System.Collections;

public class Enemy : MovingObject
{
	public int playerDamage; 							//The amount of food points to subtract from the player when attacking.
	public AudioClip attackSound1;						//First of two audio clips to play when attacking the player.
	public AudioClip attackSound2;						//Second of two audio clips to play when attacking the player.
	
	
	private Animator animator;							//Variable of type Animator to store a reference to the enemy's Animator component.
	private Transform target;							//Transform to attempt to move toward each turn.
	private bool skipMove;								//Boolean to determine whether or not enemy should skip a turn or move this turn.
	
	
	protected override void Start ()
	{
		//Register this enemy with our instance of GameManager by adding it to a list of Enemy objects. 
		//This allows the GameManager to issue movement commands.
		GameManager.instance.AddEnemyToList(this);
		
		animator = GetComponent<Animator>();
		target = GameObject.FindGameObjectWithTag("Player").transform;

		base.Start();
	}

	private void OnDestroy()
	{
		GameManager.instance.DeleteEnemy(this);
	}

	public void MoveEnemy()
	{
		if (skipMove)
		{
			skipMove = false;
			return;
		}

		skipMove = true;

		int xDir = 0;
		int yDir = 0;
		
		float dx = target.position.x - transform.position.x;
		float dy = target.position.y - transform.position.y;

		if (Mathf.Abs(dx) > Mathf.Abs(dy))
			xDir = dx > 0 ? 1 : -1;
		else
			yDir = dy > 0 ? 1 : -1;

		RaycastHit2D hit;
		if (!Move(xDir, yDir, out hit))
		{
			Player hitPlayer = GetCollision<Player>(hit);
			if (hitPlayer != null)
			{
				hitPlayer.LoseHealth(playerDamage);
				animator.SetTrigger("enemyAttack");
				SoundManager.instance.RandomizeSfx(attackSound1, attackSound2);
				return;
			}
			
			// TODO: maybe switch to integers for keeping track of position, having transform coordinates be secondary?

			// Otherwise, we maybe hit something else, probably a wall or something
			// so try moving another direction if it makes sense
			if (xDir == 0 && Mathf.Abs(dx) > float.Epsilon)
			{
				xDir = dx > 0 ? 1 : -1;
				yDir = 0;
			}
			else if (yDir == 0 && Mathf.Abs(dy) > float.Epsilon)
			{
				yDir = dy > 0 ? 1 : -1;
				xDir = 0;
			}

			Move(xDir, yDir, out hit);
		}
	}
}

