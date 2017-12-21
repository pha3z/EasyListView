using System;

namespace EasyListView {

    /// <summary>
    /// Event args used in <see cref="EasyListView.DataSourceChanged"/>
    /// </summary>
    public class EasyListViewDataSourceChangedEventArgs : EventArgs {

        /// <summary>
        /// Represents the difference between the previous number of columns to the new one
        /// </summary>
        public int ColumnCountDifference { get; private set; }

        /// <summary>
        /// Represents number of columns of the new datasource
        /// </summary>
        public int ColumnCount { get; private set; }

        /// <summary>
        /// Represents the difference between the previous number of columns to the new one
        /// </summary>
        public int RowCountDifference { get; private set; }

        /// <summary>
        /// Represents number of columns of the new datasource
        /// </summary>
        public int RowCount { get; private set; }

        /// <summary>
        /// Creates an instance of this event argument class implementation
        /// </summary>
        public EasyListViewDataSourceChangedEventArgs(int rowCtDiff, int rowCt, int colCtDiff, int colCt) {
            this.RowCountDifference = rowCtDiff;
            this.RowCount = rowCt;
            this.ColumnCountDifference = colCtDiff;
            this.ColumnCount = colCt;
        }

    }

}
