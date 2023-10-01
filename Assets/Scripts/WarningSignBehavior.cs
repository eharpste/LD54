using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningSignBehavior : MonoBehaviour
{
    public float speed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.rotation *= Quaternion.Euler(0, speed * Time.deltaTime, 0);
    }
}
