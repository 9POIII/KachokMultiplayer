using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Objects.LevelGenerator
{
    public class LevelGenerator : MonoBehaviourPun
    {
        public GameObject[] roomPrefabs;
        public GameObject doorPrefab;
        public int maxRooms = 10;

        private List<Vector3> usedPositions = new List<Vector3>();
        private List<GameObject> spawnedRooms = new List<GameObject>();

        private Vector3[] directions = new Vector3[]
        {
            new Vector3(0, 0, 10), // Вперед
            new Vector3(10, 0, 0), // Вправо
            new Vector3(0, 0, -10), // Назад
            new Vector3(-10, 0, 0) // Влево
        };

        void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                GenerateLevel();
                CreateDoors();
                photonView.RPC("SynchronizeLevel", RpcTarget.Others, usedPositions.ToArray());
            }
        }

        void GenerateLevel()
        {
            GameObject startRoom = PhotonNetwork.Instantiate(roomPrefabs[Random.Range(0, roomPrefabs.Length)].name,
                Vector3.zero, Quaternion.identity);
            usedPositions.Add(Vector3.zero);
            spawnedRooms.Add(startRoom);

            for (int i = 0; i < maxRooms - 1; i++)
            {
                PlaceNextRoom();
            }
        }

        void PlaceNextRoom()
        {
            GameObject lastRoom = spawnedRooms[spawnedRooms.Count - 1];
            Vector3 lastPosition = lastRoom.transform.position;

            List<Vector3> validPositions = new List<Vector3>();

            foreach (Vector3 direction in directions)
            {
                Vector3 newPosition = lastPosition + direction;
                if (!usedPositions.Contains(newPosition))
                {
                    validPositions.Add(newPosition);
                }
            }

            if (validPositions.Count > 0)
            {
                Vector3 selectedPosition = validPositions[Random.Range(0, validPositions.Count)];
                GameObject newRoom = PhotonNetwork.Instantiate(roomPrefabs[Random.Range(0, roomPrefabs.Length)].name,
                    selectedPosition, Quaternion.identity);

                usedPositions.Add(selectedPosition);
                spawnedRooms.Add(newRoom);
            }
        }

        void CreateDoors()
        {
            foreach (GameObject room in spawnedRooms)
            {
                Vector3 roomPosition = room.transform.position;

                foreach (Vector3 direction in directions)
                {
                    Vector3 neighborPosition = roomPosition + direction;
                    if (usedPositions.Contains(neighborPosition))
                    {
                        // Создаем дверь в направлении соседней комнаты
                        Vector3 doorPosition = roomPosition + direction / 2;
                        Quaternion doorRotation = Quaternion.LookRotation(direction);
                        PhotonNetwork.Instantiate(doorPrefab.name, doorPosition, doorRotation);
                    }
                }
            }
        }

        [PunRPC]
        void SynchronizeLevel(Vector3[] positions)
        {
            usedPositions.AddRange(positions);
        }
    }
}