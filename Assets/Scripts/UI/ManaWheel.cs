using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaWheel : MonoBehaviour {
    public Mana mana;
    public Image manaBar;
    public Image manaBarLag;
    public float manaLagRate = 30;
    private float manaLag;

    private void Update()
    {
        if (manaLag > mana.Amount)
        {
            manaLag -= manaLagRate * Time.deltaTime;
        }
        else
        {
            manaLag = mana.Amount;
        }

        manaBar.fillAmount = mana.Amount / mana.maxMana;
        manaBarLag.fillAmount = manaLag / mana.maxMana;
    }
}
