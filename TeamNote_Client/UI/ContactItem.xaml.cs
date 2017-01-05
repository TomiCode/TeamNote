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
using TeamNote.GUI;

namespace TeamNote.UI
{
  public partial class ContactItem : UserControl
  {
    public enum Buttons : byte
    {
      Information,
      Message
    }

    public class Contact
    {
      public ContactsDataUpdateDelegate onDataUpdate;

      private long m_clientId;
      private string m_clientUsername;
      private bool m_clientOnlineStatus;

      public long ClientId {
        get {
          return this.m_clientId;
        }
      }

      public bool Status {
        get {
          return this.m_clientOnlineStatus;
        }
        set {
          this.m_clientOnlineStatus = value;
          this.onDataUpdate?.Invoke();
        }
      }

      public bool Valid {
        get {
          return ((this.m_clientId != 0) && (this.m_clientUsername != null) && (this.m_clientUsername != string.Empty));
        }
      }

      public string Username {
        get {
          return this.m_clientUsername;
        }
      }

      public Contact(long clientId)
      {
        this.m_clientId = clientId;
        this.m_clientOnlineStatus = true;
      }

      public void SetUsername(string name, string surname)
      {
        this.m_clientUsername = string.Format("{0} {1}", name, surname);
        this.onDataUpdate?.Invoke();
      }
    }

    public event ContactItemButtonClickDelegate onContactItemButtonClick;

    private Contact m_clientContact;

    public Contact ClientContact {
      get {
        return this.m_clientContact;
      }
    }

    public ContactItem()
    {
      InitializeComponent();
      this.m_clientContact = new Contact(0);

      Debug.Warn("This constructor is deprecated. ContactItem is probably invalid.");
    }

    public ContactItem(ContactUpdate.Types.Client client)
    {
      InitializeComponent();
      this.m_clientContact = new Contact(client.ClientId);
      this.m_clientContact.onDataUpdate += this.OnDataUpdated;

      this.m_clientContact.SetUsername(client.Name, client.Surname);
      this.m_clientContact.Status = client.Online;
    }

    private void OnDataUpdated()
    {
      Debug.Log("ClientId={0} data updated Online={1}.", this.m_clientContact.ClientId, this.m_clientContact.Status);
      object statusContent = Application.Current.Resources[this.m_clientContact.Status ? Contacts.STATUS_ONLINE_RESOURCE : Contacts.STATUS_AWAY_RESOURCE];
      if (statusContent == null) 
        Debug.Warn("Status resource is invalid.");

      this.Dispatcher.Invoke(() => {
        if ((string)this.lbContactName.Content != this.m_clientContact.Username)
          this.lbContactName.Content = this.m_clientContact.Username;

        if (statusContent != null)
          this.lbContactStatus.Content = statusContent;
      });
    }

    private void btnContactMessage_Click(object sender, RoutedEventArgs e)
    {
      if (this.m_clientContact.Valid)
        this.onContactItemButtonClick?.Invoke(this.m_clientContact, Buttons.Message);
    }

    private void btnContactInfo_Click(object sender, RoutedEventArgs e)
    {
      if (this.m_clientContact.Valid)
        this.onContactItemButtonClick?.Invoke(this.m_clientContact, Buttons.Information);
    }
  }
}