using System;
using System.Windows;

namespace TeamNote
{
  public partial class App : Application
  {
    public delegate void ApplicationCloseDelegate(int exitCode);
    public delegate bool ApplicationLoadLanguage(string localeCode);

    private Client.ClientInstance m_clientInstance;

    public App()
    {
      this.m_clientInstance = new Client.ClientInstance(this.CloseApplication);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
      Debug.Setup("TeamNote_Client.log");
      base.OnStartup(e);

      this.m_clientInstance.Initialize(this.LoadLocale);
    }

    private void CloseApplication(int exitCode)
    {
      this.Dispatcher.Invoke(() => Current.Shutdown(exitCode));
    }

    private bool LoadLocale(string localeCode)
    {
      Debug.Log("Loading locale {0}.", localeCode);

      ResourceDictionary languageDictionary = new ResourceDictionary();
      if (string.IsNullOrEmpty(localeCode)) {
        localeCode = System.Threading.Thread.CurrentThread.CurrentCulture.ThreeLetterISOLanguageName;
      }
      try {
        languageDictionary.Source = new Uri(string.Format("Languages\\{0}.xaml", localeCode), UriKind.Relative);
        Resources.MergedDictionaries.Add(languageDictionary);
      }
      catch (Exception ex) {
        Debug.Exception(ex);
        return false;
      }
      return true;
    }
  }
}
