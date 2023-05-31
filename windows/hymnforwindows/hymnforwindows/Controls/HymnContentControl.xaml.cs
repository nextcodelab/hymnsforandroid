using HymnLibrary;
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

namespace hymnforwindows.Controls
{
    /// <summary>
    /// Interaction logic for HymnContentControl.xaml
    /// </summary>
    public partial class HymnContentControl : UserControl
    {

        public HymnContentControl()
        {
            InitializeComponent();
        }
        public void SetHymn(object hymnal)
        {
            if(hymnal is Hymn hymn)
            {
                var stanzas = HymnalManager.GetHymnStanzaById(hymn._id);
                foreach(var stanza in stanzas)
                {
                    StanzaCardControl stanzaCardControl = new StanzaCardControl();
                    stanzaCardControl.SetStanza(stanza);
                    this.stanzaPanel.Children.Add(stanzaCardControl);
                }
            }
        }
    }
}
