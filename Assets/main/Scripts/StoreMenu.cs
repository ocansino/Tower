using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoreMenu : MonoBehaviour
{
    public GameObject storeMenuUI;
    public Player player;
    public int potionCost;
    public GameObject storePotion;
    public int weaponCost;
    public GameObject storeWeapon;
    public int heartStoneCost;
    public int heartStoneValue;
    
    public void Resume()
    {
        storeMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ShowShop()
    {
        storeMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void BuyPotion()
    {
        if (GameManager.instance.LoseRubies(potionCost))
        {
            storePotion.GetComponent<Item>().Collect(player);
        }
    }

    public void BuySword()
    {
        if (GameManager.instance.LoseRubies(weaponCost))
        {
            storeWeapon.GetComponent<Item>().Collect(player);
        }
    }

    public void BuyHeartStone()
    {
        if (GameManager.instance.LoseRubies(heartStoneCost))
        {
            player.IncreaseHealthCap(heartStoneValue);
        }
    }
}