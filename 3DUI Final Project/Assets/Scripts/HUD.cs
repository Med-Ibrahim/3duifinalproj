using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//Final proj
public class HUD : MonoBehaviour
{
    public GameObject confirmBtn;
    public GameObject pointerLine;
    public GameObject deselectBtn;
    public GameObject translateBtn; //Can prob get rid of all of these buttons (except confirmbtn i think)
    public GameObject stopTranslateBtn;
    public GameObject scaleBtn;
    public GameObject rotateBtn;
    //public GameObject turret;
    //public GameObject voodooTower;  //Might be able to just instantiate the regular prefab and modify the material, but seems like more work
    public GameObject voodooWall;
    public GameObject wall; //Should be replaced by the current selected furniture object
    public GameObject imageTarget;
    public GameObject selectingPanel;
    public GameObject translatingPanel;
    public GameObject rotatingingPanel;
    public GameObject scalingPanel;
    public GameObject transformsPanel; //Possibly add panel for not selecting? Default panel of (Select Object[, select world?][end game?][controls?])
    //public GameObject resetPanel;
    public Material translucent;
    public Material silhouette;
    public Material voodooMaterial;
    public const float SCALE_FACTOR = 1.50f; //? Wanted to add sensitivity slider, but unnecessary. also public const dont work like that lmao
    public GameObject Wand;

    //Initialize some defaults and variables to be used later
    private GameObject currentPanel;
    private GameObject currentHover = null;
    private GameObject currentSelected = null;
    private string controlStyle = "hand";
    private string transformationState = "selecting"; //Could separate into various bools. e.g., isTranslating, isRotating, etc.
    private Vector3 scaleOrigin;
    private float scaleBy;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private int wallCount;
    private GameObject currentVoodoo;

    void Start()
    {
        currentPanel = selectingPanel;
    }

    void Update()
    { 
    
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
    public void ConfirmSelection()
    {
        // Make sure something is being hovered
        if (currentHover != null)
        {
            currentSelected = currentHover;
            foreach (Renderer rend in currentSelected.GetComponentsInChildren<Renderer>())
            {
                SetSilhouette(rend, 0.05f, Color.green);
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
        if (currentSelected)
        {
            foreach (Renderer rend in currentSelected.GetComponentsInChildren<Renderer>())
            {
                SetSilhouette(rend, 0f, Color.yellow);
            }
            currentSelected = null;
        }
        SwitchPanel(selectingPanel);
    }

    //Change transformation state, store original position of object incase the user decides to cancel translation, and change wall appearance/tangibility.
    public void StartTranslating()
    {
        transformationState = "translating";
        originalPosition = currentSelected.transform.position;
        SwitchPanel(translatingPanel);
        ManipulateWall(true, currentSelected);
    }

    //Change state, update UI, make wall tangible
    public void ConfirmTranslation()
    {
        transformationState = "selecting";
        SwitchPanel(transformsPanel);
        ManipulateWall(false, currentSelected);
    }

    //Return object to original position, change state, update UI, make wall tangible.
    public void CancelTranslation()
    {
        transformationState = "selecting";
        currentSelected.transform.position = originalPosition;
        SwitchPanel(transformsPanel);
        ManipulateWall(false, currentSelected);
    }

    //Change state, store original scale, update UI, make wall intangible
    public void StartScaling()
    {
        transformationState = "scaling";
        scaleOrigin = transform.position;
        originalScale = currentSelected.transform.localScale;
        originalPosition = currentSelected.transform.position;
        SwitchPanel(scalingPanel);
        ManipulateWall(true, currentSelected);
    }

    //Change state, update UI, make wall tangible
    public void ConfirmScale()
    {
        transformationState = "selecting";
        SwitchPanel(transformsPanel);
        ManipulateWall(false, currentSelected);
    }

    //Change state, revert scale, update UI, tangible
    public void CancelScale()
    {
        transformationState = "selecting";
        currentSelected.transform.localScale = originalScale;
        currentSelected.transform.position = originalPosition;
        SwitchPanel(transformsPanel);
        ManipulateWall(false, currentSelected);
    }

    //Change state, store original rotation (different for wall/turret), change UI. Create Voodoo doll instance corresponding to selected object.
    public void StartRotating()
    {
        transformationState = "rotating";
        originalRotation = currentSelected.transform.localRotation;
        SwitchPanel(rotatingingPanel);
        if (controlStyle == "pointer")
        {
            Vector3 offset = (currentSelected.transform.position - transform.position).normalized;
            currentVoodoo = Instantiate(wall, transform.position + offset, currentSelected.transform.rotation);
            currentVoodoo.GetComponent<Renderer>().material = voodooMaterial;
        }
        pointerLine.SetActive(false);
        ManipulateWall(true, currentSelected);
    }

    //Change state, update UI, destroy voodoo if necessary.
    public void ConfirmRotation()
    {
        transformationState = "selecting";
        SwitchPanel(transformsPanel);
        if (currentVoodoo != null)
            Destroy(currentVoodoo); //Maybe set currentVoodoo = null
        if (controlStyle == "pointer")
            pointerLine.SetActive(true);
        ManipulateWall(false, currentSelected);
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
        ManipulateWall(false, currentSelected);
    }

    //Create new wall instance. If virtual hand, spawn on wand (will fall to ground). If pointer, spawn on ground point being aimed at.
    public void CreateWall()
    {
        if (controlStyle == "hand")
            //Instantiate(wall, transform.position,new Quaternion(0,0,0,1),imageTarget.transform); //This is with virtual hand, need pointer
            Instantiate(wall, transform.position, new Quaternion(0, 0, 0, 1));
        else
        {
            int layerMask = 1 << 9;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, layerMask)) //If the Raycast hit a selectable object, display hover silhouette
            {
                Instantiate(wall, hit.point + new Vector3(0f, wall.transform.localScale.y / 2, 0f), new Quaternion(0, 0, 0, 1), imageTarget.transform);
            }
        }
        //Should it jump straight to translation so wall can be placed? or just place wherever it is?
        //If jump to translation, might want a panel for wall functions after creating wall or a cancel placement button, since cancel translation would not make sense (in the case of jumping to translation panel)
    }

    public void ResetObject()
    {
        currentSelected.transform.localEulerAngles = new Vector3(0, 0, 0);
        currentSelected.transform.localScale = wall.transform.GetChild(0).localScale;
        Vector3 currPos = currentSelected.transform.position;
        currentSelected.transform.position = new Vector3(currPos.x, 3/*wall.transform.localScale.y / 2*/, currPos.z);
    }

    //Reload game
    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //Change wall state. If being manipulated, it is translucent and intangible. Otherwise, selectable and tangible.
    private void ManipulateWall(bool manipulating, GameObject wall)
    {
        foreach (Collider col in wall.GetComponentsInChildren<Collider>())
        {
            col.enabled = !manipulating;
        }
        Renderer rend = wall.GetComponentInChildren<Renderer>();
        wall.GetComponentInChildren<Rigidbody>().isKinematic = manipulating;
        if (manipulating)
            rend.material = translucent;
        else
        {
            rend.material = silhouette;
            SetSilhouette(rend, .05f, Color.green);
        }
    }

    //Switch 2D UI panels, save reference to current active panel.
    private void SwitchPanel(GameObject newPanel)
    {
        newPanel.SetActive(true);
        currentPanel.SetActive(false);
        currentPanel = newPanel;
    }
}
