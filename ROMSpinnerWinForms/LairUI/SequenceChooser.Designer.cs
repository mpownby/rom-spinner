namespace ROMSpinner.LairUI
{
    partial class SequenceChooser
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.gridPointers = new System.Windows.Forms.DataGridView();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SequenceType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SequenceName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridPointers)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.gridPointers, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(361, 406);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // gridPointers
            // 
            this.gridPointers.AllowUserToAddRows = false;
            this.gridPointers.AllowUserToDeleteRows = false;
            this.gridPointers.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridPointers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridPointers.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gridPointers.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.gridPointers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridPointers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ID,
            this.SequenceType,
            this.SequenceName});
            this.gridPointers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridPointers.Location = new System.Drawing.Point(3, 3);
            this.gridPointers.MultiSelect = false;
            this.gridPointers.Name = "gridPointers";
            this.gridPointers.ReadOnly = true;
            this.gridPointers.RowHeadersVisible = false;
            this.gridPointers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridPointers.Size = new System.Drawing.Size(355, 400);
            this.gridPointers.TabIndex = 1;
            this.gridPointers.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridPointers_RowEnter);
            // 
            // ID
            // 
            this.ID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ID.FillWeight = 30F;
            this.ID.HeaderText = "ID";
            this.ID.Name = "ID";
            this.ID.ReadOnly = true;
            this.ID.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ID.Width = 42;
            // 
            // SequenceType
            // 
            this.SequenceType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.SequenceType.HeaderText = "Sequence Type";
            this.SequenceType.Name = "SequenceType";
            this.SequenceType.ReadOnly = true;
            this.SequenceType.Width = 110;
            // 
            // SequenceName
            // 
            this.SequenceName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.SequenceName.HeaderText = "Sequence Name";
            this.SequenceName.Name = "SequenceName";
            this.SequenceName.ReadOnly = true;
            // 
            // SequenceChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SequenceChooser";
            this.Size = new System.Drawing.Size(361, 406);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridPointers)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView gridPointers;
        private System.Windows.Forms.DataGridViewTextBoxColumn ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn SequenceType;
        private System.Windows.Forms.DataGridViewTextBoxColumn SequenceName;
    }
}
