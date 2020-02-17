using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Thalmic.Myo
{
    public class Recorder : MonoBehaviour
    {
        public GraphVizualizer graphVizualizer;
        public List<float>[] filteredData  = new List<float>[8];
        public DataProcessing dp;
        public int pose = 0;
        public InputField input;
        public string filepath = null;
        public Text pathText;
        int N;

        // Start is called before the first frame update
        void Start()
        {
            
            N = 140;
            filepath = PlayerPrefs.GetString("Path");
            pathText.text = filepath;
            var se = new InputField.SubmitEvent();
            se.AddListener(onPoseChange);
            input.onEndEdit = se;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void onPoseChange(string poseString)
        {
            Debug.Log(poseString);
            if (!int.TryParse(poseString, out pose))
            {
                pose = 0;
            }
            
        }

        public void writeNewFile(string path)
        {
            int[] features = new int[140 * 8 + 1];
            for (int i = 0; i < features.Length; i++)
            {
                features[i] = i;
            }
            try
            {                
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
            filteredData = graphVizualizer.filteredData;

            Debug.Log("net");
            if ((filteredData[0].Count == N) && (filepath != null))
            {
                Debug.Log("da");
                try
                {


                    List<float> result = new List<float>();
                    for (int i = 0; i < filteredData.Length; i++)
                    {
                        result = result.Concat(filteredData[i]).ToList();
                    }
                    Debug.Log(result.Count);
                    
                    StreamWriter writer = new StreamWriter(filepath, true);
                    string newRow = "";

                    if (graphVizualizer.isNormalization) result = dp.stand(result);

                    newRow = newRow + string.Join(":",result) + ":";

                    newRow = newRow + pose;

                    newRow = newRow.Replace(",", ".");
                    newRow = newRow.Replace(":", ",");

                    writer.WriteLine(newRow);

                    writer.Close();
                    //UnityEngine.Debug.Log(("st:"+string.Join(",", record)));
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Dont work", ex);
                }
            }
        }

       
        public void selectFile()
        {
#if UNITY_EDITOR
            string path = EditorUtility.OpenFilePanel("Overwrite with png", "", "txt");
            if (path.Length != 0)
            {
                filepath = path;
                PlayerPrefs.SetString("Path", filepath);
                pathText.text = filepath;
                if ((new FileInfo(filepath).Length) == 0) writeNewFile(filepath);
            }
#endif


        }
    }
}
