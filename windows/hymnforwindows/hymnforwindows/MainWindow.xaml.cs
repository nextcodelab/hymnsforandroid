using HymnLibrary;
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

namespace hymnforwindows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HymnalManager.InitData();
            var hymn = HymnalManager.GetFirstHymn();
            this.headrTxt.Text = hymn._id;
            hymnContent.SetHymn(hymn);
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuOllllllpen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void MenuToggleButton_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private void autoSearchBox_ItemSelected(object sender, HymnLibrary.Models.Hymn e)
        {
            headrTxt.Text = e._id;
            this.hymnContent.SetHymn(e);
        }
    }
}
