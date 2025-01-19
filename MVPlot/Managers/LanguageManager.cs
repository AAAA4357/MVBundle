using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace MVPlot.Managers
{
    /// <summary>
    /// 语言管理器
    /// </summary>
    public class LanguageManager : INotifyPropertyChanged
    {
        private readonly ResourceManager _resourceManager;

        private static readonly Lazy<LanguageManager> _lazy = new(() => new LanguageManager());
        public static LanguageManager Instance => _lazy.Value;

        public event PropertyChangedEventHandler? PropertyChanged;

        public LanguageManager()
        {
            _resourceManager = Lang.ResourceManager;
        }

        /// <summary>
        /// 索引器，传入字符串的下标
        /// </summary>
        /// <param name="name">字符串索引</param>
        /// <returns>国际化字符串</returns>
        /// <exception cref="ArgumentNullException">当索引值为Null时抛出</exception>
        public string? this[string name]
        {
            get
            {
                ArgumentNullException.ThrowIfNull(name);
                return _resourceManager.GetString(name);
            }
        }

        public void ChangeLanguage(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("item[]"));
        }
    }

}
