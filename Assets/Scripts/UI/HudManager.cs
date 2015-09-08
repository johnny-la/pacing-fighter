using UnityEngine;
using System.Collections;

public class HudManager : MonoBehaviour 
{
	/// <summary>
	/// Stores a prefab for a damage label, displayed on top of a character's head when hit.
	/// </summary>
	public GameObject damageLabelPrefab;

	// Visual properties
	#region
	#endregion

	/** The RectTransform for the canvas. Used to convert from world to canvas coordinates. */
	private RectTransform canvasRect;

	/// <summary>
	/// Stores a pool of damage labels to display in the HUD.
	/// </summary>
	private GameObjectPool damageLabelPool;

	void Awake () 
	{
		// Caches the RectTransform of the canvas. Used to convert from world to canvas coordinates.
		canvasRect = GetComponent<RectTransform>();

		// Create a new pool to store the damage labels, shown when a character is hit
		damageLabelPool = new GameObjectPool(damageLabelPrefab);
	}

	public void OnEnable()
	{
		// Subscribe to the Player's OnDamageDealt event. Allows us to display a damage counter
		GameManager.Instance.Player.CharacterStats.OnDealDamage += OnDealtDamage;
	}

	/// <summary>
	/// Called when the character deals damage to an adversary. Displays a damage label over the adversary's head.
	/// </summary>
	public void OnDealtDamage(float damage, Character character, Character adversary)
	{
		// Computes a position to spawn the damage label. In this case, it will be placed in the position of his 'DamageLabelSpawn' anchor.
		Vector2 damageLabelSpawnPosition = adversary.CharacterAnchor.GetAnchorPosition (Anchor.DamageLabelSpawn);

		// Display a damage label over the character which was hit. 
		ShowDamageLabel (damageLabelSpawnPosition, damage, Color.white, Color.white, new Color(0.792f,0.259f,0.263f,1.0f), new Color(0,0,0,0.5f),
						 character.CharacterMovement.FacingDirection);
	}

	/// <summary>
	/// Show a damage label at the given position.
	/// </summary>
	/// <param name="position">The position where the damage label is spawned.</param>
	/// <param name="damage">The damage displayed on the label.</param>
	/// <param name="textColor">The color of the damage text when first spawned.</param>
	/// <param name="textColor">The final color of the damage text. The color is tweened from the starting color to this one.</param>
	/// <param name="backgroundStartColor">The color of the background when first spawned.</param>
	/// <param name="backgroundEndColor">The color of the background at the end of the damage label animation.</param>
	/// <param name="flyingDirection">The direction in which the label will fly (either left or right).</param>
	public void ShowDamageLabel(Vector3 position, float damage, Color textStartColor, Color textColor, Color backgroundStartColor,
	                            Color backgroundEndColor, Direction flyingDirection)
	{
		// Retrieve a damage label from a pool
		DamageLabel damageLabel = damageLabelPool.Obtain ().GetComponent<DamageLabel>();

		// Tell the damageLabel which canvas rect it is being displayed on. Allows the label to convert its world position into canvas coordinates.
		damageLabel.CanvasRect = canvasRect;

		// Activates the damage label and makes it fly in the given direction
		damageLabel.Activate (position, damage, textStartColor, textColor, backgroundStartColor, backgroundEndColor, flyingDirection);

		// Free the damage label back into a pool once it fades out, in 'damageLabel.displayTime' seconds
		StartCoroutine(FreeIntoPool (damageLabel.gameObject, damageLabelPool, damageLabel.displayTime));
	}

	/// <summary>
	/// Frees the given GameObject back into its pool. A delay is used to ensure the GameObject is no longer needed when
	/// placed back into the pool.
	/// </summary>
	private IEnumerator FreeIntoPool(GameObject gameObject, GameObjectPool pool, float delay)
	{
		// Wait 'delay' seconds before freeing the gameObject into the given pool
		yield return new WaitForSeconds(delay);

		// Free the game object back into the given pool
		pool.Free (gameObject);
	}
}
