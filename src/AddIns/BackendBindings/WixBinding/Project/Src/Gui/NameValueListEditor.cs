﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision$</version>
// </file>

using ICSharpCode.Core;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ICSharpCode.WixBinding
{
	/// <summary>
	/// Grid view that holds name/value pairs that can be edited.
	/// </summary>
	public class NameValueListEditor : System.Windows.Forms.UserControl
	{
		bool ignoreChanges;
		
		public NameValueListEditor()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			InitStrings();
		}
		
		public event EventHandler ListChanged;
		
		/// <summary>
		/// Loads a list of name-value pairs. Each item is a single pair of the form
		/// "name=value".
		/// </summary>
		/// <param name="list">
		/// A name value pair string of the form 'name1=value1;name2=value2'
		/// </param>
		public void LoadList(string list)
		{
			NameValuePairCollection nameValuePairs = new NameValuePairCollection(list);
			try {
				ignoreChanges = true;
				dataGridView.Rows.Clear();
				foreach (NameValuePair pair in nameValuePairs) {
					dataGridView.Rows.Add(pair.Name, pair.Value);
				}
				Sort();
			} finally {
				ignoreChanges = false;
			}
		}
		
		/// <summary>
		/// Returns a list of name-value pairs.
		/// </summary>
		public string GetList()
		{
			NameValuePairCollection pairs = new NameValuePairCollection();
			for (int i = 0; i < dataGridView.Rows.Count; i++) {
				DataGridViewRow row = dataGridView.Rows[i];
				string name = GetName(row);
				string value = GetValue(row);
				if (name.Length > 0) {
					pairs.Add(new NameValuePair(name, value));
				}
			}
			return pairs.GetList();
		}
			
		protected virtual void OnListChanged(EventArgs e)
		{
			if (ListChanged != null) {
				ListChanged(this, e);
			}
		}
		
		#region Windows Forms Designer generated code
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.dataGridView = new System.Windows.Forms.DataGridView();
			this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.valueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// dataGridView
			// 
			this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
			this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
									this.nameColumn,
									this.valueColumn});
			this.dataGridView.Location = new System.Drawing.Point(0, 0);
			this.dataGridView.Name = "dataGridView";
			this.dataGridView.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.dataGridView.RowHeadersWidth = 26;
			this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			this.dataGridView.ShowEditingIcon = false;
			this.dataGridView.Size = new System.Drawing.Size(686, 376);
			this.dataGridView.TabIndex = 0;
			this.dataGridView.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.DataGridViewRowsAdded);
			this.dataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewCellValueChanged);
			this.dataGridView.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.DataGridViewRowsRemoved);
			// 
			// nameColumn
			// 
			this.nameColumn.HeaderText = "Name";
			this.nameColumn.Name = "nameColumn";
			this.nameColumn.Width = 200;
			// 
			// valueColumn
			// 
			this.valueColumn.HeaderText = "Value";
			this.valueColumn.Name = "valueColumn";
			this.valueColumn.Width = 400;
			// 
			// NameValueListEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.dataGridView);
			this.Name = "NameValueListEditor";
			this.Size = new System.Drawing.Size(686, 376);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.DataGridViewTextBoxColumn valueColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
		private System.Windows.Forms.DataGridView dataGridView;
		#endregion
	
		static string GetName(DataGridViewRow row)
		{
			return GetString(row, 0);
		}
		
		static string GetValue(DataGridViewRow row)
		{
			return GetString(row, 1);
		}
		
		static string GetString(DataGridViewRow row, int cellIndex)
		{
			string cellText = (string)row.Cells[cellIndex].Value;
			if (cellText != null) {
				return cellText.Trim();
			}
			return String.Empty;
		}
		
		/// <summary>
		/// Raises the ListChanged event only if we are not ignoring changes
		/// whilst loading the grid.
		/// </summary>
		void OnListChanged()
		{
			if (!ignoreChanges) {
				OnListChanged(new EventArgs());
			}
		}
		
		void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			OnListChanged();
		}
		
		void DataGridViewRowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
		{
			OnListChanged();
		}
		
		void DataGridViewRowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			OnListChanged();
		}
		
		void Sort()
		{
			dataGridView.Sort(nameColumn, ListSortDirection.Ascending);
		}
		
		void InitStrings()
		{
			nameColumn.HeaderText = StringParser.Parse("${res:ICSharpCode.WixBinding.NameValueListEditor.NameColumn}");
			valueColumn.HeaderText = StringParser.Parse("${res:ICSharpCode.WixBinding.NameValueListEditor.ValueColumn}");
		}
	}
}
