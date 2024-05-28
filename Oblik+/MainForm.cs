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
            GetData("select * from soldier;");
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
                if (this.tableComboBox.Text == "brigade")
                {
                    string query = @"
                SELECT 
                    brigade.id,
                    brigade.brigade_name,
                    brigade.unit_name,
                    CONCAT(location.region_name, ', ', COALESCE(location.city_name, '')) AS dislocation
                FROM 
                    brigade
                LEFT JOIN 
                    location ON brigade.location_id = location.id";
                    GetData(query);
                }
                else if(this.tableComboBox.Text == "sub_unit")
                {
                    string query = @"
                SELECT 
                    sub_unit.id,
                    sub_unit.subunit_category,
                    sub_unit.subunit_type,
                    brigade.brigade_name
                FROM 
                    sub_unit
                LEFT JOIN 
                    brigade ON sub_unit.brigade_id = brigade.id";
                    GetData(query);
                }
                else if(this.tableComboBox.Text == "soldier")
                {
                    string query = @"
                SELECT 
                    soldier.id,
                    soldier.name,
                    soldier.rank,
                    soldier.type,
                    soldier.military_speciality,
                    CONCAT(sub_unit.subunit_category, ', ', sub_unit.subunit_type) AS subunit
                FROM 
                    soldier
                LEFT JOIN 
                    sub_unit ON soldier.subunit_id = sub_unit.id";
                    GetData(query);
                }
                else if (this.tableComboBox.Text == "weapons")
                {
                    string query = @"
                SELECT 
                    weapons.id,
                    weapons.type,
                    weapons.name,
                    CONCAT(sub_unit.subunit_category, ', ', sub_unit.subunit_type) AS subunit
                FROM 
                    weapons
                LEFT JOIN 
                    sub_unit ON weapons.subunit_id = sub_unit.id";
                    GetData(query);
                }
                else if(this.tableComboBox.Text == "personal_info")
                {
                    string query = @"
    SELECT 
        personal_info.id,
        soldier.name AS soldier_name,
        personal_info.date_of_birth,
        personal_info.address,
        personal_info.phone_number,
        personal_info.email
    FROM 
        personal_info
    LEFT JOIN 
        soldier ON personal_info.soldier_id = soldier.id";
                    GetData(query);
                }
                else if(this.tableComboBox.Text == "service_history")
                {
                    string querry = @"SELECT 
    service_history.id,
    soldier.name AS soldier_name,
    CONCAT(sub_unit.subunit_type, ', ', sub_unit.subunit_category) AS subunit,
    service_history.start_date,
    service_history.end_date
FROM 
    service_history 
INNER JOIN 
    soldier ON service_history.soldier_id = soldier.id
INNER JOIN 
    sub_unit ON service_history.subunit_id = sub_unit.id;";
                    GetData(querry);
                }
                else if(this.tableComboBox.Text == "military_vehicle")
                {
                    string querry = @"SELECT
                      military_vehicle.id,
military_vehicle.type,
military_vehicle.model,
CONCAT(sub_unit.subunit_type, ', ', sub_unit.subunit_category) AS subunit
FROM 
    military_vehicle
INNER JOIN 
    sub_unit ON military_vehicle.subunit_id = sub_unit.id;
";
                    GetData(querry);
                }
            }
            else 
            {
                isPressed = false;
                string querry = searchBoxSQL.Text;
                GetData(querry);

            }
            
        }

        /*private void toolStripChangeView_Click(object sender, EventArgs e)
        {
            if (isPressed == false)
            {
                isPressed = true;
                string tableName = this.tableComboBox.Text;
                string viewName = tableName + "View";

                CreateViewIfNotExists(tableName, viewName);
                string query = $"SELECT * FROM {viewName}";
                GetData(query);
            }
            else
            {
                isPressed = false;
                string query = searchBoxSQL.Text;
                GetData(query);
            }
        }*/
    }
}
