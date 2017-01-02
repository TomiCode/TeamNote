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
using System.Windows.Shapes;

using TeamNote.Client;
using TeamNote.Protocol;

namespace TeamNote.GUI
{
  public partial class Contacts : Window
  {
    public delegate void UpdateClientDataHandler(bool onlineStatus, string name = "", string surname = "");
    public event UpdateClientDataHandler onClientDataUpdated;

    public const string STATUS_AWAY_RESOURCE = "Contacts_Status_Away";
    public const string STATUS_ONLINE_RESOURCE = "Contacts_Status_Online";

    private bool m_localOnlineStatus;
    private string m_localClientName;
    private string m_localClientSurname;

    public bool OnlineStatus {
      get {
        return this.m_localOnlineStatus;
      }
      set {
        this.SetStatus(value);
      }
    }

    public string StatusText {
      get {
        return (string)this.lbWindowStatus.Content;
      }
      set {
        this.Dispatcher.Invoke(() => {
          this.lbWindowStatus.Content = value;
        });
      }
    }

    public List<long> Clients {
      get {
        return this.Dispatcher.Invoke(() => {
          return this.spContacts.Children.Cast<UI.ContactItem>().Select(i => i.ClientId).ToList();
        });
      }
    }

    public Contacts()
    {
      InitializeComponent();
    }

    public void Setup(string name, string surname, bool status = true)
    {
      this.m_localClientName = name;
      this.m_localClientSurname = surname;
      this.m_localOnlineStatus = status;

      this.Dispatcher.Invoke(() => {
        this.lbName.Content = this.m_localClientName;
        this.lbSurname.Content = this.m_localClientSurname;
        this.UpdateStatusLabel();
      });
    }

    public void ChangeClientName(string name, string surname)
    {
      this.m_localClientName = name;
      this.m_localClientSurname = surname;

      this.Dispatcher.Invoke(() => {
        this.lbName.Content = this.m_localClientName;
        this.lbSurname.Content = this.m_localClientSurname;
      });
      this.onClientDataUpdated?.Invoke(this.m_localOnlineStatus, this.m_localClientName, this.m_localClientSurname);
    }

    public void CreateClient(ContactUpdate.Types.Client client)
    {
      Debug.Log("Creating clientId={0} Name={1} Surname={2}.", client.ClientId, client.Name, client.Surname);
      this.spContacts.Dispatcher.Invoke(() => {
        this.spContacts.Children.Add(new UI.ContactItem(client));
      });
    }

    public void RemoveClient(long clientId)
    {
      Debug.Log("Removing clientId={0}.", clientId);
      this.spContacts.Dispatcher.Invoke(() => {
        UI.ContactItem contactItem = null;

        foreach (UI.ContactItem contactElement in this.spContacts.Children) {
          if (contactElement.ClientId == clientId) {
            Debug.Log("Found Stackpanel element.");
            contactItem = contactElement;
            break;
          }
        }
        if (contactItem != null) {
          Debug.Log("Removing item from StackPanel.");
          this.spContacts.Children.Remove(contactItem);
        }
      });
    }

    private void SetStatus(bool status)
    {
      this.m_localOnlineStatus = status;
      this.UpdateStatusLabel();
      this.onClientDataUpdated?.Invoke(this.m_localOnlineStatus);
    }

    private void UpdateStatusLabel()
    {
      string statusText = null;
      if (this.m_localOnlineStatus)
        statusText = (string)Application.Current.Resources[STATUS_ONLINE_RESOURCE];
      else
        statusText = (string)Application.Current.Resources[STATUS_AWAY_RESOURCE];

      if (statusText == null) {
        Debug.Warn("Can not load resources for STATUS texts.");
        return;
      }
      this.lbStatus.Content = statusText;
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.ChangedButton == MouseButton.Left)
        DragMove();
    }
  }
}
