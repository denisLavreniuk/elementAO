using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using MySql.Data.MySqlClient;

namespace elementDB
{
    public partial class Form15 : Form
    {
        Form1 m_parent;
        bool m_isBackupCreated = false;
        public Form15(Form1 parent)
        {
            m_parent = parent;

            BackColor = Color.PowderBlue;
            InitializeComponent();

            dateTimePicker1.CustomFormat = "yyyy-MM-dd HH:mm";
            dateTimePicker2.CustomFormat = "yyyy-MM-dd HH:mm";

            typeof(DataGridView).InvokeMember("DoubleBuffered",
                 BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                 null, this.dataGridView1, new object[] { true });

            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            makeRequest();
        }

        public void makeRequest()
        {
            String sql = "SELECT * FROM journal_log";

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows.Count >= 100000)
                {
                    MessageBox.Show("Достигнут лимит записей. Импортиртуйте" +
                        " данные и очистите журнал", "Предупреждение");
                }

                foreach (DataRow row in dt.Rows)
                {
                    dataGridView1.Rows.Add(row["record_id"],
                                           String.Format("{0:yyyy-MM-dd HH:mm}", row["date_time"]),
                                           row["user"],
                                           row["operation"],
                                           row["num_code"],
                                           row["property"],
                                           row["old_value"],
                                           row["new_value"]);
                }
                dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;

                dataGridView1.ClearSelection();
            }
        }

        private void setRowNumber(DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                row.HeaderCell.Value = String.Format("{0}", row.Index + 1);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (!checkBox1.Checked && !checkBox2.Checked && !checkBox3.Checked &&
                !checkBox4.Checked && !checkBox5.Checked)
            {
                DialogResult dialogResult = MessageBox.Show(
                    "Не выбран параметр поиска. Обновить журнал?",
                    "Предупреждение", MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.Yes)
                {
                    dataGridView1.Rows.Clear();
                    makeRequest();
                }
                return;
            }

            string sql = "SELECT * FROM journal_log WHERE ";

            if (checkBox1.Checked)
            {
                sql += string.Format("date_time BETWEEN '{0}' and '{1}' ",
                  dateTimePicker1.Value.Date.ToString("yyyy-MM-dd HH:mm"),                //test and fix format
                  dateTimePicker2.Value.Date.ToString("yyyy-MM-dd HH:mm"));               //test and fix format 
            }
            if (checkBox2.Checked)
            {
                if (checkBox1.Checked)
                {
                    sql += "AND ";
                }
                sql += string.Format("user like '%{0}%' ",
                    textBox1.Text);
            }
            if (checkBox3.Checked)
            {
                if (checkBox1.Checked || checkBox2.Checked)
                {
                    sql += "AND ";
                }
                sql += string.Format("operation like '%{0}%' ",
                    textBox2.Text);
            }
            if (checkBox4.Checked)
            {
                if (checkBox1.Checked || checkBox2.Checked || checkBox3.Checked)
                {
                    sql += "AND ";
                }
                sql += string.Format("num_code like '%{0}%' ",
                    textBox3.Text);
            }
            if (checkBox5.Checked)
            {
                if (checkBox1.Checked || checkBox2.Checked || checkBox3.Checked ||
                    checkBox4.Checked)
                {
                    sql += "AND ";
                }
                sql += string.Format("property like '%{0}%' ",
                    textBox4.Text);
            }

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("Не найдено записей, удовлетворяющих поисковому" +
                    " запросу.", "Уведдомление");
                return;
            }

            dataGridView1.Rows.Clear();
            foreach (DataRow row in dt.Rows)
            {
                dataGridView1.Rows.Add(row["record_id"],
                                       String.Format("{0:yyyy-MM-dd HH:mm}", row["date_time"]),
                                       row["user"],
                                       row["operation"],
                                       row["num_code"],
                                       row["property"],
                                       row["old_value"],
                                       row["new_value"]);
            }
            setRowNumber(dataGridView1);
            //if (dataGridView1.Rows.Count > 0)
            //{
            //    dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;
            //    dataGridView1.ClearSelection();
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(
              "Вы уверены, что хотите удалить выбранную запись?",
              "Предупреждение", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.No)
            {
                return;
            }

            string sql = string.Format("DELETE FROM journal_log WHERE record_id = '{0}' ;",
                dataGridView1.SelectedRows[0].Cells[0].Value.ToString());

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.ToString() != "")
            {
                MessageBox.Show("Ошибка БД!");
            }
            else
            {
                MessageBox.Show("Запись успешно удалена");
                dataGridView1.Rows.Clear();
                makeRequest();
            }
        }

        private void dataGridView1_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.CellValue1.GetType() == typeof(int) &&
                            e.CellValue2.GetType() == typeof(int))
            {
                int value1 = Convert.ToInt32(e.CellValue1);
                int value2 = Convert.ToInt32(e.CellValue2);

                if (value1 > value2)
                {
                    e.SortResult = 1;
                }
                else if (value1 < value2)
                {
                    e.SortResult = -1;
                }
                else if (value1 == value2)
                {
                    e.SortResult = 0;
                }
            }
            else
            {
                e.SortResult = System.String.Compare(e.CellValue1.ToString(),
                    e.CellValue2.ToString());
            }

            if (e.SortResult == 0 && e.Column.Name != "ID")
            {
                e.SortResult = System.String.Compare(
                    dataGridView1.Rows[e.RowIndex1].Cells["record_id"].Value.ToString(),
                    dataGridView1.Rows[e.RowIndex2].Cells["record_id"].Value.ToString());
            }
            e.Handled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            m_parent.SaveToCSV(dataGridView1);
            m_isBackupCreated = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(
             "Вы уверены, что хотите удалить все записи из журнала?",
             "Предупреждение", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                if (m_isBackupCreated)
                {
                    string sql = "DELETE FROM journal_log";

                    DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

                    m_isBackupCreated = false;
                }
                else
                {
                    MessageBox.Show("Перед удалением сохраните все записи");
                }
            }
        }
    }
}
