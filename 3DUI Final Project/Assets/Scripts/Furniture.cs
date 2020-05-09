using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furniture : MonoBehaviour
{
    private bool selected;
    private Material mat;

    public void Select(bool b)
    {
        selected = b;
    }

    public bool IsSelected()
    {
        return selected;
    }

    public Material GetMaterial()
    {
        return mat;
    }

    public void SetMaterial(Material newMaterial)
    {
        mat = newMaterial;
    }
}
