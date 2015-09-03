using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IntensityUnitTest : MonoBehaviour 
{
	private const float nEpochs = 3;

	private List<Character> enemies = new List<Character>();

	private int nFrames = 0;

	
	void Start () 
	{
		Time.timeScale = 1;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(AIDirector.Instance.Epoch == nEpochs+1)
		{
			EndSimulation ();
		}

		nFrames++;
	}

	public void EndSimulation()
	{
		GetComponent<WriteIntensityToFile>().SaveFile ();

		Time.timeScale = 0;
	}
}
