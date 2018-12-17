using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace ver2
{
    public class PlatformManager : PlatformGenericSingleton<PlatformManager>
    {

        #region Platform Manager Custom Events
        public delegate void PlatformManagerChanged(PlatformConfigurationData data);
        public static event PlatformManagerChanged OnPlatformManagerChanged;

        public delegate void PlatformManagerUpdateUI();
        public static event PlatformManagerUpdateUI OnPlatformManagerUpdateUI;
        #endregion

        /*public enum ColorShade
        {
            GrayScale,
            RedScale,
            GreenScale,
            BlueScale,
            Random
        }*/

        public GameObject PlatformBasePref;
        public int oldM;
        public int oldN;

        public PlatformConfigurationData configurationData = new PlatformConfigurationData();
        float spaceX = 0.0f;
        float spaceZ = 0.0f;

        public GameObject[,] platformNode;

        //public bool SimulateTest = false;
        public bool Program = false;
        public bool Simulate = false;//added by Moses

        Queue<float []> rows = null;//added by Moses
        int nodeCounter = 0, totalNodes;//added by Moses

        //public ColorShade shade = ColorShade.GrayScale;

        #region Selected Node Information Display Variables
        [Header("Selected Node UI Controls")]
        GameObject currentSelection = null;
        //private PlatformDataNode[,] platformProgram;//removed by Moses 12-12

        //public Text txtSelectedNodeName;
        //public Text txtSelectedNodePosition;
        //public Image imgSelectedNodeColor;
        #endregion

        private void OnEnable()
        {
            UIManager.BuildPlatformOnClicked += UIManager_BuildPlatformOnClicked;
            UIManager.OnWriteProgramData += UIManager_OnWriteProgramData;
            UIManager.OnReadFromText += UIManager_OnReadFromText;
            //UIManager.OnUpdatePlatformNode += UIManager_OnUpdatePlatformNode;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            PlatformDataNode.OnNodeReachedPosition += PlatformDataNode_OnNodeReachedPosition;//added by Moses
        }

        private void OnDisable()
        {
            UIManager.BuildPlatformOnClicked -= UIManager_BuildPlatformOnClicked;
            UIManager.OnWriteProgramData -= UIManager_OnWriteProgramData;
            UIManager.OnReadFromText -= UIManager_OnReadFromText;
            //UIManager.OnUpdatePlatformNode -= UIManager_OnUpdatePlatformNode;
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            PlatformDataNode.OnNodeReachedPosition -= PlatformDataNode_OnNodeReachedPosition;//added by Moses
        }

        private void UIManager_BuildPlatformOnClicked(PlatformConfigurationData pcd)
        {
            Debug.Log("Sup");

            configurationData = pcd;

            BuildPlatform();

            //oldM = pcd.M;//removed 12-16
            //oldN = pcd.N;
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (platformNode != null)
            {
                if (SceneManager.GetActiveScene().name.Contains("Main Menu"))
                    DestroyPlatform();

                if (SceneManager.GetActiveScene().name.Contains("Program"))
                    Program = true;
                else
                    Program = false;

                
                //BuildPlatform();

                if (SceneManager.GetActiveScene().name.Contains("Simulation")){//Added by Moses 
                    //creates lists to control simulation
                    initializeSimulationControl();//added 12-3
                }
                else{
                    Simulate = false;
                    //Destroy 
                }

                if (OnPlatformManagerUpdateUI != null)
                    OnPlatformManagerUpdateUI();

                if(OnPlatformManagerChanged != null)
                    OnPlatformManagerChanged(configurationData);//to center the camera on platform (through UI manager -> camera control)

            }

            Debug.Log(SceneManager.GetActiveScene().name);
        }

        //private void UIManager_OnUpdatePlatformNode(PlatformDataNodeVer2 data)
        //{
        //    PlatformDataNodeVer2 pdn = platformNode[data.i, data.j].GetComponent<PlatformDataNodeVer2>();
        //    pdn = data;
        //}

        private void UIManager_OnReadFromText()
        {
            using (System.IO.StreamReader sr = new System.IO.StreamReader(Path.Combine(Application.dataPath, "WriteLines.txt")))
            {
                Debug.Log("Starting to Read from File");

                string line;
                line = sr.ReadLine();

                PlatformConfigurationData pcd = new PlatformConfigurationData();

                string[] config_string = line.Split(',');

                pcd.M = Convert.ToInt32(config_string[0]);
                pcd.N = Convert.ToInt32(config_string[1]);
                pcd.deltaSpace = (float)Convert.ToDouble(config_string[2]);
                pcd.RandomHeight = (float)Convert.ToDouble(config_string[3]);

                configurationData = pcd;
                Debug.Log("Platform Config Data Read");
                BuildPlatform();

                //platformProgram = new PlatformDataNode[configurationData.M, configurationData.N];//removed by Moses 12-12

                string[] data;
                while ((line = sr.ReadLine()) != null)
                {
                    data = line.Split(',');
                    /* PlatformDataNode nodeData = new PlatformDataNode();//changed by Moses 12-12

                    nodeData.i = Convert.ToInt32(data[0]);
                    nodeData.j = Convert.ToInt32(data[1]);
                    nodeData.NextPosition = (float)Convert.ToDouble(data[2]);

                    platformProgram[nodeData.i, nodeData.j] = nodeData;*/

                    int i = Convert.ToInt32(data[0]);//added by Moses 12-12
                    int j = Convert.ToInt32(data[1]);
                    platformNode[i,j].GetComponent<PlatformDataNode>().i = i;
                    platformNode[i,j].GetComponent<PlatformDataNode>().j = j;
                    platformNode[i,j].GetComponent<PlatformDataNode>().NextPosition = (float)Convert.ToDouble(data[2]);

                }
                Debug.Log("Node Data Read");
            }
        }

        private void UIManager_OnWriteProgramData()
        {
            // we will save the platform configuration data 
            // we will save the platform node program data
            Debug.Log("SAVING PLATFORM PROGRAM DATA ... SIMULATION");
            //Debug.Log(configurationData.ToString());

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(Application.dataPath, "WriteLines.txt")))
            {
                outputFile.WriteLine(configurationData.ToString());
                for (int i = 0; i < configurationData.M; i++)//edit by Moses (was oldM)
                {
                    for (int j = 0; j < configurationData.N; j++)//edit by Moses (was oldN)
                    {
                        //Debug.Log(platformNode[i, j].GetComponent<PlatformDataNode>().ToString());
                        outputFile.WriteLine(platformNode[i, j].GetComponent<PlatformDataNode>().ToString());
                    }
                }
            }
        }

        #region BUILD PLATFORM FROM UI

        public void DestroyPlatform()
        {
            // check to see if there is no platform currently configured
            // if there is one, delete it
            if (platformNode != null)
            {
                for (int i = 0; i < oldM; i++)
                {
                    for (int j = 0; j < oldN; j++)
                    {
                        //Destroy(platformNode[i, j], 0.1f);
                        Destroy(platformNode[i, j]);
                    }
                }

                platformNode = null;
            }
        }

        public void BuildPlatform()
        {
            DestroyPlatform();

            platformNode = new GameObject[configurationData.M, configurationData.N];

            spaceX = 0;
            spaceZ = 0;

            for (int i = 0; i < configurationData.M; i++)
            {
                spaceZ = 0.0f;
                for (int j = 0; j < configurationData.N; j++)
                {
                    float x = (i * 1) + spaceX;
                    float z = (j * 1) + spaceZ;

                    //Debug.Log(string.Format("x={0} z={1}", x, z));
                    // create a platform pref ...
                    var platformBase = Instantiate(PlatformBasePref,
                                                   new Vector3(x, 0, z),
                                                   Quaternion.identity);

                    platformBase.name = string.Format("Node[{0},{1}]", i, j);

                    platformBase.AddComponent<PlatformDataNode>();

                    platformNode[i, j] = platformBase;
                    platformNode[i, j].transform.SetParent(transform,true);//becomes child of PlatformManager, added by Moses

                    PlatformDataNode pdn = platformBase.transform.GetComponent<PlatformDataNode>();
                    pdn.Program = Program;
                    pdn.i = i;
                    pdn.j = j;

                    spaceZ += configurationData.deltaSpace;
                }
                spaceX += configurationData.deltaSpace;
            }

            if (OnPlatformManagerChanged != null)
                OnPlatformManagerChanged(configurationData);

            oldM=configurationData.M;
            oldN=configurationData.N;
        }

        public void StartSimulationButtonClick()
        {
            //SimulateTest = !SimulateTest;//removed by Moses
            
            Simulate = !Simulate;
        }
        #endregion

        public static bool NearlyEquals(float? value1, float? value2, float unimportantDifference = 0.01f)
        {
            if (value1 != value2)
            {
                if (value1 == null || value2 == null)
                    return false;

                return Math.Abs(value1.Value - value2.Value) < unimportantDifference;
            }

            return true;
        }

        // Update is called once per frame
        void Update()
        {

            // check to see if platform has been build
            if (platformNode == null)
                return;

            if (Input.GetKeyUp(KeyCode.Space))
                StartSimulationButtonClick();
            //if (Input.GetKeyUp(KeyCode.T))
            //    SimulateTest = !SimulateTest;
            //if (Input.GetKeyUp(KeyCode.H))
            //    shade = ColorShade.GrayScale;
            //if (Input.GetKeyUp(KeyCode.R))
            //    shade = ColorShade.RedScale;
            //if (Input.GetKeyUp(KeyCode.G))
            //    shade = ColorShade.GreenScale;
            //if (Input.GetKeyUp(KeyCode.B))
            //    shade = ColorShade.BlueScale;
            //if (Input.GetKeyUp(KeyCode.E))
            //    shade = ColorShade.Random;
            //if (Input.GetKeyUp(KeyCode.W))
            //{
            //    RandomHeight += 1;
            //    txtPlatformYAxisRange.text = string.Format("{0}", RandomHeight);
            //}
            //if (Input.GetKeyUp(KeyCode.S))
            //{
            //    RandomHeight -= 1;
            //    if (RandomHeight < 0)
            //        RandomHeight = 0;
            //    txtPlatformYAxisRange.text = string.Format("{0}", RandomHeight);
            //}
            if (Input.GetKey(KeyCode.Q))
                Application.Quit();

            #region Object Selection
            // you can select only if in program mode/scene
            if (Program)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (IsPointerOverUIObject())
                        return;

                    #region Screen To World
                    RaycastHit hitInfo = new RaycastHit();
                    bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
                    if (hit)
                    {
                        #region COLOR

                        if (currentSelection != null)
                        {
                            PlatformDataNode pdn = currentSelection.transform.GetComponent<PlatformDataNode>();
                            pdn.ResetDataNode();
                        }

                        currentSelection = hitInfo.transform.gameObject;
                        PlatformDataNode newPdn = currentSelection.transform.GetComponent<PlatformDataNode>();
                        newPdn.SelectNode();

                        #endregion
                    }
                    else
                    {
                        Debug.Log("No hit");
                    }
                    #endregion
                }
            }
            #endregion

            if(Simulate){//added by Moses
                float[] tempRow;
                
                if(nodeCounter == totalNodes){ //do not update control until all platforms reach their destinations
                    
                    //when each node reaches height, add 1 to nodeCounter
                    //this is handled by PlatformDataNode_OnNodeReachedPosition()

                    for(int i=0;i<configurationData.M;i++)
                    {
                        tempRow = rows.Dequeue();

                        //Debug.Log("Row "+i+": "+tempRow[0]+", "+tempRow[1]+", "+tempRow[2]+", "+tempRow[3]);//  <---  <---  <---  <---  <---  <---Remove later(for testing)

                        for(int j=0;j<configurationData.N;j++){
                            platformNode[i,j].GetComponent<PlatformDataNode>().NextPosition = tempRow[j];//may want to  
                            platformNode[i,j].GetComponent<PlatformDataNode>().Simulate = true;//use a delegate for these?    
                        }

                        rows.Enqueue(tempRow);//return to queue
                    }

                    tempRow = rows.Dequeue(); //shift the rows by one
                    rows.Enqueue(tempRow);    //will start with the next row

                    nodeCounter = 0;//reset to repeat process 
                }  
            }
        }

        /// <summary>
        /// Used to determine if we are over UI element or not.
        /// </summary>
        /// <returns></returns>
        private bool IsPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            //foreach (var result in results)
            //{
            //    Debug.Log(result.gameObject.name);
            //}
            return results.Count > 0;
        }

        #region Added by Moses
         private void initializeSimulationControl(){//added 12-3
            //create N lists, each are M nodes long
            rows = new Queue<float[]>();
            //float[] tempRow = new float[configurationData.N];

            for(int i=0; i<configurationData.M; i++){
                float[] tempRow = new float[configurationData.N];
                for(int j=0;j<configurationData.N;j++){
                    
                    platformNode[i,j].GetComponent<PlatformDataNode>().Program = false;//added 12-12
                    platformNode[i,j].GetComponent<PlatformDataNode>().Selected= false;//added 12-15
                    //platformNode[i,j].GetComponent<Material>().color = Color.white;//added 12-15
                    platformNode[i,j].GetComponent<Renderer>().material.color = Color.white;//added 12-15
                    platformNode[i,j].transform.position = new Vector3(platformNode[i,j].transform.position.x,0,platformNode[i,j].transform.position.z);//added 12 -15
                    tempRow[j]= platformNode[i,j].GetComponent<PlatformDataNode>().NextPosition;//added 12-12
                }
                rows.Enqueue(tempRow);
            }

            //Debug.Log("PM_initialize_Whats in the queue?");// <--- <--- <--- <--- <--- <--- <--- <--- <--- for testing purposes
            for(int i=0; i<configurationData.M; i++){
                float[] tempRow = new float[configurationData.N];
                tempRow = rows.Dequeue();
                //Debug.Log("PM_initialize_Row "+i+": "+tempRow[0]+", "+tempRow[1]+", "+tempRow[2]+", "+tempRow[3]);
                rows.Enqueue(tempRow);
             }

            totalNodes = configurationData.M * configurationData.N; //put outside update loop
            nodeCounter = totalNodes;//allows simulation to start

            Simulate = true;
        }

        private void destroySimulationControl(){//added 12-3

            for(int i=0; i<configurationData.M; i++){
                rows.Dequeue();
            }

            rows = null;
        }

        private void PlatformDataNode_OnNodeReachedPosition(){//added 12-3
            nodeCounter++;
            //Debug.Log("nodeCounter value: " + nodeCounter);
        }
        /*private void loadDataToNodes(){//pull data from file and place in row queue

            float[] tempRow;
            for(int i=0;i<configurationData.M;i++)
            {

                tempRow = rows.Dequeue();
                for(int j=0;j<configurationData.N;j++){
                    tempRow[j]= platformNode[i,j].GetComponent<PlatformDataNode>().NextPosition;//added 12-12
                }

                rows.Enqueue(tempRow);//return to queue
            }
            nodeCounter = totalNodes;//allows simulation to start
        }*/
        #endregion
    }
}


