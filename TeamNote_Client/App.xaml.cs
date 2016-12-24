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

    protected override void OnStartup(StartupEventArgs e)
    {
      if (this.m_clientInstance == null)
        this.m_clientInstance = new Client.ClientInstance();

      Debug.Setup("TeamNote_Client.log");
      base.OnStartup(e);

      this.m_clientInstance.Initialize();
    }
  }
}
