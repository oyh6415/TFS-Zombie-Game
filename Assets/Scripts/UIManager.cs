using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    
    public static UIManager Instance //싱글톤
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<UIManager>();

            return instance;
        }
    }
    //인스펙터 창에서 설정할 수 있도록
    [SerializeField] private GameObject gameoverUI;
    [SerializeField] private Crosshair crosshair;

    [SerializeField] private Text healthText;
    [SerializeField] private Text lifeText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text ammoText;
    [SerializeField] private Text waveText;

    public void UpdateAmmoText(int magAmmo, int remainAmmo) //탄약 표시
    {
        ammoText.text = magAmmo + "/" + remainAmmo;
    }

    public void UpdateScoreText(int newScore) //스코어 표시
    {
        scoreText.text = "Score : " + newScore;
    }
    
    public void UpdateWaveText(int waves, int count) //웨이브 표시
    {
        waveText.text = "Wave : " + waves + "\nEnemy Left : " + count;
    }

    public void UpdateLifeText(int count) //목숨 표시
    {
        lifeText.text = "Life : " + count;
    }

    public void UpdateCrossHairPosition(Vector3 worldPosition) //조준점 업데이트
    {
        crosshair.UpdatePosition(worldPosition);
    }
    
    public void UpdateHealthText(float health) //체력 업데이트
    {
        healthText.text = Mathf.Floor(health).ToString();
    }
    
    public void SetActiveCrosshair(bool active) //조준점 표시
    {
        crosshair.SetActiveCrosshair(active);
    }
    
    public void SetActiveGameoverUI(bool active) //게임오버 UI 표시
    {
        gameoverUI.SetActive(active);
    }
    
    public void GameRestart() //Restart 버튼과 연결
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); //현재 씬을 재시작
    }
}