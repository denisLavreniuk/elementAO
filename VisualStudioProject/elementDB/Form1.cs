using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using MySql.Data.MySqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace elementDB
{
    public enum users { Btk = 1, Bras, Bgir, Root, Bmtd, Storehouse, SuperRoot, Peb, Control};

    internal struct LASTINPUTINFO
    {
        public uint cbSize;

        public uint dwTime;
    }

    public partial class Form1 : Form
    {
        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        public static string userName = "";
        public static users access = 0;
        public static MySqlConnection connection;
        public static Node tree;
        Form2 frm;
        Form11 frm11;
        Form14 frm14;
        Form13 frm13;
        Form15 frm15;
        public int id = -1;
        //Timer timer;

        public Form1()
        {
            InitializeComponent();
            string connStr = "Server=192.168.0.37;charset= utf8;Database=element_db;Uid=element_user;password=lohozavr;";
            connection = new MySqlConnection(connStr);

            timer1.Start();

            initTree();

            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, this.dataGridView1, new object[] { true });

            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            BackColor = Color.PowderBlue;
            collectUnits();

            dataGridView1.ReadOnly = false;
            setAccessSettings();
            populateUnitTypesBox();
        }

        public void collectUnits()
        {
            dataGridView1.Rows.Clear();
            string sql;

            if (comboBox9.SelectedIndex == -1 || comboBox9.Items[comboBox9.SelectedIndex].ToString() == "Все изделия")
            {
                sql = "SELECT unit_info.*, deviations.deviation_type " +
                      "FROM unit_info " +
                      "LEFT JOIN deviations " +
                      "ON unit_info.unit_id = deviations.unit_id " +
                      "ORDER BY unit_num;";
            }
            else
            {
                sql = string.Format("SELECT unit_info.*, deviations.deviation_type " +
                    "FROM unit_info " +
                    "LEFT JOIN deviations " +
                    "ON unit_info.unit_id = deviations.unit_id " +
                    "WHERE product_code like '%{0}%' " +
                    "ORDER BY unit_num;", comboBox9.Items[comboBox9.SelectedIndex].ToString());
            }

            DataTable dt = SQLCustom.SQL_Request(connection, sql);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    dataGridView1.Rows.Add(dr["unit_id"],
                                           dr["unit_num"],
                                           dr["notes"],
                                           dr["product_code"],
                                           String.Format("{0:yyyy-MM-dd}", dr["release_date"]),
                                           Form2.GetFirstSW(Convert.ToInt32(dr["unit_id"].ToString())),
                                           Form2.GetLastSW(Convert.ToInt32(dr["unit_id"].ToString())),
                                           dr["deviation_type"],
                                           dr["operating_hours"],
                                           dr["failures_count"],
                                           String.Format("{0:yyyy-MM-dd}", dr["last_update"]));
                }
                setRowNumber(dataGridView1);

                populateDeviationsBox();
                populateSwSortBox();
                populateNotesBox();

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

        //Find Func
        private void Button2_Click(object sender, EventArgs e)
        {
            if (!checkBox1.Checked && !checkBox2.Checked && !checkBox3.Checked &&
                !checkBox4.Checked && !checkBox5.Checked && !checkBox6.Checked)
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

            string sql;

            if (comboBox9.SelectedIndex == -1 || comboBox9.Items[comboBox9.SelectedIndex].ToString() == "Все изделия")
            {
                sql = "SELECT unit_info.*, " +
                   "deviations.deviation_type " +
                   "FROM unit_info " +
                   "LEFT JOIN deviations " +
                   "ON unit_info.unit_id = deviations.unit_id " +
                   "WHERE ";
            }
            else
            {
                sql = string.Format("SELECT unit_info.*, " +
                   "deviations.deviation_type " +
                   "FROM unit_info " +
                   "LEFT JOIN deviations " +
                   "ON unit_info.unit_id = deviations.unit_id " +
                   "WHERE product_code like '%{0}%' AND ",
                   comboBox9.Items[comboBox9.SelectedIndex]);
            }

            if (checkBox1.Checked)
            {
                sql += string.Format("unit_num BETWEEN '{0}' and '{1}' ",
                    textBox1.Text, textBox3.Text);
            }
            if (checkBox2.Checked)
            {
                if (checkBox1.Checked)
                {
                    sql += "AND ";
                }
                sql += string.Format("release_date BETWEEN '{0}' and '{1}' ",
                    dateTimePicker2.Value.Date.ToString("yyyy-MM-dd"),
                    dateTimePicker3.Value.Date.ToString("yyyy-MM-dd"));
            }
            if (checkBox3.Checked)
            {
                if (checkBox1.Checked || checkBox2.Checked)
                {
                    sql += "AND ";
                }
                sql += string.Format("unit_info.unit_id IN " +
                    "(SELECT unit_id FROM software WHERE sw_version = '{0}')",
                comboBox1.Text);
            }
            if (checkBox4.Checked)
            {
                if (checkBox1.Checked || checkBox2.Checked || checkBox3.Checked)
                {
                    sql += "AND ";
                }
                sql += string.Format("operating_hours BETWEEN '{0}' and '{1}'",
                    textBox5.Text, textBox4.Text);
            }
            if (checkBox5.Checked)
            {
                if (checkBox1.Checked || checkBox2.Checked || checkBox3.Checked ||
                    checkBox4.Checked)
                {
                    sql += "AND ";
                }
                sql += string.Format("failures_count BETWEEN '{0}' and '{1}'",
                    textBox7.Text, textBox8.Text);
            }
            if (checkBox6.Checked)
            {
                if (checkBox1.Checked || checkBox2.Checked || checkBox3.Checked ||
                    checkBox4.Checked || checkBox5.Checked)
                {
                    sql += "AND ";
                }
                sql += string.Format("deviation_type = '{0}'",
                    comboBox7.Text);
            }

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.ToString() != "")
            {
                return;
            }

            dataGridView1.Rows.Clear();
            foreach (DataRow dr in dt.Rows)
            {
                dataGridView1.Rows.Add(dr["unit_id"],
                                       dr["unit_num"],
                                       dr["notes"],
                                       dr["product_code"],
                                       String.Format("{0:yyyy-MM-dd}", dr["release_date"]),
                                       Form2.GetFirstSW(Convert.ToInt32(dr["unit_id"].ToString())),
                                       Form2.GetLastSW(Convert.ToInt32(dr["unit_id"].ToString())),
                                       dr["deviation_type"],
                                       dr["operating_hours"],
                                       dr["failures_count"],
                                       String.Format("{0:yyyy-MM-dd}", dr["last_update"]));
            }

            setRowNumber(dataGridView1);
            if (dataGridView1.Rows.Count > 0)
            {
                dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;
                dataGridView1.ClearSelection();
            }
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCombo(comboBox2.SelectedIndex);
        }

        private void UpdateCombo(int comboItemIndex)
        {
            comboBox3.Text = "исполнение";
            comboBox3.Items.Clear();

            foreach (string data in tree.getChild(comboBox2.Items[comboItemIndex].ToString()).getChildrensData())
            {
                comboBox3.Items.Add(data);
            }

            if (comboBox3.SelectedIndex >= 0)
            {
                label4.Text = comboBox2.Text + "-" + comboBox3.Text;
            }
            else
            {
                label4.Text = comboBox2.Text;
            }
        }

        //
        private void ComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox5.Items.Clear();
            foreach (string data in tree.getChild(comboBox2.SelectedIndex)
                .getChild(comboBox3.SelectedIndex).getChildrensData())
            {
                comboBox5.Items.Add(data);
            }

            if (comboBox3.SelectedIndex >= 0)
            {
                label4.Text = comboBox2.Text + "-" + comboBox3.Text;
            }
            else
            {
                label4.Text = comboBox2.Text;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == -1 ||
                comboBox4.SelectedIndex == -1 ||
                comboBox6.SelectedIndex == -1 ||
                textBox2.Text == "" ||
                comboBox5.Text == "")
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            if (dateTimePicker1.Value.Date > DateTime.Today)
            {
                MessageBox.Show("Дата выпуска не должна быть позже текущей даты");
                return;
            }

            string sql = string.Format("SELECT unit_num FROM unit_info " +
                                       "WHERE unit_num = '{0}'", textBox2.Text);

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.Rows.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show(
                "Блок с указанным номером уже существует. " +
                "Создать еще один блок с этим номером",
                "Предупреждение", MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.No)
                {
                    return;
                }
            }

            sql = string.Format("INSERT into unit_info " +
                "(unit_num, release_date, product_code, reception_type," +
                " last_update, notes) " +
                "VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
                textBox2.Text,
                dateTimePicker1.Value.Date.ToString("yyyy-MM-d"),
                label4.Text,
                comboBox4.Text,
                DateTime.Today.ToString("yyyy-MM-dd"),
                comboBox10.Text);

            dt = SQLCustom.SQL_Request(Form1.connection, sql);

            sql = "SELECT LAST_INSERT_ID();";
            dt = SQLCustom.SQL_Request(Form1.connection, sql);

            string last_insert_id = dt.Rows[0]["LAST_INSERT_ID()"].ToString();

            sql = string.Format("INSERT into `software` (unit_id, sw_version, implem_date) " +
                "VALUES ('{0}', '{1}', '{2}');\n",
                last_insert_id,
                comboBox5.Text,
                dateTimePicker1.Value.Date.ToString("yyyy-MM-d"));

            sql += "INSERT into `product_status_changes` (unit_id, `$date`, change_name) VALUES " +
                "('" + last_insert_id + "', '" + dateTimePicker1.Value.Date.ToString("yyyy-MM-d") + "', '" +
                comboBox6.Text + "');";

            if (comboBox8.SelectedText != "Отклонение от КД" || comboBox8.SelectedIndex != -1)
            {
                sql += string.Format("INSERT INTO deviations (unit_id, deviation_type) " +
                    "VALUES ('{0}', '{1}')", last_insert_id, comboBox8.Text);
            }
            dt = SQLCustom.SQL_Request(Form1.connection, sql);

            sql = string.Format("INSERT INTO journal_log " +
                "(date_time, user, operation, num_code) " +
                "VALUES('{0}', '{1}', '{2}', '{3}')",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm"), Form1.userName,
                "Добавление блока", textBox2.Text + label4.Text);

            dt = SQLCustom.SQL_Request(connection, sql);

            MessageBox.Show("Запись успешно добавлена");

            collectUnits();
        }

        private void setAccessSettings()
        {
            switch (Form1.access)
            {
                case users.Storehouse:
                case users.Bgir:
                case users.Bras:
                case users.Peb:
                case users.Control:
                case users.Bmtd:
                    textBox2.Enabled = false;
                    comboBox2.Enabled = false;
                    comboBox3.Enabled = false;
                    comboBox4.Enabled = false;
                    comboBox5.Enabled = false;
                    comboBox6.Enabled = false;
                    comboBox8.Enabled = false;
                    comboBox10.Enabled = false;
                    dateTimePicker1.Enabled = false;
                    button1.Enabled = false;
                    button8.Enabled = false;
                    break;
                case users.Btk:
                    button8.Enabled = false;
                    break;
                case users.Root:
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
                    id = Int32.Parse(this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString());
                }
                catch (FormatException er)
                {
                }

                if (frm == null || frm.IsDisposed)
                {
                    frm = new Form2(id, this);
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

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;

            dataGridView1.ReadOnly = false;
            dgv.BeginEdit(true);
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(
                "Вы уверены, что хотите закрыть программу?",
                "Предупреждение", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                string sql = string.Format("INSERT INTO journal_log " +
                    "(date_time, user, operation) " +
                    "VALUES('{0}', '{1}', '{2}')",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Form1.userName, "Выход");

                DataTable dt = SQLCustom.SQL_Request(connection, sql);

                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        public void SaveToCSV(DataGridView DGV)
        {
            //string separator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            //string separator = ",";
            string separator = ";";
            string filename = "";
            string tmpCell = "";
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV (*.csv)|*.csv";
            sfd.FileName = "Output.csv";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(filename))
                {
                    try
                    {
                        File.Delete(filename);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show("Произошла ошибка при записи данных на диск." + ex.Message);
                    }
                }
                int columnCount = DGV.ColumnCount;
                string columnNames = "";
                string[] output = new string[DGV.RowCount + 1];
                for (int i = 1; i < columnCount; i++)
                {
                    tmpCell = DGV.Columns[i].HeaderText.ToString();
                    if (tmpCell.Contains(separator) || tmpCell.Contains("\""))
                    {
                        tmpCell = tmpCell.Insert(0, "\"");
                        tmpCell = tmpCell.Insert(tmpCell.Length, "\"");
                    }
                    columnNames += tmpCell + separator;
                }
                output[0] += columnNames;
                for (int i = 1; (i - 1) < DGV.RowCount; i++)
                {
                    for (int j = 1; j < columnCount; j++)
                    {
                        tmpCell = DGV.Rows[i - 1].Cells[j].Value.ToString();
                        if (tmpCell.Contains(separator) || tmpCell.Contains("\""))
                        {
                            tmpCell = tmpCell.Insert(0, "\"");
                            tmpCell = tmpCell.Insert(tmpCell.Length, "\"");
                        }
                        output[i] += DGV.Rows[i - 1].Cells[j].Value.ToString() + separator;
                    }
                }
                System.IO.File.WriteAllLines(sfd.FileName, output, System.Text.Encoding.UTF8);
                MessageBox.Show("Файл успешно создан.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveToCSV(dataGridView1);
        }

        private void populateDeviationsBox()
        {
            string sql = "SELECT DISTINCT deviation_type FROM deviations";

            DataTable result = SQLCustom.SQL_Request(Form1.connection, sql);

            comboBox7.Items.Clear();
            comboBox8.Items.Clear();

            foreach (DataRow row in result.Rows)
            {
                comboBox8.Items.Add(row[0]);
                comboBox7.Items.Add(row[0]);
            }
        }

        private void populateNotesBox()
        {
            string sql = "SELECT DISTINCT notes FROM unit_info";

            DataTable result = SQLCustom.SQL_Request(Form1.connection, sql);

            comboBox10.Items.Clear();

            foreach (DataRow row in result.Rows)
            {
                comboBox10.Items.Add(row[0]);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DataGridViewRow[] tmpRowList = new DataGridViewRow[dataGridView1.SelectedRows.Count];
            dataGridView1.SelectedRows.CopyTo(tmpRowList, 0);
            dataGridView1.Rows.Clear();
            dataGridView1.Rows.AddRange(tmpRowList);
            setRowNumber(dataGridView1);
        }

        public void unitDeleted()
        {
            comboBox8.Items.Clear();
            comboBox7.Items.Clear();
            collectUnits();
        }

        public void initTree()
        {
            tree = new Node();

            string connStr = "Server=192.168.0.37;Database=element_templates;Uid=element_user;password=lohozavr;";
            MySqlConnection connection = new MySqlConnection(connStr);

            string sql = "SELECT * FROM unit_names";
            DataTable unitTable = SQLCustom.SQL_Request(connection, sql);

            sql = "SELECT * FROM variant_names";
            DataTable variantTable = SQLCustom.SQL_Request(connection, sql);

            sql = "SELECT * FROM software_names";
            DataTable softwareTable = SQLCustom.SQL_Request(connection, sql);

            DataRow[] variantRows;
            DataRow[] softwareRows;

            if (unitTable.Rows.Count == 0 || variantTable.Rows.Count == 0 ||
                softwareTable.Rows.Count == 0)
            {
                MessageBox.Show(unitTable.ToString());
                MessageBox.Show(variantTable.ToString());
                MessageBox.Show(softwareTable.ToString());
                return;
            }
            foreach (DataRow unitRow in unitTable.Rows)
            {
                tree.addNode(unitRow["name"].ToString());
                variantRows = variantTable.Select("name_id = " + unitRow["name_id"].ToString());

                foreach (DataRow variantRow in variantRows)
                {
                    tree.getLastChild().
                        addNode(variantRow["variant_name"].ToString());

                    softwareRows = softwareTable.Select("variant_id = " +
                        variantRow["variant_id"].ToString());

                    foreach (DataRow softwareRow in softwareRows)
                    {
                        tree.getLastChild().
                             getLastChild().
                             addNode(softwareRow["software_name"].ToString());
                    }
                }
            }
            populateUnitBox();
        }

        private void populateUnitBox()
        {
            comboBox2.Items.Clear();
            if (comboBox9.SelectedIndex == -1 || comboBox9.Items[comboBox9.SelectedIndex].ToString() == "Все изделия")
            {
                foreach (string data in tree.getChildrensData())
                {
                    comboBox2.Items.Add(data);
                }
            }
            else
            {
                foreach (string data in tree.getChildrensData())
                {
                    if (data.Contains(comboBox9.Items[comboBox9.SelectedIndex].
                        ToString()))
                    comboBox2.Items.Add(data);
                }
            }
        }

        private void populateSwSortBox()
        {
            string sql = "SELECT DISTINCT sw_version from software";
            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

            comboBox1.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                comboBox1.Items.Add(row["sw_version"].ToString());
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (frm11 == null || frm11.IsDisposed)
            {
                frm11 = new Form11((comboBox9.SelectedIndex == -1 || comboBox9.Items[comboBox9.SelectedIndex].ToString() == "Все изделия") ? "" : comboBox9.Text);
                frm11.Show();
            }
            else
            {
                frm11.WindowState = FormWindowState.Maximized;
                frm11.Activate();
                frm11.BringToFront();
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

        private void populateUnitTypesBox()
        {
            comboBox9.Items.Clear();

            string connStr = "Server=192.168.0.37;Database=element_templates;Uid=element_user;password=lohozavr;";
            MySqlConnection connection = new MySqlConnection(connStr);

            string sql = "SELECT type_name from unit_types ORDER BY sort_number";

            DataTable dt = SQLCustom.SQL_Request(connection, sql);

            foreach (DataRow dr in dt.Rows)
            {
                comboBox9.Items.Add(dr[0]);
            }

            comboBox9.Items.Add("Все изделия");
        }

        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {
            populateUnitBox();
            collectUnits();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (frm13 == null || frm13.IsDisposed)
            {
                frm13 = new Form13(this);
                frm13.Show();
            }
            else
            {
                frm13.WindowState = FormWindowState.Normal;
                frm13.Activate();
                frm13.BringToFront();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (frm14 == null || frm14.IsDisposed)
            {
                if (comboBox9.SelectedIndex == -1 || comboBox9.Items[comboBox9.SelectedIndex].ToString() == "Все изделия")
                {
                    frm14 = new Form14("");
                }
                else
                {
                    frm14 = new Form14(comboBox9.Items[comboBox9.SelectedIndex].
                        ToString());
                }
                frm14.Show();
            }
            else
            {
                frm14.WindowState = FormWindowState.Normal;
                frm14.Activate();
                frm14.BringToFront();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (frm15 == null || frm15.IsDisposed)
            {
                frm15 = new Form15(this);
                frm15.Show();
            }
            else
            {
                frm15.WindowState = FormWindowState.Normal;
                frm15.Activate();
                frm15.BringToFront();
            }
        }

        public static uint GetIdleTime()
        {
            LASTINPUTINFO LastUserAction = new LASTINPUTINFO();
            LastUserAction.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(LastUserAction);
            GetLastInputInfo(ref LastUserAction);
            return ((uint)Environment.TickCount - LastUserAction.dwTime);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (GetIdleTime() > 900000)
                System.Environment.Exit(0);
        }

        private void ComboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}