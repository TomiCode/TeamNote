using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace TeamNote.GUI
{
  public partial class Authenticate : Window
  {
    public delegate void AcceptedAuthorization(string name, string surname);
    public delegate void CanceledAuthorization();

    public event AcceptedAuthorization onAuthorizationAccept;
    public event CanceledAuthorization onAuthorizationCancel;

    public Authenticate()
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
      this.Activate();
    }

    public new void Hide()
    {
      (Application.Current.Resources["HideWindowStoryboard"] as Storyboard)?.Begin(this);
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      if (this.onAuthorizationCancel != null) {
        this.onAuthorizationCancel.Invoke();
        this.Hide();
      }
    }

    private void btnConnect_Click(object sender, RoutedEventArgs e)
    {
      if (this.tbName.Text == string.Empty || this.tbSurname.Text == string.Empty) {
        this.SetStatusLabel("Authenticate_Status_Invalid_Empty");
      }
      else {
        this.SetStatusLabel("Authenticate_Status_Notice");
        this.onAuthorizationAccept?.Invoke(this.tbName.Text, this.tbSurname.Text);
        this.Hide();
      }
    }

    private void SetStatusLabel(string resourceString)
    {
      Debug.Log("Setting status to: {0}.", resourceString);
      if (Application.Current.Resources.Contains(resourceString)) {
        this.lbStatus.Content = Application.Current.Resources[resourceString];
      }
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.ChangedButton == MouseButton.Left)
        DragMove();
    }
  }
}
