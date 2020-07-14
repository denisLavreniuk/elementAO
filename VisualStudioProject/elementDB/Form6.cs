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
    public partial class Form6 : Form
    {
        private int _id = -1;
        private bool isCancellDateChanged = false;
        private Form2 m_parent;

        public Form6(int id, Form2 parent)
        {
            m_parent = parent;
            this.Text = m_parent.m_unitTitle + " - Список работ";
            _id = id;
            BackColor = Color.PowderBlue;
            InitializeComponent();

            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            makeRequest(_id);

            setAccessSettings();
        }

        void makeRequest(int id)
        {
            dataGridView1.Rows.Clear();
            String sql = "SELECT * FROM performed_works WHERE unit_id = " + id.ToString();

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);
            if (dt != null)
            {
                if (dt.Rows.Count != 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        dataGridView1.Rows.Add(dr["performed_work_id"],
                                               dr["name"], 
                                               dr["document"], 
                                               dr["description"],
                                               dr["work_type"],
                                               String.Format("{0:yyyy-MM-dd}", dr["start_date"]),
                                               String.Format("{0:yyyy-MM-dd}", dr["finish_date"]));
                    }

                    isCancellDateChanged = false;
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
                    dataGridView1.CurrentCell = dataGridView1[1, dataGridView1.Rows.Count - 1];
                    dataGridView1.Columns[0].Visible = false;
                }
            }
            else
            {
                MessageBox.Show("Ошибка БД!");
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if ((textBox1.Text.Length < 1)||(textBox2.Text.Length < 1)||(comboBox1.SelectedIndex == -1))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (isCancellDateChanged)
                {
                    if (row.Cells["work_type"].Value.ToString().Equals(comboBox1.Text) &&
                        row.Cells["name"].Value.ToString().Equals(textBox1.Text) &&
                        row.Cells["description"].Value.ToString().Equals(textBox2.Text) &&
                        row.Cells["document"].Value.ToString().Equals(textBox3.Text) &&
                        String.Format("{0:yyyy-MM-dd}", row.Cells["start_date"].Value).
                        Equals(String.Format("{0:yyyy-MM-dd}", dateTimePicker1.Value)) &&
                        String.Format("{0:yyyy-MM-dd}", row.Cells["finish_date"].Value).
                        Equals(String.Format("{0:yyyy-MM-dd}", dateTimePicker2.Value)))
                        {
                            MessageBox.Show("Такая запись уже существует");
                            return;
                        }
                }
                else
                {
                    if (row.Cells["work_type"].Value.ToString().Equals(comboBox1.Text) &&
                        row.Cells["name"].Value.ToString().Equals(textBox1.Text) &&
                        row.Cells["description"].Value.ToString().Equals(textBox2.Text) &&
                        row.Cells["document"].Value.ToString().Equals(textBox3.Text) &&
                        String.Format("{0:yyyy-MM-dd}", row.Cells["start_date"].Value).
                        Equals(String.Format("{0:yyyy-MM-dd}", dateTimePicker1.Value)))
                    {
                        MessageBox.Show("Такая запись уже существует");
                        return;
                    }
                }
            }

            String sql = "BEGIN;";

            if (isCancellDateChanged)
            {
                sql += "INSERT into `performed_works` " +
                    "(unit_id, work_type, name, description, document, start_date, finish_date) " +
                    "VALUES (" +
                   _id.ToString() + ", " +
                   (comboBox1.SelectedIndex + 1).ToString() + ",'" +
                   textBox1.Text + "', '" +
                   textBox2.Text + "', '" +
                   textBox3.Text + "', '" +
                   dateTimePicker1.Value.Date.ToString("yyyy-MM-d") + "','" +
                   dateTimePicker2.Value.Date.ToString("yyyy-MM-d") + "');";
            }
            else
            {
                sql += "INSERT into `performed_works` " +
                    "(unit_id, work_type, name, description, document, start_date) " +
                    "VALUES (" +
                   _id.ToString() + ", " +
                   (comboBox1.SelectedIndex + 1).ToString() + ",'" +
                   textBox1.Text + "', '" +
                   textBox2.Text + "', '" +
                   textBox3.Text + "', '" +
                   dateTimePicker1.Value.Date.ToString("yyyy-MM-d") + "');";
            }
            
            sql += string.Format("UPDATE unit_info SET last_update = '{0}' " +
                     "WHERE unit_id = {1};", DateTime.Today.ToString("yyyy-MM-dd"), _id);

            sql += "COMMIT;";

            DataTable dt =  SQLCustom.SQL_Request(Form1.connection, sql);

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
                  "Удаление работы", m_parent.m_numCode);

                dt = SQLCustom.SQL_Request(Form1.connection, sql);

                MessageBox.Show("Запись успешно добавлена");
                makeRequest(_id);
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

            sql += "DELETE FROM `performed_works` WHERE `performed_work_id` = '" +
            dataGridView1.SelectedRows[0].Cells[0].Value.ToString() + "';";

            sql += string.Format("UPDATE unit_info SET last_update = '{0}' " +
                     "WHERE unit_id = {1};", DateTime.Today.ToString("yyyy-MM-dd"), _id);

            sql += "COMMIT;";

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

            sql = string.Format("INSERT INTO journal_log " +
              "(date_time, user, operation, num_code) " +
              "VALUES('{0}', '{1}', '{2}', '{3}')",
              DateTime.Now.ToString("yyyy-MM-dd HH:mm"), Form1.userName,
              "Удаление работы", m_parent.m_numCode);

            dt = SQLCustom.SQL_Request(Form1.connection, sql);

            makeRequest(_id);
        }

        private void DataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            button2.Enabled = true;
        }

        private void dataGridView1_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            try
            {
                textBox1.Text = e.Row.Cells["name"].Value.ToString();
                comboBox1.Text = e.Row.Cells["work_type"].Value.ToString();
                textBox2.Text = e.Row.Cells["description"].Value.ToString();
                textBox3.Text = e.Row.Cells["document"].Value.ToString();
                dateTimePicker1.Value = Convert.ToDateTime(e.Row.Cells["start_date"].Value.ToString());
                if (isCancellDateChanged && e.Row.Cells["finish_date"].Value.ToString() != "")
                    dateTimePicker2.Value = Convert.ToDateTime(e.Row.Cells["finish_date"].Value.ToString());
            }
            catch (System.ArgumentException)
            {
                return;
            }
        }

        private void setAccessSettings()
        {
            switch (Form1.access)
            {
                case users.Peb:
                case users.Control:
                case users.Storehouse:
                case users.Bras:
                case users.Bmtd:
                    button1.Enabled = false;
                    button2.Enabled = false;
                    textBox1.ReadOnly = true;
                    textBox2.ReadOnly = true;
                    textBox3.ReadOnly = true;
                    comboBox1.Enabled = false;
                    dateTimePicker1.Enabled = false;
                    dateTimePicker2.Enabled = false;
                    break;
                case users.Btk:
                case users.Bgir:
                case users.Root:
                case users.SuperRoot:
                    break;
                default:
                    break;
            }
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            isCancellDateChanged = true;
        }
    }
}
