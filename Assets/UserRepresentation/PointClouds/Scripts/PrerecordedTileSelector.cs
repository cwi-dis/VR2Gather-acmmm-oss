using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRT.UserRepresentation.Voice;
using VRTCore;
using VRT.Core;
using VRT.Transport.SocketIO;
using VRT.Transport.Dash;
using VRT.Orchestrator.Wrapping;
using System;

namespace VRT.UserRepresentation.PointCloud
{
    public class PrerecordedTileSelector : MonoBehaviour
    {
        PrerecordedPointcloud prerecordedPointcloud;
 
        // Pseudo-constant: number of tiles.
        const int nTiles = 4;
        // Pseudo-constant: orientation of each tile, relative to the pointcloud centroid.
        Vector3[] TileOrientation = new Vector3[nTiles]
        {
            new Vector3(0, 0, -1),
            new Vector3(1, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(-1, 0, 0)
        };

 
        // Number of qualities available per tile.
        int nQualities;

        // Datastructure that contains all bandwidth data (per-tile, per-sequencenumber, per-quality bitrate usage)
        private List<AdaptationSet>[] aTile = null;

        public enum SelectionAlgorithm { interactive, alwaysBest, frontTileBest, greedy, uniform, hybrid };
        //
        // Set by overall controller (in production): which algorithm to use for this scene run.
        //
        public SelectionAlgorithm algorithm = SelectionAlgorithm.interactive;
        //
        // Can be set (in scene editor) to print all decision made by the algorithms.
        //
        public bool debugDecisions = false;
        //
        // Set by overall controller (in production): total available bitrate for this run.
        //
        public double bitRatebudget = 1000000;
        //
        // Temporary public variable, set by PrerecordedReader: next pointcloud we are expecting to show.
        //
        public static long curIndex;

        private Vector3 cameraForward;
        private int[] tileOrder;
        
        
        string Name()
        {
            return "PrerecordedTileSelector";
        }
        //xxxshishir adaptation set struct
        public struct AdaptationSet
        {
            public string PCframe;
            //public double[] encodedSize;
            public List<double> encodedSize;
            public void addEncodedSize(double a, int i)
            {
                if (encodedSize == null)
                    encodedSize = new List<double>();
                if ((encodedSize.Count - 1) < i)
                    encodedSize.Add(a);
                else
                    encodedSize[i] = a;
            }
        }

        public void Init(PrerecordedPointcloud _prerecordedPointcloud, int _nQualities, int _nTiles)
        {
            prerecordedPointcloud = _prerecordedPointcloud;
            nQualities = _nQualities;
            Debug.Log($"{Name()}: PrerecordedTileSelector nQualities={nQualities}, nTiles={nTiles}");
            if (_nTiles != nTiles)
            {
                Debug.LogError($"{Name()}: Only {nTiles} tiles implemented");
            }
            aTile = new List<AdaptationSet>[nTiles];

            //xxxshishir load the tile description csv files
            string rootFolder = Config.Instance.LocalUser.PCSelfConfig.PrerecordedReaderConfig.folder;
            string [] tileFolder = Config.Instance.LocalUser.PCSelfConfig.PrerecordedReaderConfig.tiles;
            for(int i=0;i < aTile.Length;i++)
            {
                aTile[i] = new List<AdaptationSet>();
                FileInfo tileDescFile = new FileInfo(System.IO.Path.Combine(rootFolder, tileFolder[i], "tiledescription.csv"));
                if(!tileDescFile.Exists)
                {
                    aTile = null; // Delete tile datastructure to forestall further errors
                    throw new System.Exception($"Tile description not found for tile " + i + " at" + System.IO.Path.Combine(rootFolder, tileFolder[i], "tiledescription.csv"));
                }
                StreamReader tileDescReader = tileDescFile.OpenText();
                //Skip header
                var aLine = tileDescReader.ReadLine();
                AdaptationSet aFrame = new AdaptationSet();
                while ((aLine = tileDescReader.ReadLine()) != null)
                {                  
                    var aLineValues = aLine.Split(',');
                    aFrame.PCframe = aLineValues[0];
                    for(int j =1;j<aLineValues.Length;j++)
                    {
                        aFrame.addEncodedSize(double.Parse(aLineValues[j]), j - 1);
                    }
                    aTile[i].Add(aFrame);
                    aFrame = new AdaptationSet();
                }
            }
        }

        private void Update()
        {
            //Debug.Log($"xxxjack PrerecordedPointcloud update called");
            if (prerecordedPointcloud == null || aTile == null)
            {
                // Not yet initialized
                return;
            }
            double[] a1 = aTile[0][(int)curIndex].encodedSize.ToArray();
            double[] a2 = aTile[1][(int)curIndex].encodedSize.ToArray();
            double[] a3 = aTile[2][(int)curIndex].encodedSize.ToArray();
            double[] a4 = aTile[3][(int)curIndex].encodedSize.ToArray();
            //xxxshishir debug code
            if (a1 == null)
            {
                Debug.Log("<color=red> Current Index </color> " + curIndex);
                a1 = aTile[0][0].encodedSize.ToArray();
            }
            if (a2 == null)
                a2 = aTile[1][0].encodedSize.ToArray();
            if (a3 == null)
                a3 = aTile[2][0].encodedSize.ToArray();
            if (a4 == null)
                a4 = aTile[3][0].encodedSize.ToArray();
            double budget = bitRatebudget;
            if (budget == 0) budget = 100000;
            //xxxshishir get camera orientation ToDo: Move to getTileOrder ?
            var cam = FindObjectOfType<Camera>().gameObject;
            if (cam == null)
                Debug.LogError("Camera not found!");
            Transform cameraTransform = cameraTransform = cam.transform;
            cameraForward = cameraTransform.forward;
            //Debug.Log("<color=red> Camera Transform </color>" + cameraForward.x + " " + cameraForward.y + " " + cameraForward.z);
            int[] selectedTileQualities = getTileQualities(a1, a2, a3, a4, budget);
            if (selectedTileQualities != null)
            {
                if(debugDecisions)
                {
                    // xxxjack: we could do this in stats: format too, may help analysis.
                    Debug.Log($"Name(): tileQualities: {selectedTileQualities[0]}, {selectedTileQualities[1]}, {selectedTileQualities[2]}, {selectedTileQualities[3]}");
                }
                prerecordedPointcloud.SelectTileQualities(selectedTileQualities);
            }
            //
            // Check whether the user wants to leave the scene (by pressing escape)
            //
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                //SceneManager.LoadScene("QualityAssesmentRatingScene", LoadSceneMode.Additive);
                SceneManager.LoadScene("QualityAssesmentRatingScene");
            }
        }

