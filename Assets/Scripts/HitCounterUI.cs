using UnityEngine;
using TMPro; // обязательно

public class HitCounterUI : MonoBehaviour
{
    public static HitCounterUI Instance;

    [SerializeField] private TMP_Text _text;

    private int _hits = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        UpdateUI();
    }

    public void RegisterHit()
    {
        _hits++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_text != null)
        {
            _text.text = $"Hits: {_hits}";
        }
    }
}
