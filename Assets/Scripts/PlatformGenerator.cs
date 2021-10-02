using System.Text.RegularExpressions;
using UnityEngine;
using Unstable.Utility;

public class PlatformGenerator : MonoBehaviour
{
    private readonly static Regex mapRegex = new Regex("^(\\d+)x(\\d+)([MRmr])(.*)$");

    public GenericDictionary<string, GameObject> Tiles;
    public string MapString;

    public float TileSize = 20.0f;
    public float TileGap = 0.5f;

    public int MapWidth;
    public int MapHeight;

    public void Start()
    {
        CreateMapFromMapString();
    }

    public void Update()
    {
        
    }

    private void DestroyAllChildren()
    {
         foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    private void CreateMapFromMapString()
    {
        DestroyAllChildren();

        // 1st section is <width>x<height> e.g. 5x5 means a 5 by 5 map
        // 2nd section is <letter> is 'M' for map string or 'R' for random map with optional seed
        // 3rd section for 'M' is map string of length width * height
        // 3rd section for 'R' is either empty, in which case generate a random seed from time, otherwise a number which is the seed

        if (string.IsNullOrWhiteSpace(MapString))
        {
            Debug.Log("Null or whitespace map string");
            return;
        }

        Match match = mapRegex.Match(MapString);
        if (!match.Success)
        {
            Debug.Log("Invalid map string");
            return;
        }

        string widthString = match.Groups[1].Value;
        if (!int.TryParse(widthString, out MapWidth))
        {
            MapWidth = 0;
            Debug.Log("Invalid width");
            return;
        }

        string heightString = match.Groups[2].Value;
        if (!int.TryParse(heightString, out MapHeight))
        {
            MapHeight = 0;
            Debug.Log("Invalid height");
            return;
        }

        switch (match.Groups[3].Value)
        {
            case "M":
                string mapTilesString = match.Groups[4].Value;
                int size = MapWidth * MapHeight;
                if (mapTilesString.Length != size)
                {
                    Debug.Log($"Map Tiles string length not correct, expected {size}, got {mapTilesString.Length}");
                    return;
                }

                for (int j = 0; j < MapHeight; j++)
                {
                    for (int i = 0; i < MapWidth; i++)
                    {
                        string character = mapTilesString[(j * MapWidth) + i].ToString();
                        if (!Tiles.TryGetValue(character, out GameObject prefab))
                        {
                            Debug.Log($"Failed to get Tile for character '{character}', adding air");
                            continue;
                        }

                        Instantiate(prefab, new Vector3(TileOffset * j, 0, TileOffset * i), Quaternion.identity, transform);
                    }
                }

                break;
            case "R":
                break;
            default:
                Debug.Log("Invalid map type");
                return;
        }
    }

    // ----- Properties ----- //

    public float TileOffset => TileSize + TileGap;
}
