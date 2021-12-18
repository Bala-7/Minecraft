using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoiseCompare : MonoBehaviour {

    public int iterations = 6;
    public Vector2 testRes = new Vector2(2000, 2000);

    public Text outputText;

    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

    //this was used to compare the values from the normal noise function and the gpu version and make sure they match up
    void tmpArrayTestTexture() {
        NoiseS3D.octaves = 1;
        NoiseS3D.seed = 10;

        Vector2[] noiseData = new Vector2[100];
        for(int t = 0; t < 100; t++) {
            noiseData[t] = new Vector2(t, Random.Range(0, 100000));
        }

        float[] outputNormal = new float[100];
        for(int t = 0; t < 100; t++) {
            outputNormal[t] = (((float)NoiseS3D.Noise(noiseData[t].x * 0.01, noiseData[t].y * 0.01)) + 1) * 0.5f;
        }

        float[] outputNoise = NoiseS3D.NoiseArrayGPU(noiseData);

        for(int t = 0; t < 100; t++) {
            Debug.Log("norm" + t + " = " + outputNormal[t] + "  :  array" + t + " = " + outputNoise[t]);
        }
    }

    // Use this for initialization
    void Start () {

        //tmpArrayTestTexture();
        //return;

        int testCount = (int)(testRes.x * testRes.y);

        sw.Start();
        for(int i = 0; i < iterations; i++) {
            for(int t = 0; t < testCount; t++) {
                float noiseValue = Mathf.PerlinNoise(i, t);
            }
        }
        sw.Stop();
        Debug.Log("Unity 2D perlin noise: " + sw.ElapsedMilliseconds / iterations + " Avg milliseconds to perform " + testCount + " noise calls.");
        outputText.text += "Unity 2D perlin noise: " + sw.ElapsedMilliseconds / iterations + " Avg milliseconds to perform " + testCount + " noise calls.\n";

        sw.Reset();

        sw.Start();
        for(int i = 0; i < iterations; i++) {
            for(int t = 0; t < testCount; t++) {
                double noiseValue = NoiseS3D.Noise(i, t);
            }
        }
        sw.Stop();
        Debug.Log("NoiseS3D 2D noise : " + sw.ElapsedMilliseconds / iterations + " Avg milliseconds to perform " + testCount + " noise calls.");
        outputText.text += "NoiseS3D 2D noise: " + sw.ElapsedMilliseconds / iterations + " Avg milliseconds to perform " + testCount + " noise calls.\n";

        sw.Reset();


        NoiseS3D.octaves = 1;
        sw.Start();
        for(int i = 0; i < iterations; i++) {
            RenderTexture noiseTex = NoiseS3D.GetNoiseRenderTexture((int)testRes.x, (int)testRes.y);
        }
        sw.Stop();
        Debug.Log("NoiseS3D 2D noise RenderTexture on GPU: " + sw.ElapsedMilliseconds / iterations + " Avg milliseconds to perform " + testCount + " noise calls.");
        outputText.text += "NoiseS3D 2D noise RenderTexture on GPU: " + sw.ElapsedMilliseconds / iterations + " Avg milliseconds to perform " + testCount + " noise calls.\n";

        sw.Reset();

        NoiseS3D.octaves = 1;
        sw.Start();
        for(int i = 0; i < iterations; i++) {
            Texture2D noiseTex = NoiseS3D.GetNoiseTexture((int)testRes.x, (int)testRes.y);    
        }
        sw.Stop();
        Debug.Log("NoiseS3D 2D noise Texture2D on GPU: " + sw.ElapsedMilliseconds / iterations + " Avg milliseconds to perform " + testCount + " noise calls.");
        outputText.text += "NoiseS3D 2D noise Texture2D on GPU: " + sw.ElapsedMilliseconds / iterations + " Avg milliseconds to perform " + testCount + " noise calls.\n";

        sw.Reset();
        sw.Stop();

        //create this data outside of the timer, otherwise its not fair comparision bc the other tests did not need to create data
        Vector2[] noiseData = new Vector2[testCount];
        for(int t = 0; t < testCount; t++) {
            noiseData[t] = new Vector2(t, Random.Range(0, 100000));
        }

        sw.Reset();

        NoiseS3D.octaves = 1;
        sw.Start();
        for(int i = 0; i < iterations; i++) {
            float[] outputNoise = NoiseS3D.NoiseArrayGPU(noiseData);
        }
        sw.Stop();
        Debug.Log("NoiseS3D 2D noise array on GPU not including array creation: " + sw.ElapsedMilliseconds / iterations + " Avg milliseconds to perform " + testCount + " noise calls.");
        outputText.text += "NoiseS3D 2D noise array on GPU not including array creation: " + sw.ElapsedMilliseconds / iterations + " Avg milliseconds to perform " + testCount + " noise calls.\n";

        sw.Reset();

        NoiseS3D.octaves = 1;
        sw.Start();
        for(int i = 0; i < iterations; i++) {
            Vector2[] noiseOutput = new Vector2[testCount];
            for(int t = 0; t < testCount; t++) {
                noiseOutput[t] = new Vector2(i, t);
            }
            float[] outputNoise = NoiseS3D.NoiseArrayGPU(noiseOutput);
        }
        sw.Stop();
        Debug.Log("NoiseS3D 2D noise array on GPU including array creation: " + sw.ElapsedMilliseconds / iterations + " Avg milliseconds to perform " + testCount + " noise calls.");
        outputText.text += "NoiseS3D 2D noise array on GPU including array creation: " + sw.ElapsedMilliseconds / iterations + " Avg milliseconds to perform " + testCount + " noise calls.\n";

        sw.Reset();

    }
	
	
}
