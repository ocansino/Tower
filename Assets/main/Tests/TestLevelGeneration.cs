using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TestLevelGeneration
{
    private Transform boardHolder;
    private BoardManager bm;
    
    private IEnumerator LoadLevel(int level)
    {
        if (GameManager.instance == null)
        {
            SceneManager.LoadScene("main/test", LoadSceneMode.Single);
            AudioListener.volume = 0;
            yield return null;
        }

        GameManager.instance.level = level - 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        AudioListener.volume = 0;
        yield return null;
        
        bm = GameManager.instance.GetComponent<BoardManager>();
        boardHolder = GameObject.Find("Board").transform;
    }

    private bool[] MapWalls()
    {
        int blockingLayer = LayerMask.NameToLayer("BlockingLayer");
        Assert.AreNotEqual(-1, blockingLayer);

        bool[] result = new bool[boardHolder.childCount];
        for (int i = 0; i < boardHolder.childCount; i++)
        {
            Transform tile = boardHolder.GetChild(i);
            int mapIdx = (int)tile.position.x + (int)tile.position.y * bm.columns;
            result[mapIdx] = tile.gameObject.layer == blockingLayer;
        }

        return result;
    }

    private int CountItems<T>()
        where T : Item
    {
        int count = 0;
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Item"))
        {
            T weapon = obj.GetComponent<T>();
            if (weapon != null) count++;
        }

        return count;
    }

    [UnityTest]
    public IEnumerator TestCorrectNumberOfTiles()
    {
        yield return LoadLevel(42);
        Assert.AreEqual(bm.columns*bm.rows, boardHolder.childCount);
    }
    
    [UnityTest]
    public IEnumerator TestPerimeterIsAllWall()
    {
        yield return LoadLevel(42);
        bool[] map = MapWalls();

        for (int x = 0; x < bm.columns; x++)
        {
            Assert.IsTrue(map[x]);
            Assert.IsTrue(map[map.Length - x - 1]);
        }

        for (int y = 0; y < bm.rows; y++)
        {
            Assert.IsTrue(map[y*bm.columns]);
            Assert.IsTrue(map[y*bm.columns + bm.columns - 1]);
        }
    }

    [UnityTest]
    public IEnumerator TestPlayerDoesNotStartAtWall()
    {
        yield return LoadLevel(42);
        bool[] map = MapWalls();
        Vector3 playerPos = GameObject.Find("Player").transform.position;
        Assert.IsFalse(map[(int)playerPos.x + (int)playerPos.y * bm.columns]);
    }

    [UnityTest]
    public IEnumerator TestFloorsAreInterconnected()
    {
        yield return LoadLevel(42);
        bool[] map = MapWalls();

        int floorCount = 0;
        for (int i = 0; i < map.Length; i++)
        {
            if (!map[i]) floorCount++;
        }

        Queue<Vector2Int> front = new Queue<Vector2Int>();
        HashSet<Vector2Int> found = new HashSet<Vector2Int>();
        Vector3 playerPos = GameObject.Find("Player").transform.position;
        Vector2Int start = new Vector2Int((int) playerPos.x, (int) playerPos.y);
        front.Enqueue(start);
        found.Add(start);

        Vector2Int[] allowedMovements = {Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};
        int accessibleFloors = 1;
        while (front.Count != 0)
        {
            Vector2Int cur = front.Dequeue();
            foreach (Vector2Int dir in allowedMovements)
            {
                Vector2Int check = cur + dir;
                if (!map[check.x + bm.columns * check.y] && !found.Contains(check))
                {
                    front.Enqueue(check);
                    found.Add(check);
                    accessibleFloors++;
                }
            }
        }
        
        Assert.AreEqual(floorCount, accessibleFloors);
    }
    
    [UnityTest]
    public IEnumerator TestNoWeaponsOnLevel1()
    {
        yield return LoadLevel(1);
        Assert.AreEqual(0,CountItems<WeaponItem>());
    }
    
    [UnityTest]
    public IEnumerator TestNoWeaponsOnLevel2()
    {
        yield return LoadLevel(2);
        Assert.AreEqual(0,CountItems<WeaponItem>());
    }
    
    [UnityTest]
    public IEnumerator TestOneWeaponOnLevel3()
    {
        yield return LoadLevel(3);
        Assert.AreEqual(1,CountItems<WeaponItem>());
    }

    [UnityTest]
    public IEnumerator TestNoEnemiesOnLevel1()
    {
        yield return LoadLevel(1);
        Assert.AreEqual(0, GameObject.FindGameObjectsWithTag("Enemy").Length);
    }

    [UnityTest]
    public IEnumerator TestOneEnemyOnLevel2()
    {
        yield return LoadLevel(2);
        Assert.AreEqual(1, GameObject.FindGameObjectsWithTag("Enemy").Length);
    }

    [UnityTest]
    public IEnumerator TestShopOnlyOnLevelsDivisibleBy5()
    {
        yield return LoadLevel(35);
        Assert.AreEqual(1, GameObject.FindGameObjectsWithTag("Shop").Length);
        yield return LoadLevel(36);
        Assert.AreEqual(0, GameObject.FindGameObjectsWithTag("Shop").Length);
    }
}
