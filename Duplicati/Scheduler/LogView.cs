﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Duplicati.Scheduler
{
    /// <summary>
    /// View log files
    /// </summary>
    public partial class LogView : Form
    {
        /// <summary>
        /// Shows history and log files
        /// </summary>
        /// <param name="aJobName">Job to show</param>
        public LogView(string aJobName)
        {
            InitializeComponent();
            Initialize(aJobName);
        }
        /// <summary>
        /// Set the form for a Job
        /// </summary>
        /// <param name="aJobName">Job</param>
        private void Initialize(string aJobName)
        {
            this.Text = aJobName;
            this.historyDataSet = new Duplicati.Scheduler.Data.HistoryDataSet();
            this.historyDataSet.Load();
            this.historyBindingSource.Filter = "Name = '" + aJobName + "'";
            this.historyBindingSource.DataSource = this.historyDataSet.History;
            this.historyBindingSource.MoveLast();
            this.historyDataGridView.AutoResizeColumns();
            this.sizeOfAddedTextBox.DataBindings["Text"].Format += new ConvertEventHandler(SizeFormat);
            this.sizeOfExaminedTextBox.DataBindings["Text"].Format += new ConvertEventHandler(SizeFormat);
            this.sizeOfModifiedTextBox.DataBindings["Text"].Format += new ConvertEventHandler(SizeFormat);
            this.TimeBeginTextBox.DataBindings["Text"].Format += new ConvertEventHandler(DateFormat);
            this.TimeEndTextBox.DataBindings["Text"].Format += new ConvertEventHandler(DateFormat);
        }

        void DateFormat(object sender, ConvertEventArgs e)
        {
            e.Value = ((DateTime)e.Value).ToString("T");
        }

        void SizeFormat(object sender, ConvertEventArgs e)
        {
            e.Value = Duplicati.Library.Utility.Utility.FormatSizeString((long)e.Value);
        }
        /// <summary>
        /// User selected a different history entry, adjust all
        /// </summary>
        private void historyBindingSource_CurrentChanged(object sender, EventArgs e)
        {
            // Get the new row from the BindingSource
            Duplicati.Scheduler.Data.HistoryDataSet.HistoryRow Row = 
                (Duplicati.Scheduler.Data.HistoryDataSet.HistoryRow)((DataRowView)this.historyBindingSource.Current).Row;
            if (Row.IsLogFileNameNull()) Row.LogFileName = string.Empty;
            string LogFile = Row.LogFileName;
            if (string.IsNullOrEmpty(LogFile) || !System.IO.File.Exists(LogFile))
                this.logListBindingSource.DataSource = null;
            else   // Put the log file into the grid
                this.logListBindingSource.DataSource = new Duplicati.Library.Logging.AppendLog.LogList(
                    System.IO.File.ReadAllLines(LogFile));
        }
        /// <summary>
        /// Returns a 16X16 bitmap of an Icon
        /// </summary>
        private static Image Bitmap16(Icon aIcon)
        {
            Bitmap Result = new Bitmap(16, 16, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (Graphics Gr = Graphics.FromImage(Result))
            {
                Gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                Gr.DrawImage(aIcon.ToBitmap(), 0, 0, 16, 16);
            }
            return Result;
        }
        /// <summary>
        /// Correlates log message type to an Icon
        /// </summary>
        private Dictionary<Duplicati.Library.Logging.LogMessageType, Image> TypeIcons = new Dictionary<Duplicati.Library.Logging.LogMessageType, Image>()
        {
            { Duplicati.Library.Logging.LogMessageType.Error, Bitmap16(System.Drawing.SystemIcons.Error) },
            { Duplicati.Library.Logging.LogMessageType.Information, Bitmap16(System.Drawing.SystemIcons.Information) },
            { Duplicati.Library.Logging.LogMessageType.Profiling, Bitmap16(System.Drawing.SystemIcons.Question) },
            { Duplicati.Library.Logging.LogMessageType.Warning, Bitmap16(System.Drawing.SystemIcons.Warning) },
        };
        /// <summary>
        /// Draws the icons depending on the log message type
        /// </summary>
        private void logListDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == this.TypeCol.Index)
            {
                e.Value = TypeIcons[(Duplicati.Library.Logging.LogMessageType)e.Value];
                e.FormattingApplied = true;
            }
        }
    }

}
