using UnityEngine;
using UnityEngine.AI;

public class ItemSpawner : MonoBehaviour
{
    public GameObject[] items;
    public Transform playerTransform; //플레이어 위치
    
    private float lastSpawnTime; //마지막으로 아이템 생성한 시간
    public float maxDistance = 5f; //플레이어를 중점으로 아이템 생성할 최대 거리
    
    private float timeBetSpawn; //아이템 생성 대기 시간
    //위 시간을 결정한 최소, 최대 시간
    public float timeBetSpawnMax = 7f;
    public float timeBetSpawnMin = 2f;

    private void Start()
    {
        timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);
        lastSpawnTime = 0f;
    }

    private void Update()
    {
        if (Time.time >= lastSpawnTime + timeBetSpawn && playerTransform != null) //아이템  생성 대기 시간이 지나고 플레이어가 존재한다면
        {
            Spawn(); //아이템 생성
            lastSpawnTime = Time.time; //마지막 아이템 생성 시간 교체
            timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax); //아이템 생성 대기 시간 교체
        }
    }

    private void Spawn()
    {
        //원을 그린 후 랜덤한 위치 반환
        var spawnPosition = Utility.GetRandomPointOnNavMesh(playerTransform.position, maxDistance, NavMesh.AllAreas);
        //아이템 생성 후 5초 뒤 파괴
        spawnPosition += Vector3.up * 0.5f;
        var item = Instantiate(items[Random.Range(0, items.Length)], spawnPosition, Quaternion.identity);
        Destroy(item, 5f);
    }
}