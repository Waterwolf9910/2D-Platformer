using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoWayDoor : MonoBehaviour {

    [SerializeField] private GameObject EnterDoor;
    [SerializeField] private GameObject ExitDoor;
    [SerializeField] private Transform Gem;
    [SerializeField] private Transform Gem2;
    private bool playerInEnterDoor = false;
    private bool playerInExitDoor = false;
    private bool playerInDoor => playerInEnterDoor || playerInExitDoor;
    private float lastEnterTime = 0;

    // Start is called before the first frame update
    void Start() {
        var _enter = EnterDoor.GetComponent<Door>();
        var _bottom = ExitDoor.GetComponent<Door>();
        _enter.CollisionEnter += EnterDoorInCollision;
        _enter.CollisionExit += EnterDoorOutCollision;
        _bottom.CollisionEnter += ExitDoorInCollision;
        _bottom.CollisionExit += ExitDoorOutCollision;
    }

    // Update is called once per frame
    void Update() {
        if (Gem != null) {
            Gem.Rotate(0, 90 * Time.deltaTime, 0);
        }
        if (Gem2 != null) {
            Gem2.Rotate(0, -90 * Time.deltaTime, 0);
        }
        if (Input.GetAxisRaw("Vertical") == 1 && playerInDoor && Time.realtimeSinceStartup - lastEnterTime > .25f) {
            Vector3 vec = Vector3.zero;
            if (playerInEnterDoor) {
                vec = ExitDoor.transform.position;
            }
            if (playerInExitDoor) {
                vec = EnterDoor.transform.position;
            }
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) {
                player.transform.position = vec;
            }
            foreach (var go in GameObject.FindGameObjectsWithTag("Allies")) {
                go.transform.position = vec;
            }
            lastEnterTime = Time.realtimeSinceStartup;
            playerInEnterDoor = false;
            playerInExitDoor = false;
        }
    }

    public void EnterDoorInCollision(Collider2D collider) {
        this.playerInEnterDoor = true;
    }

    public void EnterDoorOutCollision(Collider2D collider) {
        this.playerInEnterDoor = false;
    }

    public void ExitDoorInCollision(Collider2D collider) {
        this.playerInExitDoor = true;
    }


    public void ExitDoorOutCollision(Collider2D collider) {
        this.playerInExitDoor = false;
    }
}
