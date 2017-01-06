using System;
using System.Windows.Controls;

namespace TeamNote.UI
{
  public partial class MessageItem : UserControl
  {
    new public string Content {
      set {
        this.tbContent.Dispatcher.Invoke(() => this.tbContent.Text = value);
      }
      get {
        return this.tbContent.Text;
      }
    }

    public object Username {
      set {
        this.lbUsername.Dispatcher.Invoke(() => this.lbUsername.Content = value);
      }
      get {
        return this.lbUsername.Content;
      }
    }

    public DateTime Date {
      set {
        this.lbDatetime.Dispatcher.Invoke(() => this.lbDatetime.Content = value.ToString("HH:mm, dddd d MMMM"));
      }
    }
  
    public MessageItem()
    {
      InitializeComponent();
    }
  }
}
