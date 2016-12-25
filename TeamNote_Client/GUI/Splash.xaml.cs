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
    }

    public void SetMessage(string resourceMessage)
    {
      string message = Application.Current.Resources[resourceMessage] as string;
      if (message == null) {
        Debug.Log("Message '{0}' not found in Language Dictionary.", resourceMessage);
        return;
      }

      this.lbl_status.Dispatcher.Invoke(() => {
        this.lbl_status.Content = message;
      });
    }
  }
}
