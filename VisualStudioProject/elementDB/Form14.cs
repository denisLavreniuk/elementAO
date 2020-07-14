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

namespace elementDB
{
    public partial class Form14 : Form
    {
        private Form16 frm;
        private int m_id;
        private string m_filter;
        private bool isCustomerDateChanged = false;
        private bool isStoreHouseDateChanged = false;
        private bool isInvoiceChanged = false;

        public Form14(string filter)
        {
            BackColor = Color.PowderBlue;
            InitializeComponent();

            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, this.dataGridView1, new object[] { true });

            m_filter = filter;

            collectUnits();

            setAccessSettings();
        }

        public void collectUnits()
        {
            dataGridView1.Rows.Clear();

            string sql = string.Format("SELECT u.unit_id, u.unit_num, u.product_code, u.release_date, d.* " +
                "FROM unit_info AS u " +
                "LEFT JOIN unit_dispatches AS d ON " +
                "d.dispatch_id = ( " +
                "SELECT d1.dispatch_id " +
                "FROM unit_dispatches AS d1 " +
                "WHERE u.unit_id = d1.unit_id " +
                "ORDER BY d1.dispatch_id DESC LIMIT 1 " +
                ") " +
                "WHERE product_code like '%{0}%' " +
                "order by u.unit_num;",
                m_filter);

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    dataGridView1.Rows.Add(dr["unit_id"],
                                           dr["unit_num"],
                                           dr["product_code"],
                                           String.Format("{0:yyyy-MM-dd}", dr["release_date"]),
                                           dr["dispatch_id"],
                                           String.Format("{0:yyyy-MM-dd}", dr["date_getting"]),
                                           String.Format("{0:yyyy-MM-dd}", dr["date_sending"]),
                                           dr["invoice"]);
                }
                setRowNumber(dataGridView1);
                dataGridView1.FirstDisplayedScrollingRowIndex = 0;
                dataGridView1.ClearSelection();
                indicateSendings();
            }
        }
        private void setRowNumber(DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                row.HeaderCell.Value = String.Format("{0}", row.Index + 1);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isCustomerDateChanged == false &&
                isStoreHouseDateChanged == false &&
                isInvoiceChanged == false ||
                dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите нужный блок Вам блок и введите " +
                    "данные для редактирования");
                return;
            }

            int id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value);

            bool isSuccessful = false;

            if (isRecordConsists(id))
            {
                isSuccessful = updateRecord(id);
            }
            else
            {
                isSuccessful = insertRecord(id);
            }

            if (isSuccessful)
            {
                dataGridView1.Rows.Clear();
                collectUnits();
            }
            else
            {
                MessageBox.Show("Error");
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            isStoreHouseDateChanged = true;
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            isCustomerDateChanged = true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            isInvoiceChanged = true;
        }

        private void dataGridView1_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (e.Row.Cells[5].Value.ToString() != "")
            {
                dateTimePicker1.Value = Convert.ToDateTime(
                    e.Row.Cells[5].Value.ToString());
            }

            if (e.Row.Cells[6].Value.ToString() != "")
            {
                dateTimePicker2.Value = Convert.ToDateTime(
                    e.Row.Cells[6].Value.ToString());
            }

            textBox1.Text = e.Row.Cells[7].Value.ToString();

            isInvoiceChanged = false;
            isCustomerDateChanged = false;
            isStoreHouseDateChanged = false;
        }

        private bool updateRecord(int id)
        {
            string sql = "UPDATE unit_dispatches SET ";

            if (isStoreHouseDateChanged)
            {
                sql += string.Format("date_getting = '{0}', ",
                    dateTimePicker1.Value.Date.ToString("yyyy-MM-dd"));
            }

            if (isCustomerDateChanged)
            {
                sql += string.Format("date_sending = '{0}', ",
                    dateTimePicker2.Value.Date.ToString("yyyy-MM-dd"));
            }

            if (isInvoiceChanged)
            {
                sql += string.Format("invoice = '{0}', ", textBox1.Text);
            }

            sql = sql.Remove(sql.Length - 2, 2);
            sql += " ";

            sql += string.Format("WHERE unit_id = {0}; \n", id);

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.ToString() != "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool insertRecord(int id)
        {
            string sql = string.Format("INSERT into unit_dispatches " +
                "(unit_id, date_getting, date_sending, invoice)" +
                "VALUES ('{0}', '{1}', '{2}', '{3}')",
                id,
                dateTimePicker1.Value.Date.ToString("yyyy-MM-d"),
                dateTimePicker2.Value.Date.ToString("yyyy-MM-d"),
                textBox1.Text);

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.ToString() != "")
                return false;
            else
                return true;
        }

        private bool isRecordConsists(int id)
        {
            string sql = string.Format("SELECT * FROM unit_dispatches " +
                "WHERE unit_id = {0};", id);

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!checkBox1.Checked && !checkBox2.Checked && !checkBox3.Checked &&
                !checkBox4.Checked && !checkBox5.Checked)
            {
                DialogResult dialogResult = MessageBox.Show(
                    "Не выбран параметр поиска. Обновить список блоков?",
                    "Предупреждение", MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.Yes)
                {
                    collectUnits();
                }
                return;
            }

            string sql = string.Format("SELECT u.unit_id, u.unit_num, u.product_code, u.release_date, d.* " +
                         "FROM unit_info AS u " +
                         "LEFT JOIN unit_dispatches AS d ON " +
                         "d.dispatch_id = ( " +
                         "SELECT d1.dispatch_id " +
                         "FROM unit_dispatches AS d1 " +
                         "WHERE u.unit_id = d1.unit_id " +
                         "ORDER BY d1.dispatch_id DESC LIMIT 1 " +
                         ") " +
                         "WHERE product_code like '%{0}%' " +
                         "AND ", m_filter);

            if (checkBox1.Checked)
            {
                sql += string.Format("u.unit_num BETWEEN '{0}' and '{1}' ",
                    textBox2.Text, textBox3.Text);
            }

            if (checkBox2.Checked)
            {
                if (checkBox1.Checked)
                {
                    sql += "AND ";
                }
                sql += string.Format("u.release_date BETWEEN '{0}' and '{1}' ",
                    dateTimePicker3.Value.Date.ToString("yyyy-MM-dd"),
                    dateTimePicker4.Value.Date.ToString("yyyy-MM-dd"));
            }

            if (checkBox3.Checked)
            {
                if (checkBox1.Checked || checkBox2.Checked)
                {
                    sql += "AND ";
                }
                sql += string.Format("u.unit_id IN " +
                    "(SELECT unit_id FROM unit_dispatches " +
                    "WHERE date_getting BETWEEN '{0}' and '{1}') ",
                    dateTimePicker5.Value.Date.ToString("yyyy-MM-dd"),
                    dateTimePicker6.Value.Date.ToString("yyyy-MM-dd"));
            }

            if (checkBox4.Checked)
            {
                if (checkBox1.Checked || checkBox2.Checked || checkBox3.Checked)
                {
                    sql += "AND ";
                }
                sql += string.Format("u.unit_id IN " +
                  "(SELECT unit_id FROM unit_dispatches " +
                  "WHERE date_sending BETWEEN '{0}' and '{1}') ",
                  dateTimePicker7.Value.Date.ToString("yyyy-MM-dd"),
                  dateTimePicker8.Value.Date.ToString("yyyy-MM-dd"));
            }

            if (checkBox5.Checked)
            {
                if (checkBox1.Checked || checkBox2.Checked || checkBox3.Checked ||
                    checkBox4.Checked)
                {
                    sql += "AND ";
                }
                sql += string.Format("u.unit_id IN " +
                                     "(SELECT unit_id FROM unit_dispatches " +
                                     "WHERE invoice like '%{0}%') ",
                                     textBox4.Text);
            }

            sql += "order by u.unit_num;";

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.Rows.Count > 0)
            {
                dataGridView1.Rows.Clear();
                foreach (DataRow dr in dt.Rows)
                {
                    dataGridView1.Rows.Add(dr["unit_id"],
                                           dr["unit_num"],
                                           dr["product_code"],
                                           String.Format("{0:yyyy-MM-dd}", dr["release_date"]),
                                           dr["dispatch_id"],
                                           String.Format("{0:yyyy-MM-dd}", dr["date_getting"]),
                                           String.Format("{0:yyyy-MM-dd}", dr["date_sending"]),
                                           dr["invoice"]);
                }
                setRowNumber(dataGridView1);
                dataGridView1.ClearSelection();
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
                    dataGridView1.Rows[e.RowIndex1].Cells["unit_num"].Value.ToString(),
                    dataGridView1.Rows[e.RowIndex2].Cells["unit_num"].Value.ToString());
            }
            e.Handled = true;
        }

        private void setAccessSettings()
        {
            switch (Form1.access)
            {
                case users.Peb:
                case users.Control:
                case users.Bras:
                case users.Bgir:
                case users.Bmtd:
                    dateTimePicker1.Enabled = false;
                    dateTimePicker2.Enabled = false;
                    textBox1.Enabled = false;
                    button1.Enabled = false;
                    break;
                case users.Root:
                case users.Storehouse:
                case users.Btk:
                case users.SuperRoot:
                    break;
                default:
                    break;
            }
        }

        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridView1.Rows[e.RowIndex].Selected = true;
                try
                {
                    m_id = Int32.Parse(this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString());
                }
                catch (FormatException er)
                {
                }

                if (frm == null || frm.IsDisposed)
                {
                    frm = new Form16(this, m_id);
                    frm.Show();
                }
                else
                {
                    frm.WindowState = FormWindowState.Normal;
                    frm.Activate();
                    frm.BringToFront();
                }
            }
        }

        private void indicateSendings()
        {
            string sql;
            DataTable dt;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                sql = string.Format("SELECT COUNT(*) FROM unit_dispatches " +
                    "WHERE unit_dispatches.unit_id = {0}", row.Cells[0].Value);

                dt = SQLCustom.SQL_Request(Form1.connection, sql);

                if (Int32.Parse(dt.Rows[0][0].ToString()) > 1)
                {
                    row.Cells["unit_num"].Style.BackColor = Color.Yellow;
                }
            }
        }
    }
}