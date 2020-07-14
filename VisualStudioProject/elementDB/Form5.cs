using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace elementDB
{
    public partial class Form5 : Form
    {
        private int m_id;
        private bool isCancellDateChanged = false;
        private Form2 m_parent;

        public Form5(int id, Form2 parent)
        {
            this.Text = parent.m_unitTitle + " - ПО";
            m_id = id;
            m_parent = parent;
            BackColor = Color.PowderBlue;
            InitializeComponent();

            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            makeRequest(id);

            setAccessSettings();

            populateVersionBox();
        }

        void makeRequest(int id)
        {
            String sql = "SELECT * FROM software WHERE unit_id = " + id.ToString();

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);
            if (dt != null)
            {
                if (dt.Rows.Count != 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        dataGridView1.Rows.Add(dr["sw_id"],
                                               String.Format("{0:yyyy-MM-dd}", dr["implem_date"]),
                                               dr["dev_goal"],
                                               dr["sw_version"],
                                               String.Format("{0:yyyy-MM-dd}", dr["cancell_date"]));
                    }
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
                    isCancellDateChanged = false;
                }
            }
            else
            {
                MessageBox.Show("Ошибка работы с БД!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            String sql = "";
            DataTable dt;

            if (textBox1.Text != "" && comboBox1.Text != "" && dateTimePicker1.Text != "")
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["dev_goal"].Value.ToString().Equals(textBox1.Text) &&
                        row.Cells["sw_version"].Value.ToString().Equals(comboBox1.Text))
                    {
                        MessageBox.Show("Такая запись уже существует");
                        return;
                    }
                }

                sql = String.Format("UPDATE software SET cancell_date = '{0}' WHERE unit_id = {1} ORDER BY sw_id DESC LIMIT 1",
                    dateTimePicker1.Value.Date.ToString("yyyy-MM-dd"),
                    m_id);

                dt = SQLCustom.SQL_Request(Form1.connection, sql);

                sql = "BEGIN;";

                if (isCancellDateChanged)
                {
                    sql += String.Format("INSERT INTO `software` (unit_id, sw_version, dev_goal, implem_date, cancell_date) " +
                        "VALUES ({0}, '{1}', '{2}', '{3}', '{4}');", m_id, comboBox1.Text, textBox1.Text,
                        dateTimePicker1.Value.Date.ToString("yyyy-MM-dd"),
                        dateTimePicker2.Value.Date.ToString("yyyy-MM-dd"));
                }
                else
                {
                    sql += String.Format("INSERT INTO `software` (unit_id, sw_version, dev_goal, implem_date) " +
                        "VALUES ({0}, '{1}', '{2}', '{3}');", m_id, comboBox1.Text, textBox1.Text,
                        dateTimePicker1.Value.Date.ToString("yyyy-MM-dd"));
                }

                sql += string.Format("UPDATE unit_info SET last_update = '{0}' " +
                       "WHERE unit_id = {1};", DateTime.Today.ToString("yyyy-MM-dd"), m_id);

                sql += "COMMIT;";

                dt = SQLCustom.SQL_Request(Form1.connection, sql);
            }
            else
            {
                MessageBox.Show("Заполните поля", "Предупреждение");
                dt = null;
                return;
            }

            if (dt.HasErrors)
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
                  "Добавление ПО", m_parent.m_numCode);

                dt = SQLCustom.SQL_Request(Form1.connection, sql);

                dataGridView1.Rows.Clear();
                makeRequest(m_id);
                m_parent.makeRequest(m_id);
                m_parent.generalDataUpdated();
            }
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

            String sql = "";
            DataTable dt;

            sql = "BEGIN;";

            sql += String.Format("DELETE FROM software WHERE sw_id = '{0}';",
                dataGridView1.SelectedRows[0].Cells[0].Value.ToString());

            sql += string.Format("UPDATE unit_info SET last_update = '{0}' " +
                                 "WHERE unit_id = {1};", DateTime.Today.ToString("yyyy-MM-dd"), m_id);

            sql += "COMMIT;";

            dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.ToString() != "")
            {
                MessageBox.Show("Ошибка БД!");
            }
            else
            {
                if (dataGridView1.SelectedRows[0].Index > 1)
                {
                    sql = string.Format("UPDATE software SET cancell_date = NULL " +
                                        "WHERE sw_id = '{0}'",
                        dataGridView1.Rows[dataGridView1.SelectedRows[0].Index - 1].
                        Cells[0].Value.ToString());

                    dt = SQLCustom.SQL_Request(Form1.connection, sql);
                }

                sql = string.Format("INSERT INTO journal_log " +
                  "(date_time, user, operation, num_code) " +
                  "VALUES('{0}', '{1}', '{2}', '{3}')",
                  DateTime.Now.ToString("yyyy-MM-dd HH:mm"), Form1.userName,
                  "Удаление ПО", m_parent.m_numCode);

                dt = SQLCustom.SQL_Request(Form1.connection, sql);

                dataGridView1.Rows.Clear();
                makeRequest(m_id);
                m_parent.makeRequest(m_id);
                m_parent.generalDataUpdated();

                MessageBox.Show("Запись успешно удалена");
            }
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            isCancellDateChanged = true;
        }

        private void dataGridView1_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (e.Row.Cells[1].Value.ToString() != "")
            {
                dateTimePicker1.Value = Convert.ToDateTime(e.Row.Cells[1].Value.ToString());
            }
            textBox1.Text = e.Row.Cells[2].Value.ToString();
            comboBox1.Text = "";
            comboBox1.SelectedText = e.Row.Cells[3].Value.ToString();
        }

        private void setAccessSettings()
        {
            button1.Enabled = false;

            switch (Form1.access)
            {
                case users.Peb:
                case users.Control:
                case users.Storehouse:
                case users.Bmtd:
                case users.Bras:
                case users.Bgir:
                    button2.Enabled = false;
                    button3.Enabled = false;
                    textBox1.ReadOnly = true;
                    comboBox1.Enabled= false;
                    dateTimePicker1.Enabled = false;
                    dateTimePicker2.Enabled = false;
                    break;
                case users.Btk:
                    break;
                case users.Root:
                case users.SuperRoot:
                    button1.Enabled = true;
                    break;
                default:
                    break;
            }
        }

        private void populateVersionBox()
        {
            if (this.Text.Length < 4)
            {
                return;
            }
            comboBox1.Items.Clear();

            string name = this.Text;

            name = name.Remove(0, name.IndexOf(' ') + 1);
            name = name.Remove(name.IndexOf(' '), name.Length - name.IndexOf(' '));

            bool isBreak = false;
            for (int i = 0; i < Form1.tree.getChildrenCount(); ++i)
            {
                for (int j = 0; j < Form1.tree.getChild(i).getChildrenCount(); ++j)
                {
                    string tmp = Form1.tree.getChild(i).getData() + "-" +
                        Form1.tree.getChild(i).getChild(j).getData();
                    if (name.Equals(tmp))
                    {
                        List<string> softwareList = Form1.tree.getChild(i).
                            getChild(j).getChildrensData();

                        foreach (string software in softwareList)
                        {
                            comboBox1.Items.Add(software);
                        }

                        isBreak = true;
                        break;
                    }
                }
                if (isBreak)
                    break;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string sql = "BEGIN;\n";
            string sqlJournal = "BEGIN; ";
            string version;
            string goal;
            string implemDate;
            string cancelDate;
            DataTable dt;

            string sqlOld = string.Format("Select " +
                "sw_version, dev_goal, implem_date, cancell_date " +
                "FROM software WHERE sw_id = {0}", dataGridView1.SelectedRows[0].Cells[0].Value);

            dt = SQLCustom.SQL_Request(Form1.connection, sqlOld);

            version = dt.Rows[0]["sw_version"].ToString();
            goal = dt.Rows[0]["dev_goal"].ToString();
            implemDate = dt.Rows[0]["implem_date"].ToString();
            cancelDate = dt.Rows[0]["cancell_date"].ToString();

            sql += "UPDATE software SET ";
            int querySize = sql.Length;

            sqlJournal += "INSERT INTO journal_log " +
               "(date_time, user, operation, num_code, property, old_value, new_value) " +
               "VALUES ";

            if (comboBox1.Text != "" && comboBox1.Text != dataGridView1.
                SelectedRows[0].Cells["sw_version"].Value.ToString())
            {
                sql += string.Format("sw_version = '{0}',", comboBox1.Text);

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    m_parent.m_numCode,
                    "Версия ПО",
                    version,
                    comboBox1.Text);
            }

            if (textBox1.Text != "" && textBox1.Text != dataGridView1.
                SelectedRows[0].Cells["dev_goal"].Value.ToString())
            {
                sql += string.Format(" dev_goal = '{0}',", textBox1.Text);

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    m_parent.m_numCode,
                    "Цель (Изменение ПО)",
                    goal,
                    textBox1.Text);
            }

            if (dateTimePicker1.Value.Date.ToString("yyyy-MM-dd") != "" &&
                dateTimePicker1.Value.Date.ToString("yyyy-MM-dd") !=
                dataGridView1.SelectedRows[0].Cells["implem_date"].Value.
                ToString())
            {
                sql += string.Format(" implem_date = '{0}',",
                    dateTimePicker1.Value.Date.ToString("yyyy-MM-dd"));

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                     "'{4}', '{5}', '{6}'), ",
                     DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                     Form1.userName,
                     "Изменение",
                     m_parent.m_numCode,
                     "Дата внедрения (Изменение ПО)",
                     implemDate,
                     dateTimePicker1.Value.Date.ToString("yyyy-MM-dd"));
            }

            if (isCancellDateChanged)
            {
                sql += string.Format(" cancell_date = '{0}',",
                dateTimePicker2.Value.Date.ToString("yyyy-MM-dd"));

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    m_parent.m_numCode,
                    "Дата аннулирования (Изменение ПО)",
                    implemDate,
                    dateTimePicker1.Value.Date.ToString("yyyy-MM-dd"));
            }

            if (sql.Length == querySize)
            {
                MessageBox.Show("Данные не изменены. Введите повторно.");
                return;
            }

            sql = sql.Remove(sql.Length - 1, 1);
            sql += " ";

            sql += string.Format("WHERE sw_id = {0}; \n",
                   Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value));

            sql += string.Format("UPDATE unit_info SET last_update = '{0}' " +
                   "WHERE unit_id = {1};\n", DateTime.Today.ToString("yyyy-MM-dd"), m_id);

            sql += "COMMIT;";

            sqlJournal = sqlJournal.Remove(sqlJournal.Length - 2, 2);
            sqlJournal += ';';

            sqlJournal += "COMMIT;";

            dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.ToString() != "")
            {
                MessageBox.Show("Ошибка БД!");

                MessageBox.Show(dt.ToString());
                MessageBox.Show(sql);
                //MessageBox.Show(sqlJournal);
            }
            else
            {
                MessageBox.Show("Запись успешно обновлена");

                dt = SQLCustom.SQL_Request(Form1.connection, sqlJournal);
                dataGridView1.Rows.Clear();
                makeRequest(m_id);
                m_parent.makeRequest(m_id);
                m_parent.generalDataUpdated();
            }
        }
    }
}
