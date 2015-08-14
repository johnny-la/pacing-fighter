
using UnityEngine;
using System;
using System.IO;
using System.Collections;

public class WriteIntensityToFile : MonoBehaviour
{
	public string fileName;
	private StreamWriter fileWriter;

	void Start()
	{
		string fileName = Application.persistentDataPath + "/" + this.fileName;

		fileWriter = File.CreateText (fileName);

		StartCoroutine (UpdateLoop());
	}

	private IEnumerator UpdateLoop()
	{
		yield return new WaitForEndOfFrame();

		while(true)
		{
			fileWriter.WriteLine (Time.time + " " + AIDirector.Instance.GameIntensity);

			yield return new WaitForSeconds(AIDirector.aiTimeStep);
		}
	}

	void OnDisable()
	{
		SaveFile ();
	}

	public void SaveFile()
	{
		fileWriter.Close ();
	}
}


