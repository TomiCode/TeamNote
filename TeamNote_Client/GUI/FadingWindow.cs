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
  class FadingWindow : Window
  {
    private DoubleAnimation m_hideAnimation;
    private DoubleAnimation m_showAnimation;

    public FadingWindow()
    {
      this.Opacity = 0;
      this.m_showAnimation = new DoubleAnimation(1, TimeSpan.FromMilliseconds(300));

      this.m_hideAnimation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300));
      this.m_hideAnimation.Completed += (s, _) => base.Hide();
    }

    public new void Hide()
    {
      this.BeginAnimation(OpacityProperty, this.m_hideAnimation);
    }

    public new void Show()
    {
      base.Show();
      this.BeginAnimation(OpacityProperty, this.m_showAnimation);
    }
  }
}
