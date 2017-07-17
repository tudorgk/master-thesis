using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SliderListener : MonoBehaviour {

    private Text textLabel;
	// Use this for initialization
	void Start () {
        textLabel = GetComponent<Text>();
        textLabel.text = "-3";
	}

    public void OnSliderValueChanged(Slider slider) {
        textLabel.text = slider.gameObject.name + "=" + slider.value;
    }
}
