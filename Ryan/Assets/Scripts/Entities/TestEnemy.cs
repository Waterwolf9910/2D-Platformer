using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : Entity
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            collision.gameObject.GetComponentInChildren<Player>().ActivateBattle(this.gameObject, new() { new(this, Random.Range(2, 4)) });
        }
    }

    public TestEnemy(): base("test", 10, 1, 0) {
        this.Alignment = EntityAlignment.Enemy;
    }
}
