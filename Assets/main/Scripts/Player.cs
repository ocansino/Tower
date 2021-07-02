using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Player : MovingObject
{
	public float restartLevelDelay = 1f;		//Delay time in seconds to restart level.
	public Text foodText;						//UI Text to display current player food total.
	public Text dmgText;
	public AudioClip gameOverSound;				//Audio clip to play when player dies.
	private Animator animator;					//Used to store a reference to the Player's animator component.
	private int health;                         //Used to store player health during level.
	private int healthCap;
	public static int attackDamage;				    //How much damage a player does.
	private bool inShop = false;
	private bool preferHoriz = true;
	
	protected override void Start ()
	{
		animator = GetComponent<Animator>();
		
		SyncFromGameManager();
		
		foodText.text = "Health: " + health + "/" + healthCap;
		dmgText.text = "Equipped: " + GameManager.instance.GetWeapon(attackDamage);
		base.Start ();
	}

	public void SyncFromGameManager()
	{
		health = GameManager.instance.playerHealth;
		attackDamage = GameManager.instance.playerDamage;
		healthCap = GameManager.instance.playerHealthCap;
	}

	public void SyncToGameManager()
	{
		GameManager.instance.playerHealth = health;
		GameManager.instance.playerDamage = attackDamage;
		GameManager.instance.playerHealthCap = healthCap;
	}
	
	private void OnDisable ()
	{
		SyncToGameManager();
	}
	
	
	private void Update ()
	{
		if (!GameManager.instance.playersTurn || isMoving) return;

		int horizontal = (int) Input.GetAxisRaw("Horizontal");
		int vertical = (int) Input.GetAxisRaw("Vertical");
		
		// Only allow one move at a time, choose which one depending on preferHoriz
		if (horizontal != 0 && vertical != 0)
		{
			if (preferHoriz) vertical = 0;
			else horizontal = 0;
			preferHoriz = !preferHoriz;
		}
		
		if (horizontal != 0 || vertical != 0)
		{
			foodText.text = "Health: " + health + "/" + healthCap;
			dmgText.text = "Equipped: " + GameManager.instance.GetWeapon(attackDamage);

			// move sounds are annoying so I got rid of them
			//SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
			RaycastHit2D hit;
			if (!Move(horizontal, vertical, out hit))
			{
				Mortal toAttack = GetCollision<Mortal>(hit);
				if (toAttack != null)
				{
					toAttack.Damage(attackDamage);
					animator.SetTrigger("playerAttack");
				}
			}
			
			GameManager.instance.playersTurn = false;
		}
	}
	
	public void GainHealth(int healing)
	{
		health += healing;
		if (health > healthCap) health = healthCap;
		foodText.text = "+" + healing + " Health: " + health + "/" + healthCap;
	}

	public void IncreaseHealthCap(int amount)
	{
		healthCap += amount;
		foodText.text = "Health: " + health + "/" + healthCap;
	}

	public void UpgradeWeapon()
	{
		attackDamage++;
		dmgText.text = "Equipped: " + GameManager.instance.GetWeapon(attackDamage);
	}
	
	private void OnTriggerEnter2D (Collider2D other)
	{
		if(other.tag == "Exit")
		{
			Invoke ("Restart", restartLevelDelay);
			
			//Disable the player object since level is over.
			enabled = false;
		}
		
		else if (other.tag == "Item")
		{
			other.gameObject.GetComponent<Item>().Collect(this);
			other.gameObject.SetActive(false);
		}
		
		else if (other.tag == "Shop" && !inShop)
		{
			// the inShop thing is just because it gets triggered when moving out of a shop
			// there's probably a better way to fix this.
			inShop = true;
			StoreMenu menu = GameObject.Find("Menus").GetComponent<StoreMenu>();
			menu.ShowShop();
		}
		
		else
		{
			inShop = false;
		}
	}
	
	
	private void Restart ()
	{
		//Load the last scene loaded, in this case Main, the only scene in the game. And we load it in "Single" mode so it replace the existing one
        //and not load all the scene object in the current scene.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
	}
	
	
	public void LoseHealth (int loss)
	{
		animator.SetTrigger("playerHit");
		health -= loss;
		foodText.text = "-"+ loss + " Health: " + health + "/" + healthCap;
		CheckIfGameOver();
	}
	
	private void CheckIfGameOver()
	{
		if (health <= 0) 
		{
			SoundManager.instance.PlaySingle(gameOverSound);
			SoundManager.instance.musicSource.Stop();
			GameManager.instance.GameOver();
		}
	}
}