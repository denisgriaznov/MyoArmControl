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

        public GameObject myoObj = null;
        private ThalmicMyo myo;
        [SerializeField] private Sprite circleSprite;
        public RectTransform graphContainer;
        public RectTransform averageContainer;
        public RectTransform derivativeContainer;
        private Transform container;
        public DataProcessing dp;

        public int pose = 0;
        public Text poseText;
        public Text gestureForce;
        public List<float>[] filteredData = new List<float>[8];
        public List<float> average = new List<float>();
        public List<float> derivative = new List<float>();

        public bool isFiltering = true;
        private bool isNormalization = true;
        private List<float>[] originData = new List<float>[8];

        public float treshold = 70f;
        public float force = 0;
        public bool waitGesture = true;
        public int N = 100;

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

        private void Update()

        {
            float mean = 0;
           
            poseText.text = "Pose:" + pose.ToString();
            gestureForce.text = "Force:" + force.ToString();

            for (int i = 0; i < filteredData.Length; i++)
            { 
                originData[i]  = myo.GetEmg()[i];

                if (isFiltering)
                {
                    filteredData[i] = dp.mov_av(myo.GetEmg()[i], 5);
                    mean = mean + dp.getMean(filteredData[i]);
                }
                else
                {
                    filteredData[i] = myo.GetEmg()[i];
                    mean = mean + dp.getMean(dp.mov_av(filteredData[i],5));
                }

            }
            average.Add(mean);
            derivative.Add(average[N-1]- average[N - 2]);
            average.RemoveAt(0);
            derivative.RemoveAt(0);

            if ((mean > treshold) && (derivative[N - 1] < 0) && (derivative[N - 2] > 0))
            {
                waitGesture = false;
                force = mean - treshold;
            } 
            
            if (waitGesture)
            {
                ShowGraph(filteredData[1], graphContainer, 100f, Color.green);
                ShowGraph(average, averageContainer, 230f, Color.blue);
                ShowGraph(derivative, derivativeContainer, 130f, Color.red);
                force = 0;
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
            
            float xSize = 1f;

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

        public void writeNewFile()
        {
            int[] features = new int[100 * 8 + 1];
            for (int i = 0; i < features.Length; i++)
            {
                features[i] = i;
            }
            try
            {
                string path = "Assets/Resources/test.txt";
                StreamWriter writer = new StreamWriter(path, true);

                writer.WriteLine(string.Join(",", features));

                writer.Close();

            }
            catch (Exception ex)
            {
                throw new ApplicationException("Dont work", ex);
            }
        }
        public void writeCsv()
        {

            try
            {
                

                List<float> result = new List<float>();
                for (int i = 0; i < filteredData.Length; i++)
                {
                    result = result.Concat(filteredData[i]).ToList();
                }
                Debug.Log(result.Count);
                string path = "Assets/Resources/test.txt";
                //Write some text to the test.txt file
                StreamWriter writer = new StreamWriter(path, true);
                string newRow = "";

                if (isNormalization) result = dp.stand(result);
               
                newRow = newRow + string.Join(",", result) + ",";
                
                newRow = newRow + pose;
                writer.WriteLine(newRow);

                writer.Close();
                //UnityEngine.Debug.Log(("st:"+string.Join(",", record)));
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Dont work", ex);
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
