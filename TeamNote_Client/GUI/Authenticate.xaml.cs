using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
  public partial class Authenticate : Window
  {
    public delegate void AcceptedAuthorization(string name, string surname);
    public delegate void CanceledAuthorization();

    public event AcceptedAuthorization onAuthorizationAccept;
    public event CanceledAuthorization onAuthorizationCancel;

    private Regex m_textValidator;

    public Authenticate()
    {
      InitializeComponent();
      this.m_textValidator = new Regex("^[a-zA-Z0-9]*$");
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
      else if (!this.m_textValidator.IsMatch(this.tbName.Text)) {
        this.SetStatusLabel("Authenticate_Status_Invalid_Name");
      }
      else if (!this.m_textValidator.IsMatch(this.tbSurname.Text)) {
        this.SetStatusLabel("Authenticate_Status_Invalid_Surname");
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
