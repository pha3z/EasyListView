using System.Windows.Forms;
using System.Data;
using System.Drawing;
using System;
using System.Linq;
using System.Threading;

namespace EasyListView {

    /// <summary>
    /// An implementation of ListView with added one functionality
    /// to automatically layout DataTable onto the UI
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    public class EasyListView : ListView {

        /// <summary>
        /// Private field representing the datasource used in this context
        /// </summary>
        private DataTable _dataSource;

        /// <summary>
        /// Private field used to change the style of the column header
        /// </summary>
        private ColumnHeaderAutoResizeStyle _colHeadAutRezStyle;

        /// <summary>
        /// Default border color used on this rectangle
        /// </summary>
        public static Color DEF_BORDER_COLOR = Color.LightGray;

        /// <summary>
        /// Points to the event that is raised whenever the DataSource is changed
        /// </summary>
        public delegate void DataSourceChangedEventHandler(object sender, EasyListViewDataSourceChangedEventArgs e);

        /// <summary>
        /// Event raised whenever the DataSource is changed
        /// </summary>
        public event DataSourceChangedEventHandler DataSourceChanged;

        /// <summary>
        /// Determines whether or not the ListView subscribe to the
        /// sorting feature that is done when user clicks the column header
        /// </summary>
        public bool ClickingColumnHeaderSorts {

            get { return ClickingColumnHeaderSorts; }

            set {

                ClickingColumnHeaderSorts = value;

                if (value) {
                    this.ColumnClick += EasyListView_ColumnClick;
                } else {
                    this.ColumnClick -= EasyListView_ColumnClick;
                }

            }
        }

        /// <summary>
        /// Determines whether the clicking the column 
        /// header will perform an ascending or descending sort
        /// </summary>
        private bool _isDescendingSort { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public EasyListView() {
            this._colHeadAutRezStyle = ColumnHeaderAutoResizeStyle.HeaderSize;
            this._dataSource = null;
        }

        /// <summary>
        /// Performs an ascending / descending sort based on the column clicked
        /// </summary>
        private void EasyListView_ColumnClick(object sender, ColumnClickEventArgs e) {

            if (this.Items.Count == 0) return;

            if (this._dataSource == null) return;

            if (this._dataSource
                .AsEnumerable()
                .Any(r => r[e.Column].Equals(DBNull.Value))) return;

            var newRows = this._isDescendingSort ?

                from row in this._dataSource.AsEnumerable()
                orderby row[e.Column] descending
                select row

                :

                from row in this._dataSource.AsEnumerable()
                orderby row[e.Column] ascending
                select row;


            this._dataSource = newRows.AsDataView().ToTable();

            this.SetDataSource(this._dataSource);

            this._isDescendingSort = !this._isDescendingSort;

        }

        /// <summary>
        /// Gets or Sets the ColumnHeaderAutoResizeStyle
        /// </summary>
        public ColumnHeaderAutoResizeStyle ColumnResizeStyle {
            get {
                return _colHeadAutRezStyle;
            }
            set {
                _colHeadAutRezStyle = value;
            }
        }

        /// <summary>
        /// Iterate through the param DataTable to display its items on this listview
        /// (whole process runs using a separate thread)
        /// </summary>
        /// <param name="dt">The DataTable container of the items needed to be displayed</param>
        public void SetDataSource(DataTable dataSource) {

            if (this.IsDisposed) return;

            if (!this.IsHandleCreated) return;

            Thread t = new Thread(() => {

                try {

                    this.BeginInvoke((MethodInvoker)delegate () {

                        var prevRowCount = (this._dataSource != null) ? this._dataSource.Rows.Count : 0;
                        var prevColCount = (this._dataSource != null) ? this._dataSource.Columns.Count : 0;

                        this.Clear();

                        this.View = View.Details;

                        this.Visible = false;

                        #region START OF ITERATION

                        if (dataSource != null) {

                            this._dataSource = dataSource.Copy() as DataTable;

                            this._dataSource.AsEnumerable().ToList().ForEach(r => {

                                this._dataSource.Columns.Cast<DataColumn>().ToList().ForEach(c => {
                                    if (c.DataType == typeof(DateTime)) {
                                        r[c.ColumnName] = TimeZoneInfo.ConvertTime((r[c.ColumnName] as DateTime?) ?? DateTime.MinValue, TimeZoneInfo.Utc, TimeZoneInfo.Local);
                                    }
                                });

                            });

                            dataSource.Columns.Cast<DataColumn>().ToList().ForEach(c => this.Columns.Add(c.ColumnName));

                            ListViewItem lvi;

                            foreach (DataRow row in dataSource.Rows) {

                                lvi = new ListViewItem();

                                foreach (DataColumn col in dataSource.Columns) {

                                    if (row[col.ColumnName] != null && !Convert.IsDBNull(row[col.ColumnName]) &&
                                        !string.IsNullOrEmpty(Convert.ToString(row[col.ColumnName]))) {
                                        if (col.Ordinal == 0) lvi.Text = row[col.ColumnName].ToString();
                                        else lvi.SubItems.Add(row[col.ColumnName].ToString());
                                    } else {
                                        if (col.Ordinal == 0) lvi.Text = string.Empty;
                                        else lvi.SubItems.Add(string.Empty);
                                    }

                                }

                                this.Items.Add(lvi);
                            }
                        }

                        #endregion

                        this.Visible = true;

                        this.AutoResizeColumns(_colHeadAutRezStyle);

                        this.DataSourceChanged?.Invoke(this,
                                 new EasyListViewDataSourceChangedEventArgs(
                                   prevRowCount - (this._dataSource != null ? this._dataSource.Rows.Count : 0),
                                   this._dataSource != null ? this._dataSource.Rows.Count : 0,
                                   prevColCount - (this._dataSource != null ? this._dataSource.Columns.Count : 0),
                                   this._dataSource != null ? this._dataSource.Columns.Count : 0));

                    });

                } catch {
                    //supress error
                }

            }) { IsBackground = true };

            t.Start();

        }

        /// <summary>
        /// Returns the DataTable that's used on this context for display
        /// </summary>
        /// <returns>The DataTable displayed on this ListView</returns>
        public DataTable GetDataSource() => this._dataSource;

    }


}
