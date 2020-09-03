using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


namespace Thalmic.Myo
{
    public class Calibrate : MonoBehaviour
    {
        // Start is called before the first frame update
        public Text state;
        public bool isCalibrating = false;
        public GameObject[] columns;
        public GameObject[] newcolumns;
        public GraphVizualizer graphVizualizer;
        private float[] stand;
        public float[] newdata;

        DataProcessing dp;
        public Text shifttext;
        public Text forcetext;

        void Start()
        {
            state.text = "Calibrated";
            dp = new DataProcessing();
            stand = new float[] { 19.44429f, 44.68857f, 47.70428f, 36.51571f, 13.99857f, 6.862856f, 5.420001f, 7.87143f };
            newdata = new float[] { 23, 54, 65, 75, 35, 56, 35, 75 };
            var max = dp.getMax(stand);
            
            for (int i = 0; i < 8; i++)
            {
                stand[i] = (stand[i] / max) * 140;
            }
            for (int i = 0; i < 8; i++)
            {
                columns[i].GetComponent<RectTransform>().sizeDelta = new Vector2(50, stand[i] );
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void startCalibrating()
        {
            if (!isCalibrating)
            {
                isCalibrating = true;
                StartCoroutine("calibrating");
            }

        }

        IEnumerator calibrating()
        {
            state.text = "Perform wave out gesture";
            graphVizualizer.calibrationShift = 0;
            while (graphVizualizer.waitGesture)
            {
                yield return new WaitForSecondsRealtime(0.2f);

            }

            for (int i = 0; i < 8; i++)
            {
                newdata[i] = dp.getMean(graphVizualizer.filteredData[i],0);
            }
            var max = dp.getMax(newdata);

            for (int i = 0; i < 8; i++)
            {
                newdata[i] = (newdata[i] / max) * 140;
                newcolumns[i].GetComponent<RectTransform>().sizeDelta = new Vector2(50, newdata[i]);
            }
            graphVizualizer.calibrationShift = dp.getShift(stand, newdata);
            graphVizualizer.calibrationForce = graphVizualizer.force;
            isCalibrating = false;
            state.text = "Calibrated";
            shifttext.text = "Shift: " + graphVizualizer.calibrationShift;
            forcetext.text = "Force: " + graphVizualizer.calibrationForce;
            Debug.Log("gesture");
            yield return null;

        }
    }
}
