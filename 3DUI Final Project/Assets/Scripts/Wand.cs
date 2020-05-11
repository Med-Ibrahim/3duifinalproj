using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//Final proj
public class Wand : MonoBehaviour
{
    public GameObject confirmBtn;
    public GameObject pointerLine;
    public GameObject deselectBtn;
    public GameObject translateBtn; //Can prob get rid of all of these buttons (except confirmbtn i think)
    public GameObject scaleBtn;
    public GameObject rotateBtn;
    //public GameObject turret;
    //public GameObject voodooTower;  //Might be able to just instantiate the regular prefab and modify the material, but seems like more work
    public GameObject voodooWall;
    public GameObject furniturePiece; //Should be replaced by the current selected furniture object
    public GameObject imageTarget;
    public GameObject defaultPanel;
    public GameObject translatingPanel;
    public GameObject rotatingPanel;
    public GameObject scalingPanel;
    public GameObject transformsPanel; //Possibly add panel for not selecting? Default panel of (Select Object[, select world?][end game?][controls?])
    public Material translucent;
    public Material silhouette;
    public Material voodooMaterial;
    public const float SCALE_FACTOR = 1.50f; //? Wanted to add sensitivity slider, but unnecessary. also public const dont work like that lmao
    public GameObject HUD;
    public GameObject WiM;
    public GameObject groundPlane;

    public GameObject furnitureMenu;
    public Text menuText;

    //Initialize some defaults and variables to be used later
    private GameObject currentPanel;
    private GameObject currentHover = null;
    private GameObject currentSelected = null;
    private List<GameObject> selectedArr = new List<GameObject>();
    private List<GameObject> furnitureList = new List<GameObject>();
    private Vector3 groupMidpoint;
    private GameObject groupParent = null;

    private string controlStyle = "hand";
    private string transformationState = "selecting"; //Could separate into various bools. e.g., isTranslating, isRotating, etc.
    private Vector3 scaleOrigin;
    private float scaleBy;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private int furnitureCount;
    private GameObject currentVoodoo;
    private const float MINI_SCALE = 10f;

    void Start()
    {
        currentPanel = defaultPanel;
    }

