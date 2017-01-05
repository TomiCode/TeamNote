using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace TeamNote.GUI
{
  public delegate void NoticeWindowClickDelegate(object param);

  public partial class Notice : Window
  {
    public event NoticeWindowClickDelegate onButtonClick;

    public object SenderObject { private get; set; }

    public string MessageResource {
      set {
        object resourceContent = Application.Current.Resources[value];
        if (resourceContent != null)
          this.Dispatcher.Invoke(() => this.lbResource.Content = resourceContent);
        else
          Debug.Warn("Invalid resource {0}.", value);
      }
    }

    public string ButtonResource {
      set {
        object buttonContent = Application.Current.Resources[value];
        if (buttonContent != null)
          this.Dispatcher.Invoke(() => this.btnNotice.Content = buttonContent);
        else
          Debug.Warn("Button resource '{0}' invalid.", value);
      }
    }

    public string MessageContent {
      set {
        this.Dispatcher.Invoke(() => this.lbContent.Content = value);
      }
    }

    private Storyboard m_inStoryboard;
    private Storyboard m_outStoryboard;

    public Notice()
    {
      InitializeComponent();

      this.Opacity = 0;
      this.Loaded += (object a, RoutedEventArgs s) => {
        Rect desktopWorkingArea = SystemParameters.WorkArea;
        this.Left = (desktopWorkingArea.Right - this.Width);
        this.Top = (desktopWorkingArea.Bottom - this.Height);
      };
      this.btnNotice.Click += (object sender, RoutedEventArgs e) => this.onButtonClick?.Invoke(this.SenderObject);
      this.IsVisibleChanged += (object s, DependencyPropertyChangedEventArgs e) => {
        if (!(bool)e.NewValue) {
          Debug.Warn("Closing notice.");
          this.Close();
        }
      };

      this.MouseEnter += (s, e) => {
        this.m_outStoryboard.Stop(this);
        (Application.Current.Resources["NoticeRevertFadeout"] as Storyboard).Begin(this);
      };
      this.MouseLeave += (s, e) => {
        this.m_outStoryboard.Begin(this, true);
        this.m_outStoryboard.Seek(this, TimeSpan.FromSeconds(4.5), TimeSeekOrigin.BeginTime);
      };
    }

    public new void Show()
    {
      base.Show();
      this.m_inStoryboard = (Application.Current.Resources["ShowWindowStoryboard"] as Storyboard).Clone();
      this.m_outStoryboard = Application.Current.Resources["NoticeSlowFadeout"] as Storyboard;

      this.m_inStoryboard.Completed += (s, e) => this.m_outStoryboard.Begin(this, true);
      this.m_inStoryboard.Begin(this, true);
    }
  }
}
