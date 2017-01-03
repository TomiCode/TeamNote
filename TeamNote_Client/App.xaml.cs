using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TeamNote
{
  public partial class App : Application
  {
    public delegate void ApplicationCloseDelegate(int exitCode);

    private Client.ClientInstance m_clientInstance;

    public App()
    {
      this.m_clientInstance = new Client.ClientInstance(this.CloseApplication);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
      Debug.Setup("TeamNote_Client.log");

      this.m_clientInstance.Initialize();
      base.OnStartup(e);
    }

    private void CloseApplication(int exitCode)
    {
      this.Dispatcher.Invoke(() => Current.Shutdown(exitCode));
    }
  }
}
