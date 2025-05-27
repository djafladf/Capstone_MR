using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DualGaugeUI : MonoBehaviour
{
    public Image rivalGauge;         // Image1 (상대 게이지)
    public Image myGauge;            // Image2 (내 게이지)
    public RectTransform sparkEffect;
    public PosePlayer myPlayer;
    public PosePlayer rivalPlayer;   // 새로 추가
    public TextMeshProUGUI rivalScoreText;
    public TextMeshProUGUI myScoreText;


    float MySum = 0.5f;
    float RivalSum = 0.5f;
    void FixedUpdate()
    {
        float myScore = Random.Range(50f,100f);
        float rivalScore = Random.Range(75f, 85f);
        /* float myScore = myPlayer != null ? myPlayer.GetLiveScore() : 0f;
         float rivalScore = rivalPlayer != null ? rivalPlayer.GetLiveScore() : 0f;*/
        if (myScore < rivalScore) { MySum -= 0.001f; RivalSum += 0.001f;  }
        if (myScore > rivalScore) { MySum += 0.001f; RivalSum -= 0.001f; }

        myGauge.fillAmount = MySum;
        rivalGauge.fillAmount = RivalSum;

        float width = myGauge.rectTransform.rect.width;
        float sparkX = (MySum * width) - (width / 2f);
        sparkEffect.localPosition = new Vector3(sparkX, 0f, 0f);
        rivalScoreText.text = RivalSum.ToString("F2");
        myScoreText.text = MySum.ToString("F2");
    }
}
