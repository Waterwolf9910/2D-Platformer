using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayPortal : MonoBehaviour {

    [SerializeField] private GameObject Enter;
    [SerializeField] private GameObject Exit;

    // Start is called before the first frame update
    void Start() {
        Enter.GetComponent<EnterPortal>().Trigger += (collsion) => {
            collsion.transform.position = Exit.transform.position;
        };
    }
}
