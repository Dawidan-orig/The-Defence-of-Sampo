#if UNITY_EDITOR
using UnityEngine;

namespace Verpha.HierarchyDesigner
{
    [System.Serializable]
    public class HierarchyDesigner_Info_Separator
    {
        #region Properties
        private string name = "";
        private Color textColor = Color.white;
        private Color backgroundColor = Color.gray;
        private FontStyle fontStyle = FontStyle.Normal;
        private int fontSize = 12;
        private TextAnchor textAlignment = TextAnchor.MiddleCenter;
        private BackgroundImageType backgroundImageType = BackgroundImageType.Classic;
        #endregion

        #region Accessors
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Color TextColor
        {
            get { return textColor; }
            set { textColor = value; }
        }

        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

        public FontStyle FontStyle
        {
            get { return fontStyle; }
            set { fontStyle = value; }
        }

        public int FontSize
        {
            get { return fontSize; }
            set { fontSize = value; }
        }

        public TextAnchor TextAlignment
        {
            get { return textAlignment; }
            set { textAlignment = value; }
        }

        public BackgroundImageType ImageType
        {
            get { return backgroundImageType; }
            set { backgroundImageType = value; }
        }

        public enum BackgroundImageType 
        { 
            Classic,
            ClassicFadedTop,
            ClassicFadedLeft,
            ClassicFadedRight,
            ClassicFadedBottom,
            ClassicFadedLeftAndRight,
            ModernI,
            ModernII,
            ModernIII,
        }
        #endregion

        public HierarchyDesigner_Info_Separator(string name, Color textColor, Color backgroundColor, FontStyle fontStyle, int fontSize, TextAnchor textAlignment, BackgroundImageType backgroundImageType)
        {
            this.name = name;
            this.textColor = textColor;
            this.backgroundColor = backgroundColor;
            this.fontStyle = fontStyle;
            this.fontSize = fontSize;
            this.textAlignment = textAlignment;
            this.backgroundImageType = backgroundImageType;
        }
    }
}
#endif