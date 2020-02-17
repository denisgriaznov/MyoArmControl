using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using System.IO;
using System.Linq;

namespace Thalmic.Myo
{

    public class GraphVizualizer : MonoBehaviour
    {
        public int calibrationShift = 0;
        public float calibrationForce = 0;
        public GameObject myoObj = null;
        private ThalmicMyo myo;
        [SerializeField] private Sprite circleSprite;
        public RectTransform graphContainer;
        public RectTransform averageContainer;
        public RectTransform derivativeContainer;
        private Transform container;
        public DataProcessing dp;
        public Recorder recorder;
        
        
        public Text gestureForce;
        public List<float>[] filteredData = new List<float>[8];
        public List<float> average = new List<float>();
        public List<float> derivative = new List<float>();

        public bool isFiltering = true;
        public bool isNormalization = true;
        private List<float>[] originData = new List<float>[8];

        public float treshold = 70f;
        public float force = 0;
        public bool waitGesture = true;
        public static int N = 140;
        float mean = 0;

        public bool autorecord = false;
        public ObjectDetection objectDetection;
        public bool detecting;

        public JointOrientation jointOrientation;

        private void Awake()
        {
            myo = myoObj.GetComponent<ThalmicMyo>();

            for(int i = 0; i < N; i++)
            {
                
                average.Add(0);
                derivative.Add(0);
            }
            
            //graphContainer = container.GetComponent<RectTransform>();
            //dp = GetComponent<DataProcessing>();
        }

        void Start()
        {
            // Begin our heavy work in a coroutine.
            StartCoroutine(YieldingWork());
            StartCoroutine(Detecting());
        }

        public void ChangeFiltering()
        {
            isFiltering = !isFiltering;
        }

        public void ChangeNormalization()
        {
            isNormalization = !isNormalization ;
        }

        public void ChangeWait()
        {
            waitGesture = !waitGesture;
        }

        public void ChangeRecord()
        {
            autorecord = !autorecord;
        }

        IEnumerator YieldingWork()
        {
            bool workDone = false;

            while (!workDone)
            {
                // Let the engine run for a frame.
                yield return null;

                // Do Work...
            }
        }

        public void onTresholdChange(string trshString)
        {
            Debug.Log(trshString);
            if (!float.TryParse(trshString, out treshold))
            {
                treshold = 100;
            }

        }

        private void Update()

        {
            mean = 0;
           
            //poseText.text = "Pose:" + pose.ToString();
            gestureForce.text = "Force:" + force.ToString();

            for (int i = 0; i < filteredData.Length; i++)
            { 
                originData[(i + calibrationShift) % 8] = myo.GetEmg()[i];
            }
            for (int i = 0; i < filteredData.Length; i++)
                {
                    if (isFiltering)
                {
                    filteredData[i] = dp.mov_av(originData[i], 5);
                    mean = mean + dp.getMean(filteredData[i],30);
                }
                else
                {
                    filteredData[i] = originData[i];
                    mean = mean + dp.getMean(dp.mov_av(filteredData[i],5),30);
                }

            }
            Debug.Log(originData[1].Count);
            average.Add(mean);
            derivative.Add(average[N-1]- average[N - 2]);
            average.RemoveAt(0);
            derivative.RemoveAt(0);

            

        }

        IEnumerator Detecting()
        {
            while(true)
            {
                if ((average[N-2] > treshold) && (average[N - 2] > average[N - 1]))
                {
                    waitGesture = false;
                    force = average[N - 2] - treshold;

                    if (detecting)
                    {
                        List<float> result = new List<float>();
                        for (int i = 0; i < filteredData.Length; i++)
                        {
                            result = result.Concat(filteredData[i]).ToList();
                        }
                        result = dp.stand(result);
                        float[] input = result.ToArray();
                        objectDetection.predict(input, force );
                    }

                    if (autorecord)
                    {
                        recorder.writeCsv();
                    }
                    
                    yield return new WaitForSeconds(0.8f);
                    waitGesture = true;
                }

                if (waitGesture)
                {
                    Debug.Log("wait");
                    try
                    { 
                        ShowGraph(filteredData[1], graphContainer, 100f, Color.green);
                        ShowGraph(average, averageContainer, 230f, Color.blue);
                        ShowGraph(derivative, derivativeContainer, 130f, Color.red);
                        force = 0;
                    }
                    catch
                    {

                    }
                }
                yield return null;
            }
        }

        private GameObject CreateCircle(Vector2 anchoredPosition, RectTransform graphContainer)
        {
            GameObject gameObject = new GameObject("circle", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().sprite = circleSprite;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(0.1f, 0.1f);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            return gameObject;
        }

        private void ShowGraph(List<float> valueList, RectTransform graphContainer, float yMaximum , Color color)
        {
            container = graphContainer.transform;
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }

            float graphHeight = graphContainer.sizeDelta.y;
            
            float xSize = 0.7f;

            GameObject lastCircleGameObject = null;
            for (int i = 0; i < valueList.Count; i++)
            {
                float xPosition = xSize + i * xSize;
                float yPosition = (valueList[i] / yMaximum) * graphHeight;
                GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition), graphContainer);
                if (lastCircleGameObject != null)
                {
                    CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition, graphContainer,color);
                }
                lastCircleGameObject = circleGameObject;
            }
        }

        

        private void CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB, RectTransform graphContainer,Color color)
        {
            GameObject gameObject = new GameObject("dotConnection", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().color = color;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            Vector2 dir = (dotPositionB - dotPositionA).normalized;
            float distance = Vector2.Distance(dotPositionA, dotPositionB);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(distance, 1.2f);
            rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
            rectTransform.localEulerAngles = new UnityEngine.Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));
        }
    }
}
