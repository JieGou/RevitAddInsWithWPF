using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace RevitAddInsWPFSample
{
    /// <summary>
    /// Interaction logic for Viewer.xaml
    /// </summary>
    public partial class Viewer : Page, IDockablePaneProvider
    {
        public Document doc = null;

        // IDockablePaneProvider abstrat method
        public void SetupDockablePane(DockablePaneProviderData data)
        {
            // wpf object with pane's interface
            data.FrameworkElement = this as FrameworkElement;
            // initial state position
            data.InitialState = new DockablePaneState
            {
                DockPosition = DockPosition.Tabbed,
                TabBehind = DockablePanes.BuiltInDockablePanes.ProjectBrowser
            };
        }

        // constructor
        public Viewer()
        {
            InitializeComponent();
        }

        // custom initiator
        public void CustomInitiator(Document doc)
        {
            this.doc = doc;

            // get the current document name
            docName.Text = doc.PathName.ToString().Split('\\').Last();
            // get the active view name
            viewName.Text = doc.ActiveView.Name;
            // call the treeview display method
            DisplayTreeViewItem();
        }

        public void DisplayTreeViewItem()
        {
            // clear items first
            treeview.Items.Clear();

            // viewtypename and treeviewitem dictionary
            SortedDictionary<string, TreeViewItem> viewTypeDictionary = new SortedDictionary<string, TreeViewItem>();
            // viewtypename
            List<string> viewTypenames = new List<string>();

            // collect view type
            List<Element> elements = new FilteredElementCollector(doc).OfClass(typeof(View)).ToList();

            foreach (Element element in elements)
            {
                // view
                View view = element as View;
                // view typename
                //string viewTypeName = view?.ViewType.ToString();

                //直接改为使用参数获取与当前语言相关的名字
                string viewTypeName = view?.get_Parameter(BuiltInParameter.VIEW_TYPE).AsString();

                if (string.IsNullOrEmpty(viewTypeName))
                {
                    continue;
                }
                viewTypenames.Add(viewTypeName);
            }

            // create treeviewitem for viewtype
            foreach (string viewTypename in viewTypenames.Distinct().OrderBy(name => name).ToList())
            {
                // create viewtype treeviewitem
                TreeViewItem viewTypeItem = new TreeViewItem() { Header = viewTypename };
                // store in dict
                viewTypeDictionary[viewTypename] = viewTypeItem;
                // add to treeview
                treeview.Items.Add(viewTypeItem);
            }

            foreach (Element element in elements)
            {
                // view
                View view = element as View;
                // viewname
                string itemViewName = view?.Name;
                // view typename
                //string viewTypeName = view?.ViewType.ToString();
                string viewTypeName = view?.get_Parameter(BuiltInParameter.VIEW_TYPE).AsString();

                if (string.IsNullOrEmpty(viewTypeName))
                {
                    continue;
                }

                // create view treeviewitem
                if (itemViewName != null)
                {
                    TreeViewItem viewItem = new TreeViewItem() { Header = itemViewName };

                    // view item add to view type
                    viewTypeDictionary[viewTypeName].Items.Add(viewItem);
                }
            }
        }

        private void TreeViewItem_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && this.treeview.SelectedItem != null)
            {
                this.Button_Click(sender, e);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TreeViewItem item)
            {
                JumpView(item);
            }

            e.Handled = true;
        }

        private void JumpView(TreeViewItem item)
        {
            var header = item.Header;

            ItemsControl parent = GetSelectedTreeViewItemParent(item);

            TreeViewItem treeitem = parent as TreeViewItem;

            string parantHeader = treeitem?.Header.ToString();

            //跳转到指定视图
            MessageBox.Show($"跳转到指定视图 => {parantHeader + ": " + header}");
        }

        public ItemsControl GetSelectedTreeViewItemParent(TreeViewItem item)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(item);
            while (!(parent is TreeViewItem || parent is TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as ItemsControl;
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //奇怪 这里获取得到的节点树
            if (sender is TreeViewItem item)
            {
                var parantHeader = item.Header;

                var clickedItem = TryGetClickedItem(this.treeview, e);
                var itemHeader = clickedItem.Header;

                //TODO 跳转视图
                MessageBox.Show($"跳转到指定视图 => {parantHeader + ": " + itemHeader}");
            }

            e.Handled = true;
        }

        /// <summary>
        /// 获取双击选择的项
        /// </summary>
        /// <param name="treeView"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private TreeViewItem TryGetClickedItem(TreeView treeView, MouseButtonEventArgs e)
        {
            var hit = e.OriginalSource as DependencyObject;
            while (hit != null && !(hit is TreeViewItem))
            {
                hit = VisualTreeHelper.GetParent(hit);
            }

            return hit as TreeViewItem;
        }
    }
}