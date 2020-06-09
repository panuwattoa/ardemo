using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changeMaterial : MonoBehaviour
{
    public Material[] mats;
    private int pos;
    public MeshRenderer currentGround;
    public GameObject matObj;
    bool isOpen;

    public void NextPlane()
    {
        pos = pos + 1;
        if (pos >= mats.Length)
        {
            pos = 0;
        }

        MeshRenderer[] rands = transform.GetComponentsInChildren<MeshRenderer>();
        foreach (var go in rands)
        {
            go.material = mats[pos];
        }

        currentGround.material = mats[pos];
    }

    public void OnClickChangeMat()
    {
        if (isOpen)
        {
            matObj.SetActive(false);
        }
        else
        {
            matObj.SetActive(true);
        }
        isOpen = !isOpen;
    }
}
