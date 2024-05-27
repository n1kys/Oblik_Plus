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

        public MainForm()
        {
            InitializeComponent();

            sizeChanger();
            dataGridViewSQL.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            GetData("select * from soldier;");
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
    }
}
