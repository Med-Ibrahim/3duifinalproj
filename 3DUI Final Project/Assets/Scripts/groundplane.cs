using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class groundplane : MonoBehaviour
{

    public GameObject furnituremenu;
    public Text buttontext;
    public GameObject floor;


    public void toggleMenu()
    {
        if (furnituremenu.activeSelf)
        {
            buttontext.text = "Show Furniture Menu";
            furnituremenu.SetActive(false);
        }
        else
        {
            buttontext.text = "Hide Furniture Menu";
            furnituremenu.SetActive(true);
        }
    }

}
