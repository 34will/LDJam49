using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Unstable;
using Unstable.Utility;

public class PlatformGenerator : MonoBehaviour
{
    private List<GameObject> Platforms = new List<GameObject>();

    public GenericDictionary<string, GameObject> Tiles;

    public string[][] CurrentTiles;

    public float TileSize = 20.0f;
    public float TileGap = 0.5f;

    private void DestroyAllChildren()
    {
        foreach (GameObject platform in Platforms)
            PhotonNetwork.Destroy(platform);

        Platforms.Clear();
        CurrentTiles = null;
    }

    public void CreateMapFromOptions()
    {
        DestroyAllChildren();

        UnstableRoomOptions options = UnstableRoomOptions.Current;
        int MapWidth = options.Width;
        int MapHeight = options.Height;

        CurrentTiles = new string[MapWidth][];
        for (int i = 0; i < MapWidth; i++)
            CurrentTiles[i] = new string[MapHeight];

        switch (options.MapType)
        {
            case MapType.Map:
                string mapTilesString = options.MapString;
                int size = MapWidth * MapHeight;
                if (mapTilesString.Length != size)
                {
                    Debug.Log($"Map Tiles string length not correct, expected {size}, got {mapTilesString.Length}");
                    return;
                }

                Platforms = new List<GameObject>(size);
                for (int j = 0; j < MapHeight; j++)
                {
                    for (int i = 0; i < MapWidth; i++)
                    {
                        string character = mapTilesString[(j * MapWidth) + i].ToString();
                        if (!Tiles.TryGetValue(character, out GameObject prefab))
                        {
                            Debug.Log($"Failed to get Tile for character '{character}', adding air");
                            CurrentTiles[i][j] = null;
                            continue;
                        }

                        GameObject platform = PhotonNetwork.InstantiateRoomObject($"Platforms/{prefab.name}", new Vector3(TileOffset * j, 0, TileOffset * i), Quaternion.identity);
                        Platforms.Add(platform);
                        CurrentTiles[i][j] = character;
                    }
                }

                break;
            case MapType.Random:
                break;
            default:
                Debug.Log("Invalid map type");
                return;
        }
    }

    // ----- Properties ----- //

    public float TileOffset => TileSize + TileGap;
}
