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
using TeamNote.Protocol;

namespace TeamNote.UI
{
  public partial class ContactItem : UserControl
  {
    public delegate void ContactItemButtonClickHandler(long client);

    public event ContactItemButtonClickHandler onMessageClick;
    public event ContactItemButtonClickHandler onInfotmationClick;

    private long m_clientId;
    private bool m_clientStatus;

    public long ClientId {
      get {
        return this.m_clientId;
      }
    }

    public bool Status {
      get {
        return this.m_clientStatus;
      }
      set {
        this.m_clientStatus = value;
        this.UpdateStatusLabel();
      }
    }

    public bool IsValid {
      get {
        return (this.m_clientId != 0);
      }
    }

    public ContactItem()
    {
      InitializeComponent();
      this.m_clientId = 0;
    }

    public ContactItem(ContactUpdate.Types.Client client)
    {
      InitializeComponent();
      this.m_clientId = client.ClientId;
      this.Dispatcher.Invoke(() => {
        this.lbContactName.Content = string.Format("{0} {1}", client.Name, client.Surname);
        this.UpdateStatusLabel();
      });
    }

    public void UpdateContactProfile(string name, string surname)
    {

    }

    private void UpdateStatusLabel()
    {
      string contactStatus = (string)Application.Current.Resources[this.m_clientStatus ? GUI.Contacts.STATUS_ONLINE_RESOURCE : GUI.Contacts.STATUS_AWAY_RESOURCE];
      if (contactStatus == null) {
        Debug.Warn("Cannot read STATUS resources.");
        return;
      }
      this.Dispatcher.Invoke(() => this.lbContactStatus.Content = contactStatus);
    }

    private void btnContactMessage_Click(object sender, RoutedEventArgs e)
    {
      Debug.Log("Clicked message button ClientId={0}.", this.m_clientId);
      this.onMessageClick?.Invoke(this.m_clientId);
    }

    private void btnContactInfo_Click(object sender, RoutedEventArgs e)
    {
      Debug.Log("Clicked information button ClientId={0}.", this.m_clientId);
      this.onInfotmationClick?.Invoke(this.m_clientId);
    }
  }
}
