namespace NibiruTask
{
    public class ThemeApiData
    {
        private string themeName;
        private string themeSign;
        private string themeIcon;


        public ThemeApiData(string themeName, string themeSign, string themeIcon)
        {
            this.setThemeName(themeName);
            this.setThemeSign(themeSign);
            this.setThemeIcon(themeIcon);
        }

        public string getThemeName()
        {
            return this.themeName;
        }

        public void setThemeName(string themeName)
        {
            this.themeName = themeName;
        }

        public string getThemeSign()
        {
            return this.themeSign;
        }

        public void setThemeSign(string themeSign)
        {
            this.themeSign = themeSign;
        }

        public string getThemeIcon()
        {
            return this.themeIcon;
        }

        public void setThemeIcon(string themeIcon)
        {
            this.themeIcon = themeIcon;
        }

        public string toString()
        {
            return "ThemeApiData [themeName=" + this.themeName + ", themeSign=" + this.themeSign + ", themeIcon=" + this.themeIcon + "]";
        }
    }
}