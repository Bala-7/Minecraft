using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureNoise : MonoBehaviour {

    public float noiseScale = 0.1f;
    public RawImage noiseImage;

    [Header("Octave options")]
    public bool UseOctaves = false;
    public int octaves = 4;

    [Header("Seed options")]
    public bool useSeed = false;
    public int seed = 0;

    Texture2D noiseTex = null;

    // Use this for initialization
    float prevNoiseScale = 1f;
    void Start() {

        MakeTexture();

        prevNoiseScale = noiseScale;
    }


    void Update() {
        if(noiseScale == prevNoiseScale)
            return;

        MakeTexture();

        prevNoiseScale = noiseScale;
    }

    void MakeTexture() {

        if(useSeed)
            NoiseS3D.seed = seed;

        if(noiseTex)
            Destroy(noiseTex);

        noiseTex = new Texture2D(Screen.width, Screen.height);
        noiseTex.filterMode = FilterMode.Point;

        for(int x = 0; x < noiseTex.width; x++) {
            for(int y = 0; y < noiseTex.height; y++) {
                float noiseValue = 0;

                if(UseOctaves) {
                    NoiseS3D.octaves = octaves;
                    noiseValue = (float)NoiseS3D.NoiseCombinedOctaves(x * noiseScale, y * noiseScale);
                } else {
                    noiseValue = (float)NoiseS3D.Noise(x * noiseScale, y * noiseScale);
                }

                //remap the value to 0 - 1 for color purposes
                noiseValue = (noiseValue + 1) * 0.5f;

                noiseTex.SetPixel(x, y, new Color(noiseValue, noiseValue, noiseValue));
            }
        }

        noiseTex.Apply();

        noiseImage.texture = noiseTex;

    }

}
