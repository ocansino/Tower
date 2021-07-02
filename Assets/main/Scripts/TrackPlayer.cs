using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackPlayer : MonoBehaviour
{
    public GameObject player;
    public Vector2 bounds;
    
    void Update()
    {
        Vector3 playerPos = player.transform.position;
        Vector3 camPos = transform.position;
        
        transform.position = new Vector3(
            Mathf.Clamp(camPos.x,  playerPos.x - bounds.x, playerPos.x + bounds.x),
            Mathf.Clamp(camPos.y,  playerPos.y - bounds.y, playerPos.y + bounds.y),
            camPos.z
        );
    }
}
