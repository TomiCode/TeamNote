using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TeamNote.UI
{
  public partial class ContactItem : UserControl
  {
    public delegate void ContactItemButtonClickHandler(long client);

    public event ContactItemButtonClickHandler onMessageClick;
    public event ContactItemButtonClickHandler onInfotmationClick;

    private long m_clientId;

    public long ClientId {
      get {
        return this.m_clientId;
      }
    }

    public bool IsValid {
      get {
        return (this.m_clientId != 0);
      }
    }

    public ContactItem(long clientId)
    {
      InitializeComponent();
      this.m_clientId = clientId;
    }
  }
}
