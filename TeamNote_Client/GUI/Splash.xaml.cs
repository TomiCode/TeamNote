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
  public partial class Splash : Window
  {
    public Splash()
    {
      InitializeComponent();
      this.MessageTimeout();
    }

    public void UpdateMessage(string content, int timeout)
    {
      Debug.Log("Message set to: {0}", content);
      this.lbl_status.Dispatcher.Invoke(() => this.UpdateLabelContent(content));
    }

    private void MessageTimeout()
    {
      this.lbl_status.Content = "";
    }

    private void UpdateLabelContent(string content)
    {
      this.lbl_status.Content = content;
    }
  }
}
