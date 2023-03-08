using UnityEngine;

public class HeightMap : MonoBehaviour
{
    public GameObject cube;

    [Header("Map Setting")]
    public int size = 100;
    public float scale = 13;
    public float m = 7;

    void Start()
    {
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                float noise = Mathf.PerlinNoise(x / scale, z / scale);
                float y = Mathf.RoundToInt(noise * m);

                Vector3 childPos = new Vector3(x, y, z);

                GameObject go = Instantiate(cube, childPos, Quaternion.identity, transform);
                go.GetComponent<MeshRenderer>().material.color = new Color(noise, noise, noise, noise);
            }
        }
    }
}