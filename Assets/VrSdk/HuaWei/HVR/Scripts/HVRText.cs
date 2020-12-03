using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class HVRText : Text
{
    private readonly string markList = @"(\！|\？|\，|\。|\《|\》|\）|\：|\“|\‘|\、|\；|\+|\-)";

    StringBuilder textStr;
    public override void SetVerticesDirty()
    {
        var settings = GetGenerationSettings(rectTransform.rect.size);
        cachedTextGenerator.Populate(this.text, settings);

        textStr = new StringBuilder(this.text);

        IList<UILineInfo> lineList = this.cachedTextGenerator.lines;
        int changeIndex = -1;
        for (int i = 1; i < lineList.Count; i++)
        {
            bool isMark = Regex.IsMatch(text[lineList[i].startCharIdx].ToString(), markList);

            if (isMark)
            {
                changeIndex = lineList[i].startCharIdx - 1;
                string str = text.Substring(lineList[i - 1].startCharIdx, lineList[i].startCharIdx);
                MatchCollection richStrMatch = Regex.Matches(str, ".(</color>|<color=#\\w{6}>|" + markList + ")+$");
                if (richStrMatch.Count > 0)
                {
                    string richStr = richStrMatch[0].ToString();
                    int length = richStr.Length;
                    changeIndex = lineList[i].startCharIdx - length;
                    break;
                }
            }
        }

        if (changeIndex >= 0)
        {
            textStr.Insert(changeIndex, '\n');
            this.text = textStr.ToString();
        }

        base.SetVerticesDirty();
    }
}
