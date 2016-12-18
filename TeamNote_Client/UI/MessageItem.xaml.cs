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
    public MessageItem(DateTime messageTime, string userName, string messageContent)
    {
      InitializeComponent();

      this.lbl_datetime.Content = messageTime.ToString("dd MMM, HH:mm");
      this.tbl_messageContent.Text = messageContent;
      this.lbl_messageUser.Content = userName;
    }
  }
}
