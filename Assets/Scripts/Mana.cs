using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mana : MonoBehaviour {
    public float initialMana = 100;
    public float maxMana = 100;
    private float mana;

    public float Amount
    {
        get
        {
            return mana;
        }
    }

    public bool HasMana
    {
        get
        {
            return mana > 0;
        }
    }

    public bool MaxMana
    {
        get
        {
            return mana >= maxMana;
        }
    }

    private void Start()
    {
        mana = initialMana;
    }

    private void Update()
    {
        mana = Mathf.Clamp(mana, 0, maxMana);
    }

    public void ReplenishMana(float amount)
    {
        mana += amount;
    }

    public void UseMana(float amount)
    {
        mana -= amount;
    }
}
