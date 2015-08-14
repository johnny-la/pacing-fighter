using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SteeringUnitTest : MonoBehaviour 
{
	private const float simulationTime = 20f;

	private List<Character> enemies = new List<Character>();
	private List<float> movementTimes = new List<float>();

	private float totalMovementTime;
	private int enemiesMoved;

	private float totalDistanceFromPlayer;

	private float totalDistanceFromMeanEnemyPosition;

	private int nFrames = 0;

	
	void Start () 
	{
		GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag ("Enemy");
		for(int i = 0; i < enemyObjects.Length; i++)
		{
			enemies.Add(enemyObjects[i].GetComponent<Character>());
			movementTimes.Add (0);
		}

		StartCoroutine (StartSimulation());
	}
	
	// Update is called once per frame
	void Update () 
	{
		//Debug.Log ("Update: Time scale: " + Time.timeScale);
		Vector2 averageEnemyPosition = Vector2.zero;

		for(int i = 0; i < enemies.Count; i++)
		{
			averageEnemyPosition += (Vector2)enemies[i].Transform.position;
		}

		averageEnemyPosition /= enemies.Count;

		for(int i = 0; i < enemies.Count; i++)
		{
			Character enemy = enemies[i];
			float movementTime  = movementTimes[i];

			totalDistanceFromPlayer += (GameManager.Instance.Player.Transform.position - enemy.Transform.position).magnitude;
			totalDistanceFromMeanEnemyPosition += ((Vector2)enemy.Transform.position - averageEnemyPosition).magnitude;

			// If the character is moving
			if(enemy.GetComponent<Rigidbody2D>().velocity.magnitude > 0)
			{
				movementTime += Time.deltaTime;
			}
			// Else, if the enemy is idle.
			else
			{
				// If the enemy just stopped moving
				if(movementTime > 0)
				{
					totalMovementTime += movementTime;
					enemiesMoved++;

					movementTime = 0;
				}				
			}

			movementTimes[i] = movementTime;
		}

		nFrames++;
	}

	public void EndSimulation()
	{
		for(int i = 0; i < enemies.Count; i++)
		{
			Character enemy = enemies[i];
			float movementTime  = movementTimes[i];
			
			
			// If the enemy just stopped moving
			if(movementTime > 0)
			{
				totalMovementTime += movementTime;
				enemiesMoved++;
				
				movementTime = 0;
			}
			
			movementTimes[i] = movementTime;
		}

		float averageDistanceFromPlayer = totalDistanceFromPlayer / (enemies.Count * nFrames);
		float averageDistanceFromMeanEnemyPosition = totalDistanceFromMeanEnemyPosition / (enemies.Count * nFrames);

		Debug.LogWarning ("Average movement time: " + (totalMovementTime/enemiesMoved));
		Debug.LogWarning ("Average distance from player: " + (averageDistanceFromPlayer));
		Debug.LogWarning ("Average distance from enemies' average position: " + (averageDistanceFromMeanEnemyPosition));
	}

	private IEnumerator StartSimulation()
	{
		Time.timeScale = 1;

		yield return new WaitForSeconds(simulationTime);

		EndSimulation();

		Time.timeScale = 0;
	}
}
