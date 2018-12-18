using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using ver2;

public class UIManager : MonoBehaviour
{
    #region UI Custom Events
    public delegate void BuildPlatformClicked(PlatformConfigurationData pcd);
    public static event BuildPlatformClicked BuildPlatformOnClicked;

    public delegate void UpdateCameraPosition(PlatformConfigurationData pcd);
    public static event UpdateCameraPosition OnUpdateComeraPosition;

    public delegate void NodeProgramChanged(Slider s);
    public static event NodeProgramChanged OnNodeProgramChanged;

    public delegate void WriteProgramData();
    public static event WriteProgramData OnWriteProgramData;

    public delegate void ReadFromText();
    public static event ReadFromText OnReadFromText;


    #endregion

    #region UI Control Ref.
    #region PLATFORM BUTTON CONTROLS
    //public Button buttonStartSimulation;

    public Text txtPlatformDimensions;
    public Text txtPlatformDeltaSpacing;
    public Text txtPlatformYAxisRange;
    #endregion

    #region PANEL RECT TRANSFORM
    //[Header("Rect Transform for Panels")]
    //bool DisplayConfigurationPanels = true;
    //public RectTransform panelPlatformConfiguration;
    //public RectTransform panelPlatformColorConfiguration;
    //public RectTransform panelPlatformNodeData;
    #endregion

    #region Platform Configuration UI Variables
    [Header("Platform Configuration Fields")]
    public InputField inputPlatformMDimension;
    public InputField inputPlatformNDimension;
    public Slider sliderDeltaSpacing;
    public Slider sliderYRange;

    public Slider sliderLerpSpeed;//added 12-17
    public Text txtLerpSpeed;//added 12-17
    #endregion

    #region Selected Node Information Display Variables
    [Header("Selected Node UI Controls")]
    //GameObject currentSelection = null;
    public Text txtSelectedNodeName;
    public Text txtSelectedNodePosition;
    //public Image imgSelectedNodeColor;
    #endregion
    #endregion

    void start(){
        txtPlatformDimensions = null;
        txtPlatformYAxisRange = null;
    }

    private void OnEnable()
    {
        PlatformManager.OnPlatformManagerChanged += PlatformManager_OnPlatformManagerChanged;
        PlatformManager.OnPlatformManagerUpdateUI += PlatformManager_OnPlatformManagerUpdateUI;

        PlatformDataNode.OnUpdatePlatformDataNodeUI += PlatformDataNode_OnUpdatePlatformDataNodeUI;
    }

    private void OnDisable()
    {
        PlatformManager.OnPlatformManagerChanged -= PlatformManager_OnPlatformManagerChanged;
        PlatformManager.OnPlatformManagerUpdateUI -= PlatformManager_OnPlatformManagerUpdateUI;

        PlatformDataNode.OnUpdatePlatformDataNodeUI -= PlatformDataNode_OnUpdatePlatformDataNodeUI;
    }

    private void PlatformDataNode_OnUpdatePlatformDataNodeUI(PlatformDataNode dataNode)
    {
        dataNode.sliderSelectedProgramNodeHeight = sliderYRange;

        // update the min/max values for slider
        dataNode.sliderSelectedProgramNodeHeight.minValue = 0;
        dataNode.sliderSelectedProgramNodeHeight.maxValue = PlatformManager.Instance.configurationData.RandomHeight;

        dataNode.txtSelectedNodeName = txtSelectedNodeName;
        dataNode.txtSelectedNodePosition = txtSelectedNodePosition;
    }

    private void PlatformManager_OnPlatformManagerUpdateUI()
    {
        if(!PlatformManager.Instance.Program && sliderYRange != null)
            sliderYRange.value = PlatformManager.Instance.configurationData.RandomHeight; //0.0f;
        else if(sliderYRange != null)
            sliderYRange.value = 0.0f;

        // check to see if they are null or not ...
        if (txtSelectedNodeName!=null)
            txtSelectedNodeName.text = "";
        if(txtSelectedNodePosition!=null)
            txtSelectedNodePosition.text = string.Format("Height:");
    }

