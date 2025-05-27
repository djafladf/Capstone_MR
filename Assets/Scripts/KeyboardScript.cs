using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class KeyboardScript : MonoBehaviour
{
    public static KeyboardScript keyboard;
    public TMP_InputField TextField;
    public GameObject KorLayout, KorLayoutBig, EngLayoutSml, EngLayoutBig, SymbLayout;

    private readonly string[] choseong = { "ㄱ", "ㄲ", "ㄴ", "ㄷ", "ㄸ", "ㄹ", "ㅁ", "ㅂ", "ㅃ", "ㅅ", "ㅆ", "ㅇ", "ㅈ", "ㅉ", "ㅊ", "ㅋ", "ㅌ", "ㅍ", "ㅎ" };
    private readonly string[] jungseong = { "ㅏ", "ㅐ", "ㅑ", "ㅒ", "ㅓ", "ㅔ", "ㅕ", "ㅖ", "ㅗ", "ㅘ", "ㅙ", "ㅚ", "ㅛ", "ㅜ", "ㅝ", "ㅞ", "ㅟ", "ㅠ", "ㅡ", "ㅢ", "ㅣ" };

    private readonly string[] jongseong = {
        "","ㄱ","ㄲ","ㄳ","ㄴ","ㄵ","ㄶ","ㄷ","ㄹ","ㄺ","ㄻ","ㄼ","ㄽ","ㄾ","ㄿ","ㅀ","ㅁ","ㅂ","ㅄ","ㅅ","ㅆ","ㅇ","ㅈ","ㅊ","ㅋ","ㅌ","ㅍ","ㅎ"
    };

    private void Awake()
    {
        if (keyboard == null) keyboard = this;
        else Destroy(gameObject);
        gameObject.SetActive(false);
    }

    int HangulSub = 0;
    int[] sub = { -1, -1, -1 };
    public void HangulFunction(string input)
    {
        if (HangulSub == 0 || HangulSub == 3)
        {
            HangulSub = 0;
            int i = Array.IndexOf(choseong, input);
            if (i >= 0)
            {
                sub[0] = i;
                HangulSub = 1;
                TextField.text += input;
            }
            else
            {
                int x = Array.IndexOf(jungseong, input);
                if (sub[2] != -1 && sub[2] != 3 && sub[2] != 5 && sub[2] != 6 && !(sub[2] >= 9 && sub[2] <= 15) && sub[2] != 18 && x >= 0)
                {
                    TextField.text = TextField.text.Remove(TextField.text.Length - 1);
                    TextField.text += (char)(0xAC00 + sub[0] * 588 + sub[1] * 28); TextField.text += jongseong[sub[2]];
                    sub[0] = Array.IndexOf(choseong, jongseong[sub[2]]); HangulSub = 1; HangulFunction(input);
                }
                else
                {
                    TextField.text += input;
                    sub[0] = sub[1] = sub[2] = -1;
                }
            }
        }
        else if (HangulSub == 1)
        {
            int j = Array.IndexOf(jungseong, input);
            if (j >= 0)
            {
                TextField.text = TextField.text.Remove(TextField.text.Length - 1);
                sub[1] = j;
                TextField.text += (char)(0xAC00 + sub[0] * 588 + j * 28);
                HangulSub = 2;
            }
            else
            {
                TextField.text += choseong[sub[0]];
                HangulSub = 0;
            }
        }
        else if (HangulSub == 2)
        {
            int k = Array.IndexOf(jungseong, input);

            char composed;
            bool DoubleMedial = false;

            if (sub[1] == 8)
            {
                if (k == 0) { sub[1] = 9; DoubleMedial = true; }
                else if (k == 1) { sub[1] = 10; DoubleMedial = true; }
                else if (k == 20) { sub[1] = 11; DoubleMedial = true; }
            }
            else if (sub[1] == 13)
            {
                if (k == 4) { sub[1] = 14; DoubleMedial = true; }
                else if (k == 5) { sub[1] = 15; DoubleMedial = true; }
                else if (k == 20) { sub[1] = 16; DoubleMedial = true; }
            }
            else if (sub[1] == 18 && k == 20) { sub[1] = 19; DoubleMedial = true; }

            if (DoubleMedial)
            {
                TextField.text = TextField.text.Remove(TextField.text.Length - 1);
                TextField.text += (char)(0xAC00 + sub[0] * 588 + sub[1] * 28);
                return;
            }

            k = Array.IndexOf(jongseong, input);
            if (k >= 0)
            {
                HangulSub = 3;
                TextField.text = TextField.text.Remove(TextField.text.Length - 1);
                sub[2] = k;
                composed = (char)(0xAC00 + sub[0] * 588 + sub[1] * 28 + sub[2]);
                TextField.text += composed;
            }
            else
            {
                HangulSub = 0;
                HangulFunction(input); // 다음 글자 다시 시도
            }
        }
    }

    public void alphabetFunction(string alphabet)
    {
        HangulSub = 0; sub[0] = sub[1] = sub[2] = -1;
        TextField.text += alphabet;
    }

    public void IntFunction(char value)
    {
        HangulSub = 0; sub[0] = sub[1] = sub[2] = -1;
        TextField.text += value;
    }

    public void SpecFunction(char value)
    {
        HangulSub = 0; sub[0] = sub[1] = sub[2] = -1;
        TextField.text += value;
    }

    List<bool> LanSup = new List<bool>();
    public void RegisterSelf(TMP_InputField sc, List<bool> lansup = null)
    {
        CloseAllLayouts();
        if (sc != null)
        {
            TextField = sc; LanSup = lansup;
            if (LanSup[1]) EngLayoutSml.SetActive(true);
            else if (LanSup[2]) KorLayout.SetActive(true);
            else SymbLayout.SetActive(true);
        }
    }

    public void KeyboardChange(int type)
    {

        if (type == 0 && LanSup[0]) { CloseAllLayouts(); SymbLayout.SetActive(true); }
        if (type == 1 && LanSup[1])
        {
            CloseAllLayouts(); EngLayoutSml.SetActive(true);
        }
        if (type == 2 && LanSup[2]) { CloseAllLayouts(); KorLayout.SetActive(true); }
    }

    public void BackSpace()
    {
        if (TextField.text.Length > 0)
        {
            TextField.text = TextField.text.Remove(TextField.text.Length - 1);
            if (HangulSub == 2)
            {
                HangulSub = 1; TextField.text += choseong[sub[0]];
            }
            else if (HangulSub == 3)
            {
                HangulSub = 2; TextField.text += (char)(0xAC00 + sub[0] * 588 + sub[1] * 28);
            }
            else
            {
                HangulSub = 0;
                sub[0] = sub[1] = sub[2] = -1;
            }
        }

    }

    public void CloseAllLayouts()
    {

        KorLayout.SetActive(false);
        KorLayoutBig.SetActive(false);
        EngLayoutSml.SetActive(false);
        EngLayoutBig.SetActive(false);
        SymbLayout.SetActive(false);

    }

    private void OnEnable()
    {
        HangulSub = 0;
    }

    public void ShowLayout(GameObject SetLayout)
    {

        CloseAllLayouts();
        SetLayout.SetActive(true);

    }

}
