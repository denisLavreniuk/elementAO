using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace elementDB
{
    public partial class Form16 : Form
    {
        private int m_id;
        private Form14 m_parent;
        private string m_unitNum;
        private string m_unitCode;
        public Form16(Form14 parent, int id)
        {
            m_id = id;
            m_parent = parent;
            BackColor = Color.PowderBlue;
            InitializeComponent();

            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            setTitle(m_id);
            makeRequest(id);

            setAccessSettings();
        }

        void makeRequest(int id)
        {
            String sql = string.Format("SELECT * FROM unit_dispatches WHERE unit_id = {0} order by dispatch_id", id);

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);
            if (dt != null)
            {
                if (dt.Rows.Count != 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        dataGridView1.Rows.Add(dr["dispatch_id"],
                                               string.Format("{0:yyyy-MM-dd}", dr["date_getting"]),
                                               string.Format("{0:yyyy-MM-dd}", dr["date_sending"]),
                                               dr["invoice"]);
                    }
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
                }
            }
            else
            {
                MessageBox.Show("Ошибка работы с БД!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && dateTimePicker2.Text != "" && dateTimePicker1.Text != "")
            {
                string sql = string.Format("INSERT into unit_dispatches " +
                    "(unit_id, date_getting, date_sending, invoice)" +
                    "VALUES ('{0}', '{1}', '{2}', '{3}')",
                    m_id,
                    dateTimePicker1.Value.Date.ToString("yyyy-MM-d"),
                    dateTimePicker2.Value.Date.ToString("yyyy-MM-d"),
                    textBox1.Text);

                DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

                if (dt.ToString() == "")
                {
                    MessageBox.Show("Запись успешно добавлена");

                    sql = string.Format("INSERT INTO journal_log " +
                      "(date_time, user, operation, num_code) " +
                      "VALUES('{0}', '{1}', '{2}', '{3}')",
                      DateTime.Now.ToString("yyyy-MM-dd HH:mm"), Form1.userName,
                      "Добавление отправки", m_unitNum + " " + m_unitCode);

                    dt = SQLCustom.SQL_Request(Form1.connection, sql);

                    dataGridView1.Rows.Clear();
                    makeRequest(m_id);
                    m_parent.collectUnits();
                }
            }
            else
            {
                MessageBox.Show("Заполните поля", "Предупреждение");
                return;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
                return;

            DialogResult dialogResult = MessageBox.Show(
                "Вы уверены, что хотите удалить выбранную запись?",
                "Предупреждение", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.No)
            {
                return;
            }

            string sql = String.Format("DELETE FROM unit_dispatches WHERE dispatch_id = '{0}';",
                dataGridView1.SelectedRows[0].Cells[0].Value.ToString());

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.ToString() != "")
            {
                MessageBox.Show("Ошибка БД!");
            }
            else
            {
                sql = string.Format("INSERT INTO journal_log " +
                  "(date_time, user, operation, num_code) " +
                  "VALUES('{0}', '{1}', '{2}', '{3}')",
                  DateTime.Now.ToString("yyyy-MM-dd HH:mm"), Form1.userName,
                  "Удаление отправки", m_unitNum + " " + m_unitCode);

                dt = SQLCustom.SQL_Request(Form1.connection, sql);

                MessageBox.Show("Запись успешно удалена");

                dataGridView1.Rows.Clear();
                makeRequest(m_id);
                m_parent.collectUnits();
            }
        }

        private void dataGridView1_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (e.Row.Cells[1].Value.ToString() != "")
            {
                dateTimePicker1.Value = Convert.ToDateTime(e.Row.Cells[1].Value.ToString());
            }
            if (e.Row.Cells[2].Value.ToString() != "")
            {
                dateTimePicker2.Value = Convert.ToDateTime(e.Row.Cells[2].Value.ToString());
            }
            textBox1.Text = e.Row.Cells[3].Value.ToString();
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
                    button2.Enabled = false;
                    button3.Enabled = false;
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

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
                return;

            string sql = "BEGIN;\n";
            string sqlJournal = "BEGIN; ";
            string getting;
            string sending;
            string invoiceVal;

            DataTable dt;

            string sqlOld = string.Format("Select * FROM unit_dispatches " +
                "WHERE dispatch_id = '{0}';", 
                dataGridView1.SelectedRows[0].Cells["dispatche_id"].Value.ToString());

            DataTable dtOld = SQLCustom.SQL_Request(Form1.connection, sqlOld);

            getting = String.Format("{0:yyyy-MM-dd}", dtOld.Rows[0]["date_getting"]);
            sending = String.Format("{0:yyyy-MM-dd}", dtOld.Rows[0]["date_sending"]);
            invoiceVal = dtOld.Rows[0]["invoice"].ToString();

            sql += "UPDATE unit_dispatches SET ";

            sqlJournal += "INSERT INTO journal_log " +
               "(date_time, user, operation, num_code, property, old_value, new_value) " +
               "VALUES ";

            if (getting != dateTimePicker1.Value.Date.ToString("yyyy-MM-dd"))
            {
                sql += string.Format("date_getting = '{0}', ",
                    dateTimePicker1.Value.Date.ToString("yyyy-MM-dd"));

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    m_unitNum + " " + m_unitCode,
                    "Дата получения",
                    getting,
                    dateTimePicker1.Value.Date.ToString("yyyy-MM-dd"));
            }

            if (sending != dateTimePicker2.Value.Date.ToString("yyyy-MM-dd"))
            {
                sql += string.Format("date_sending = '{0}', ",
                    dateTimePicker2.Value.Date.ToString("yyyy-MM-dd"));

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    m_unitNum + " " + m_unitCode,
                    "Дата отправки",
                    sending,
                    dateTimePicker2.Value.Date.ToString("yyyy-MM-dd"));
            }

            if (invoiceVal != textBox1.Text)
            {
                sql += string.Format("invoice = '{0}', ", textBox1.Text);

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                     "'{4}', '{5}', '{6}'), ",
                     DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                     Form1.userName,
                     "Изменение",
                     m_unitNum + " " + m_unitCode,
                     "Накладная",
                     invoiceVal,
                     textBox1.Text);
            }

            sql = sql.Remove(sql.Length - 2, 2);
            sql += " ";

            sql += string.Format("WHERE dispatch_id = {0}; \n",
                   Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value));

            sql += "COMMIT;";

            sqlJournal = sqlJournal.Remove(sqlJournal.Length - 2, 2);
            sqlJournal += ';';

            sqlJournal += "COMMIT;";

            dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.ToString() != "")
            {
                MessageBox.Show("Ошибка БД!");
            }
            else
            {
                MessageBox.Show("Запись успешно обновлена");

                dt = SQLCustom.SQL_Request(Form1.connection, sqlJournal);

                dataGridView1.Rows.Clear();
                makeRequest(m_id);
                m_parent.collectUnits();
            }
        }

        private void setTitle(int id)
        {
            string sql = "SELECT product_code, unit_num FROM unit_info WHERE unit_id = "
                + id.ToString();

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

            m_unitCode = dt.Rows[0][0].ToString();
            m_unitNum = dt.Rows[0][1].ToString();
            this.Text = m_unitNum + " " + m_unitCode;
        }
    }
}
