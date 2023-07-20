using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterPortal : MonoBehaviour {

    public event System.Action<Collider2D> Trigger = (_) => { };

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!triggered) {
            Trigger(collision);
            triggered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        triggered = false;
    }
}
