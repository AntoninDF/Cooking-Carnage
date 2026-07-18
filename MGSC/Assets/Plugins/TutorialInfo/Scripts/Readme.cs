using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Readme", menuName = "Readme", order = 1000)]
public class Readme : ScriptableObject
{
    public Texture2D icon;
    public string title;
    public Section[] sections;
    public bool loadedLayout;

    [Serializable]
    public class Section
    {
        public string heading, text, linkText, url;
    }

    public Readme()
    {
        title = "New readme";
        Section first = new Section();
        first.heading = "How to edit a readme ?";
        first.text = "Click on the three dots on the right of the inspector tab, and select debug mode. Once in debug mode, you can edit the title, icon, and sections.";
        sections = new Section[]{ first};
    }
}
