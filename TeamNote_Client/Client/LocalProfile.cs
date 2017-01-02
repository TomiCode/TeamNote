using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamNote.Client
{
  class LocalProfile
  {
    private string m_clientName;
    private string m_clientSurname;
    private bool m_clientOnlineStatus;

    public string Name {
      get {
        return this.m_clientName;
      }
    }

    public string Surname {
      get {
        return this.m_clientSurname;
      }
    }

    public bool Online {
      get {
        return this.m_clientOnlineStatus;
      }
    }

    public bool Valid {
      get {
        return (this.m_clientName != string.Empty && this.m_clientSurname != string.Empty);
      }
    }

    public LocalProfile()
    {
      this.m_clientOnlineStatus = true;
    }

    public void Setup(string name, string surname)
    {
      this.m_clientName = name;
      this.m_clientSurname = surname;
    }

    public void ChangeStatus(bool onlineStatus)
    {
      this.m_clientOnlineStatus = onlineStatus;
    }
  }
}
