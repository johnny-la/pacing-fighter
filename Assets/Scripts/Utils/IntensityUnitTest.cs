using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IntensityUnitTest : MonoBehaviour 
{
	private const float simulationTime = 20f;

	private List<Character> enemies = new List<Character>();

	private int nFrames = 0;

	
	void Start () 
	{
		GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag ("Enemy");
		for(int i = 0; i < enemyObjects.Length; i++)
		{
			enemies.Add(enemyObjects[i].GetComponent<Character>());
		}

		StartCoroutine (StartSimulation());
	}
	
	// Update is called once per frame
	void Update () 
	{
		nFrames++;
	}

	public void EndSimulation()
	{

	}

	private IEnumerator StartSimulation()
	{
		Time.timeScale = 1;

		yield return new WaitForSeconds(simulationTime);

		EndSimulation();

		Time.timeScale = 0;
	}
}
