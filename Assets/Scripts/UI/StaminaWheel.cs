using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaWheel : MonoBehaviour {
    public Stamina stamina;
    public float redDepelenishRate = 0.5f;
    public float wheelFadeRate = 1;
    public CanvasGroup canvasGroup;
    public Image greenWheel;
    public Image redWheel;
    public Image yellowWheel;
    private float redAmount;
	
	private void Update ()
    {
        UpdateRed();
        UpdateImages();
        UpdateFullFade();
    }

    private void UpdateRed()
    {
        // Change stamina red to the actual stamina value slowly
        if (redAmount > stamina.Amount)
        {
            redAmount -= redDepelenishRate * Time.deltaTime;
        }
        else
        {
            redAmount = stamina.Amount;
        }
    }

    private void UpdateImages()
    {
        greenWheel.fillAmount = stamina.Amount;
        redWheel.fillAmount = redAmount;
        yellowWheel.fillAmount = stamina.FullReplenishAmount;
    }

    private void UpdateFullFade()
    {
        // Fade out stamina if stamina = 1
        // Else, fade in
        float staminaWheelAlpha = canvasGroup.alpha;
        if (stamina.Amount == 1)
        {
            staminaWheelAlpha -= wheelFadeRate * Time.deltaTime;
        }
        else
        {
            staminaWheelAlpha += wheelFadeRate * Time.deltaTime;
        }
        staminaWheelAlpha = Mathf.Clamp(staminaWheelAlpha, 0, 1);
        canvasGroup.alpha = staminaWheelAlpha;
    }
}
