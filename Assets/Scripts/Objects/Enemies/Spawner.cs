using Photon.Pun;
using UnityEngine;

namespace Objects.Enemies
{
    public class Spawner : MonoBehaviour
    {
        public GameObject[] enemy;
        public Vector2 spawnArea = new Vector2(4.5f, 4.5f);
        public int numberOfEnemies = 2;

        void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < numberOfEnemies; i++)
                {
                    Vector3 randomPosition = new Vector3(
                        Random.Range(-spawnArea.x, spawnArea.x), 
                        0.5f, 
                        Random.Range(-spawnArea.y, spawnArea.y));
                    
                    PhotonNetwork.Instantiate(enemy[0].name, randomPosition, Quaternion.identity);
                    PhotonNetwork.Instantiate(enemy[1].name, randomPosition, Quaternion.identity);

                }
            }
        }
    }
}
