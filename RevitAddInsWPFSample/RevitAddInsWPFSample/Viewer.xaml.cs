using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
                viewTypenames.Add(view?.ViewType.ToString());
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
                string viewTypename = view?.ViewType.ToString();
                // create view treeviewitem
                if (itemViewName != null)
                {
                    TreeViewItem viewItem = new TreeViewItem() { Header = itemViewName };

                    // view item add to view type
                    viewTypeDictionary[viewTypename].Items.Add(viewItem);
                }
            }
        }
    }
}