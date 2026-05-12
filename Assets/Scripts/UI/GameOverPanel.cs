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
        // GameOver 전용. StageClear는 별도 패널 없이 StageTransitionUI 페이드로 처리.
        // 중첩 ChangeState 안전을 위해 매개변수 대신 CurrentState 기준으로 판단.
        if (GameManager.Instance.CurrentState == GameManager.GameState.GameOver)
        {
            if (titleText != null) titleText.text = "GAME OVER";
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
