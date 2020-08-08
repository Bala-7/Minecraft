using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureNoiseGPU : MonoBehaviour {

    public float noiseScale = 0.1f;
    public float scrollSpeed = 40;
    public Vector2 noiseOffset;
    public RawImage noiseImage;

    [Header("Octave options")]
    public bool UseOctaves = false;
    public int octaves = 4;

    [Header("Seed options")]
    public bool useSeed = false;
    public int seed = 0;

	RenderTexture noiseTex = null;


    // Use this for initialization
    void Start() {
        MakeTexture();
    }


    void Update() {
        MakeTexture();

        noiseOffset += Vector2.one * scrollSpeed * Time.deltaTime;

    }

    void MakeTexture() {

        if(useSeed)
            NoiseS3D.seed = seed;

        if(noiseTex)
            Destroy(noiseTex);

        if(UseOctaves) {
            NoiseS3D.octaves = octaves;
        } else {
            NoiseS3D.octaves = 1;
        }

        noiseTex = NoiseS3D.GetNoiseRenderTexture(Screen.width, Screen.height, noiseOffset.x, noiseOffset.y, noiseScale);

		if(noiseTex){
        	noiseTex.filterMode = FilterMode.Point;
			
        	noiseImage.texture = noiseTex;
		}

    }

}
