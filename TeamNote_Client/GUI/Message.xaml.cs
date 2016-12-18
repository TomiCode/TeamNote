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
  public partial class Message : Window
  {
    public Message()
    {
      InitializeComponent();
    }

    private void btn_send_Click(object sender, RoutedEventArgs e)
    {
      this.sp_messageList.Children.Add(new UI.MessageItem(DateTime.Now, "Name Surname", this.tbx_message.Text));
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed)
        this.DragMove();
    }
  }
}
