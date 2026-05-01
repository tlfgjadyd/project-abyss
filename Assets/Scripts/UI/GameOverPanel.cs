using TMPro;
using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;    // "GAME OVER" 또는 "STAGE CLEAR"

    void Start()
    {
        gameObject.SetActive(false);
        GameManager.Instance.OnGameStateChanged += OnStateChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= OnStateChanged;
    }

    void OnStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.GameOver)
        {
            if (titleText != null) titleText.text = "GAME OVER";
            gameObject.SetActive(true);
        }
        else if (state == GameManager.GameState.StageClear)
        {
            if (titleText != null) titleText.text = "STAGE CLEAR";
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // 재시작 버튼 OnClick에 연결
    public void OnRestartButton()
    {
        GameManager.Instance.RestartGame();
    }
}
