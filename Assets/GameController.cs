using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Canvas _gameOverCanvas;
    [SerializeField] private Button _restartButton;
    [SerializeField] private TMP_Text _scoreText;

    private float _hiscore;
    private InputAction _inputActions;

    void Awake()
    {
        if (_playerController != null)
        {
            _playerController.OnPlayerDied += HandlePlayerDeath;
        }

        _restartButton.onClick.AddListener(HandleRestartGame);

        _inputActions = new InputAction();

    }

    private void HandleRestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Start()
    {
        _gameOverCanvas.gameObject.SetActive(false);
    }

    void HandlePlayerDeath()
    {
        _hiscore = PlayerPrefs.GetFloat("Hiscore", 0);

        var time = Time.timeSinceLevelLoad;
        _hiscore = Mathf.Max(_hiscore, time);

        PlayerPrefs.SetFloat("Hiscore", _hiscore);

        _scoreText.text = $"You lasted for {time: 0.00} sec\nHigh Score: {_hiscore: 0.00} sec";
        _playerController.OnPlayerDied -= HandlePlayerDeath;
        _gameOverCanvas.gameObject.SetActive(true);

    }


}
