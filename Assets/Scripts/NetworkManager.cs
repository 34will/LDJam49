using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unstable.UI;
using Random = UnityEngine.Random;

namespace Unstable
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        private PlatformGenerator platformGenerator;

        private Dictionary<string, NetworkPlayer> players = new Dictionary<string, NetworkPlayer>();

        private float gameRestartTimer = 0.0f;

        private bool gameOver = false;

        private bool restarting = false;

        public GameObject PlayerPrefab;
        public GameObject PlatformsPrefab;
        public GameObject DebrisPrefab;

        public UnstableUI UI;

        public float SpawnHeight = 30.0f;

        public float GameRestartTime = 10.0f;

        public void Start()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            UnstableRoomOptions options = UnstableRoomOptions.Current;

            GameObject platforms = Instantiate(PlatformsPrefab, Vector3.zero, Quaternion.identity);
            platformGenerator = platforms.GetComponent<PlatformGenerator>();

            platformGenerator.CreateMapFromOptions();

            float tileOffset = platformGenerator.TileOffset;
            Vector3 mapCenter = new Vector3((options.Height - 1) / 2.0f * tileOffset, SpawnHeight, (options.Width - 1) / 2.0f * tileOffset);

            Player[] players = PhotonNetwork.PlayerList;
            List<Vector3> validSpawns = GetValidSpawnPositions(players.Length);
            if (validSpawns == null || validSpawns.Count == 0 || players.Length != validSpawns.Count)
                return;
            for (int i = 0; i < players.Length; i++)
                photonView.RPC("SpawnPlayer", players[i], validSpawns[i], mapCenter);

            if (options.Debris)
            {
                GameObject debris = Instantiate(DebrisPrefab, Vector3.zero, Quaternion.identity);
                DebrisController debrisController = debris.GetComponent<DebrisController>();
                debrisController.PlatformGenerator = platformGenerator;
            }
        }

        private List<Vector3> GetAllValidSpawnPositions()
        {
            if (platformGenerator == null || platformGenerator.CurrentTiles == null || platformGenerator.CurrentTiles.Length <= 0 || platformGenerator.CurrentTiles[0].Length <= 0)
                return null;
                
            int width = platformGenerator.CurrentTiles.Length;
            int height = platformGenerator.CurrentTiles[0].Length;
            float tileOffset = platformGenerator.TileOffset;
            List<Vector3> positions = new List<Vector3>(width * height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    string tile = platformGenerator.CurrentTiles[i][j];
                    if (tile == null)
                        continue;

                    positions.Add(new Vector3(j * tileOffset, SpawnHeight, i * tileOffset));
                }
            }
            return positions;
        }

        private List<Vector3> GetValidSpawnPositions(int count)
        {
            if (count <= 0)
                return null;

            List<Vector3> validSpawns = GetAllValidSpawnPositions();
            if (validSpawns == null || validSpawns.Count <= 0)
                return null;

            List<Vector3> result = new List<Vector3>(count);
            for (int i = 0; i < count; i++)
            {
                if (validSpawns.Count <= 0)
                    validSpawns = GetAllValidSpawnPositions();

                int index = Random.Range(0, validSpawns.Count);
                result.Add(validSpawns[index]);
                validSpawns.RemoveAt(index);
            }
            return result;
        }

        [PunRPC]
        public void SpawnPlayer(Vector3 position, Vector3 mapCenter)
        {
            GameObject player = PhotonNetwork.Instantiate(PlayerPrefab.name, position, Quaternion.identity, 0);
            NetworkPlayer networkPlayer = player.GetComponent<NetworkPlayer>();
            networkPlayer.MapCenter = mapCenter;

            RespawnPlayer(position);
        }

        public void RegisterPlayer(NetworkPlayer player)
        {
            players[player.photonView.Owner.UserId] = player;
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (PhotonNetwork.IsMasterClient)
                players.Remove(otherPlayer.UserId);
        }

        [PunRPC]
        public void RespawnPlayer(Vector3 position)
        {
            NetworkPlayer player = NetworkPlayer.Instance;
            if (player == null)
                return;

            player.Revive();
            player.transform.position = position;
            player.transform.LookAt(player.MapCenter);
        }

        private void Reset()
        {
            platformGenerator.CreateMapFromOptions();

            Player[] players = PhotonNetwork.PlayerList;
            List<Vector3> validSpawns = GetValidSpawnPositions(players.Length);
            if (validSpawns == null || validSpawns.Count == 0 || players.Length != validSpawns.Count)
                return;
            for (int i = 0; i < players.Length; i++)
                photonView.RPC("RespawnPlayer", players[i], validSpawns[i]);

            gameOver = false;
            restarting = true;
            gameRestartTimer = 0.0f;
        }

        [PunRPC]
        private void Victory(string message)
        {
            gameOver = true;

            UI.VictoryMessage.Text.text = message;

            if (!UI.DeathMessage.IsPlaying())
            {
                UI.VictoryMessage.gameObject.SetActive(true);
                UI.VictoryMessage.Animate();
                return;
            }
            
            EventHandler handler = null;
            handler = (object sender, EventArgs e) =>
            {
                UI.DeathMessage.gameObject.SetActive(false);
                UI.VictoryMessage.gameObject.SetActive(true);
                UI.VictoryMessage.Animate();
                UI.DeathMessage.OnMessageShown -= handler;
            };
            UI.DeathMessage.OnMessageShown += handler;
        }

        private void CheckWinCondition()
        {
            if (gameOver)
                return;

            UnstableRoomOptions roomOptions = UnstableRoomOptions.Current;
            switch (roomOptions.Gamemode)
            {
                case Gamemode.LastPersonStanding:
                    var allPlayers = players.Values;
                    NetworkPlayer[] alive = allPlayers
                        .Where(x => !x.IsDead)
                        .ToArray();
                    if (alive.Length > 1)
                        return;
                    if (alive.Length == 0)
                    {
                        string message = "Draw";
                        photonView.RPC("Victory", RpcTarget.Others, message);
                        Victory(message);
                    }
                    else if (allPlayers.Count != 1)
                    {
                        NetworkPlayer winner = alive[0];
                        string message = $"{winner.photonView.Owner.NickName} wins!";
                        winner.Win();
                        photonView.RPC("Victory", RpcTarget.Others, message);
                        Victory(message);
                    }
                    break;
                case Gamemode.Tag:
                    break;
                case Gamemode.CaptureTheFlag:
                    break;
                case Gamemode.KingOfTheHill:
                    break;
            }
        }

        public void Update()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            if (!restarting)
                CheckWinCondition();
            else
                restarting = players.Values.Any(x => x.IsDead);

            if (!gameOver)
                return;

            gameRestartTimer += Time.deltaTime;
            if (gameRestartTimer < GameRestartTime)
                return;

            Reset();
        }

        public override void OnLeftRoom()
        {
            SceneManager.LoadScene("Launcher");
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }
    }
}
