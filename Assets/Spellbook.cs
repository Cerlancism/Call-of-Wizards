using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spellbook : MonoBehaviour {
    public Spell spell;
    public SpellCollected spellCollected;

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.AddSpell(spell);
            spellCollected.CollectSpell(spell);
            Destroy(gameObject);
        }
    }
}
