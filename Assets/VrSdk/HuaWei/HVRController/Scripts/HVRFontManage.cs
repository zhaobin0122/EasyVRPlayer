using System;
using UnityEngine;
using UnityEngine.UI;
using HVRCORE;
public class HVRFontManage : MonoBehaviour
{
    private static readonly string TAG = "Unity_HVRFontManage";
    private Image m_Image;
    private Text m_Text;
    private Image m_Arrow;
    private string m_Msg;
    private string m_LastTriggerMsg, m_LastVolumeMsg, m_LastConfirmMsg, m_LastBackLongMsg, m_LastBackShortMsg, m_LastHomeLongMsg, m_LastHomeShortMsg;
    private int m_DefaultEdge;
    private int m_Anchor;
    private int m_DefaultFontCount = 10;
    private Transform m_Spot, m_ArrowAttach;

    private bool m_IsControllerLeft, m_IsControllerRight;
    private const float m_DelayShowTime = 0.1f;
    private void Awake()
    {
        m_Image = transform.Find("msg").GetComponent<Image>();
        m_Text = transform.Find("Text").GetComponent<Text>();
    }

    void Start()
    {
        if (transform.name == "Trigger" || transform.name == "Volume" || transform.name == "Confirm")
        {
            m_Arrow = this.transform.Find("Arrow").GetComponent<Image>();
        }
        if (transform.name == "short" || transform.name == "long")
        {
            m_Arrow = this.transform.parent.Find("Arrow").GetComponent<Image>();
        }
        m_Spot = m_Arrow.transform.Find("spot");
        m_ArrowAttach = m_Arrow.transform.Find("arrow");
        if (transform.name == "Trigger" || transform.parent.name == "Back" || transform.name == "Confirm")
        {
            m_IsControllerLeft = true;
            m_Anchor = 1;
            m_DefaultEdge = -10;
        }
        if (transform.name == "Volume" || transform.parent.name == "Home")
        {
            m_IsControllerRight = true;
            m_Anchor = 0;
            m_DefaultEdge = 10;
        }
        Invoke("ArrowCtrl", m_DelayShowTime);
    }

    void Update()
    {
        if (m_Image == null || m_Text == null)
        {
            HVRLogCore.LOGI(TAG, "Image or Text is null");
            return;
        }
        m_Msg = m_Text.text;
        if (m_IsControllerLeft)
        {
            if (m_LastTriggerMsg != m_Msg)
            {
                m_LastTriggerMsg = m_Msg;
                MsgCtrl(m_Anchor, m_DefaultEdge);
            }
            else if (m_LastBackLongMsg != m_Msg)
            {
                m_LastBackLongMsg = m_Msg;
                MsgCtrl(m_Anchor, m_DefaultEdge);
            }
            else if (m_LastBackShortMsg != m_Msg)
            {
                m_LastBackShortMsg = m_Msg;
                MsgCtrl(m_Anchor, m_DefaultEdge);
            }
            else if (m_LastConfirmMsg != m_Msg)
            {
                m_LastConfirmMsg = m_Msg;
                MsgCtrl(m_Anchor, m_DefaultEdge);
            }
        }
        if (m_IsControllerRight)
        {
            if (m_LastHomeLongMsg != m_Msg)
            {
                m_LastHomeLongMsg = m_Msg;
                MsgCtrl(m_Anchor, m_DefaultEdge);
            }
            else if (m_LastHomeShortMsg != m_Msg)
            {
                m_LastHomeShortMsg = m_Msg;
                MsgCtrl(m_Anchor, m_DefaultEdge);
            }
            else if (m_LastVolumeMsg != m_Msg)
            {
                m_LastVolumeMsg = m_Msg;
                MsgCtrl(m_Anchor, m_DefaultEdge);
            }
        }
    }

    private void MsgCtrl(int anchor, int edge)
    {
        Font font = m_Text.font;
        font.RequestCharactersInTexture(m_Msg, m_Text.fontSize, m_Text.fontStyle);

        CharacterInfo chara = new CharacterInfo();
        char[] arr = m_Msg.ToCharArray();
        int length = 0;
        foreach (char c in arr)
        {
            font.GetCharacterInfo(c, out chara, m_Text.fontSize);
            length += chara.advance;
        }

        int msgLength = 0;
        int msgWidth = 0;
        if (length >= m_DefaultFontCount * m_Text.fontSize)
        {
            msgLength = m_DefaultFontCount * m_Text.fontSize;
            msgWidth = (int)Math.Ceiling((double)length / msgLength) * (m_Text.fontSize + 3) - 1;
        }
        else
        {
            msgLength = length;
            msgWidth = 20;
        }

        m_Image.rectTransform.anchorMin = new Vector2(anchor, 0.5f);
        m_Image.rectTransform.anchorMax = new Vector2(anchor, 0.5f);
        m_Image.rectTransform.pivot = new Vector2(anchor, 0.5f);
        m_Image.rectTransform.anchoredPosition = new Vector2(0, 0);
        m_Image.rectTransform.sizeDelta = new Vector2(msgLength + Mathf.Abs(edge) * 2, msgWidth + 4); //25
        m_Image.GetComponent<Image>().color = HVRHelpMessage.m_ImageColor;

        m_Text.rectTransform.anchorMin = new Vector2(anchor, 0.5f);
        m_Text.rectTransform.anchorMax = new Vector2(anchor, 0.5f);
        m_Text.rectTransform.pivot = new Vector2(anchor, 0.5f);
        m_Text.rectTransform.anchoredPosition = new Vector2(0 + edge, 0.0f);
        m_Text.rectTransform.sizeDelta = new Vector2(msgLength, msgWidth);
        m_Text.GetComponent<Text>().color = HVRHelpMessage.m_TextColor;
        m_Text.GetComponent<Text>().fontSize = HVRHelpMessage.m_FontSize;
        Outline outLine = m_Text.transform.GetComponent<Outline>();
        if (outLine != null)
        {
            if (m_Text.GetComponent<Text>().color.r > 0.5f)
            {
                outLine.effectColor = Color.black;
            }
            else
            {
                outLine.effectColor = Color.white;
            }
        }
    }
    private void ArrowCtrl()
    {
        if (m_Arrow == null)
        {
            HVRLogCore.LOGI(TAG, "mArrow is null");
            return;
        }
        m_Arrow.color = HVRHelpMessage.m_ArrowColor;

        if (m_Spot != null)
        {
            m_Spot.GetComponent<Image>().color = HVRHelpMessage.m_ArrowColor;
        }

        if (m_ArrowAttach != null)
        {
            m_ArrowAttach.GetComponent<Image>().color = HVRHelpMessage.m_ArrowColor;
        }
    }
}
