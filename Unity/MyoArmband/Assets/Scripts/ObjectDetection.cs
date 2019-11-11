using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using TensorFlow;
using System.Threading;
using System.Threading.Tasks;

namespace Thalmic.Myo
{
    public class ObjectDetection : MonoBehaviour {

        [Header("Constants")]

        private const int INPUT_SIZE = 90;


        [Header("Inspector Stuff")]
        public TextAsset model;
        public float[] result = new float[3] { 0.0f, 0.0f, 0.0f };

        [Header("Private member")]
        private TFGraph graph;
        private TFSession session;

        [Header("Thread stuff")]
        Thread _thread;
        float[] input;
        TFTensor[] output;
        bool dataUpdated = false;
        bool processingMyo = true;

        public GameObject graphObj = null;
        private Window_Graph myo;

        // Use this for initialization
        IEnumerator Start() {
            myo = graphObj.GetComponent<Window_Graph>();

            #if UNITY_ANDROID
                            TensorFlowSharp.Android.NativeBinding.Init();
            #endif

            input = new float[INPUT_SIZE * 8];

            for (int i = 0; i < INPUT_SIZE * 8; i++)
            {
                input[i] = 0.2f;
            }

            Debug.Log("Loading graph...");
            graph = new TFGraph();
            graph.Import(model.bytes);
            session = new TFSession(graph);
            Debug.Log("Graph Loaded!!!");

            // Begin our heavy work on a new thread.
            _thread = new Thread(ThreadedWork);
            _thread.Start();
            //do this to avoid warnings
            processingMyo = true;
            yield return new WaitForEndOfFrame();
            processingMyo = false;
        }

        int[][] GetData(string path)
        {
            return System.IO.File.ReadAllText(path).Split(new[] { '\r', '\n' })
               .Select(r => r.Split(new[] { ',' })
                   .Select(c => System.Convert.ToInt32(c)).ToArray()
               ).ToArray();
        }

        void ThreadedWork() {
            while (true) {
                if (dataUpdated) {
                    TFShape shape = new TFShape(1, INPUT_SIZE, 8);
                    var tensor = TFTensor.FromBuffer(shape, input, 0, input.Length);
                    var runner = session.GetRunner();
                    runner.AddInput(graph["lstm_13_input_1"][0], tensor);
                    runner.Fetch(graph["dense_12_1/Softmax"][0]);
                    output = runner.Run();
                    float[,] res = output[0].GetValue() as float[,];
                    result[0] = res[0, 0];
                    result[1] = res[0, 2];
                    //result[2] = res[0, 2];

                    dataUpdated = false;
                }
            }
        }

        public void predict()
        {
            float[] data = new float[720] {1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,1,1,1,1,1,2,1,1,1,1,1,1,1,1,2,2,3,3,3,3,4,5,5,6,5,5,4,3,3,3,3,3,2,2,1,3,3,3,4,4,3,3,2,2,2,2,3,4,4,4,4,5,5,5,4,4,5,4,3,2,2,2,2,1,1,1,1,1,2,2,2,2,2,3,3,3,4,4,4,4,3,2,2,2,3,3,3,3,2,2,2,1,1,1,1,1,2,3,3,3,4,3,4,3,3,3,3,2,2,2,5,6,8,8,8,8,9,7,6,6,7,7,5,4,5,7,8,6,5,6,7,8,6,7,7,6,7,6,7,8,8,8,8,7,6,6,7,7,6,5,4,4,4,3,3,3,2,2,3,3,4,3,4,4,4,4,4,3,2,2,2,3,3,2,3,2,3,3,2,3,4,7,10,12,14,14,11,12,11,12,14,13,13,19,21,18,13,24,35,42,39,37,34,35,26,17,14,17,24,25,19,18,14,25,31,37,40,38,37,36,36,36,36,40,37,31,21,17,12,14,16,28,33,34,33,30,28,28,24,16,10,16,13,17,18,27,39,45,45,37,38,38,33,29,25,5,4,2,2,2,2,3,4,4,4,3,3,2,2,2,2,3,3,2,3,4,4,4,4,5,4,4,5,7,10,10,11,12,12,11,12,13,14,15,13,12,12,9,8,7,11,10,8,9,9,12,15,15,16,13,13,11,7,8,8,7,10,12,13,11,13,17,19,20,20,16,16,13,10,6,4,4,4,6,7,9,10,10,10,6,6,4,5,6,8,3,3,2,2,2,1,2,3,3,3,3,4,3,3,3,3,4,3,2,2,3,3,3,4,4,3,4,5,6,7,7,6,6,5,3,3,4,6,6,5,5,4,4,4,3,4,4,3,4,3,5,5,6,6,5,5,4,3,2,1,2,2,3,4,3,3,4,5,5,4,3,3,3,3,2,2,2,3,3,3,3,3,3,2,2,2,2,2,3,2,2,2,2,2,1,1,1,1,1,1,2,2,1,1,1,2,1,1,1,1,1,1,2,2,2,2,2,2,3,3,3,2,3,3,2,3,3,3,4,3,2,2,2,2,1,3,3,3,4,3,3,3,3,2,2,1,3,5,6,5,6,8,9,9,8,6,7,6,4,3,3,4,5,4,3,3,2,2,1,1,2,2,3,3,3,3,2,2,2,1,2,1,1,1,1,0,0,0,0,0,1,1,1,1,1,1,2,2,1,2,1,1,1,1,1,1,1,1,1,1,1,1,1,2,1,2,2,2,3,2,2,2,2,1,1,2,2,3,3,3,3,2,1,0,0,0,3,6,9,10,10,10,9,7,5,5,7,9,9,8,6,6,5,4,4,4,5,5,4,3,2,2,2,2,2,1,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,1,1,1,1,1,1,1,1,1,1,1,1,0,1,1,1,1,1,2,3,3,4,4,4,4,2,2,2,4,4,4,4,4,5,5,5,6,5,6,5,4,4,2,0,7,11,14,15,17,18,17,15,10,6,11,10,10,8,7,7,6,6,5,4,5,6,6,6,5,5,3,3,2};
            TFShape shape = new TFShape(1, INPUT_SIZE, 8);
            var tensor = TFTensor.FromBuffer(shape, data, 0, data.Length);
            var runner = session.GetRunner();
            runner.AddInput(graph["lstm_13_input_1"][0], tensor);
            runner.Fetch(graph["dense_12_1/Softmax"][0]);
            output = runner.Run();
            float[,] res = output[0].GetValue() as float[,];
            result[0] = res[0, 0];
            result[1] = res[0, 1];
            Debug.Log(res[0, 0].ToString() + " " + res[0, 1].ToString() + " " + res[0, 2].ToString());
        }

        IEnumerator ProcessImage() {
            List<int>[] filteredData = myo.filteredData;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < INPUT_SIZE; j++)
                {
                    try
                    {
                        input[i * INPUT_SIZE + j] = (float)filteredData[i][j];
                    } catch
                    {

                    }
                }
                   
            }
            //flip bool so other thread will execute
            dataUpdated = true;
            //Resources.UnloadUnusedAssets();
            processingMyo = false;
            yield return null;
        }

        private void Update() {
            if (!dataUpdated && !processingMyo) {
                processingMyo = true;
                StartCoroutine(ProcessImage());
            }
            

        }

    }
}