        void getTileOrder()
        {
            tileOrder = new int[nTiles];
            //Initialize index array
            for (int i = 0; i < nTiles; i++)
            {
                tileOrder[i] = i;
            }
            float[] tileUtilities = new float[nTiles];
            for (int i=0; i<nTiles; i++)
            {
                tileUtilities[i] = Vector3.Dot(cameraForward, TileOrientation[i]);
            }
            //Sort tile utilities and apply the same sort to tileOrder
            Array.Sort(tileUtilities, tileOrder);
            //The tile vectors represent the camera that sees the tile not the orientation of tile surface (ie dot product of 1 is highest utility tile, dot product of -1 is the lowest utility tile)
            Array.Reverse(tileOrder);
        }

        // Get array of per-tile quality wanted, based on current timestamp/framenumber, budget
        // and algorithm
        int[] getTileQualities(double[] a1, double[] a2, double[] a3, double[] a4, double budget)
        {
            switch (algorithm)
            {
                case SelectionAlgorithm.interactive:
                    return getTileQualities_Interactive(a1, a2, a3, a4, budget);
                case SelectionAlgorithm.alwaysBest:
                    return getTileQualities_AlwaysBest(a1, a2, a3, a4, budget);
                case SelectionAlgorithm.frontTileBest:
                    return getTilesFrontTileBest(a1, a2, a3, a4, budget);
                case SelectionAlgorithm.greedy:
                    return getTileQualities_Greedy(a1, a2, a3, a4, budget);
                case SelectionAlgorithm.uniform:
                    return getTileQualities_Uniform(a1, a2, a3, a4, budget);
                case SelectionAlgorithm.hybrid:
                    return getTileQualities_Hybrid(a1, a2, a3, a4, budget);
                default:
                    Debug.LogError($"{Name()}: Unknown algorithm");
                    return null;
            }
        }

