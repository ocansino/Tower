using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TestItem
{
    private Player player;

    public IEnumerator SetPlayerParams(int rubies, int health, int healthCap, int damage)
    {
        if (GameManager.instance == null)
        {
            SceneManager.LoadScene("main/test", LoadSceneMode.Single);
            AudioListener.volume = 0;
            yield return null;
        }
        
        player = GameObject.Find("Player").GetComponent<Player>();
        GameManager.instance.rubies = rubies;
        GameManager.instance.playerHealth = health;
        GameManager.instance.playerHealthCap = healthCap;
        GameManager.instance.playerDamage = damage;
        player.SyncFromGameManager();
    }

    public T DummyItem<T>()
        where T:Item
    {
        GameObject obj = new GameObject();
        T item = obj.AddComponent<T>();
        item.collectSounds = new AudioClip[] { };
        return item;
    }

    [UnityTest]
    public IEnumerator TestHealthItem()
    {
        yield return SetPlayerParams(0, 40, 100, 0);
        HealthItem item = DummyItem<HealthItem>();
        item.healing = 42;
        item.Collect(player);
        player.SyncToGameManager();
        Assert.AreEqual(82, GameManager.instance.playerHealth);
    }
    
    [UnityTest]
    public IEnumerator TestRubyItem()
    {
        yield return SetPlayerParams(30, 40, 100, 0);
        RubyItem item = DummyItem<RubyItem>();
        item.value = 42;
        item.Collect(player);
        player.SyncToGameManager();
        Assert.AreEqual(72, GameManager.instance.rubies);
    }
    
    [UnityTest]
    public IEnumerator TestWeaponItem()
    {
        yield return SetPlayerParams(30, 40, 100, 3);
        WeaponItem item = DummyItem<WeaponItem>();
        item.Collect(player);
        player.SyncToGameManager();
        Assert.AreEqual(4, GameManager.instance.playerDamage);
    }
    
}
