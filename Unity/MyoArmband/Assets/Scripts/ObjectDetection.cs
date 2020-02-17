using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TensorFlow;
using System.Threading;
using System.Threading.Tasks;

namespace Thalmic.Myo
{
    public class ObjectDetection : MonoBehaviour {

        public ModeManager modeManager;
        [Header("Constants")]

        private const int INPUT_SIZE = 140;


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

        public GraphVizualizer myo;

        public JointOrientation jointOrientation;

        public int detectedPose = 0;

        public Image[] images;
        // Use this for initialization
        IEnumerator Start() {

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
            //_thread = new Thread(ThreadedWork);
            //_thread.Start();
            //do this to avoid warnings
            processingMyo = true;
            yield return new WaitForEndOfFrame();
            processingMyo = false;
        }

        void ThreadedWork() {
            while (true) {
                if (dataUpdated) {
                   
                }
            }
        }

        public void predict(float[] input,float force)
        {
            StartCoroutine(Prediction(input,force));
        }

        IEnumerator Prediction(float[] input, float force)
        {
            TFShape shape = new TFShape(1, 8, INPUT_SIZE);
            var tensor = TFTensor.FromBuffer(shape, input, 0, input.Length);
            var runner = session.GetRunner();
            runner.AddInput(graph["lstm_41_input_1"][0], tensor);
            runner.Fetch(graph["dense_33_1/Softmax"][0]);
            output = runner.Run();
            float[,] res = output[0].GetValue() as float[,];
            result[0] = res[0, 0];
            result[1] = res[0, 1];
            result[2] = res[0, 2];
            result[3] = res[0, 3];
            result[4] = res[0, 4];

            float max = result.Max();

            // Positioning max
            detectedPose = Array.IndexOf(result, max);

            if (modeManager.current == 0)
            {
                switch (detectedPose)
                {
                    case 2:
                        jointOrientation.grab();
                        break;
                    case 1:
                        jointOrientation.left(force);
                        break;
                    case 0:
                        jointOrientation.right(force);
                        break;
                    case 3:
                        jointOrientation.idle();
                        break;
                    default:
                        Console.WriteLine("Default case");
                        break;
                }
            }
            if (modeManager.current == 2)
            {
                for (int i = 0; i < images.Length; i++)
                {
                    var clr = images[i].color;
                    clr.a = 0.3f;
                    images[i].color = clr;
                }
                var tempColor = images[detectedPose].color;
                tempColor.a = 1f;
                images[detectedPose].color = tempColor;
                yield return new WaitForSeconds(1);
                tempColor.a = 0.3f;
            }
            dataUpdated = false;
            yield return new WaitForEndOfFrame();
        }

            IEnumerator ProcessImage() {
            List<float>[] filteredData = myo.filteredData;
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

