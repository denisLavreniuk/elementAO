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
    public partial class Form10 : Form
    {
        private int m_id;
        Form2 m_parent;

        public Form10(int id, Form2 parent)
        {
            m_parent = parent;
            this.Text = m_parent.m_unitTitle + " - Ремонт";
            InitializeComponent();

            m_id = id;

            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            BackColor = Color.PowderBlue;

            makeRequest(m_id);

            setAccessSettings();
        }

        private void makeRequest(int id)
        {
            String sql = "SELECT * FROM `repairs` WHERE unit_id = " + id.ToString();

            dataGridView1.Rows.Clear();

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    dataGridView1.Rows.Add(dr["repair_id"],
                                           dr["r_number"],
                                           dr["r_type"],
                                           String.Format("{0:yyyy-MM-dd}", dr["r_date"]),
                                           dr["place"],
                                           dr["operating_hours"]);

                    dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
                    dataGridView1.CurrentCell = dataGridView1[1, dataGridView1.Rows.Count - 1];
                }
            }
        }

        private void setAccessSettings()
        {
            switch (Form1.access)
            {
                case users.Peb:
                case users.Control:
                case users.Storehouse:
                case users.Bmtd:
                case users.Bras:
                    button1.Enabled = false;
                    textBox1.ReadOnly = true;
                    textBox2.ReadOnly = true;
                    textBox3.ReadOnly = true;
                    textBox4.ReadOnly = true;
                    dateTimePicker1.Enabled = false;
                    dateTimePicker1.Enabled = false;
                    button2.Enabled = false;
                    break;
                case users.Bgir:
                case users.Btk:
                case users.Root:
                case users.SuperRoot:
                    break;
                default:
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //add
            String sql = "";
            DataTable dt;

            if (textBox1.Text != "" && textBox2.Text != "" && textBox4.Text != "")
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["r_number"].Value.ToString().Equals(textBox1.Text) &&
                        row.Cells["r_type"].Value.ToString().Equals(textBox2.Text) &&
                        row.Cells["place"].Value.ToString().Equals(textBox4.Text) &&
                        row.Cells["operating_hours"].Value.ToString().Equals(textBox3.Text)) 
                    {
                        MessageBox.Show("Такая запись уже существует");
                        return;
                    }
                }

                sql += "BEGIN;";

                sql += String.Format("INSERT INTO `repairs` " +
                    "(unit_id, r_number, r_type, r_date, place, operating_hours)" +
                    " VALUES ({0}, '{1}', '{2}', '{3}', '{4}', '{5}'); ",
                    m_id,
                    textBox1.Text,
                    textBox2.Text, 
                    dateTimePicker1.Value.Date.ToString("yyyy-MM-dd"),
                    textBox4.Text,
                    textBox3.Text);

                sql += string.Format("UPDATE unit_info SET last_update = '{0}' " +
                                     "WHERE unit_id = {1}; ", DateTime.Today.ToString("yyyy-MM-dd"), m_id);
                sql += "COMMIT;";

                dt = SQLCustom.SQL_Request(Form1.connection, sql);
            }
            else
            {
                MessageBox.Show("Заполните поля", "Предупреждение");
                dt = null;
                return;
            }

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
                  "Добавление ремонта", m_parent.m_numCode);

                dt = SQLCustom.SQL_Request(Form1.connection, sql);

                MessageBox.Show("Запись успешно добавлена");
                makeRequest(m_id);
                m_parent.makeRequest(m_id);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //delete
            DialogResult dialogResult = MessageBox.Show(
               "Вы уверены, что хотите удалить выбранную запись?",
               "Предупреждение", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.No)
            {
                return;
            }

            String sql = "";
            DataTable dt;

            sql += "BEGIN;";

            sql = string.Format("DELETE FROM repairs WHERE repair_id = '{0}' ;",
                dataGridView1.SelectedRows[0].Cells[0].Value.ToString());

            sql += string.Format("UPDATE unit_info SET last_update = '{0}' " +
                                 "WHERE unit_id = {1}; ", DateTime.Today.ToString("yyyy-MM-dd"), m_id);

            sql += "COMMIT;";

            dt = SQLCustom.SQL_Request(Form1.connection, sql);

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
                 "Удаление ремонта", m_parent.m_numCode);

                MessageBox.Show("Запись успешно удалена");
                makeRequest(m_id);
                m_parent.makeRequest(m_id);
            }
        }

        private void dataGridView1_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            dateTimePicker1.Value = Convert.ToDateTime(e.Row.Cells[3].Value.ToString());
            textBox1.Text = e.Row.Cells[1].Value.ToString();
            textBox2.Text = e.Row.Cells[2].Value.ToString();
            textBox4.Text = e.Row.Cells[4].Value.ToString();
        }
    }
}
