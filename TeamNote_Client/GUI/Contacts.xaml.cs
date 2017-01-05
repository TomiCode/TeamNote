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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using TeamNote.Client;
using TeamNote.Protocol;

namespace TeamNote.GUI
{
  public delegate void ApplicationCloseDelegate();

  public delegate void ContactsDataUpdateDelegate();
  public delegate void ContactItemButtonClickDelegate(UI.ContactItem.Contact sender, UI.ContactItem.Buttons button);

  public partial class Contacts : Window
  {
    public const string STATUS_AWAY_RESOURCE = "Contacts_Status_Away";
    public const string STATUS_ONLINE_RESOURCE = "Contacts_Status_Online";

    public class LocalClient
    {
      public ContactsDataUpdateDelegate onDataUpdate;

      private bool m_clientStatus;
      private string m_clientName;
      private string m_clientSurname;

      public bool Status {
        get {
          return this.m_clientStatus;
        }
        set {
          this.m_clientStatus = value;
          this.onDataUpdate?.Invoke();
        }
      }

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

      public ContactUpdateChangeRequest ContactUpdate {
        get {
          ContactUpdateChangeRequest contactUpdate = new ContactUpdateChangeRequest();
          contactUpdate.Online = this.m_clientStatus;
          contactUpdate.Name = this.m_clientName;
          contactUpdate.Surname = this.m_clientSurname;

          return contactUpdate;
        }
      }

      public LocalClient()
      {
        this.m_clientStatus = true;
        this.m_clientName = string.Empty;
        this.m_clientSurname = string.Empty;
      }

      public void SetUsername(string name, string surname)
      {
        Debug.Log("Setting up local username. [{0} {1}]", name, surname);

        this.m_clientName = name;
        this.m_clientSurname = surname;
        this.onDataUpdate?.Invoke();
      }
    }

    public event ApplicationCloseDelegate onApplicationClose;

    private ContactItemButtonClickDelegate m_clientButtonHandler;
    private LocalClient m_localClientContact;

    public LocalClient LocalContact {
      get {
        return this.m_localClientContact;
      }
   }

    public string StatusText {
      get {
        return (string)this.lbWindowStatus.Content;
      }
      set {
        this.lbWindowStatus.Dispatcher.Invoke(() => this.lbWindowStatus.Content = value);
      }
    }

    public List<long> Clients {
      get {
        return this.Dispatcher.Invoke(() => {
          return this.spContacts.Children.Cast<UI.ContactItem>().Select(i => i.ClientContact.ClientId).ToList();
        });
      }
    }

    public Contacts(ContactItemButtonClickDelegate buttonHandler, ContactsDataUpdateDelegate updateHandler)
    {
      InitializeComponent();

      this.m_clientButtonHandler = buttonHandler;

      this.m_localClientContact = new LocalClient();
      this.m_localClientContact.onDataUpdate += updateHandler;
      this.m_localClientContact.onDataUpdate += OnDataUpdate;

      this.Opacity = 0;

      this.MouseDown += (object s, MouseButtonEventArgs e) => {
        if (e.LeftButton == MouseButtonState.Pressed)
          this.DragMove();
      };
      this.IsVisibleChanged += (object s, DependencyPropertyChangedEventArgs e) => {
        if (!(bool)e.NewValue) {
          Debug.Warn("Hiding window, cause IsVisible changed.");
          base.Hide();
        }
      };
    }

    public new void Show()
    {
      base.Show();
      (Application.Current.Resources["ShowWindowStoryboard"] as Storyboard)?.Begin(this);
      this.Activate();
    }

    public new void Hide()
    {
      (Application.Current.Resources["HideWindowStoryboard"] as Storyboard)?.Begin(this);
    }

    public void CreateClient(ContactUpdate.Types.Client client)
    {
      Debug.Log("Creating clientId={0} Name={1} Surname={2}.", client.ClientId, client.Name, client.Surname);
      this.spContacts.Dispatcher.Invoke(() => {
        UI.ContactItem createdContact = new UI.ContactItem(client);
        createdContact.onContactItemButtonClick += this.m_clientButtonHandler;
        this.spContacts.Children.Add(createdContact);
      });
    }

    public void RemoveClient(long clientId)
    {
      Debug.Log("Removing clientId={0}.", clientId);
      this.spContacts.Dispatcher.Invoke(() => {
        UI.ContactItem contactItem = null;

        foreach (UI.ContactItem contactElement in this.spContacts.Children) {
          if (contactElement.ClientContact.ClientId == clientId) {
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

    public UI.ContactItem.Contact GetClientContact(long clientId)
    {
      Debug.Log("Requesting ClientId={0} contact informations.", clientId);
      return this.Dispatcher.Invoke(() => this.RequestClientContact(clientId));
    }

    private UI.ContactItem.Contact RequestClientContact(long clientId)
    {
      Debug.Log("Requesting ClientId={0}.", clientId);
      foreach (UI.ContactItem contactItem in this.spContacts.Children) {
        if (contactItem.ClientContact.ClientId == clientId)
          return contactItem.ClientContact;
      }
      return null;
    }

    private void OnDataUpdate()
    {
      Debug.Log("Updating local contact data. [{0} {1}]", this.m_localClientContact.Name, this.m_localClientContact.Surname);
      object statusContent = Application.Current.Resources[this.m_localClientContact.Status ? STATUS_ONLINE_RESOURCE : STATUS_AWAY_RESOURCE];

      this.Dispatcher.Invoke(() => {
        if ((string)this.lbName.Content != this.m_localClientContact.Name)
          this.lbName.Content = this.m_localClientContact.Name;

        if ((string)this.lbSurname.Content != this.m_localClientContact.Surname)
          this.lbSurname.Content = this.m_localClientContact.Surname;

        if (statusContent != null)
          this.btnStatus.Content = statusContent;
      });
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
      this.Hide();
      this.onApplicationClose?.Invoke();
    }

    private void btnStatus_Click(object sender, RoutedEventArgs e)
    {
      Debug.Log("Status button clicked.");
      if (this.btnStatus.IsEnabled) {
        Debug.Log("Changing status.");
        this.m_localClientContact.Status = !this.m_localClientContact.Status;
      }

      this.btnStatus.IsEnabled = false;
      Task.Delay(6000).ContinueWith(task => {
        this.btnStatus.Dispatcher.Invoke(() => this.btnStatus.IsEnabled = true);
      });
    }

    private void btnTEST_Click(object s, RoutedEventArgs e)
    {
      Notice notice = new Notice();
      notice.MessageContent = "Testing";
      notice.Show();
    }
  }
}
