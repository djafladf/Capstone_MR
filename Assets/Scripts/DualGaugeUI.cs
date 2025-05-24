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

    void Update()
    {
        float myScore = myPlayer != null ? myPlayer.GetLiveScore() : 0f;
        float rivalScore = rivalPlayer != null ? rivalPlayer.GetLiveScore() : 0f;
        float total = Mathf.Max(myScore + rivalScore, 0.001f);
        float myRatio = myScore / total;
        float rivalRatio = rivalScore / total;

        myGauge.fillAmount = myRatio;
        rivalGauge.fillAmount = rivalRatio;

        if (sparkEffect != null && myGauge != null)
        {
            float width = myGauge.rectTransform.rect.width;
            float sparkX = (myRatio * width) - (width / 2f);
            sparkEffect.localPosition = new Vector3(sparkX, 0f, 0f);
        }

        if (rivalScoreText != null)
        {
            rivalScoreText.text = $"{rivalScore:F1}";
        }
    }
}
