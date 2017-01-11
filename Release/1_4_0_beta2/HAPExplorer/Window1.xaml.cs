#region

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using HtmlAgilityPack;
using System.Net;
using System.Windows.Input;

#endregion

namespace HAPExplorer
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        #region Fields

        private Microsoft.Win32.OpenFileDialog _fileDialog = new Microsoft.Win32.OpenFileDialog();
        private HtmlDocument _html = new HtmlDocument();

        #endregion

        #region Constructors

        public Window1()
        {
            InitializeComponent();
            txtHtml.Text = File.ReadAllText("mshome.htm");
            InitializeFileDialog();
        }


        private void InitializeFileDialog()
        {
            _fileDialog.FileName = "Document"; // Default file name
            _fileDialog.DefaultExt = ".html"; // Default file extension
            _fileDialog.Filter = "Text documents (.html,.htm,.aspx)|*.html;*.htm;*.aspx"; // Filter files by extension

        }

        #endregion

        #region Private Methods

        private void btnParse_Click(object sender, RoutedEventArgs e)
        {
            if (txtHtml.Text.IsEmpty()) return;

            _html = new HtmlDocument();
            _html.LoadHtml(txtHtml.Text);

            PopulateTreeview();
        }

        private TreeViewItem BuildTree(HtmlNode htmlNode)
        {
            //Create the main treeview node for this htmlnode
            var item = new TreeViewItem { DataContext = htmlNode }; //preserve reference to _html node for databinding

            //if we have psuedo element, show it's text
            if (htmlNode.NodeType == HtmlNodeType.Text || htmlNode.NodeType == HtmlNodeType.Comment)
                item.Header = string.Format("<{0}> = {1}", htmlNode.OriginalName, htmlNode.InnerText.Trim());
            else
                item.Header = string.Format("<{0}>", htmlNode.OriginalName);

            //Create Attribute collection
            PopulateItem(htmlNode, item);

            return item;
        }

        private void hapTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!(e.NewValue is TreeViewItem)) return;

            var t = e.NewValue as TreeViewItem;

            if (t.DataContext is HtmlNode)
            {
                HtmlAttributeViewer1.Visibility = Visibility.Hidden;
                HtmlNodeViewer1.DataContext = null;
                HtmlNodeViewer1.Visibility = Visibility.Visible;
                HtmlNodeViewer1.DataContext = t.DataContext;
                return;
            }
            if (t.DataContext is HtmlAttribute)
            {
                HtmlNodeViewer1.Visibility = Visibility.Hidden;
                HtmlAttributeViewer1.DataContext = null;
                HtmlAttributeViewer1.Visibility = Visibility.Visible;
                HtmlAttributeViewer1.DataContext = t.DataContext;
                return;
            }
        }

        private void PopulateItem(HtmlNode htmlNode, ItemsControl item)
        {
            var attributes = new TreeViewItem { Header = "Attributes" };
            foreach (var att in htmlNode.Attributes)
                attributes.Items.Add(new TreeViewItem
                                         {
                                             Header = string.Format("{0} = {1}", att.OriginalName, att.Value),
                                             DataContext = att
                                         });
            //If we don't have any attributes, don't add the node
            if (attributes.Items.Count > 0)
                item.Items.Add(attributes);

            //Create the Elements Collection
            var elements = new TreeViewItem { Header = "Elements", DataContext = htmlNode };
            foreach (var node in htmlNode.ChildNodes)
            {
                //If there are no attributes, no need to add a node inbetween the parent in the treeview
                if (attributes.Items.Count > 0)
                    elements.Items.Add(BuildTree(node));
                else
                    item.Items.Add(BuildTree(node));
            }

            //If there are no nodes in the elements collection, don't add to the parent 
            if (elements.Items.Count > 0)
                item.Items.Add(elements);
        }

        private void PopulateTreeview()
        {
            hapTree.Items.Clear();
            //We create the base node here, that way as new nodes are added we can animate them ;)
            var document = new TreeViewItem { Header = "DocumentElement", DataContext = _html.DocumentNode, };
            hapTree.Items.Add(document);
            PopulateItem(_html.DocumentNode, document);
        }

        #endregion

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void mnuOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var result = _fileDialog.ShowDialog();
            if (result != true) return;
            try
            {
                Cursor = Cursors.Wait;
                var file = _fileDialog.FileName;
                txtHtml.Text = File.ReadAllText(file);
            }
            catch (FileNotFoundException fEx)
            {
                MessageBox.Show("Error loading file: " + fEx.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
            catch (FileLoadException fEx)
            {
                MessageBox.Show("Error loading file: " + fEx.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }

        }

        private void mnuOpenUrl_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new UrlDialog();
            if (dialog.ShowDialog() != true) return;
            try
            {
                Cursor = Cursors.Wait;
                var req = (HttpWebRequest)HttpWebRequest.Create(dialog.Url);
                using (var resp = req.GetResponse().GetResponseStream())
                using (var read = new StreamReader(resp))
                {
                    var txt = read.ReadToEnd();
                    txtHtml.Text = txt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void btnTestCode_Click(object sender, RoutedEventArgs e)
        {
            var nodes = _html.DocumentNode.Descendants("input").Count();

        }
    }
}