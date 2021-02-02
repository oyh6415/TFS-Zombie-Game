using System.Collections.Generic;
using UnityEngine;

// 적 게임 오브젝트를 주기적으로 생성
public class EnemySpawner : MonoBehaviour
{
    //readonly: 이 변수에 덮어쓰기 불가능
    private readonly List<Enemy> enemies = new List<Enemy>();

    public float damageMax = 40f;
    public float damageMin = 20f;
    public Enemy enemyPrefab;

    public float healthMax = 200f;
    public float healthMin = 100f;

    public Transform[] spawnPoints;

    public float speedMax = 12f;
    public float speedMin = 3f;

    public Color strongEnemyColor = Color.red;
    private int wave;

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameover) return;
        
        if (enemies.Count <= 0) SpawnWave();
        
        UpdateUI();
    }

    private void UpdateUI()
    {
        UIManager.Instance.UpdateWaveText(wave, enemies.Count);
    }
    
    private void SpawnWave()
    {
        wave++;
        //웨이브가 오르면 적의 수 증가
        var spawnCount = Mathf.RoundToInt(wave * 5f);

        for(var i=0; i< spawnCount; i++)
        {
            var enemyIntensity = Random.Range(0f, 1f); //적의 강함 결정
            CreateEnemy(enemyIntensity); //적 생성
        }
    }
    
    private void CreateEnemy(float intensity)
    {
        var health = Mathf.Lerp(healthMin, healthMax, intensity); //적 체력
        var damage = Mathf.Lerp(damageMin, damageMax, intensity); //적 데미지
        var speed = Mathf.Lerp(speedMin, speedMax, intensity); //적 스피드

        var skinColor = Color.Lerp(Color.white, strongEnemyColor, intensity); //적의 컬러

        var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)]; //생성 위치

        var enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation); //적 생성

        enemy.Setup(health, damage, speed, speed * 0.3f, skinColor); //스탯 셋 업

        enemies.Add(enemy); 
        //적이 죽었을 때 이벤트 추가
        enemy.OnDeath += () => enemies.Remove(enemy);
        enemy.OnDeath += () => Destroy(enemy.gameObject, 10f);
        enemy.OnDeath += () => GameManager.Instance.AddScore(100);
    }
}