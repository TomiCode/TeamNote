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

namespace TeamNote.GUI
{
  public delegate bool MessageSendHandlerDelegate(string messageContent);

  public partial class Message : Window
  {
    public event MessageSendHandlerDelegate onMessageAccept;

    private Contacts.LocalClient m_localClient;
    private bool m_autoScroll;

    public Message()
    {
      InitializeComponent();
      this.m_autoScroll = true;
      this.Opacity = 0;

      this.btnClose.Click += (object sender, RoutedEventArgs e) => {
        this.Hide();
      };

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
    }

    public new void Hide()
    {
      (Application.Current.Resources["HideWindowStoryboard"] as Storyboard)?.Begin(this);
    }

    public void SetWindow(Contacts.LocalClient localContact, UI.ContactItem.Contact selectedContact)
    {
      this.m_localClient = localContact;
      this.lbUsername.Dispatcher.Invoke(() => this.lbUsername.Content = selectedContact.Username);
    }

    public void AddServerMessage(string resourceMessage)
    {
      object serverName = Application.Current.Resources["Message_ServerName"];
      string messageContent = Application.Current.Resources[resourceMessage] as string;
      if (serverName == null || messageContent == null) {
        Debug.Warn("Cannot read reources for Server messages. Resource message: '{0}'.", resourceMessage);
        return;
      }

      UI.MessageItem serverMessage = null;
      this.Dispatcher.Invoke(() => serverMessage = new UI.MessageItem());
      if (serverMessage == null) {
        Debug.Error("Cannot create MessageItem object from dispatcher.");
        return;
      }

      serverMessage.Content = messageContent;
      serverMessage.Username = serverName;
      serverMessage.Date = DateTime.Now;

      this.spMessageList.Dispatcher.Invoke(() => this.spMessageList.Children.Add(serverMessage));
    }

    public void AddMessage(UI.ContactItem.Contact senderContact, string messageContent)
    {
      this.spMessageList.Dispatcher.Invoke(() => {
        UI.MessageItem receivedMessage = new UI.MessageItem();
        receivedMessage.Content = messageContent;
        receivedMessage.Username = senderContact.Username;
        receivedMessage.Date = DateTime.Now;

        this.spMessageList.Children.Add(receivedMessage);
      });
    }

    private void tbMessage_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter) {
        e.Handled = true;
        this.btnSend_Click(null, null);
      }
      return;
    }

    private void btnSend_Click(object sender, RoutedEventArgs e)
    {
      string messageContent = this.tbMessage.Text;
      if (messageContent == string.Empty) {
        Debug.Warn("Cannot send empty messages.");
        return;
      }

      this.tbMessage.Clear();
      if (this.onMessageAccept?.Invoke(messageContent) == true) {
        UI.MessageItem sendMessage = new UI.MessageItem();
        sendMessage.Content = messageContent;
        sendMessage.Username = string.Format("{0} {1}", this.m_localClient.Name, this.m_localClient.Surname);
        sendMessage.Date = DateTime.Now;
        this.spMessageList.Children.Add(sendMessage);
      }
      else {
        this.AddServerMessage("Message_SendFailed");
      }
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      if (!(sender is ScrollViewer)) return;

      ScrollViewer scrollViewer = sender as ScrollViewer;
      if (e.ExtentHeightChange == 0)
        this.m_autoScroll = (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight);
      
      if (this.m_autoScroll && e.ExtentHeightChange != 0)
        scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
    }
  }
}