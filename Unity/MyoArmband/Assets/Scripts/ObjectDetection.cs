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
        public float[] result = new float[3] { 0.0f, 0.0f, 0.0f};
        public string Pose = "None";

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

        void ThreadedWork() {
            while (true) {
                if (dataUpdated) {
                    predict(input);
                }
            }
        }

        public void predict(float[] input)
        {
            TFShape shape = new TFShape(1, 8, INPUT_SIZE);
            var tensor = TFTensor.FromBuffer(shape, input, 0, input.Length);
            var runner = session.GetRunner();
            runner.AddInput(graph["lstm_21_input_4"][0], tensor);
            runner.Fetch(graph["dense_18_4/Softmax"][0]);
            output = runner.Run();
            float[,] res = output[0].GetValue() as float[,];
            result[0] = res[0, 0];
            result[1] = res[0, 1];
            result[2] = res[0, 2];
            if (res[0, 0] > 0.9f)
            {
                Pose = "Fist";
                Thread.Sleep(1000);
            }
            if (res[0, 1] > 0.9f) 
            {
                Pose = "Glave";
                Thread.Sleep(1000);
            }
            if (res[0, 2] > 0.7f) Pose = "No Action";
            
            dataUpdated = false;
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
                    } catch { }
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

