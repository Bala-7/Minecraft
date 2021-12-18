using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeNoiseGenerator : MonoBehaviour {

    public float noiseScale = 0.1f;
    public int gridSize = 30;

    public Material noiseMat;

    void Start() {

        GameObject cubeParent = new GameObject();
        cubeParent.name = "Noise Cubes";

        for(int x = 0; x < gridSize; x++) {
            for(int y = 0; y < gridSize; y++) {
                for(int z = 0; z < gridSize; z++) {
                    float noiseValue = (float)NoiseS3D.NoiseCombinedOctaves(x * noiseScale, y * noiseScale, z * noiseScale);

                    //remap the value to 0 - 1 for color purposes
                    noiseValue = (noiseValue + 1) * 0.5f;

                    GameObject noiseCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
					noiseCube.GetComponent<Renderer>().sharedMaterial = Instantiate(noiseMat) as Material;
                    Destroy(noiseCube.GetComponent<Collider>());

                    noiseCube.GetComponent<Renderer>().sharedMaterial.SetColor("_TintColor", new Color(noiseValue, noiseValue, noiseValue, noiseValue * 0.015f));

                    noiseCube.transform.SetParent(cubeParent.transform);
                    noiseCube.transform.position = new Vector3(x, y, z);
                }
            }
        }

        cubeParent.transform.position -= new Vector3(gridSize / 2, 0, gridSize / 2);

    }

}