    void Update()
    {
        if (controlStyle == "pointer")
        {
            // bit shift to create layer mask
            if (transformationState == "selecting")
            {
                //Funiture on layer 8
                int layerMask = 1 << 8;

                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, layerMask)) //If the Raycast hit a selectable object, display hover silhouette
                {
                    if (hit.collider.gameObject != currentSelected) //If needed, or else currently selected objects become yellow when pointed at
                    {
                        foreach (Renderer rend in hit.collider.GetComponentsInChildren<Renderer>())
                            SetSilhouette(rend, 2f, Color.yellow);
                    }
                    currentHover = hit.collider.gameObject;
                    if (currentHover != currentSelected)
                        confirmBtn.SetActive(true);
                }
                else if (currentHover != null && currentHover != currentSelected) //if nothing was hit, and we were just hovering something, stop hovering
                {
                    foreach (Renderer rend in currentHover.GetComponentsInChildren<Renderer>())
                        SetSilhouette(rend, 0f, Color.yellow);
                    currentHover = null;
                    confirmBtn.SetActive(false);
                }
            }
            else if (transformationState == "rotating") //TODO Update rotation, remove voodoo
            {
                //Create voodoo wall, apply rotation, then apply rotation to actual wall
                currentVoodoo.transform.LookAt(transform.position);
                currentSelected.transform.rotation = currentVoodoo.transform.rotation;
            }
            else if (transformationState == "translating")
            {
                //Detect ground layer (9)
                int layerMask = 1 << 9;
                //Raycast to detect the point on the ground being pointed at. Place object there, shifted up so that it is on the ground, not inside of it.
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, layerMask)) //If the Raycast hit a selectable object, display hover silhouette
                {
                    foreach(GameObject obj in selectedArr)
                    {
                        groupParent.transform.position = hit.point + new Vector3(0, selectedArr[0].transform.localScale.y / 2.0f, 0);
                    }
                    //currentSelected.transform.position = hit.point + new Vector3(0f,currentSelected.transform.localScale.y/2,0f);
                }
            }
            else if (transformationState == "scaling")
            {
                //I wanted this to emulate the unity method of scaling, when you hold the center piece of the scaling gizmo. You move the mouse up and to the right to increase, down and to the left to shrink, no matter your perspective when you start scaling.
                //This more or less works, but I originally wanted to project the vector representing the difference between new wand position and starting position onto a 2D frame whose normal pointed at the object to make the idea more similar to the unity method and really regularize the motion to any starting point, but it didn't really work, and seemd a little extra.
                //Vector3 currentPoint = transform.position; //prob dont need this, just put Vector3 scaleDir = transfrom.position - scaleOrigin;
                Vector3 scaleDir = transform.position - scaleOrigin;
                float x0 = scaleDir.x;
                float y0 = scaleDir.y;
                float dist = Mathf.Abs(x0 + y0); 
                float growOrShrink = Mathf.Sign(x0 + y0);
                float scaleIt = growOrShrink * dist * SCALE_FACTOR;
                currentSelected.transform.localScale = originalScale + new Vector3(scaleIt, scaleIt, scaleIt); 
                currentSelected.transform.position = originalPosition + new Vector3(0, scaleIt / 2, 0); //So the hw said not to only do one transform at a time, but if this isn't here then the walls get scaled into the ground every time and go crazy because of rigidbody collisions. This way makes more sense intuitively either way, for the user.
            }
        }
        else //Control style: hand
        {
            if (transformationState == "translating") //TODO Update furniture translation, fix group translation. (Currently uses avg position, prefer "median"
            {
                //If group is selected, translate according to average of item positions   
                if (selectedArr.Count > 1)
                {
                    /*foreach (GameObject obj in selectedArr)
                    {
                        //Vector3 offset = obj.transform.position - groupMidpoint;
                        obj.transform.position = transform.position;
                    }*/
                    groupParent.transform.position = transform.position;
                }
                else
                {
                    currentSelected.transform.position = transform.position;
                }
            }
            else if (transformationState == "scaling")
            {
                //Scaling works same for both virtual hand and pointer. Not what the assignment asked for, but I couldn't figure out another scale method and this way works fine for both.
                
                //Vector3 currentPoint = transform.position; //prob dont need this, just put Vector3 scaleDir = transfrom.position - scaleOrigin;
                Vector3 scaleDir = transform.position - scaleOrigin;
                float x0 = scaleDir.x;
                float y0 = scaleDir.y;
                float dist = Mathf.Abs(x0 + y0); 
                float growOrShrink = Mathf.Sign(x0 + y0);
                float scaleIt = growOrShrink * dist * SCALE_FACTOR;
                currentSelected.transform.localScale = originalScale + new Vector3(scaleIt, scaleIt, scaleIt);
            }
            else if (transformationState == "rotating")
            {
                currentSelected.transform.LookAt(transform.position);
            }
        }
        //possibly additional state: selected. For when deciding to transform or not

    }

    void OnTriggerEnter(Collider other) //Might need to copy into OnTriggerStay as well. If deselected while wand is in other object, will need to move hand then re-enter into object to confirm selection
    {
        if (controlStyle == "hand" && currentHover == null && other.gameObject.layer == LayerMask.NameToLayer("Furniture"))
        {
            //Display yellow outline indicating hover for selection
            foreach (Renderer rend in other.GetComponentsInChildren<Renderer>())
            {
                SetSilhouette(rend, 2f, Color.yellow);
            }
            currentHover = other.gameObject;
            //Display "Confirm Selection" button
            if (currentHover != currentSelected)
                confirmBtn.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        //Stop highlighting the SELECTABLE object being hovered, unless that object is what we have currently selected.
        if (controlStyle == "hand" && currentSelected != other.gameObject && other.gameObject.layer == LayerMask.NameToLayer("Furniture")) //Might be too many conditions...First condition is bc if you are using pointer style and come in contact with object, can still deselect it
        {
            //Remove yellow outline, no longer hovering
            foreach (Renderer rend in other.GetComponentsInChildren<Renderer>())
            {
                SetSilhouette(rend, 0f, Color.yellow);
            }
            currentHover = null;
            //Hide "Confirm Selection" button
            confirmBtn.SetActive(false);
        }
    }

    private void SetSilhouette(Renderer rend, float lineWidth)  //Can probably abstract a ~little~ more, and put the foreach loops into this function
    {
        //The shader has up to 2 different silhouette outlines
        rend.material.SetFloat("_FirstOutlineWidth", lineWidth);
    }

    private void SetSilhouette(Renderer rend, float lineWidth, Color color)
    {
        rend.material.SetFloat("_FirstOutlineWidth", lineWidth);
        rend.material.SetColor("_FirstOutlineColor", color);
    }

    //Change control mothed between vritual hand and pointer
    public void ChangeStyle()
    {
        if (controlStyle == "hand")
        {
            controlStyle = "pointer";
            pointerLine.SetActive(true);
        }
        else
        {
            controlStyle = "hand";
            pointerLine.SetActive(false);
        }
    }

    //Confirm the current object being hovered, and select it. Update UI to reflect this change and color silhouette green
    /*public void ConfirmSelection()
    {
        // Make sure something is being hovered
        if (currentHover != null)
        {
            currentSelected = currentHover;
            selectedArr.Add(currentHover);
            currentHover = null;
            foreach (Renderer rend in currentSelected.GetComponentsInChildren<Renderer>())
            {
                SetSilhouette(rend, 2f, Color.green);
            }
            if (selectedArr.Count == 3) 
            { 
                confirmBtn.SetActive(false);
                SwitchPanel(transformsPanel);
                translateBtn.SetActive(true);
                scaleBtn.SetActive(true);
            }
            
        }
    }*/

    public void ConfirmSelection()
    {
        // Make sure something is being hovered
        if (currentHover != null)
        {
            if (currentHover.CompareTag("menu"))
            {
                GameObject newHover = Instantiate(currentHover);
                newHover.tag = "Untagged";
                newHover.transform.Translate(currentHover.transform.position);
                currentSelected = newHover;
                furnitureMenu.SetActive(false);
                menuText.text = "Show Furniture Menu";
            }
            else
            {
                currentSelected = currentHover;
            }

            foreach (Renderer rend in currentSelected.GetComponentsInChildren<Renderer>())
            {
                SetSilhouette(rend, 2f, Color.green);
            }
            confirmBtn.SetActive(false);
            SwitchPanel(transformsPanel);
            translateBtn.SetActive(true);
            scaleBtn.SetActive(true);
        }
    }

    //Deselect currently selected object. Update silhouette/UI
    public void CancelSelection()
    {   
        if (selectedArr.Count > 0)
        {
            foreach (GameObject obj in selectedArr)
            {
                foreach (Renderer rend in obj.GetComponentsInChildren<Renderer>())
                {
                    SetSilhouette(rend, 0f, Color.yellow);
                }
                //currentSelected = null;
            }
            selectedArr.Clear();
        }
        /*if (currentSelected)
        {
            foreach (Renderer rend in currentSelected.GetComponentsInChildren<Renderer>())
            {
                SetSilhouette(rend, 0f, Color.yellow);
            }
            currentSelected = null;
        }*/
        SwitchPanel(defaultPanel);
    }

    //Change transformation state, store original position of object incase the user decides to cancel translation, and change wall appearance/tangibility.
    public void StartTranslating()
    {
        transformationState = "translating";
        originalPosition = currentSelected.transform.position;
        if (selectedArr.Count > 0)
        {
            groupParent = new GameObject();
            Vector3 center = FindGroupCenter(selectedArr);
            groupParent.transform.position = center;
            foreach (GameObject obj in selectedArr)
            {
                obj.transform.SetParent(groupParent.transform);
                ManipulateFurniture(true, obj);
            }
        }
        SwitchPanel(translatingPanel);
        
    }

    //Change state, update UI, make wall tangible
    public void ConfirmTranslation()
    {
        transformationState = "selecting";
        SwitchPanel(transformsPanel);
        foreach (GameObject obj in selectedArr)
            ManipulateFurniture(false, obj);
    }

    //Return object to original position, change state, update UI, make wall tangible.
    public void CancelTranslation()
    {
        transformationState = "selecting";
        currentSelected.transform.position = originalPosition;
        SwitchPanel(transformsPanel);
        foreach (GameObject obj in selectedArr)
            ManipulateFurniture(false, currentSelected);
    }

    //Change state, store original scale, update UI, make wall intangible
    public void StartScaling()
    {
        transformationState = "scaling";
        scaleOrigin = transform.position;
        originalScale = currentSelected.transform.localScale;
        originalPosition = currentSelected.transform.position;
        SwitchPanel(scalingPanel);
        ManipulateFurniture(true, currentSelected);
    }

    //Change state, update UI, make wall tangible
    public void ConfirmScale()
    {
        transformationState = "selecting";
        SwitchPanel(transformsPanel);
        ManipulateFurniture(false, currentSelected);
    }

    //Change state, revert scale, update UI, tangible
    public void CancelScale()
    {
        transformationState = "selecting";
        currentSelected.transform.localScale = originalScale;
        currentSelected.transform.position = originalPosition;
        SwitchPanel(transformsPanel);
        ManipulateFurniture(false, currentSelected);
    }

    //Change state, store original rotation (different for wall/turret), change UI. Create Voodoo doll instance corresponding to selected object.
    public void StartRotating()
    {
        transformationState = "rotating";
        originalRotation = currentSelected.transform.localRotation;
        SwitchPanel(rotatingPanel);
        if (controlStyle == "pointer")
        {
            Vector3 offset = (currentSelected.transform.position - transform.position).normalized;
            currentVoodoo = Instantiate(furniturePiece, transform.position+offset, currentSelected.transform.rotation);
            currentVoodoo.GetComponent<Renderer>().material = voodooMaterial;
        }
        pointerLine.SetActive(false);
        ManipulateFurniture(true, currentSelected);
    }

    //Change state, update UI, destroy voodoo if necessary.
    public void ConfirmRotation()
    {
        transformationState = "selecting";
        SwitchPanel(transformsPanel);
        if (currentVoodoo != null)
            Destroy(currentVoodoo); //Maybe set currentVoodoo = null
        if(controlStyle == "pointer")
            pointerLine.SetActive(true);
        ManipulateFurniture(false, currentSelected);
    }

    //Change state, revert rotation, destroy voodoo if necessary
    public void CancelRotation()
    {
        transformationState = "selecting";
        currentSelected.transform.localRotation = originalRotation;
        SwitchPanel(transformsPanel);
        if (currentVoodoo != null)
            Destroy(currentVoodoo);
        if (controlStyle == "pointer")
            pointerLine.SetActive(true);
        ManipulateFurniture(false, currentSelected);
    }

    //Create new wall instance. If virtual hand, spawn on wand (will fall to ground). If pointer, spawn on ground point being aimed at.
    public void CreateFurniture()
    {
        if (controlStyle == "hand")
        {    //Instantiate(wall, transform.position,new Quaternion(0,0,0,1),imageTarget.transform); //This is with virtual hand, need pointer
            GameObject newFurniture = Instantiate(furniturePiece, groundPlane.transform.position, new Quaternion(0, 0, 0, 1), groundPlane.transform);
            furnitureList.Add(newFurniture);
        }
        else
        {
            int layerMask = 1 << 9;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, layerMask)) //If the Raycast hit a selectable object, display hover silhouette
            {
                Instantiate(furniturePiece, hit.point + new Vector3(0f, furniturePiece.transform.localScale.y / 2, 0f), new Quaternion(0, 0, 0, 1), imageTarget.transform);
            }
        }
        //Should it jump straight to translation so wall can be placed? or just place wherever it is?
        //If jump to translation, might want a panel for wall functions after creating wall or a cancel placement button, since cancel translation would not make sense (in the case of jumping to translation panel)
        GameObject miniObject = Instantiate(furniturePiece, WiM.transform.position, WiM.transform.rotation, WiM.transform);
        //Scale down mini
        miniObject.transform.localScale /= MINI_SCALE;
    }

    public void DeleteFurniture()
    {
        if (selectedArr.Count > 0)
        {
            foreach (GameObject obj in selectedArr) //Should be Furniture, not GameObject :(
            {
                int objID = obj.GetInstanceID();
                furnitureList.Remove(obj);
                Destroy(obj);
            }
            selectedArr.Clear();
        }
    }

    //Reload game
    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //Change wall state. If being manipulated, it is translucent and intangible. Otherwise, selectable and tangible.
    private void ManipulateFurniture(bool manipulating, GameObject furn)
    {
        foreach (Collider col in furn.GetComponentsInChildren<Collider>())
        {
            col.enabled = !manipulating;
        }
        //Renderer rend = wall.GetComponentInChildren<Renderer>();
        furn.GetComponentInChildren<Rigidbody>().isKinematic = manipulating;
        foreach (Renderer rend in furn.GetComponentsInChildren<Renderer>())
        {
            if (manipulating)
                rend.material = translucent;
            else
            {
                rend.material = silhouette;
                SetSilhouette(rend, 2f, Color.green);
            }
        }
    }

    //Switch 2D UI panels, save reference to current active panel.
    private void SwitchPanel(GameObject newPanel)
    {
        newPanel.SetActive(true);
        currentPanel.SetActive(false);
        currentPanel = newPanel;
    }

    private void ChangeMaterial(Material newMaterial) 
    {

    }

    public void CreateWIM()
    {
        GameObject wimParent = new GameObject();
        Vector3 center = FindGroupCenter(furnitureList);

        wimParent.transform.position = center;
        foreach (GameObject obj in furnitureList)
        {
            GameObject clone = Instantiate(obj, wimParent.transform);
            clone.transform.localScale /= 3.0f;
        }
        wimParent.transform.position = transform.position;

    }

    private Vector3 FindGroupCenter(List<GameObject> gameObjectList)
    {
        Vector3 center = new Vector3(0, 0, 0);
        foreach (GameObject obj in gameObjectList)
        {
            center += obj.transform.position;
        }
        center /= gameObjectList.Count;
        return center;
    }
}
