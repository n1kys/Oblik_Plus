using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace Oblik_
{
    public partial class MainForm : Form
    {
        private static readonly string connectionString = "Server=(localdb)\\mssqllocaldb;Database=MilitaryDB;Trusted_Connection=True;";
        private static readonly SqlConnection connection = new SqlConnection(connectionString);
        private SqlDataAdapter dataAdapter = new SqlDataAdapter();
        private bool isPressed = false;

        public MainForm()
        {
            InitializeComponent();

            sizeChanger();
            dataGridViewSQL.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            tableComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            toolStripAttributeSearch.DropDownStyle = ComboBoxStyle.DropDownList;
            GetTables();
        }

        public void GetData(string selectCommand)
        {
            try
            {
                dataAdapter = new SqlDataAdapter(selectCommand, connection);

                SqlCommandBuilder builder = new SqlCommandBuilder(dataAdapter);

                DataTable table = new DataTable
                {
                    Locale = System.Globalization.CultureInfo.InvariantCulture
                };

                dataAdapter.Fill(table);

                List<string> columns = table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();
                
                toolStripAttributeSearch.ComboBox.DataSource = columns;

                sourceSQL.DataSource = table;

                if (dataGridViewSQL != null)
                {
                    dataGridViewSQL.DataSource = sourceSQL;

                    dataGridViewSQL.AutoResizeColumns(
                        DataGridViewAutoSizeColumnsMode.AllCells);
                    dataGridViewSQL.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells);
                };
            }
            catch (SqlException ex)
            {
                MessageBox.Show(selectCommand + "\n\n" + ex.Message, "SQL-query Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GetTables()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    DataTable tables = connection.GetSchema("Tables", new string[] { null, null, null, "BASE TABLE" });

                    foreach (DataRow row in tables.Rows)
                    {
                        string tableName = (string)row["TABLE_NAME"];
                        tableComboBox.Items.Add(tableName);

                    }

                    tableComboBox.SelectedIndex = 0;

                }
            }
            catch
            {
                MessageBox.Show("Connection is not open", "Connection",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void exitAppToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();

        }

        private void aboutDevelopersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Цей застосунок був розроблений студентом групи 535В, в рамках курсового проєкту\nАвтор: Лебідь Нікіта\nEmail: ---", "Про розробників", 
                MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.FormSize = Size;
            Properties.Settings.Default.Save();
            Application.Exit();
        }

        private void sizeChanger()
        {
            if (Properties.Settings.Default.FormSize != null)
                Size = Properties.Settings.Default.FormSize;
        }

        private void buttonSQL_Click(object sender, EventArgs e)
        {
            GetData(this.searchBoxSQL.Text);
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            dataAdapter.Update((DataTable)sourceSQL.DataSource);
        }

        private void toolStripSearchBox_TextChanged(object sender, EventArgs e)
        {
            string columnName = toolStripAttributeSearch.Text;
            string searchText = toolStripSearchBox.Text;
            bool isNumericSearch = int.TryParse(searchText, out int searchId);

            try
            {
                using (DataView view = new DataView((DataTable)sourceSQL.DataSource))
                {
                    DataColumn column = view.Table.Columns[columnName];
                    if (column == null)
                    {
                        MessageBox.Show($"{columnName} isn't found");
                        return;
                    }

                    if (column.DataType == typeof(int))
                    {
                        if (isNumericSearch)
                        {
                            view.RowFilter = $"{columnName} = {searchId}";
                        }
                        else
                        {
                            view.RowFilter = "1 = 0";
                        }
                    }
                    else
                    {
                        view.RowFilter = $"{columnName} LIKE '%{searchText}%'";
                    }

                    if (view.Count > 0)
                    {
                        DataTable filteredTable = view.ToTable();

                        BindingSource filteredSource = new BindingSource();
                        filteredSource.DataSource = filteredTable;

                        dataGridViewSQL.DataSource = filteredSource;
                    }
                    else
                    {
                        dataGridViewSQL.DataSource = sourceSQL;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void tableComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetData("select * from " + this.tableComboBox.Text);
            searchBoxSQL.Text = "select * from " + this.tableComboBox.Text;
        }

        private void toolStripChangeView_Click(object sender, EventArgs e)
        {
            if (isPressed == false)
            {
                isPressed = true;
                string tableName = this.tableComboBox.Text;
                string viewName = tableName + "View";

                string query = $"SELECT * FROM {viewName}";
                GetData(query);
            }
            else
            {
                isPressed = false;
                string query = searchBoxSQL.Text;
                GetData(query);
            }
        }

        private void searchBoxSQL_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode== Keys.Enter)
            {
                buttonSQL_Click(sender, e);
            }
        }
    }
}