        int[] getTileQualities_Interactive(double[] a1, double[] a2, double[] a3, double[] a4, double budget)
        {
            int[] selectedQualities = new int[nTiles];
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                for (int i = 0; i < nTiles; i++) selectedQualities[i] = 0;
                return selectedQualities;
            }
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                for (int i = 0; i < nTiles; i++) selectedQualities[i] = nQualities - 1;
                return selectedQualities;
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                for (int i = 0; i < nTiles; i++) selectedQualities[i] = 0;
                selectedQualities[0] = nQualities - 1;
                return selectedQualities;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                for (int i = 0; i < nTiles; i++) selectedQualities[i] = 0;
                selectedQualities[1] = nQualities - 1;
                return selectedQualities;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                for (int i = 0; i < nTiles; i++) selectedQualities[i] = 0;
                selectedQualities[2] = nQualities - 1;
                return selectedQualities;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                for (int i = 0; i < nTiles; i++) selectedQualities[i] = 0;
                selectedQualities[3] = nQualities - 1;
                return selectedQualities;
            }
            return null;
        }
        int[] getTileQualities_AlwaysBest(double[] a1, double[] a2, double[] a3, double[] a4, double budget)
        {
            int[] selectedQualities = new int[nTiles];

            for (int i = 0; i < nTiles; i++) selectedQualities[i] = nQualities - 1;
            return selectedQualities;
        }
        int[] getTilesFrontTileBest(double[] a1, double[] a2, double[] a3, double[] a4, double budget)
        {
            getTileOrder();
            int[] selectedQualities = new int[nTiles];
            for (int i = 0; i < nTiles; i++) selectedQualities[i] = 0;
            selectedQualities[tileOrder[0]] = nQualities - 1;
            return selectedQualities;
        }

        //xxxshishir actual tile selection strategies used for evaluation
        int[] getTileQualities_Greedy(double[] a1, double[] a2, double[] a3, double[] a4, double budget)
        {
            double savings;
            double spent = 0;
            getTileOrder();
            int[] selectedQualities = new int[nTiles];
            selectedQualities[0] = 0;
            selectedQualities[1] = 0;
            selectedQualities[2] = 0;
            selectedQualities[3] = 0;
            double[][] adaptationSet = new double[nTiles][];
            adaptationSet[0] = a1;
            adaptationSet[1] = a2;
            adaptationSet[2] = a3;
            adaptationSet[3] = a4;
            spent = a1[0] + a2[0] + a3[0] + a4[0];
            double nextSpend;
            bool representationSet = false;
            bool stepComplete = false;
            while (representationSet != true)
            {
                stepComplete = false;
                for (int i = 0; i < nTiles; i++)
                {
                    if (selectedQualities[tileOrder[i]] < (a1.Length - 1))
                    {
                        nextSpend = adaptationSet[tileOrder[i]][(selectedQualities[tileOrder[i]] + 1)] - adaptationSet[tileOrder[i]][selectedQualities[tileOrder[i]]];
                        if ((spent + nextSpend) <= budget)
                        {
                            selectedQualities[tileOrder[i]]++;
                            stepComplete = true;
                            spent = spent + nextSpend;
                            break;
                        }
                    }
                }
                if (stepComplete == false)
                {
                    representationSet = true;
                    savings = budget - spent;
                    // UnityEngine.Debug.Log("<color=green> XXXDebug Budget" + budget + " spent " + spent + " savings " + savings + " </color> ");
                }
            }
            return selectedQualities;
        }
        int[] getTileQualities_Uniform(double[] a1, double[] a2, double[] a3, double[] a4, double budget)
        {
            double savings;
            getTileOrder();
            int[] selectedQualities = new int[nTiles];
            selectedQualities[0] = 0;
            selectedQualities[1] = 0;
            selectedQualities[2] = 0;
            selectedQualities[3] = 0;
            double[][] adaptationSet = new double[nTiles][];
            adaptationSet[0] = a1;
            adaptationSet[1] = a2;
            adaptationSet[2] = a3;
            adaptationSet[3] = a4;
            double spent;
            spent = a1[0] + a2[0] + a3[0] + a4[0];
            double nextSpend;
            bool representationSet = false;
            bool stepComplete = false;
            while (representationSet != true)
            {
                stepComplete = false;
                for (int i = 0; i < nTiles; i++)
                {
                    if (selectedQualities[tileOrder[i]] < (a1.Length - 1))
                    {
                        nextSpend = adaptationSet[tileOrder[i]][(selectedQualities[tileOrder[i]] + 1)] - adaptationSet[tileOrder[i]][selectedQualities[tileOrder[i]]];
                        if ((spent + nextSpend) <= budget)
                        {
                            selectedQualities[tileOrder[i]]++;
                            stepComplete = true;
                            spent = spent + nextSpend;
                        }
                    }

                }
                if (stepComplete == false)
                {
                    representationSet = true;
                    savings = budget - spent;
                }
            }
            return selectedQualities;
        }
        int[] getTileQualities_Hybrid(double[] a1, double[] a2, double[] a3, double[] a4, double budget)
        {
            double savings;
            getTileOrder();
            bool[] tileVisibility = getTileVisibility();
            int[] selectedQualities = new int[nTiles];
            selectedQualities[0] = 0;
            selectedQualities[1] = 0;
            selectedQualities[2] = 0;
            selectedQualities[3] = 0;
            double[][] adaptationSet = new double[nTiles][];
            adaptationSet[0] = a1;
            adaptationSet[1] = a2;
            adaptationSet[2] = a3;
            adaptationSet[3] = a4;
            double spent;
            spent = a1[0] + a2[0] + a3[0] + a4[0];
            double nextSpend;
            bool representationSet = false;
            bool stepComplete = false;
            while (representationSet != true)
            {
                stepComplete = false;
                for (int i = 0; i < nTiles; i++)
                {
                    if (selectedQualities[tileOrder[i]] < (a1.Length - 1))
                    {
                        nextSpend = adaptationSet[tileOrder[i]][(selectedQualities[tileOrder[i]] + 1)] - adaptationSet[tileOrder[i]][selectedQualities[tileOrder[i]]];
                        if ((spent + nextSpend) <= budget && tileVisibility[tileOrder[i]] == true)
                        {
                            selectedQualities[tileOrder[i]]++;
                            stepComplete = true;
                            spent = spent + nextSpend;
                        }
                    }

                }
                //Increse representation of tiles facing away from the user if the visible tiles are already maxed
                if (stepComplete == false)
                {
                    for (int i = 0; i < nTiles; i++)
                    {
                        if (selectedQualities[tileOrder[i]] < (a1.Length - 1))
                        {
                            nextSpend = adaptationSet[tileOrder[i]][(selectedQualities[tileOrder[i]] + 1)] - adaptationSet[tileOrder[i]][selectedQualities[tileOrder[i]]];
                            if ((spent + nextSpend) < budget && tileVisibility[tileOrder[i]] == false)
                            {
                                selectedQualities[tileOrder[i]]++;
                                stepComplete = true;
                                spent = spent + nextSpend;
                            }
                        }

                    }
                }
                if (stepComplete == false)
                {
                    representationSet = true;
                    savings = budget - spent;
                }
            }
            return selectedQualities;
        }
        bool[] getTileVisibility()
        {
            bool[] tileVisibility = new bool[nTiles];
            //Tiles with dot product > 0 have the tile cameras facing in the same direction as the current scene camera (Note: TileC1-C4 contain the orientation of tile cameras NOT tile surfaces)
            for (int i = 0; i < nTiles; i++)
            {
                tileVisibility[i] = Vector3.Dot(cameraForward, TileOrientation[i]) > 0;
            }
            return tileVisibility;
        }
        public void setCamera(Vector3 Orientation)
        {
            cameraForward = Orientation;
        }
        public void setBudget(double budget)
        {
            bitRatebudget = budget;
        }
    }
}
