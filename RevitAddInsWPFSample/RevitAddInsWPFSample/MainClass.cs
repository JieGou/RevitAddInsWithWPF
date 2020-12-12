using System;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.Attributes;

namespace RevitAddInsWPFSample
{
    // external application class
    public class MainClass : IExternalApplication
    {
        private Viewer _dockableWindow = null;
        private Document _doc;

        /// <summary>
        /// 插件入口
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        // execute when application open
        public Result OnStartup(UIControlledApplication application)
        {
            #region 注册可停靠窗体

            // dockable window
            Viewer dock = new Viewer();
            _dockableWindow = dock;

            // create a new dockable pane id
            DockablePaneId id = new DockablePaneId(new Guid("{68D44FAC-CF09-46B2-9544-D5A3F809373C}"));
            try
            {
                // register dockable pane
                application.RegisterDockablePane(id, "TwentyTwo Dockable Sample", _dockableWindow as IDockablePaneProvider);

#if DEBUG
                TaskDialog.Show("Info Message", "Dockable window have registered!");
#endif

                // subscribe view activated event
                application.ViewActivated += new EventHandler<ViewActivatedEventArgs>(Application_ViewActivated);
            }
            catch (Exception ex)
            {
                // show error info dialog
                TaskDialog.Show("Info Message", ex.Message);
            }

            #endregion 注册可停靠窗体

            // create a ribbon panel
            RibbonPanel ribbonPanel = application.CreateRibbonPanel(Tab.AddIns, "TwentyTwo Sample");
            // assembly
            Assembly assembly = Assembly.GetExecutingAssembly();
            // assembly path
            string assemblyPath = assembly.Location;

            // Create Show Button
            PushButton showButton = ribbonPanel.AddItem(new PushButtonData("Show Window", "Show", assemblyPath,
                "RevitAddInsWPFSample.Show")) as PushButton;
            // btn tooltip
            showButton.ToolTip = "Show the registered dockable window.";
            // show button icon images
            showButton.LargeImage = GetResourceImage(assembly, "RevitAddInsWPFSample.Resources.show32.png");
            showButton.Image = GetResourceImage(assembly, "RevitAddInsWPFSample.Resources.show16.png");

            // return status
            return Result.Succeeded;
        }

        // execute when application close
        public Result OnShutdown(UIControlledApplication application)
        {
            // return status
            return Result.Succeeded;
        }

        // get embedded images from assembly resources
        public ImageSource GetResourceImage(Assembly assembly, string imageName)
        {
            try
            {
                // bitmap stream to construct bitmap frame
                Stream resource = assembly.GetManifestResourceStream(imageName);
                // return image data
                return BitmapFrame.Create(resource);
            }
            catch
            {
                return null;
            }
        }

        // view activated event
        public void Application_ViewActivated(object sender, ViewActivatedEventArgs e)
        {
            _doc = e.CurrentActiveView.Document;
            // provide ExternalCommandData object to dockable page
            _dockableWindow.CustomInitiator(_doc);
        }
    }

    // external command availability
    public class CommandAvailability : IExternalCommandAvailability
    {
        // interface member method
        public bool IsCommandAvailable(UIApplication app, CategorySet cate)
        {
            // zero doc state
            if (app.ActiveUIDocument == null)
            {
                // disable register btn
                return true;
            }
            // enable register btn
            return false;
        }
    }

    // external command class
    [Transaction(TransactionMode.Manual)]
    public class Show : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // dockable window id
                DockablePaneId id = new DockablePaneId(new Guid("{68D44FAC-CF09-46B2-9544-D5A3F809373C}"));
                DockablePane dockableWindow = commandData.Application.GetDockablePane(id);
                dockableWindow.Show();
            }
            catch (Exception ex)
            {
                // show error info dialog
                TaskDialog.Show("Info Message", ex.Message);
            }
            // return result
            return Result.Succeeded;
        }
    }
}