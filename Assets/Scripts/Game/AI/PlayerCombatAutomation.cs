using UnityEngine;
using System.Collections;

public class PlayerCombatAutomation : MonoBehaviour 
{
	private const float IdleTimeSmart = 1;
	private const float IdleTimeNaive = 6;

	private const float AttackTimeSmart = 0.5f;
	private const float AttackTimeNaive = 1.3f;

	private const float AttackRadiusSmart = 3.5f;
	private const float AttackRadiusNaive = 2.0f;

	/** The player instance to automate */
	private Character player;

	private Level levelToTraverse;

	private Vector2 targetPosition;
	private bool reachedTarget = false;

	private Character enemyTarget;
	private bool attacking = false;

	private float idleTime = 3;
	private float attackTime = 0.5f;

	private int cellsTraversed = 0;

	/** The enemies which surround the player. */
	public Neighbourhood enemyNeighbourhood;

	/** If true, the player is naive at combat and dies more easily. */
	public bool naive;
	
	void Start () 
	{
		player = GetComponent<Character>();

		levelToTraverse = GameManager.Instance.CurrentLevel;

		targetPosition = player.Transform.position;

		if(naive)
		{
			idleTime = IdleTimeNaive;
			attackTime = AttackTimeNaive;

			enemyNeighbourhood.SetRadius (AttackRadiusNaive);
		}
		else
		{
			idleTime = IdleTimeSmart;
			attackTime = AttackTimeSmart;

			enemyNeighbourhood.SetRadius (AttackRadiusSmart);
		}

		StartCoroutine (UpdateLoop ());
	}

	IEnumerator UpdateLoop () 
	{
		yield return new WaitForEndOfFrame();

		while(true)
		{
			if(!attacking && enemyNeighbourhood.NeighbourCount != 0)
			{
				Character enemyToAttack = enemyNeighbourhood.GetNeighbour(Random.Range (0, enemyNeighbourhood.NeighbourCount)).GetComponent<Character>();

				enemyTarget = enemyToAttack;
				attacking = true;
				reachedTarget = true;
			}


			if(attacking)
			{
				if(enemyTarget.CharacterStats.IsDead ())
				{
					attacking = false;
					enemyTarget = null;
				}

				if(enemyTarget != null)
				{
					// Keep attacking until the enemy is dead
					while(enemyTarget != null && enemyTarget.CharacterStats.IsDead () == false)
					{
						// Make the player attack the enemy
						player.CharacterControl.OnTouch (new TouchInfo(0), InputType.Click, enemyTarget.gameObject, SwipeDirection.Horizontal);
						
						yield return new WaitForSeconds(attackTime);
					}
				}
			}
			else
			{

				// If the player has reached his destination
				if(reachedTarget || ((Vector2)player.transform.position - targetPosition).sqrMagnitude < 0.5f * 0.5f)
				{
					yield return new WaitForSeconds(idleTime);

					cellsTraversed++;

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

					reachedTarget = false;
					
				}
			}

			yield return null;
		}
	}
}
