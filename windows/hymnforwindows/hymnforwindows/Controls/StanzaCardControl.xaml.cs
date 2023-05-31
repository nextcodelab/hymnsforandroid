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
    /// Interaction logic for StanzaCardControl.xaml
    /// </summary>
    public partial class StanzaCardControl : UserControl
    {
        public StanzaCardControl()
        {
            InitializeComponent();
            var stanza = "What a Friend we have in Jesus,<br/>All our sins and griefs to bear!<br/>What a privilege to carry<br/>Everything to God in prayer!<br/>O what peace we often forfeit,<br/>O what needless pain we bear,<br/>All because we do not carry<br/>Everything to God in prayer!<br/>";
            this.stanzaText.Text = HymnalManager.AlignStanza(stanza);
        }
        public void SetStanza(Stanza stanza)
        {
            this.numberText.Text = stanza.no;
            this.stanzaText.Text = HymnalManager.AlignStanza(stanza.text);
        }

    }
}
