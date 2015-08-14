using UnityEngine;
using System.Collections;

public class PlayerWalkAutomation : MonoBehaviour 
{
	private const float idleTime = 3;

	/** The player instance to automate */
	private Character player;

	private Level levelToTraverse;

	private Vector2 targetPosition;

	private int cellsTraversed = 0;
	
	void Start () 
	{
		player = GetComponent<Character>();

		levelToTraverse = GameManager.Instance.CurrentLevel;

		targetPosition = player.Transform.position;

		StartCoroutine (UpdateLoop ());
	}

	IEnumerator UpdateLoop () 
	{
		yield return new WaitForEndOfFrame();

		while(true)
		{
			// If the player has reached his destination
			if(((Vector2)player.transform.position - targetPosition).sqrMagnitude < 0.5f * 0.5f)
			{
				cellsTraversed++;

				/*if((cellsTraversed % 3) == 0)
				{
					yield return new WaitForSeconds(idleTime);
					continue;
				}*/

				int playerToStart = levelToTraverse.GetLevelCell (levelToTraverse.GetCellFromPosition (player.Transform.position)).DistanceToStart;

				// Choose the next to traverse to
				LevelCell nextCell = levelToTraverse.GetCellAtDistance (playerToStart + 1);

				if(nextCell == null)
				{
					nextCell = levelToTraverse.GetCellAtDistance (playerToStart - 1);
				}

				// Update the target position
				targetPosition = nextCell.Transform.position;

				Action walk = player.CharacterControl.ActionSet.basicActions.GetBasicAction(BasicActionType.Walk);

				Debug.Log ("WALK " + targetPosition);

				walk.targetPosition = targetPosition;

				player.CharacterControl.PerformAction(walk);

			}

			yield return null;
		}
	}
}
