using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

    public event System.Action<Collider2D> CollisionEnter = (_) => { };
    public event System.Action<Collider2D> CollisionExit = (_) => { };

    private void OnTriggerEnter2D(Collider2D collision) {
        CollisionEnter(collision);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        CollisionExit(collision);
    }
}
