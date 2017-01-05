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
  public partial class Splash : Window
  {
    Storyboard m_statusLabelHideStoryboard;
    string m_statusUpdateContent;

    public Splash()
    {
      InitializeComponent();
      this.m_statusUpdateContent = null;
      this.lbStatus.Opacity = 0;

      this.Opacity = 0;
      this.IsVisibleChanged += (object s, DependencyPropertyChangedEventArgs e) => {
        if (!(bool)e.NewValue) {
          Debug.Warn("Hiding window, cause IsVisible changed.");
          try {
            base.Hide();
          }
          catch (Exception ex) {
            Debug.Exception(ex);
          }
        }
      };
    }

    public void SetMessage(string resourceMessage)
    {
      if (this.m_statusLabelHideStoryboard == null)
        this.UpdateStoryboards();

      this.m_statusUpdateContent = Application.Current.Resources[resourceMessage] as string;
      if (this.m_statusUpdateContent == null) {
        Debug.Log("Message '{0}' not found in Language Dictionary.", resourceMessage);
        return;
      }
      this.Dispatcher.Invoke(() => this.m_statusLabelHideStoryboard.Begin(this.lbStatus));
    }

    public new void Show()
    {
      this.Dispatcher.Invoke(() => {
        base.Show();
        (Application.Current.Resources["ShowWindowStoryboard"] as Storyboard)?.Begin(this);
      });
    }

    public new void Hide()
    {
      (Application.Current.Resources["SpashStatusHide"] as Storyboard)?.Begin(this.lbStatus);
      (Application.Current.Resources["HideWindowStoryboard"] as Storyboard)?.Begin(this);
    }

    private void UpdateStoryboards()
    {
      Storyboard showStoryboard = Application.Current.Resources["SpashStatusShow"] as Storyboard;
      this.m_statusLabelHideStoryboard = (Application.Current.Resources["SpashStatusHide"] as Storyboard)?.Clone();
      this.m_statusLabelHideStoryboard.Completed += (s, _) => {
        this.lbStatus.Content = this.m_statusUpdateContent;
        showStoryboard.Begin(this.lbStatus);
      };
    }
  }
}
