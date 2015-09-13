using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Controls a text label which displays damage above a character's head. Note that this script is attached to the
/// parent of a damage label
/// </summary>
public class DamageLabel : MonoBehaviour 
{
	/// <summary>
	/// The text used to display damage
	/// </summary>
	public GameObject textObject;
	/** Stores the text GameObject's useful components. */
	private Text damageText;
	private Outline textOutline;
	private Shadow textShadow;
	private RectTransform textRectTransform;

	/// <summary>
	/// The background in back of the text.
	/// </summary>
	public GameObject backgroundObject;
	/** Stores the background GameObject's useful components. */
	private Image backgroundImage;
	private RectTransform backgroundRectTransform;

	// Animation properties
	#region
	public float fadeInTime;
	public float fadeOutTime;
	public float displayTime;	// The total time the damage label stays on-screen

	public Range startAngle;
	public float rotationAmount;

	public Range xVelocity;
	public Range yVelocity;

	private Vector3 originalTextScale;

	private Color originalBackgroundColor;
	private Vector3 originalBackgroundScale;
	#endregion

	/** Caches this GameObject's Transform. */
	private new Transform transform;

	/** Allows the Damage Label to follow a position in the world. Allows the label to fly above a character's head and stay in this position. */
	private WorldToCanvas worldToCanvas;

	// Use this for initialization
	void Awake () 
	{
		// Caches the damage text's useful components
		damageText = textObject.GetComponent<Text>();
		textRectTransform = textObject.GetComponent<RectTransform>();
		Shadow[] effects = damageText.GetComponents<Shadow>();	// Get the Outline/Shadow components in the text
		// Cycle through each effect on the text
		for(int i = 0; i < effects.Length; i++)
		{
			// If the effect is of type 'Outline', save the component in the "outline" variable
			if(effects[i] is Outline)
				textOutline = (Outline)effects[i];
			// Else, the effect is a shadow. Thus, save the component in the "shadow" variable
			else
				textShadow = effects[i];
		}

		// Caches the background image's useful components
		backgroundImage = backgroundObject.GetComponent<Image>();
		backgroundRectTransform = backgroundObject.GetComponent<RectTransform>();

		// Stores the original scale for the text and background.
		originalTextScale = textRectTransform.localScale;
		originalBackgroundScale = backgroundRectTransform.localScale;

		// Stores the background image's original color. This is the color the background will preserve when fading in
		originalBackgroundColor = backgroundImage.color;

		// Cache this gameObject's Transform component
		transform = GetComponent<Transform>();

		// Create a 'WorldToCanvas' component, allowing the damage label to be placed over a character's head in the world.
		worldToCanvas = gameObject.AddComponent<WorldToCanvas>();
		// Add a rigidobdy to the "worldToCanvas" component. Allows the damage label to fall down using gravity.
		worldToCanvas.CreateRigidbody ();

		//StartCoroutine (Test());
	}
	
	// Update is called once per frame
	void Update () 
	{
	}

	private IEnumerator Test()
	{
		while(true)
		{
			Activate (new Vector2(0,0), 15.3f, Color.yellow, Color.white, Color.red, Color.black, Direction.Right);

			yield return new WaitForSeconds(displayTime + fadeOutTime);
		}
	}

	/// <summary>
	/// Displays the damage label with the given amount of damage. Starts playing the damage label's animations
	/// </summary>
	public void Activate(Vector2 position, float damage, Color textStartColor, Color textColor, Color backgroundStartColor,
	                     Color backgroundEndColor, Direction flyingDirection)
	{
		// Converts the damage into an integer and sets it as the damage text
		damageText.text = ((int)damage).ToString ();

		// Sets the position for the "worldToCanvas" component. The damage label will follow this Transform's position.
		worldToCanvas.WorldTransform.position = position;

		// Stores true if the damage label will fly to the right
		bool flyRight = (flyingDirection == Direction.Right);
		// Applies a random velocity on the damage label, going in the direction it is supposed to fly.
		Vector2 velocity = new Vector2(xVelocity.RandomValue () * ((flyRight)? 1 : -1), yVelocity.RandomValue ());
		// Set the velocity for the "worldToCanvas" rigidbody. This damage label will follow around this rigidbody's position.
		worldToCanvas.WorldRigidbody.velocity = velocity;

		// Play the damage label's animations. 
		PlayAnimations(textStartColor, textColor, backgroundStartColor, backgroundEndColor);
	}

