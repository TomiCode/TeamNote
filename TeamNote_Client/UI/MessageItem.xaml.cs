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
