using HymnLibrary;
using HymnLibrary.Helpers;
using HymnLibrary.Models;
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
        public MaterialDesignThemes.Wpf.BundledTheme BundledTheme { get; set; }
        public ColorBytes ColorByte { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var resources = App.Current.Resources;
            var bundle = resources.MergedDictionaries[0];
            if (bundle is MaterialDesignThemes.Wpf.BundledTheme style)
            {
                BundledTheme = style;
                //BundledTheme.PrimaryColor = MaterialDesignColors.PrimaryColor.Grey;
            }
            HymnalManager.InitData();
            var hymn = await HymnalManager.GetFirstHymnAsync();
            var data = TextCache.GetCache("currenthymn");
            if (data != null)
            {
                hymn = HymnalManager.GetHymnById(data.Value);
            }
            SetHymn(hymn);
            
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
            SetHymn(e);
        }
        void SetHymn(HymnLibrary.Models.Hymn e)
        {
            headrTxt.Text = e._id;
            this.hymnContent.SetHymn(e);
            this.itemsListBox.ItemsSource = HymnalManager.GetHymnsByNumber(e.no);
            TextCache.SaveCache(new DataCache() { UniqueId = "currenthymn", Value = e._id });
           
            NavDrawer.IsLeftDrawerOpen = false;
            UpdateColor(e);
        }
        void UpdateColor(Hymn e)
        {
            ColorByte = HymnalManager.GetColorBytes(HymnalManager.GetLetters(e._id));
            this.colorZone.Background = new SolidColorBrush(Color.FromRgb(ColorByte.BorderColor.R, ColorByte.BorderColor.G, ColorByte.BorderColor.B));
            
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListBoxItem;
            if (item != null && item.Content is Hymn hymn)
            {
                SetHymn(hymn);
            }
        }
    }
}
