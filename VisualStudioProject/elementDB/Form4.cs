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
    public partial class Form4 : Form
    {
        private int _id;
        private Form2 m_parent;

        public Form4(int id, Form2 parent)
        {
            m_parent = parent;
            this.Text = m_parent.m_unitTitle + " - Список отказов";
            BackColor = Color.PowderBlue;
            InitializeComponent();
            _id = id;

            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            makeRequest(_id);

            setAccessSettings();
        }

        void makeRequest(int id)
        {
            dataGridView1.Rows.Clear();

            String sql = "SELECT * FROM `failures` WHERE `unit_id` = " + id.ToString();

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);
            if (dt != null)
            {
                if (dt.Rows.Count != 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        dataGridView1.Rows.Add(dr["failure_id"],
                                               String.Format("{0:yyyy-MM-dd}", dr["time_stamp"]),
                                               dr["failure_name"],
                                               dr["description"],
                                               dr["document"]);
                    }
                    dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
                    dataGridView1.CurrentCell = dataGridView1[1, dataGridView1.Rows.Count - 1];
                }
            }
            else
            {
                MessageBox.Show("Ошибка работы с БД!");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tableLayoutPanel1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            String sql = "";
            DataTable dt;

            if (textBox2.Text != "" && textBox3.Text != "" &&
                dateTimePicker1.Text != "" && textBox1.Text != "")
            {
                foreach(DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["failure_name"].Value.ToString().Equals(textBox2.Text) &&
                        row.Cells["description"].Value.ToString().Equals(textBox3.Text) &&
                        row.Cells["document"].Value.ToString().Equals(textBox1.Text))
                    {
                        MessageBox.Show("Такая запись уже существует");
                        return;
                    }
                }
                
                sql += "BEGIN;";

                sql += String.Format("INSERT INTO `failures` " +
                    "(unit_id, time_stamp, failure_name, description, document)" +
                    " VALUES ({0}, '{1}', '{2}', '{3}', '{4}'); ",
                _id, dateTimePicker1.Value.Date.ToString("yyyy-MM-dd"), 
                textBox2.Text, textBox3.Text, textBox1.Text);

                sql += string.Format("UPDATE unit_info SET last_update = '{0}' " +
                                     "WHERE unit_id = {1}; ", DateTime.Today.ToString("yyyy-MM-dd"), _id);

                sql += "COMMIT;";

                dt = SQLCustom.SQL_Request(Form1.connection, sql);
            }
            else
            {
                MessageBox.Show("Заполните поля", "Предупреждение");
                dt = null;
                return;
            }

            if (dt == null)
            {
                MessageBox.Show("Ошибка БД!");
            }
            else
            {
                MessageBox.Show("Запись успешно добавлена");

                sql = string.Format("INSERT INTO journal_log " +
                  "(date_time, user, operation, num_code) " +
                  "VALUES('{0}', '{1}', '{2}', '{3}')",
                  DateTime.Now.ToString("yyyy-MM-dd HH:mm"), Form1.userName,
                  "Добавление отказа", m_parent.m_numCode);

                dt = SQLCustom.SQL_Request(Form1.connection, sql);

                makeRequest(_id);
                m_parent.generalDataUpdated();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
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

            sql = String.Format("DELETE FROM failures WHERE failure_id = '{0}' ;",
                dataGridView1.SelectedRows[0].Cells[0].Value.ToString());

            sql += string.Format("UPDATE unit_info SET last_update = '{0}' ;" +
                                 "WHERE unit_id = {1}; ", DateTime.Today.ToString("yyyy-MM-dd"), _id);

            sql += "COMMIT;";

            dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt == null)
            {
                MessageBox.Show("Ошибка БД!");
            }
            else
            {
                MessageBox.Show("Запись успешно удалена");

                sql = string.Format("INSERT INTO journal_log " +
                  "(date_time, user, operation, num_code) " +
                  "VALUES('{0}', '{1}', '{2}', '{3}')",
                  DateTime.Now.ToString("yyyy-MM-dd HH:mm"), Form1.userName,
                  "Удаление отказа", m_parent.m_numCode);

                dt = SQLCustom.SQL_Request(Form1.connection, sql);

                dataGridView1.Rows.Clear();
                makeRequest(_id);
                m_parent.generalDataUpdated();
            }
        }

        private void dataGridView1_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            dateTimePicker1.Value = Convert.ToDateTime(e.Row.Cells[1].Value.ToString());
            textBox2.Text = e.Row.Cells[2].Value.ToString();
            textBox3.Text = e.Row.Cells[3].Value.ToString();
            textBox1.Text = e.Row.Cells[4].Value.ToString();
        }

        private void setAccessSettings()
        {
            switch (Form1.access)
            {
                case users.Peb:
                case users.Control:
                case users.Bgir:
                case users.Storehouse:
                case users.Bmtd:
                case users.Bras:
                    button2.Enabled = false;
                    textBox1.ReadOnly = true;
                    textBox2.ReadOnly = true;
                    textBox3.ReadOnly = true;
                    dateTimePicker1.Enabled = false;
                    button3.Enabled = false;
                    break;
                case users.Btk:
                case users.Root:
                case users.SuperRoot:
                    break;
                default:
                    break;
            }
        }
    }
}