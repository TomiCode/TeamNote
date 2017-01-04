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

namespace TeamNote.GUI
{
  public delegate bool MessageSendHandlerDelegate(string messageContent);

  public partial class Message : Window
  {
    public event MessageSendHandlerDelegate onMessageAccept;

    private Contacts.LocalClient m_localClient;

    public Message()
    {
      InitializeComponent();
    }

    public void SetWindow(Contacts.LocalClient localContact, UI.ContactItem.Contact selectedContact)
    {
      this.m_localClient = localContact;
      this.lbUsername.Dispatcher.Invoke(() => this.lbUsername.Content = selectedContact.Username);
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed)
        this.DragMove();
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
        UI.MessageItem sendMessage = new UI.MessageItem();
        sendMessage.Content = "Cannot send message :(";
        sendMessage.Username = "Server";
        sendMessage.Date = DateTime.Now;
        this.spMessageList.Children.Add(sendMessage);
      }

    }

    private void AddServerMessage(string messageContent)
    {

    }
    }
}