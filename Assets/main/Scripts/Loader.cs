using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour 
{
	public GameObject gameManager;			//GameManager prefab to instantiate.
	public GameObject soundManager;			//SoundManager prefab to instantiate.

	void Awake ()
	{
		if (GameManager.instance == null)
			Instantiate(gameManager);
		
		if (SoundManager.instance == null)
			Instantiate(soundManager);
	}
}