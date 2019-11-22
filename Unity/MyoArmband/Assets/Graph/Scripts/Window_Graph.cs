/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using System.IO;
namespace Thalmic.Myo
{

    public class Window_Graph : MonoBehaviour {

    public GameObject myoObj = null;
    private ThalmicMyo myo;
    [SerializeField] private Sprite circleSprite;
    private RectTransform graphContainer;
    private Transform container;
    private SavitzkyGolayFilter sgf;

    public int pose = 0;
    public Text poseText;
    public List<int>[] filteredData = new List<int>[8];

        private void Awake()
        {
        myo = myoObj.GetComponent<ThalmicMyo>();
        container = transform.Find("GraphContainer");
        graphContainer = container.GetComponent<RectTransform>();
        sgf = GetComponent<SavitzkyGolayFilter>();
         }

        void Start()
    {
        // Begin our heavy work in a coroutine.
        StartCoroutine(YieldingWork());
    }    

    IEnumerator YieldingWork()
    {
        bool workDone = false;

        while(!workDone)
        {
            // Let the engine run for a frame.
            yield return null;

            // Do Work...
        }
    }

    private void Update()
        
        {
            poseText.text = "Pose:" + pose.ToString(); 
            foreach (Transform child in container)
            {
                GameObject.Destroy(child.gameObject);
            }
            for (int i = 0; i < filteredData.Length; i++)
            {
                //filteredData[i] = sgf.filtering(myo.GetEmg()[i]);
                filteredData[i] = myo.GetEmg()[i];

            }
            ShowGraph(filteredData[1]);
        }

        private GameObject CreateCircle(Vector2 anchoredPosition) {
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

    private void ShowGraph(List<int> valueList) {
        float graphHeight = graphContainer.sizeDelta.y;
        float yMaximum = 150f;
        float xSize = 1.7f;

        GameObject lastCircleGameObject = null;
        for (int i = 0; i < valueList.Count; i++) {
            float xPosition = xSize + i * xSize;
            float yPosition = (valueList[i] / yMaximum) * graphHeight;
            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition));
            if (lastCircleGameObject != null) {
                CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObject = circleGameObject;
        }
    }

        public void writeNewFile()
        {
            int[] features = new int[100  * 8 + 1];
            for(int i = 0; i < features.Length; i++)
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
                string path = "Assets/Resources/test.txt";
                //Write some text to the test.txt file
                StreamWriter writer = new StreamWriter(path, true);
                string newRow = "";
                for (int i = 0; i < filteredData.Length; i++)
                {
                    newRow = newRow + string.Join(",", filteredData[i]) + ",";
                }
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

        private void CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB) {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = new Color(1, 0, 0, 1f);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 0.8f);
        rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
        rectTransform.localEulerAngles = new UnityEngine.Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));
    }
}
}