	/// <summary>
	/// Plays the damage label's animations. Should be called the instant the label is shown.
	/// </summary>
	private void PlayAnimations(Color textStartColor, Color textColor, Color backgroundStartColor, Color backgroundEndColor)
	{
		/***********************
		 *       FADE IN       *
		 ***********************/
		
		// Fade in the damage text
		damageText.color = Color.clear;	// Ensure that the text is initially transparent
		LeanTween.value (gameObject, Color.clear, Color.white, fadeInTime).setOnUpdate(
			(Color color)=>{
			damageText.color = color;
			}
		);
		
		// Fade in the background
		LeanTween.value (gameObject, Color.clear, backgroundStartColor, fadeInTime).setOnUpdate (
			(Color color)=>{
			backgroundImage.color = color;
			}
		).setDelay (fadeInTime);
		
		
		/****************************
		 *  IN-BETWEEN ANIMATIONS   *
		 ****************************/
		
		// Rotate the entire damage label
		transform.eulerAngles = new Vector3(0,0,startAngle.RandomValue());
		LeanTween.rotateZ (gameObject, transform.eulerAngles.z + rotationAmount, displayTime).setEase (LeanTweenType.easeOutExpo);
		
		// Rotate the text in
		LeanTween.value (textObject, new Vector3(0,0,50), new Vector3(0,0,-10), fadeOutTime).setOnUpdate (
			(Vector3 eulerAngles)=>{
			textRectTransform.eulerAngles = transform.eulerAngles + eulerAngles;
			}
		).setEase (LeanTweenType.easeOutQuad);
		// Rotate the text out
		LeanTween.value (textObject, new Vector3(0,0,-10), new Vector3(0,0,-70), displayTime-fadeOutTime).setOnUpdate (
			(Vector3 eulerAngles)=>{
			textRectTransform.eulerAngles = transform.eulerAngles + eulerAngles;
			}
		).setEase (LeanTweenType.easeInSine).setDelay (fadeOutTime);
		
		//Scale the damage text
		textRectTransform.localScale = new Vector3(0,0,0);
		LeanTween.scale (textRectTransform, originalTextScale, fadeInTime*4).setEase (LeanTweenType.easeOutBack);
		LeanTween.scale (textRectTransform, new Vector3(0,0,0), fadeInTime*4).setEase (LeanTweenType.easeInBack).setDelay (fadeInTime*4);

		// Tween the text color from 'textStartColor' to 'textColor'	
		LeanTween.value (textObject, textStartColor, textColor, displayTime/* - fadeInTime*/).setOnUpdate (
			(Color color)=>{
			textShadow.effectColor = color;
			textOutline.effectColor = color;
			}
		).setEase (LeanTweenType.easeOutQuart)/*.setDelay (fadeInTime)*/;
		
		//Scale the background
		backgroundRectTransform.localScale = new Vector3(0,0,0);
		LeanTween.scale (backgroundRectTransform, originalBackgroundScale, fadeInTime*4).setEase (LeanTweenType.easeOutBack).setDelay (fadeInTime);
		LeanTween.scale (backgroundRectTransform, new Vector2(0,0), fadeInTime*4).setEase (LeanTweenType.easeInBack).setDelay (fadeInTime*5);

		// Tween the background color from 'backgroundStartColor' to 'backgroundEndColor'	
		LeanTween.value (backgroundObject, backgroundStartColor, backgroundEndColor, displayTime - fadeInTime*2).setOnUpdate (
			(Color color)=>{
			backgroundImage.color = color;
			}
		).setEase (LeanTweenType.easeOutQuart).setDelay (fadeInTime*2);

		
		/***********************
		 *       FADE OUT      *
		 ***********************/
		
		// Fade out the damage text
		LeanTween.textAlpha(textRectTransform, 0.0f, fadeOutTime).setDelay (displayTime - fadeOutTime);
		
		// Fade out the background image
		LeanTween.value (gameObject, backgroundEndColor, Color.clear, fadeOutTime).setOnUpdate (
			(Color color)=>{
			backgroundImage.color = new Color(backgroundImage.color.r,backgroundImage.color.g,backgroundImage.color.b,color.a);
			}
		).setDelay (displayTime - fadeOutTime);
	}

	/// <summary>
	/// The RectTransform for the canvas this label is placed in. Allows the damage label to convert its world coordinates (on top of a character's
	/// head) into canvas coordinates.
	/// </summary>
	public RectTransform CanvasRect
	{
		get { return worldToCanvas.CanvasRect; }
		set 
		{ 
			// Make the damage label a child of the canvas.
			transform.SetParent (value);
			// Update the canvas used to conver the world coordinates to canvas coordinates.
			worldToCanvas.CanvasRect = value; 
		}
	}

	/// <summary>
	/// A cached version of the damage label's Transform
	/// </summary>
	public Transform Transform
	{
		get { return transform; }
	}
}
