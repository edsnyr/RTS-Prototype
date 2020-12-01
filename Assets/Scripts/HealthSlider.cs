using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthSlider : MonoBehaviour
{

    Slider slider;
    Transform parent;

    private void Update() {
        if(parent != null) { //position the health bar above the unit
            transform.position = Camera.main.WorldToScreenPoint(parent.position) + Vector3.up * 25;
        }
    }

    /// <summary>
    /// Get references and set slider values.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="maxValue"></param>
    public void InitializeSlider(Transform target, int maxValue) {
        slider = GetComponent<Slider>();
        parent = target;
        slider.maxValue = maxValue;
        slider.value = maxValue;
    }

    /// <summary>
    /// Reduce the sliders value by the given amount. If zero, disable the unit.
    /// </summary>
    /// <param name="value"></param>
    public void TakeDamage(int value) {
        Debug.Log("Damage");
        slider.value -= value;
        if(slider.value <= 0) {
            parent.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }

}
