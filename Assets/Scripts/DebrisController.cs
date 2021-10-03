using System.Collections.Generic;
using UnityEngine;
using Unstable;

public class Debris
{
    public GameObject GameObject;

    public bool Active;
}

public class DebrisController : MonoBehaviour
{
    private float spawnTimer = 0.0f;
    private int spawnCounter = 0;

    public List<GameObject> DebrisPrefabs = new List<GameObject>();

    public PlatformGenerator PlatformGenerator;

    public List<Debris> Debris;

    public float SpawnInterval = 5.0f;

    public float SpawnHeight = 100.0f;

    public float MinScale = 1.0f;
    public float MaxScale = 3.0f;

    public int InitialDebrisCount = 20;

    public void Start()
    {
        Debris = new List<Debris>(InitialDebrisCount);

        for (int i = 0; i < InitialDebrisCount; i++)
        {
            int index = Random.Range(0, DebrisPrefabs.Count);
            GameObject debris = Instantiate(DebrisPrefabs[index], new Vector3(0, -10000, 0), Quaternion.identity, transform);
            debris.SetActive(false);
            Debris.Add(new Debris
            {
                GameObject = debris,
                Active = false
            });
        }
    }

    private Vector3? GetRandomSpawnPosition()
    {
        UnstableRoomOptions options = UnstableRoomOptions.Current;
        int width = options.Width;
        int height = options.Height;
        if (width <= 0 || height <= 0)
            return null;

        float maxX = PlatformGenerator.TileOffset * width;
        float maxY = PlatformGenerator.TileOffset * height;
        float x = Random.Range(0, maxX);
        float y = Random.Range(0, maxY);
        return new Vector3(x, SpawnHeight, y);
    }

    private void SpawnObject()
    {
        Debris nextDebris = Debris[spawnCounter];
        if (nextDebris.Active)
            return;

        Vector3? position = GetRandomSpawnPosition();
        if (position == null)
            return;

        float scale = Random.Range(MinScale, MaxScale);
        nextDebris.Active = true;
        nextDebris.GameObject.transform.position = position.Value;
        nextDebris.GameObject.transform.localScale = new Vector3(scale, scale, scale);
        nextDebris.GameObject.SetActive(true);
        spawnCounter++;
        if (spawnCounter >= Debris.Count)
            spawnCounter = 0;
    }

    public void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer < SpawnInterval)
            return;

        SpawnObject();
        spawnTimer -= SpawnInterval;
    }
}
