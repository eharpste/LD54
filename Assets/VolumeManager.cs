using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeManager : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public int depth = 10;

    public GameObject cubePrefab;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Build Volume")]
    public void GenerateVolume()
    {
        for (int w=0; w < width; w++)
        {
            for (int h=0; h < height; h++)
            {
                for (int d=0; d < depth; d++)
                {
                    Vector3 pos = new Vector3(w, h, d);
                    GameObject.Instantiate(cubePrefab, pos, Quaternion.identity, this.gameObject.transform);
                }
            }
        }
    }
}
