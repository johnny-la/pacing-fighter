using System;
using UnityEngine;
using UnityEngine.UI;

public class SliderText : MonoBehaviour
{
	private Text text;
	public Slider slider;
	public string textBeforeString = "Value: ";
	public void Awake()
	{
		this.text = base.GetComponent<Text>();
		this.UpdateText();
	}
	public void UpdateText()
	{
		this.text.text = this.textBeforeString + this.slider.value;
	}
}
