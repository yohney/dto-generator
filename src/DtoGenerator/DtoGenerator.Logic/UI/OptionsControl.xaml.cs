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

namespace DtoGenerator.Logic.UI
{
    /// <summary>
    /// Interaction logic for OptionsControl.xaml
    /// </summary>
    public partial class OptionsControl : UserControl
    {
        public event EventHandler OnCancel;
        public event EventHandler OnConfirm;

        public OptionsControl()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.OnCancel != null)
                this.OnCancel.Invoke(this, EventArgs.Empty);
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (this.OnConfirm != null)
                this.OnConfirm.Invoke(this, EventArgs.Empty);
        }
    }
}
