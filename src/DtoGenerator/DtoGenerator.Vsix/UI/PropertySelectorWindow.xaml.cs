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

namespace DtoGenerator.Vsix.UI
{
    /// <summary>
    /// Interaction logic for PropertySelectorWindow.xaml
    /// </summary>
    public partial class PropertySelectorWindow
    {
        public PropertySelectorWindow()
        {
            InitializeComponent();
        }

        private void container_OnCancel(object sender, EventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void container_OnConfirm(object sender, EventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
