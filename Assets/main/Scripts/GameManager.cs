using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public float levelStartDelay = 2f;						//Time to wait before starting level, in seconds.
	public float turnDelay = 0.1f;							//Delay between each Player turn.
	public int playerHealth;					     	//Starting value for Player health.
	public int playerHealthCap;
	public int playerDamage = 0;
	public static GameManager instance = null;				//Static instance of GameManager which allows it to be accessed by any other script.
	[HideInInspector] public bool playersTurn = true;		//Boolean to check if it's players turn, hidden in inspector but public.
	
	
	private Text levelText;									//Text to display current level number.
	private GameObject levelImage;							//Image to block out level as levels are being set up, background for levelText.
	private GameObject levelImageTitle;
	private GameObject Image;							//Image to block out level as levels are being set up, background for levelText.
	private GameObject menuPanel;
	private BoardManager boardScript;						//Store a reference to our BoardManager which will set up the level.
	public int level = 1;									//Current level number, expressed in game as "Day 1".
	private List<Enemy> enemies;							//List of all Enemy units, used to issue them move commands.
	private bool enemiesMoving;								//Boolean to check if enemies are moving.
	private bool doingSetup = true;							//Boolean to check if we're setting up board, prevent Player from moving during setup.
	private Text dmgText;
	private Text CurrText;
	[HideInInspector] public List<Vector2> destinationTiles; // places that things are trying to move to
	public int rubies = 0;
	string[] weapons = {
		"No weapon!",
		"Training sword",
		"Pocket knife",
		"Saber",
		"Longsword",
		"Vorpal blade",
		"Lightsaber"
	};
	
	void Awake()
	{
        //singleton
        if (instance == null) 
            instance = this;
		else if (instance != this)
            Destroy(gameObject);	
		
		DontDestroyOnLoad(gameObject);
		
		enemies = new List<Enemy>();
		boardScript = GetComponent<BoardManager>();

		InitGame();
	}

    //this is called only once, and the paramter tell it to be called only after the scene was loaded
    //(otherwise, our Scene Load callback would be called the very first load, and we don't want that)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static public void CallbackInitialization()
    {
        //register the callback to be called everytime the scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        instance.Image = GameObject.Find("Image");
		instance.menuPanel = GameObject.Find("menuPanel");
        instance.Image.SetActive(false);
		instance.menuPanel.SetActive(false);
        instance.level++;
        instance.InitGame();
    }

    public void GainRubies(int num)
    {
    	rubies += num;
    	CurrText = GameObject.Find("CurrText").GetComponent<Text>();
		CurrText.text = "Rubies: " + rubies;
    }

    public bool LoseRubies(int num)
    {
	    if (rubies >= num)
	    {
		    rubies -= num;
		    CurrText = GameObject.Find("CurrText").GetComponent<Text>();
		    CurrText.text = "Rubies: " + rubies;
		    return true;
	    }

	    return false;
    }
	
	void InitGame()
	{
		doingSetup = true;
		
		//title screen setup
		levelImageTitle = GameObject.Find("LevelImageTitle");
		levelImageTitle.SetActive(true);
		Invoke("HideTitle", levelStartDelay);

		levelImage = GameObject.Find("LevelImage");
		levelText = GameObject.Find("LevelText").GetComponent<Text>();
		levelText.text = "Floor: " + level;
		levelImage.SetActive(true);
		
		Invoke("HideLevelImage", levelStartDelay);
		//Clear any Enemy objects in our List to prepare for next level.
		//rubies += 10;
		CurrText = GameObject.Find("CurrText").GetComponent<Text>();
		CurrText.text = "Rubies: " + rubies;
		enemies.Clear();
		
		boardScript.SetupScene(level);
	}
	
	void HideTitle(){
		levelImageTitle.SetActive(false);
	}
	
	void HideLevelImage()
	{
		levelImage.SetActive(false);
		
		// allow player to move again.
		doingSetup = false;
	}

	public string GetWeapon(int damage)
	{
		return weapons[damage < weapons.Length? damage : weapons.Length - 1];
	}
	
	void Update()
	{
		if(playersTurn || enemiesMoving || doingSetup || destinationTiles.Count != 0)
			return;

		//Start moving enemies.
		StartCoroutine(MoveEnemies());
	}
	
	public void AddEnemyToList(Enemy script)
	{
		enemies.Add(script);
	}
	
	public void DeleteEnemy(Enemy script){
		enemies.Remove(script);
	}
	
	public void GameOver()
	{
		levelText.text = "After " + level + " floors, \nyou died.\nYou collected \n" + rubies + " rubies";
		levelImage.SetActive(true);
		levelImage.transform.Find("GameOverQuitButton").gameObject.SetActive(true);
		
		//Disable this GameManager.
		enabled = false;
	}
	
	IEnumerator MoveEnemies()
	{
		//While enemiesMoving is true player is unable to move.
		enemiesMoving = true;
	
		yield return new WaitForSeconds(turnDelay);
		
		//If there are no enemies spawned (IE in first level):
		if (enemies.Count == 0) 
		{
			//Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
			yield return new WaitForSeconds(turnDelay);
		}
		
		for (int i = 0; i < enemies.Count; i++)
		{
			enemies[i].MoveEnemy();
		}

		while (true)
		{
			bool stillMoving = false;
			for (int i = 0; i < enemies.Count; i++)
			{
				if (enemies[i].isMoving) stillMoving = true;
			}

			if (!stillMoving) break;
			yield return null;
		}
		
		// Enemies done moving
		playersTurn = true;
		enemiesMoving = false;
	}
}

