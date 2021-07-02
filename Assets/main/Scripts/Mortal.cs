using UnityEngine;
using System.Collections;

public class Mortal : MonoBehaviour
{
	public AudioClip dmgSound1;				    //1 of 2 audio clips that play when attacked by the player.
	public AudioClip dmgSound2;				    //2 of 2 audio clips that play when attacked by the player.
	public Sprite dmgSprite;					//Alternate sprite to display after Mortal has been attacked by player.
	public int hp = 3;							//hit points for the wall.
	
	
	private SpriteRenderer spriteRenderer;		//Store a component reference to the attached SpriteRenderer.

	void Awake ()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}
	
	// TODO: relate to Player LoseHealth?
	public void Damage(int loss)
	{
		SoundManager.instance.RandomizeSfx(dmgSound1, dmgSound2);

		// TODO: outdated
		spriteRenderer.sprite = dmgSprite;
		
		hp -= loss;
		if (hp <= 0)
		{
			GameManager.instance.GainRubies(10);
			Destroy(gameObject);
		}
	}
}
