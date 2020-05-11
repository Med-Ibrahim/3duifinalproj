using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furniture : MonoBehaviour
{
    private bool selected;
    private Material mat;
    private int id;

    public Material translucent;
    public Material silhouette;

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

    public void ManipulateFurniture(bool manipulating)
    {
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = !manipulating;
        }
        //Renderer rend = wall.GetComponentInChildren<Renderer>();
        GetComponentInChildren<Rigidbody>().isKinematic = manipulating;
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            if (manipulating)
                rend.material = translucent; // use setmaterial
            else
            {
                rend.material = silhouette; // use set material
                SetSilhouette(rend, 2f, Color.green);
            }
        }
    }

    public void SetSilhouette(Renderer rend, float lineWidth)  //Can probably abstract a ~little~ more, and put the foreach loops into this function
    {
        //The shader has up to 2 different silhouette outlines
        //Renderer rend = GetComponent<Renderer>();
        rend.material.SetFloat("_FirstOutlineWidth", lineWidth);
    }

    public void SetSilhouette(Renderer rend, float lineWidth, Color color)
    {
        //Renderer rend = GetComponent<Renderer>();
        rend.material.SetFloat("_FirstOutlineWidth", lineWidth);
        rend.material.SetColor("_FirstOutlineColor", color);
    }

    public int GetID()
    {
        return id;
    }
}
