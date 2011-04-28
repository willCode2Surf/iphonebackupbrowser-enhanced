﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IDevice.IPhone;
using System.IO;
using PList;
using System.Xml;
using NLog;

namespace IDevice.Plugins.Menus
{
    public class FolderContextMenuPlugin : AbstractPlugin
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ToolStripMenuItem open = new ToolStripMenuItem("Open");
        private ToolStripMenuItem export = new ToolStripMenuItem("Export folder");
        private ToolStripSeparator sep1 = new ToolStripSeparator();
        private ToolStripMenuItem properties = new ToolStripMenuItem("Properties");

        public FolderContextMenuPlugin()
        {
            open.Click += new EventHandler(open_Click);
            open.Enabled = false;

            export.Enabled = false;
            export.Click += new EventHandler(export_Click);

            properties.Enabled = false;
            properties.Click += new EventHandler(properties_Click);

        }

        protected override void OnSelectionChanged(object sender, EventArgs e)
        {
            bool enable = SelectedApp != null;
            open.Enabled = enable;
            export.Enabled = enable;
            properties.Enabled = enable;
        }

        void open_Click(object sender, EventArgs e)
        {
            IPhoneApp app = SelectedApp;
            if (app != null)
            {
                Model.Select(app);
            }
        }

        void export_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select a place to export the application data";
            if (dialog.ShowDialog(Model.Window) == DialogResult.OK)
            {
                IPhoneApp app = SelectedApp;
                IPhoneBackup backup = SelectedBackup;
                string path = dialog.SelectedPath;
                Model.Dispatch(app.Files, delegate(IPhoneFile file)
                {
                    string source = Path.Combine(backup.Path, file.Key);
                    string dest = Path.Combine(path, app.Name, file.Path.Replace("/", Path.DirectorySeparatorChar.ToString()));

                    // If there is a folder structur
                    // create it.
                    int lastIndex = file.Path.LastIndexOf("/");
                    if (lastIndex >= 0)
                    {
                        string fileFolder = file.Path.Substring(0, lastIndex);
                        Directory.CreateDirectory(Path.Combine(path, app.Name, fileFolder.Replace("/", Path.DirectorySeparatorChar.ToString())));
                    }

                    // Copy the file (overwrite)
                    File.Copy(source, dest, true);

                    FileInfo info = new FileInfo(dest);
                    if (info.Extension == ".plist")
                    {
                        try
                        {
                            PListRoot root = PListRoot.Load(info.FullName);
                            if (root.Format == PListFormat.Binary)
                            {
                                using (XmlWriter writer = XmlWriter.Create(Path.Combine(info.Directory.FullName, "converted_" + info.Name)))
                                {
                                    root.WriteXml(writer);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorException(ex.Message, ex);
                        }
                    }
                    return true;
                }, Cursors.WaitCursor);
            }
        }

        void properties_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not implemented");
        }

        protected override void OnRegisterMenu(Managers.MenuManager manager)
        {
            manager.Add(Managers.MenuContainer.FolderContext, open);
            manager.Add(Managers.MenuContainer.FolderContext, export);
            manager.Add(Managers.MenuContainer.FolderContext, sep1);
            manager.Add(Managers.MenuContainer.FolderContext, properties);
        }

        protected override void OnUnregisterMenu(Managers.MenuManager manager)
        {
            manager.Remove(Managers.MenuContainer.FolderContext, open);
            manager.Remove(Managers.MenuContainer.FolderContext, export);
            manager.Remove(Managers.MenuContainer.FolderContext, sep1);
            manager.Remove(Managers.MenuContainer.FolderContext, properties);
        }

        public override string Author
        {
            get { return "Isak Karlsson"; }
        }

        public override string Description
        {
            get { return "Give menu items to the folder context menu"; }
        }

        public override string Name
        {
            get { return "FolderContextMenuPlugin"; }
        }

        public override System.Drawing.Icon Icon
        {
            get { return null; }
        }

        public override void Dispose()
        {
            base.Dispose();
            open.Click -= new EventHandler(open_Click);
        }
    }
}