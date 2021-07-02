using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class MovingObject : MonoBehaviour
{
	public float moveTime = 0.1f;			//Time it will take object to move, in seconds.
	public LayerMask blockingLayer;			//Layer on which collision will be checked.
	
	
	private BoxCollider2D boxCollider; 		//The BoxCollider2D component attached to this object.
	private Rigidbody2D rb2D;				//The Rigidbody2D component attached to this object.
	private float inverseMoveTime;
	[HideInInspector] public bool isMoving;
	
	
	protected virtual void Start ()
	{
		boxCollider = GetComponent <BoxCollider2D> ();
		rb2D = GetComponent <Rigidbody2D> ();
		
		//By storing the reciprocal of the move time we can use it by multiplying instead of dividing, this is more efficient.
		inverseMoveTime = 1f / moveTime;
	}
	
	
	// Move returns true if move succeeds 
	protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
	{
		Vector2 start = transform.position;
		Vector2 end = start + new Vector2(xDir, yDir);
		
		//Disable the boxCollider so that linecast doesn't hit this object's own collider.
		boxCollider.enabled = false;
		hit = Physics2D.Linecast(start, end, blockingLayer);
		boxCollider.enabled = true;

		List<Vector2> destinationTiles = GameManager.instance.destinationTiles;
		
		if (hit.transform == null && !isMoving && !destinationTiles.Contains(end))
		{
			destinationTiles.Add(end);
			StartCoroutine(SmoothMovement(end));
			return true;
		}
		
		return false;
	}

	protected IEnumerator SmoothMovement(Vector3 end)
	{
		isMoving = true;
		
		//Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
		//Square magnitude is used instead of magnitude because it's computationally cheaper.
		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
		
		while(sqrRemainingDistance > float.Epsilon)
		{
			Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
			rb2D.MovePosition(newPostion);
		
			sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			yield return null;
		}
		
		//Make sure the object is exactly at the end of its movement.
		rb2D.MovePosition(end);
		isMoving = false;
		GameManager.instance.destinationTiles.Remove(end);
	}

	protected T GetCollision<T>(RaycastHit2D hit)
		where T : Component
	{
		if (hit.transform == null) return null;
		return hit.transform.GetComponent<T>();
	}
}

