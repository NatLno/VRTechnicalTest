using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GunScoreManager : MonoBehaviour
{
    [SerializeField]
    private int m_score;
    
    [SerializeField]
    private TextMeshPro m_scoreText;

    public int Score
    {
        get => m_score;
        set 
        { 
            m_score = value; 
            m_scoreText.text = string.Format("{0:00}", m_score);
        }
    }

    public TextMeshPro ScoreText
    {
        get => m_scoreText;
        set => m_scoreText = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        ScoreText.text = string.Format("{0:00}", m_score);
    }

    public void IncrementScore()
    {
        Score++;
    }

    public void ResetScore()
    {
        Score = 0;
    }
}
