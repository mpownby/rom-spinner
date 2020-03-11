using System;
using System.Drawing;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Resources;
using ROMSpinner.Common;
using ROMSpinner.Business;
using ROMSpinner.Lair;

namespace ROMSpinner.Win
{
    /// <summary>
    /// Summary description for MainForm.
    /// </summary>
    public class MainForm : System.Windows.Forms.Form
	{
        private Prefs m_prefs = null;
        private ROMManager m_romManager = null;
		private CLair m_lair = null;
        private ResourceManager m_ResourceManager = new ResourceManager("ROMSpinner.Win.Strings",
            Assembly.GetExecutingAssembly());

        private System.Windows.Forms.TreeView treeView1;
        private TabControl tabControl1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem openROMToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private TableLayoutPanel tableLayoutPanel1;
        private Label lblTitle;
        private SplitContainer splitContainer1;
        private TabPage tabPage1;
        private TreeView treeViewFlow;
        private TreeView treeViewDetailed;
        private TabPage tabPage2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MainForm(Prefs p)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(asm.Location);
            this.Text += " v" + info.FileVersion;
            m_prefs = p;
		}

        private void UpdateTree(List<SceneSelection> t, TreeView treeView)
		{
			// add attract mode separately since it isn't part of the scene selection
			TreeNode nodeAttract = new TreeNode("Attract Mode");
			nodeAttract.Tag = (byte) 0xA5;	// the scene index of the attract mode
			treeView.Nodes.Add(nodeAttract);

			TreeNode nodeCat = new TreeNode("Scene Selections");
			for (int i = 0; i < t.Count; i++)
			{
				SceneSelection ss = (SceneSelection) t[i];
				string s = "Scene Selection #" + (i + 0x80).ToString("x");	// add 0x80 because that's how it is represented internally
				s += " - " + ss.Buf.ToString();
				ArrayList lstBranches = ss.GetBranches();

				TreeNode nodeScenes = new TreeNode(s);

				// add some info about the scenes
				ByteArrayAndObj bao = ss.Type;
				s = "Chooser: " + bao.OurObj + " - " + bao.OurByteArray.ToString();
				nodeScenes.Nodes.Add(new TreeNode(s));

				ByteArrayAndUint bau = ss.SceneCount;
				s = "Scene Count: " + bau.OurUint + " - " +
					bau.OurByteArray.ToString();
				nodeScenes.Nodes.Add(new TreeNode(s));

				// add each scene
				for (int idxBranch = 0; idxBranch < lstBranches.Count; idxBranch++)
				{
					ByteArray ba = (ByteArray) lstBranches[idxBranch];
					byte u8Branch = ba.Array[0];

					TreeNode nodeChild = new TreeNode(CLair.SceneIdxToName(u8Branch) + " - " +
						ba.ToString());
					nodeChild.Tag = u8Branch;	// remember this for later
					nodeChild.ForeColor = Color.Blue;
					nodeScenes.Nodes.Add(nodeChild);	// add to the tree
				}
				nodeCat.Nodes.Add(nodeScenes);
			}
			treeView.Nodes.Add(nodeCat);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.treeViewFlow = new System.Windows.Forms.TreeView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.treeViewDetailed = new System.Windows.Forms.TreeView();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.openROMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            resources.ApplyResources(this.treeView1, "treeView1");
            this.treeView1.Name = "treeView1";
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.treeViewFlow);
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // treeViewFlow
            // 
            resources.ApplyResources(this.treeViewFlow, "treeViewFlow");
            this.treeViewFlow.Name = "treeViewFlow";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.treeViewDetailed);
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // treeViewDetailed
            // 
            resources.ApplyResources(this.treeViewDetailed, "treeViewDetailed");
            this.treeViewDetailed.Name = "treeViewDetailed";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.helpToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openROMToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            // 
            // openROMToolStripMenuItem
            // 
            this.openROMToolStripMenuItem.Name = "openROMToolStripMenuItem";
            resources.ApplyResources(this.openROMToolStripMenuItem, "openROMToolStripMenuItem");
            this.openROMToolStripMenuItem.Click += new System.EventHandler(this.openROMToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.lblTitle, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // lblTitle
            // 
            resources.ApplyResources(this.lblTitle, "lblTitle");
            this.lblTitle.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblTitle.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblTitle.Name = "lblTitle";
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
            Prefs p = new Prefs();
            try
            {
                // load prefs before we do any UI.  It's ok if they don't exist.
                p = Dirs.LoadPrefs();
            }
            catch { }

			Application.Run(new MainForm(p));

            // save prefs
            Dirs.SavePrefs(p);
		}

		private void treeView1_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			byte u8Branch = 0;
			
			if (e.Node.Tag != null)
			{
				u8Branch = (byte) e.Node.Tag;
                DoFlowAndDetailed(u8Branch);
			}
			// else they didn't click on anything that we can act upon
		}

        private void DoFlowAndDetailed(byte u8SceneIdx)
        {
            DoFlowScene(u8SceneIdx);
            DoDetailedScene(u8SceneIdx);
        }

        private void DoFlowScene(byte u8SceneIdx)
        {
            treeViewFlow.BeginUpdate();
            treeViewFlow.Nodes.Clear();
            WinTreeView view = new WinTreeView(treeViewFlow);
            LairFlowUI uiFlow = new LairFlowUI(m_lair, u8SceneIdx, view);
            uiFlow.UpdateTreeView();
            treeViewFlow.EndUpdate();
        }

		private void DoDetailedScene(byte u8SceneIdx)
		{
            treeViewDetailed.BeginUpdate();
            treeViewDetailed.Nodes.Clear(); // clear all previous entries
            WinTreeView view = new WinTreeView(treeViewDetailed);
            LairDetailedUI uiDetailed = new LairDetailedUI(m_lair, u8SceneIdx, view);
            uiDetailed.UpdateTreeView();
            treeViewDetailed.EndUpdate();
		}

        private void treeViewFlow_AfterSelect(object sender, TreeViewEventArgs e)
        {
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(m_ResourceManager.GetString("About"), "About ROM Spinner");
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            //string s = "";
            string[] arr = null;
            try
            {
                arr = (string[])e.Data.GetData("FileNameW");
                LoadROM(arr[0]);
            }
            catch
            {
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if ((e.AllowedEffect & DragDropEffects.Copy) != 0)
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void openROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Zipped ROM Images|*.zip";
            DialogResult res = dlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                LoadROM(dlg.FileName);
            }
        }

        private void LoadROM(string strZipFileName)
        {
            /*
            // authentication is required to go passed this point
            if (m_authData == null)
            {
                LoginForm l = new LoginForm(m_prefs);
                l.ShowDialog();
                m_authData = l.AuthData;
            }
             */

            m_romManager = new ROMManager();

            if (m_romManager.LoadROM(strZipFileName))
            {
                lblTitle.Text = m_romManager.RomInstance.Name;
                byte[] arrFileData = m_romManager.RomBuffer;

                m_lair = new CLair(arrFileData);
                List<SceneSelection> t = m_lair.GetSceneSelections();

                // if we're not a Guest
                if (t.Count > 0)
                {
                    UpdateTree(t, treeView1);
                }
                // else we're a guest, so show the one scene that we can show
                else
                {
                    DoFlowAndDetailed(0x83);    // currently it's flaming ropes, so we hard-code that value here
                }
            }
            else
            {
                MessageBox.Show(m_ResourceManager.GetString("UnknownROM"));
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
	}
}
