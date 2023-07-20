using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    
    // Speed and Jump of the Player
    public float speed = 5;
    public float jump = 5;
    // The player's rigidbody
    private Rigidbody2D _rigidbody;
    
    // Get switch layers to treat as ground, water, etc.
    [SerializeField]
    private LayerMask ground;
    [SerializeField]
    private LayerMask water;
    [SerializeField]
    private LayerMask hazard;
    [SerializeField]
    private LayerMask spawn;

    // Some Ground States
    private bool isGrounded = false;
    private bool isSwimming = false;
    private bool jumppedInAir = false;

    private Vector2 spawnpoint = new Vector3(-25.5f, -0.5f, 0f);

    void Start() {
        // Get the Rigidbody for this object
        _rigidbody = this.GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        var _has = spawn.value & (1 << collision.gameObject.layer);
        if (_has != 0) {
            spawnpoint = collision.gameObject.transform.position;
        }
    }

    // Update is called once per frame
    void Update() {
        isGrounded = _rigidbody.IsTouchingLayers(ground.value);
        isSwimming = _rigidbody.IsTouchingLayers(water.value);

        if (_rigidbody.IsTouchingLayers(hazard.value)) {
            this.transform.position = spawnpoint;
        }
        if (isSwimming) {
            isGrounded = false;
        }
        if (isGrounded) {
            jumppedInAir = false;
        }
        float x = Input.GetAxisRaw("Horizontal");
        
        _rigidbody.velocity = new(x * speed, GetJumps());
    }

    public float GetJumps() { // Calculate Jumps
        if (Input.GetButtonDown("Jump")) {
            if (isSwimming) {
                Debug.Log("Swimming");
                jumppedInAir = false;
                return jump / 2;
            }
            if (isGrounded || !jumppedInAir) {
                if (!isGrounded) {
                    jumppedInAir = true;
                }
                return jump;
            }
        }
        return _rigidbody.velocity.y;
    }

}
