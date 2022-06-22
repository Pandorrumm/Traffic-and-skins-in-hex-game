using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEntity;
using Singleton;

public class StartHexSpawner : MonoBehaviour
{
    public HexOfWorld hexPrefub;
    public Transform spawnPosition;

    public void Awake()
    {
        HexOfWorld hex = Instantiate(hexPrefub.gameObject, spawnPosition.position, hexPrefub.transform.rotation).GetComponent<HexOfWorld>();

        int randomIndex = Random.Range(0, SingletonHexCollection.Instance.hexesAvailable.Count);
        hex.ChangeModel(SingletonHexCollection.Instance.hexesAvailable[randomIndex].data.Copy(), Quaternion.identity);
        hex.transform.SetParent(transform);
    }
}