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
    private Client.ClientInstance m_clientInstance;

    public App()
    {
      this.m_clientInstance = new Client.ClientInstance();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
      Debug.Setup("TeamNote_Client.log");

      this.m_clientInstance.Initialize();
      base.OnStartup(e);
    }
  }
}
