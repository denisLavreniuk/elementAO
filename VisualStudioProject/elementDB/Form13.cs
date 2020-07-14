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
    public partial class Form13 : Form
    {
        private Form1 m_parent;
        private MySqlConnection m_connection;
        private int m_type = 0;
        private int m_typeId = -1;
        private int m_variant = 0;
        private int m_variantId = -1;
        private int m_software = 0;

        public Form13(Form parent)
        {
            m_parent = parent as Form1;

            BackColor = Color.PowderBlue;

            InitializeComponent();

            string connStr = "Server=192.168.0.37;charset= utf8;Database=element_templates;Uid=element_user;password=lohozavr;";
            m_connection = new MySqlConnection(connStr);

            setColumns();
            getDataFromTree();

            setAccessSettings();
        }

        private void getDataFromTree()
        {
            listView1.View = View.Details;

            listView1.Items.Clear();

            foreach (string item in Form1.tree.getChildrensData())
            {
                listView1.Items.Add(item);
            }
        }

        private void setColumns()
        {
            listView1.View = View.Details;
            listView2.View = View.Details;
            listView3.View = View.Details;

            listView1.Columns.Add("Тип", listView1.Width - 4);
            listView2.Columns.Add("Исполнение", listView1.Width);
            listView3.Columns.Add("Верися ПО", listView1.Width);
        }

        private void resizeCloumns()
        {
            listView1.Columns[0].Width = listView1.Width - 4;
            listView2.Columns[0].Width = listView2.Width - 4;
            listView3.Columns[0].Width = listView3.Width - 4;
        }

        private void Form13_Resize(object sender, EventArgs e)
        {
            resizeCloumns();
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            m_type = e.ItemIndex;
            textBox1.Text = e.Item.Text;
            textBox2.Text = "";
            textBox3.Text = "";

            listView2.Items.Clear();
            listView3.Items.Clear();

            foreach (string item in Form1.tree.getChild(e.ItemIndex).getChildrensData())
            {
                listView2.Items.Add(item);
            }
        }

        private void listView2_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            m_variant = e.ItemIndex; 
            textBox2.Text = e.Item.Text;
            textBox3.Text = "";

            listView3.Items.Clear();

            foreach (string item in Form1.tree.getChild(m_type).
                getChild(e.ItemIndex).getChildrensData())
            {
                listView3.Items.Add(item);
            }
        }

        private void listView3_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            m_software = e.ItemIndex;
            textBox3.Text = e.Item.Text;
        }

        //add button 1
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Заполните поле");
                return;
            }
            if (isRecordConsists("unit_names", "name", textBox1.Text))
            {
                DialogResult dialogResult = MessageBox.Show(
                    "Такая запись уже существует. Продолжить добавление?",
                    "Предупреждение", MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.No)
                {
                    return;
                }
            }

            string sql = string.Format("INSERT INTO unit_names (name) " +
                "VALUES ('{0}');", textBox1.Text);

            DataTable dt = SQLCustom.SQL_Request(m_connection, sql);

            if (dt.ToString() != "")
            {
                MessageBox.Show("Ошибка записи в БД");
            }
            else
            {
                m_parent.initTree();
                getDataFromTree();
                listView2.Items.Clear();
                listView3.Items.Clear();
                MessageBox.Show("Запись успешно добавлена");
            }
        }

        //add button 2
        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                MessageBox.Show("Заполните поле");
                return;
            }

            string sql = string.Format("INSERT INTO variant_names " +
                "(name_id, variant_name) " +
                "SELECT unit_names.name_id, '{0}' FROM unit_names " +
                "WHERE name = '{1}';", textBox2.Text, textBox1.Text);

            DataTable dt = SQLCustom.SQL_Request(m_connection, sql);

            if (dt.ToString() != "")
            {
                MessageBox.Show("Ошибка записи в БД");
            }
            else
            {
                m_parent.initTree();
                getDataFromTree();
                listView1.Items[m_type].Selected = true;
                listView3.Items.Clear();
                MessageBox.Show("Запись успешно добавлена");
            }
        }

        //add button 3
        private void button7_Click(object sender, EventArgs e)
        {
            if (textBox3.Text == "")
            {
                MessageBox.Show("Заполните поле");
                return;
            }

            string sql = string.Format("SELECT variant_id FROM variant_names " +
                "WHERE variant_name = '{0}' AND " +
                "name_id IN (SELECT name_id FROM unit_names WHERE name = '{1}')",
                listView2.Items[m_variant].Text,
                listView1.Items[m_type].Text);

            DataTable dt = SQLCustom.SQL_Request(m_connection, sql);

            if (dt.Rows.Count > 0)
            {
                m_variantId = Convert.ToInt32(dt.Rows[0][0]);
            }
            else
            {
                MessageBox.Show("error");
                return;
            }

            sql = string.Format("INSERT INTO software_names " +
                "(variant_id, software_name)  VALUES({0}, '{1}')",
                m_variantId, textBox3.Text);

            dt = SQLCustom.SQL_Request(m_connection, sql);

            if (dt.ToString() != "")
            {
                MessageBox.Show("Ошибка записи в БД");
            }
            else
            {
                m_parent.initTree();
                getDataFromTree();

                if (listView2.Items[m_variant].Selected == true)
                {
                    listView2.Items[m_variant].Selected = false;
                }
                else
                {
                    listView2.Items[m_variant].Selected = true;
                }
                
                MessageBox.Show("Запись успешно добавлена");
            }
        }

        //update button 1
        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Заполните поле");
                return;
            }

            if (isRecordConsists("unit_names", "name", textBox1.Text))
            {
                DialogResult dialogResult = MessageBox.Show(
                    "Такая запись уже существует. Обновить обе записи?",
                    "Предупреждение", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.No)
                {
                    return;
                }
            }

            string sql = string.Format("UPDATE unit_names SET name = '{0}' " +
                "WHERE name = '{1}'", textBox1.Text,
                listView1.Items[m_type].Text);

            DataTable dt = SQLCustom.SQL_Request(m_connection, sql);

            if (dt.ToString() != "")
            {
                MessageBox.Show("Ошибка записи в БД");
            }
            else
            {
                m_parent.initTree();
                getDataFromTree();
                MessageBox.Show("Запись успешно обновлена");
            }
        }

        //update button 2
        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                MessageBox.Show("Заполните поле");
                return;
            }

            string sql = string.Format("SELECT name_id FROM unit_names " +
                "WHERE name = '{0}'; ", listView1.Items[m_type].Text);

            DataTable dt = SQLCustom.SQL_Request(m_connection, sql);

            if (dt.Rows.Count > 0)
            {
                m_typeId = Convert.ToInt32(dt.Rows[0][0]);
            }
            else
            {
                MessageBox.Show("error");
                return;
            }

            sql = string.Format("UPDATE variant_names SET " +
               "variant_name = '{0}' " +
               "WHERE variant_name = '{1}' AND name_id = {2}; ",
               textBox2.Text,
               listView2.Items[m_variant].Text,
               m_typeId);

            dt = SQLCustom.SQL_Request(m_connection, sql);

            if (dt.ToString() != "")
            {
                MessageBox.Show("Ошибка записи в БД");
            }
            else
            {
                m_parent.initTree();
                getDataFromTree();
                listView1.Items[m_type].Selected = true;
                MessageBox.Show("Запись успешно обновлена");
            }
        }

        //update button 3
        private void button8_Click(object sender, EventArgs e)
        {
            if (textBox3.Text == "")
            {
                MessageBox.Show("Заполните поле");
                return;
            }

            string sql = string.Format("SELECT variant_id FROM variant_names " +
                "WHERE variant_name = '{0}' AND " +
                "name_id IN (SELECT name_id FROM unit_names WHERE name = '{1}')",
                listView2.Items[m_variant].Text,
                listView1.Items[m_type].Text);

            DataTable dt = SQLCustom.SQL_Request(m_connection, sql);

            if (dt.Rows.Count > 0)
            {
                m_variantId = Convert.ToInt32(dt.Rows[0][0]);
            }
            else
            {
                MessageBox.Show("error");
                return;
            }

            sql = string.Format("UPDATE software_names SET " +
                "software_name = '{0}' " +
                "WHERE software_name = '{1}' and variant_id = {2}",
                textBox3.Text,
                listView3.Items[m_software].Text,
                m_variantId);

            dt = SQLCustom.SQL_Request(m_connection, sql);

            if (dt.ToString() != "")
            {
                MessageBox.Show("Ошибка записи в БД");
            }
            else
            {
                m_parent.initTree();
                getDataFromTree();

                if (listView2.Items[m_variant].Selected == true)
                {
                    listView2.Items[m_variant].Selected = false;
                }
                else
                {
                    listView2.Items[m_variant].Selected = true;
                }

                MessageBox.Show("Запись успешно обновлена");
            }
        }

        //delete button 1
        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(
                    "Вы действительно хотите удалить эту запись?",
                    "Предупреждение", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.No)
            {
                return;
            }

            string sql = string.Format("DELETE FROM unit_names " +
                "WHERE name = '{0}';", listView1.Items[m_type].Text);

            DataTable dt = SQLCustom.SQL_Request(m_connection, sql);

            if (dt.ToString() != "")
            {
                MessageBox.Show("Ошибка удаления");
            }
            else
            {
                m_parent.initTree();
                getDataFromTree();
                listView2.Items.Clear();
                listView3.Items.Clear();
                MessageBox.Show("Удаление выполнено успешно");
            }
        }

        //delete button 2
        private void button6_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(
                    "Вы действительно хотите удалить эту запись?",
                    "Предупреждение", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.No)
            {
                return;
            }

            string sql = string.Format("SELECT name_id FROM unit_names " +
                "WHERE name = '{0}'; ", listView1.Items[m_type].Text);

            DataTable dt = SQLCustom.SQL_Request(m_connection, sql);

            if (dt.Rows.Count > 0)
            {
                m_typeId = Convert.ToInt32(dt.Rows[0][0]);
            }
            else
            {
                MessageBox.Show("error");
                return;
            }

            sql = string.Format("DELETE FROM variant_names " +
                "WHERE variant_name = '{0}' AND name_id = {1}; ",
                listView2.Items[m_variant].Text,
                m_typeId);

            dt = SQLCustom.SQL_Request(m_connection, sql);

            if (dt.ToString() != "")
            {
                MessageBox.Show("Ошибка удаления");
            }
            else
            {
                m_parent.initTree();
                getDataFromTree();
                listView1.Items[m_type].Selected = true;
                listView3.Items.Clear();
                MessageBox.Show("Удаление выполнено успешно");
            }
        }

        //delete button 3
        private void button9_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(
                "Вы действительно хотите удалить эту запись?",
                "Предупреждение", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.No)
            {
                return;
            }

            string sql = string.Format("SELECT variant_id FROM variant_names " +
                "WHERE variant_name = '{0}' AND " +
                "name_id IN (SELECT name_id FROM unit_names WHERE name = '{1}')",
                listView2.Items[m_variant].Text,
                listView1.Items[m_type].Text);

            DataTable dt = SQLCustom.SQL_Request(m_connection, sql);

            if (dt.Rows.Count > 0)
            {
                m_variantId = Convert.ToInt32(dt.Rows[0][0]);
            }
            else
            {
                MessageBox.Show("error");
                return;
            }

            sql = string.Format("DELETE FROM software_names " +
               "WHERE software_name = '{0}' and variant_id = {1}",
               listView3.Items[m_software].Text,
               m_variantId);

            dt = SQLCustom.SQL_Request(m_connection, sql);

            if (dt.ToString() != "")
            {
                MessageBox.Show("Ошибка удаления");
            }
            else
            {
                m_parent.initTree();
                getDataFromTree();

                if (listView2.Items[m_variant].Selected == true)
                {
                    listView2.Items[m_variant].Selected = false;
                }
                else
                {
                    listView2.Items[m_variant].Selected = true;
                }

                MessageBox.Show("Удаление выполнено успешно");
            }
        }

        private bool isRecordConsists(string table, string record, string value)
        {
            string sql = string.Format("SELECT * FROM {0} " +
                "WHERE {1} = '{2}' ", table, record, value);

            DataTable dt = SQLCustom.SQL_Request(m_connection, sql);

            if (dt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void setAccessSettings()
        {
            switch (Form1.access)
            {
                case users.Peb:
                case users.Control:
                case users.Bgir:
                case users.Bmtd:
                case users.Btk:
                case users.Storehouse:
                    textBox1.ReadOnly = true;
                    textBox2.ReadOnly = true;
                    textBox3.ReadOnly = true;
                    button1.Enabled = false;
                    button2.Enabled = false;
                    button3.Enabled = false;
                    button4.Enabled = false;
                    button5.Enabled = false;
                    button6.Enabled = false;
                    button7.Enabled = false;
                    button8.Enabled = false;
                    button9.Enabled = false;
                    break;
                case users.Root:
                case users.SuperRoot:
                case users.Bras:
                    break;
                default:
                    break;
            }
        }
    }
}
