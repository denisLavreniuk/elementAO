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
    public partial class Form8 : Form
    {
        private int m_id = -1;
        private Form2 m_parent;

        public Form8(int id, Form2 parent)
        {
            m_parent = parent;
            this.Text = m_parent.m_unitTitle + " - Эксплуатирующие организации";
            m_id = id;
            BackColor = Color.PowderBlue;
            InitializeComponent();

            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            makeRequest(m_id);

            setAccessSettings();
        }

        void makeRequest(int id)
        {
            dataGridView1.Rows.Clear();

            String sql = "SELECT * FROM `exploit_place` WHERE `unit_id` = " + id.ToString();

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);
            if (dt != null)
            {
                if (dt.Rows.Count != 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        dataGridView1.Rows.Add(dr["place_id"],
                                               dr["place_name"],
                                               dr["la_number"],
                                               dr["ad_number"],
                                               dr["ad_side"],
                                               dr["stand_name"]);
                    }
                    dataGridView1.CurrentCell = dataGridView1[1, dataGridView1.Rows.Count - 1];
                    dataGridView1.Columns[0].Visible = false;
                }
            }
            else
            {
                MessageBox.Show("Ошибка работы с БД!");
            }
        }

        private void Button2_Click_1(object sender, EventArgs e)
        {
            if (textBox2.Text == "" && (textBox3.Text == "" || textBox4.Text == "" || comboBox1.SelectedIndex == -1) && textBox1.Text == "")
            {
                MessageBox.Show("Заполните поля!", "Предупреждение");
                return;
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["place_name"].Value.ToString().Equals(textBox2.Text) &&
                    row.Cells["la_number"].Value.ToString().Equals(textBox3.Text) &&
                    row.Cells["ad_number"].Value.ToString().Equals(textBox4.Text) &&
                    row.Cells["ad_side"].Value.ToString().Equals(comboBox1.Text) &&
                    row.Cells["stand_name"].Value.ToString().Equals(textBox1.Text))
                {
                    MessageBox.Show("Такая запись уже существует");
                    return;
                }
            }

            String sql = "BEGIN;";

            sql += String.Format("INSERT INTO `exploit_place` " +
                "(unit_id, place_name, la_number, ad_side, ad_number, stand_name)" +
                " VALUES ({0},' {1}', '{2}', '{3}', '{4}', '{5}'); ",
                m_id.ToString(),
                textBox2.Text,
                textBox3.Text,
                comboBox1.Text,
                textBox4.Text,
                textBox1.Text);

            sql += string.Format("UPDATE unit_info SET last_update = '{0}' " +
                     "WHERE unit_id = {1};", DateTime.Today.ToString("yyyy-MM-dd"), m_id);

            sql += "COMMIT;";

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.ToString() != "")
            {
                MessageBox.Show("Ошибка записи в БД", "Ошибка");
            }
            else
            {
                MessageBox.Show("Запись успешно добавлена в БД");

                sql = string.Format("INSERT INTO journal_log " +
                    "(date_time, user, operation, num_code) " +
                    "VALUES('{0}', '{1}', '{2}', '{3}')",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"), Form1.userName,
                    "Добавление ЭО", m_parent.m_numCode);

                dt = SQLCustom.SQL_Request(Form1.connection, sql);

                makeRequest(m_id);
                m_parent.makeRequest(m_id);
            }
        }

        private void Button3_Click_1(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(
                "Вы уверены, что хотите удалить выбранную запись?",
                "Предупреждение", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.No)
            {
                return;
            }

            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Сначала выберите объект!");
                return;
            }

            String sql = "BEGIN;";

            sql += String.Format("DELETE FROM `exploit_place` WHERE place_id = '{0}';",
                dataGridView1.SelectedRows[0].Cells[0].Value.ToString());

            sql += string.Format("UPDATE unit_info SET last_update = '{0}' " +
                     "WHERE unit_id = {1};", DateTime.Today.ToString("yyyy-MM-dd"), m_id);

            sql += "COMMIT;";

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);
            if (dt == null)
            {
                MessageBox.Show("Ошибка работы с БД", "Ошибка");
            }
            else
            {
                MessageBox.Show("Запись успешно удалена из БД");

                sql = string.Format("INSERT INTO journal_log " +
                  "(date_time, user, operation, num_code) " +
                  "VALUES('{0}', '{1}', '{2}', '{3}')",
                  DateTime.Now.ToString("yyyy-MM-dd HH:mm"), Form1.userName,
                  "Удаление ЭО", m_parent.m_numCode);

                dt = SQLCustom.SQL_Request(Form1.connection, sql);

                makeRequest(m_id);
                m_parent.makeRequest(m_id);
            }
        }

        private void dataGridView1_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (dataGridView1.Rows.Count > 0)
            {
                textBox2.Text = e.Row.Cells["place_name"].Value.ToString();
                textBox3.Text = e.Row.Cells["la_number"].Value.ToString();
                comboBox1.Text = e.Row.Cells["ad_side"].Value.ToString();
                textBox4.Text = e.Row.Cells["ad_number"].Value.ToString();
                textBox1.Text = e.Row.Cells["stand_name"].Value.ToString();
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
                    button2.Enabled = false;
                    button3.Enabled = false;
                    textBox1.ReadOnly = true;
                    textBox2.ReadOnly = true;
                    textBox3.ReadOnly = true;
                    textBox4.ReadOnly = true;
                    comboBox1.Enabled = false;
                    button3.Enabled = false;
                    break;
                case users.Bgir:
                case users.Btk:
                case users.SuperRoot:
                case users.Root:
                    break;
                default:
                    break;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (textBox3.Text != "")
            {
                textBox1.Enabled = false;
            }
            else
            {
                textBox1.Enabled = true;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                textBox3.Enabled = false;
                textBox4.Enabled = false;
                comboBox1.Enabled = false;
            }
            else
            {
                textBox3.Enabled = true;
                textBox4.Enabled = true;
                comboBox1.Enabled = true;
            }
        }
    }
}
