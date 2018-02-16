using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spellbook : MonoBehaviour {
    public Spell spell;
    public Message spellCollected;
    public float turnSpeed = 90;

    private void Update()
    {
        transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.AddSpell(spell);
            spellCollected.ShowMessage(spell.displayName + " spell found!");
            Destroy(gameObject);
        }
    }
}
