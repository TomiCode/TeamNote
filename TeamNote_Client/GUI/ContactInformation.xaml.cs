using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
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
  public partial class ContactInformation : Window
  {
    public ContactInformation()
    {
      InitializeComponent();

      this.Opacity = 0;
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

    public void UpdatePublicKey(AsymmetricKeyParameter key)
    {
      if (key == null) {
        this.Dispatcher.Invoke(() => {
          this.tbModulus.Text = "not requested";
        });
        return;
      }

      RsaKeyParameters rsaParameters = key as RsaKeyParameters;
      byte[] modulusBytes = rsaParameters.Modulus.ToByteArray();

      this.Dispatcher.Invoke(() => {
        this.tbModulus.Text = Convert.ToBase64String(modulusBytes);
      });
    }

    public void UpdateClient(string name, long client)
    {
      this.Dispatcher.Invoke(() => {
        this.lbContactId.Content = client.ToString();
        this.lbContactName.Content = name;
      });
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.ChangedButton == MouseButton.Left)
        DragMove();
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
      this.Hide();
    }
  }
}
