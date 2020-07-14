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
    public partial class Form7 : Form
    {
        private int m_id = -1;
        private Form2 m_parent;

        public Form7(int id, Form2 parent)
        {
            m_parent = parent;
            this.Text = parent.m_unitTitle + " - История статуса";
            BackColor = Color.PowderBlue;
            InitializeComponent();

            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            makeRequest(id);
            m_id = id;

            setAccessSettings();
        }

        void makeRequest(int id)
        {
            dataGridView1.Rows.Clear();
            String sql = "SELECT * FROM product_status_changes WHERE unit_id = " + id.ToString();

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);
            if (dt != null)
            {
                if (dt.Rows.Count != 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        dataGridView1.Rows.Add(dr["change_id"],
                                               dr["change_name"],
                                               dr["description"],
                                               String.Format("{0:yyyy-MM-dd}", dr["$date"]));
                    }
                    dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
                    dataGridView1.CurrentCell = dataGridView1[1, dataGridView1.Rows.Count - 1];
                }
            }
            else
            {
                MessageBox.Show("Ошибка БД!");
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if ((textBox2.Text.Length < 1) || (comboBox6.Text == "Статус"))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["name"].Value.ToString().Equals(comboBox6.Text) &&
                    row.Cells["description"].Value.ToString().Equals(textBox2.Text))
                {
                    MessageBox.Show("Такая запись уже существует");
                    return;
                }
            }

            String sql = "BEGIN;";
            sql += "INSERT into `product_status_changes` " +
                   "(unit_id, change_name, `$date`, description) VALUES (" +
                   m_id.ToString() + ", '" +
                   comboBox6.Text + "','" +
                   dateTimePicker1.Value.Date.ToString("yyyy-MM-d") + "', '" +
                   textBox2.Text + "');";

            sql += string.Format("UPDATE unit_info SET last_update = '{0}' " +
                                 "WHERE unit_id = {1};", DateTime.Today.ToString("yyyy-MM-dd"), m_id);
            sql += "COMMIT;";

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.HasErrors)
            {
                MessageBox.Show("Error");
            }
            else
            {
                sql = string.Format("INSERT INTO journal_log " +
                  "(date_time, user, operation, num_code) " +
                  "VALUES('{0}', '{1}', '{2}', '{3}')",
                  DateTime.Now.ToString("yyyy-MM-dd HH:mm"), Form1.userName,
                  "Добавление статуса", m_parent.m_numCode);

                dt = SQLCustom.SQL_Request(Form1.connection, sql);

                textBox2.Clear();       //HZ zachem, vozmojno potom udaly
                makeRequest(m_id);
                m_parent.makeRequest(m_id);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(
                "Вы уверены, что хотите удалить выбранную запись?",
                "Предупреждение", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.No)
            {
                return;
            }

            String sql = "BEGIN;";

            sql += "DELETE FROM `product_status_changes` WHERE `change_id` = '" +
            dataGridView1.SelectedRows[0].Cells[0].Value.ToString() + "';";

            sql += string.Format("UPDATE unit_info SET last_update = '{0}' " +
                                 "WHERE unit_id = {1};", DateTime.Today.ToString("yyyy-MM-dd"), m_id);
            sql += "COMMIT;";

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.HasErrors)
            {
                MessageBox.Show("Error");
            }
            else
            {
                sql = string.Format("INSERT INTO journal_log " +
                  "(date_time, user, operation, num_code) " +
                  "VALUES('{0}', '{1}', '{2}', '{3}')",
                  DateTime.Now.ToString("yyyy-MM-dd HH:mm"), Form1.userName,
                  "Удаление статуса", m_parent.m_numCode);

                dt = SQLCustom.SQL_Request(Form1.connection, sql);

                makeRequest(m_id);
                m_parent.makeRequest(m_id);
            }
        }

        private void DataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            button2.Enabled = true;
        }

        private void dataGridView1_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            dateTimePicker1.Value = Convert.ToDateTime(e.Row.Cells[3].Value.ToString());
            comboBox6.Text = e.Row.Cells[1].Value.ToString();
            textBox2.Text = e.Row.Cells[2].Value.ToString();
        }   

        private void setAccessSettings()
        {
            switch (Form1.access)
            {
                case users.Storehouse:
                case users.Bmtd:
                case users.Bras:
                case users.Bgir:
                    button1.Enabled = false;
                    textBox2.ReadOnly = true;
                    comboBox6.Enabled = false;
                    dateTimePicker1.Enabled = false;
                    button2.Enabled = false;
                    break;
                case users.Btk:
                case users.SuperRoot:
                case users.Root:
                    break;
                default:
                    break;
            }
        }
    }
}