    private void PlatformManager_OnPlatformManagerChanged(PlatformConfigurationData data)
    {
        if(data != null)
        {
            //Debug.Log("Hello");
            if (inputPlatformMDimension != null)
                inputPlatformMDimension.text = data.M.ToString();
            if (inputPlatformNDimension != null)
                inputPlatformNDimension.text = data.N.ToString();
            if (sliderDeltaSpacing != null)
                sliderDeltaSpacing.value = data.deltaSpace;
            if (sliderYRange != null && SceneManager.GetActiveScene().name.Contains("Programming"))//added by Moses 12-17. Reset slider to zero on programming scene
                sliderYRange.value = 0;
            else if (sliderYRange != null)
                sliderYRange.value = data.RandomHeight;

            if (txtPlatformDeltaSpacing!=null)
                txtPlatformDeltaSpacing.text = string.Format("{0:0.00}f", data.deltaSpace);
            if (txtPlatformDimensions!=null)
                txtPlatformDimensions.text = string.Format("{0}x{1}", data.M, data.N);
            if (txtPlatformYAxisRange!=null && SceneManager.GetActiveScene().name.Contains("Programming"))//added by Moses 12-17. Reset value to zero on programming scene
                txtPlatformYAxisRange.text = string.Format("{0}", 0);
            else if (txtPlatformYAxisRange!=null)
                txtPlatformYAxisRange.text = string.Format("{0:0.00}f", data.RandomHeight);



            if (OnUpdateComeraPosition != null)
                OnUpdateComeraPosition(data);
        }
    }

    #region FUNCTION TO HANDLE MAIN MENU FEATURES
    public void ButtonClicked(Button b)
    {
        switch(b.name)
        {
            case "Main Menu Button":
                {
                    Debug.Log("Load main menu scene");
                    SceneManager.LoadScene("Main Menu");
                    break;
                }
            case "Configuration Button":
                {
                    Debug.Log("Load configuration scene");
                    SceneManager.LoadScene("Configuration");
                    break;
                }
            case "Programming Button":
                {
                    Debug.Log("Load programming scene");
                    SceneManager.LoadScene("Programming");
                    break;
                }
            case "Simulate Button":
                {
                    Debug.Log("Load simulate scene");
                    SceneManager.LoadScene("Simulation");
                    break;
                }
            case "Build Button":
                {
                    Debug.Log("Build platform");
                    if (BuildPlatformOnClicked != null)
                    {
                        PlatformConfigurationData pcd = new PlatformConfigurationData();
                        pcd.M = Convert.ToInt32(inputPlatformMDimension.text);
                        pcd.N = Convert.ToInt32(inputPlatformNDimension.text);
                        pcd.deltaSpace = sliderDeltaSpacing.value;
                        pcd.RandomHeight = sliderYRange.value;
                        BuildPlatformOnClicked(pcd);
                    }

                    break;
                }

            case "Read Button":
                {
                    Debug.Log("reading button");
                    if (OnReadFromText != null)
                    {
                        OnReadFromText();
                    }   
                    break;

                }
            case "Program Platform Button":
                {
                    Debug.Log("Program selected node");

                    if (OnWriteProgramData != null)
                        OnWriteProgramData();

                    break;
                }

            case "Exit Button":
                {
                    Debug.Log("Exit application");
                    Application.Quit();
                    break;
                }
        }
    }

    public void SliderValueChanged(Slider s)
    {
        switch(s.name)
        {
            case "Delta Spacing Slider":
                {
                    txtPlatformDeltaSpacing.text = string.Format("{0:0.00}f", s.value);
                    break;
                }
            case "Y Displacement Slider":
                {
                    txtPlatformYAxisRange.text = string.Format("{0:0.00}f", s.value);
                    break;
                }
            case "Node Height Slider":
                {
                    txtPlatformYAxisRange.text = string.Format("{0:0.00}f", s.value);
                    if (OnNodeProgramChanged != null)
                        OnNodeProgramChanged(s);

                    break;
                }
            case "Speed Slider"://added 12-17
                {
                    txtLerpSpeed.text = string.Format("{0:0.00}f", s.value);
                    if (OnNodeProgramChanged != null)
                        OnNodeProgramChanged(s);

                    break;
                }
        }
    }
    #endregion
}
