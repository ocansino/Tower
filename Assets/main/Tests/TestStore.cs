using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TestStore
{
    private StoreMenu store;
    private Player player;
    
    public IEnumerator SetPlayerParams(int rubies, int health, int healthCap, int damage)
    {
        if (GameManager.instance == null)
        {
            SceneManager.LoadScene("main/test", LoadSceneMode.Single);
            AudioListener.volume = 0;
            yield return null;
        }
        
        store = GameObject.Find("Menus").GetComponent<StoreMenu>();
        player = GameObject.Find("Player").GetComponent<Player>();
        GameManager.instance.rubies = rubies;
        GameManager.instance.playerHealth = health;
        GameManager.instance.playerHealthCap = healthCap;
        GameManager.instance.playerDamage = damage;
        player.SyncFromGameManager();
    }

    [UnityTest]
    public IEnumerator TestBuyPotion()
    {
        yield return SetPlayerParams(35, 50, 100, 0);
        store.BuyPotion();
        player.SyncToGameManager();
        yield return null;
        Assert.AreEqual(70, GameManager.instance.playerHealth);
        Assert.AreEqual(5, GameManager.instance.rubies);
    }
    
    [UnityTest]
    public IEnumerator TestCantBuyPotion()
    {
        yield return SetPlayerParams(25, 50, 100, 0);
        store.BuyPotion();
        player.SyncToGameManager();
        yield return null;
        Assert.AreEqual(50, GameManager.instance.playerHealth);
        Assert.AreEqual(25, GameManager.instance.rubies);
    }
    
    [UnityTest]
    public IEnumerator TestBuyPotionDoesntExceedHealthCap()
    {
        yield return SetPlayerParams(30, 50, 60, 0);
        store.BuyPotion();
        player.SyncToGameManager();
        yield return null;
        Assert.AreEqual(60, GameManager.instance.playerHealth);
    }
    
    
    [UnityTest]
    public IEnumerator TestBuySword()
    {
        yield return SetPlayerParams(155, 100, 100, 3);
        store.BuySword();
        player.SyncToGameManager();
        yield return null;
        Assert.AreEqual(4, GameManager.instance.playerDamage);
        Assert.AreEqual(5, GameManager.instance.rubies);
    }
    
    [UnityTest]
    public IEnumerator TestCantBuySword()
    {
        yield return SetPlayerParams(25, 100, 100, 3);
        store.BuySword();
        player.SyncToGameManager();
        yield return null;
        Assert.AreEqual(3, GameManager.instance.playerDamage);
        Assert.AreEqual(25, GameManager.instance.rubies);
    }
    
    [UnityTest]
    public IEnumerator TestBuyHeartStone()
    {
        yield return SetPlayerParams(205, 50, 60, 0);
        player.SyncFromGameManager();
        store.BuyHeartStone();
        player.SyncToGameManager();
        yield return null;
        Assert.AreEqual(70, GameManager.instance.playerHealthCap);
        Assert.AreEqual(50, GameManager.instance.playerHealth);
        Assert.AreEqual(5, GameManager.instance.rubies);
    }
    
    [UnityTest]
    public IEnumerator TestCantBuyHeartStone()
    {
        yield return SetPlayerParams(25, 50, 60, 0);
        player.SyncFromGameManager();
        store.BuyHeartStone();
        player.SyncToGameManager();
        yield return null;
        Assert.AreEqual(60, GameManager.instance.playerHealthCap);
        Assert.AreEqual(50, GameManager.instance.playerHealth);
        Assert.AreEqual(25, GameManager.instance.rubies);
    }
}
