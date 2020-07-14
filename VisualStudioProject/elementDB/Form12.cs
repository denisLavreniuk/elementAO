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
    public partial class Form12 : Form
    {
        private int m_id;
        private bool isContractDataChanged = false;
        private bool isResourcesDataChanged = false;
        private Form11 m_parent;
        private string unitNum;

        public Form12(int id, Form parent)
        {
            m_id = id;
            m_parent = parent as Form11;
            this.Text = m_parent.m_title;
            BackColor = Color.PowderBlue;
            InitializeComponent();
            makeRequest(id);
            setTitle(m_id);
            setAccessSettings();
        }

        void makeRequest(int id)
        {
            String sql = "SELECT unit_info.var_stor_period col_2, " +
                "varranty_res.period_value col_3, " +
                "varranty_res.operating_hours col_4, " +
                "bef_first_repair_res.period_value col_5, " +
                "bef_first_repair_res.operating_hours col_6, " +
                "between_repairs_res.period_value col_7, " +
                "between_repairs_res.operating_hours col_8, " +
                "assigned_res.period_value col_9, " +
                "assigned_res.operating_hours col_10, " +
                "refurbished_res.period_value col_11, " +
                "refurbished_res.operating_hours col_12, " +
                "unit_info.remark col_13 " +
                "FROM unit_info, varranty_res, bef_first_repair_res, " +
                "between_repairs_res, assigned_res, refurbished_res " +
                "WHERE unit_info.unit_id = varranty_res.unit_id AND " +
                "unit_info.unit_id = bef_first_repair_res.unit_id AND " +
                "unit_info.unit_id = between_repairs_res.unit_id AND " +
                "unit_info.unit_id = assigned_res.unit_id AND " +
                "unit_info.unit_id = refurbished_res.unit_id AND " +
                "unit_info.unit_id = " + id.ToString();

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);
            if (dt.Rows.Count != 0)
            {
                textBox10.Text = dt.Rows[0]["col_2"].ToString();
                textBox9.Text = dt.Rows[0]["col_3"].ToString();
                textBox8.Text = dt.Rows[0]["col_4"].ToString();
                textBox2.Text = dt.Rows[0]["col_5"].ToString();
                textBox1.Text = dt.Rows[0]["col_6"].ToString();
                textBox4.Text = dt.Rows[0]["col_7"].ToString();
                textBox3.Text = dt.Rows[0]["col_8"].ToString();
                textBox6.Text = dt.Rows[0]["col_9"].ToString();
                textBox5.Text = dt.Rows[0]["col_10"].ToString();
                textBox11.Text = dt.Rows[0]["col_11"].ToString();
                textBox7.Text = dt.Rows[0]["col_12"].ToString();
            }

            sql = "SELECT * from contracts WHERE unit_id = " + id;

            dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.Rows.Count != 0)
            {
                textBox12.Text = dt.Rows[0]["contract_num"].ToString();
                dateTimePicker1.Value = Convert.ToDateTime(dt.Rows[0]["contract_date"].ToString());
                textBox14.Text = dt.Rows[0]["associate"].ToString();
                textBox15.Text = dt.Rows[0]["director_decision"].ToString();
            }
            isContractDataChanged = false;
            isResourcesDataChanged = false;
        }

        private bool updateResources()
        {
            string sqlOld = "SELECT unit_info.var_stor_period col_2, " +
                "varranty_res.period_value col_3, " +
                "varranty_res.operating_hours col_4, " +
                "bef_first_repair_res.period_value col_5, " +
                "bef_first_repair_res.operating_hours col_6, " +
                "between_repairs_res.period_value col_7, " +
                "between_repairs_res.operating_hours col_8, " +
                "assigned_res.period_value col_9, " +
                "assigned_res.operating_hours col_10, " +
                "refurbished_res.period_value col_11, " +
                "refurbished_res.operating_hours col_12, " +
                "unit_info.remark col_13 " +
                "FROM unit_info, varranty_res, bef_first_repair_res, " +
                "between_repairs_res, assigned_res, refurbished_res " +
                "WHERE unit_info.unit_id = varranty_res.unit_id AND " +
                "unit_info.unit_id = bef_first_repair_res.unit_id AND " +
                "unit_info.unit_id = between_repairs_res.unit_id AND " +
                "unit_info.unit_id = assigned_res.unit_id AND " +
                "unit_info.unit_id = refurbished_res.unit_id AND " +
                "unit_info.unit_id = " + m_id.ToString();

            DataTable dtOld = SQLCustom.SQL_Request(Form1.connection, sqlOld);

            string sql = "BEGIN;";
            string sqlJournal = "BEGIN; ";

            sqlJournal += "INSERT INTO journal_log " +
                "(date_time, user, operation, num_code, property, old_value, new_value) " +
                "VALUES ";

            if (dtOld.Rows[0]["col_3"].ToString() != textBox9.Text)
            {
                sql += string.Format("UPDATE varranty_res SET " +
                                "period_value = {0} " +
                                "WHERE unit_id = {1};",
                                textBox9.Text, m_id);

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    unitNum + " " + label9.Text,
                    "Гарантийный срок эксплуатации",
                    dtOld.Rows[0]["col_3"].ToString(),
                    textBox9.Text);

            }
            if (dtOld.Rows[0]["col_4"].ToString() != textBox8.Text)
            {
                sql += string.Format("UPDATE varranty_res SET " +
                                "operating_hours = {0} " +
                                "WHERE unit_id = {1};",
                                textBox8.Text, m_id);

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    unitNum + " " + label9.Text,
                    "Гарантийная наработка",
                    dtOld.Rows[0]["col_4"].ToString(),
                    textBox8.Text);
            }

            if (dtOld.Rows[0]["col_5"].ToString() != textBox2.Text)
            {
                sql += string.Format("UPDATE bef_first_repair_res SET " +
                     "period_value = {0} " +
                     "WHERE unit_id = {1};",
                     textBox2.Text, m_id);

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    unitNum + " " + label9.Text,
                    "Срок эксплуатации до 1-го ремонта",
                    dtOld.Rows[0]["col_5"].ToString(),
                    textBox2.Text);
            }

            if (dtOld.Rows[0]["col_6"].ToString() != textBox1.Text)
            {
                sql += string.Format("UPDATE bef_first_repair_res SET " +
                     "operating_hours = {0} " +
                     "WHERE unit_id = {1};",
                     textBox1.Text, m_id);

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    unitNum + " " + label9.Text,
                    "Наработка до 1-го ремонта",
                    dtOld.Rows[0]["col_6"].ToString(),
                    textBox1.Text);
            }

            if (dtOld.Rows[0]["col_7"].ToString() != textBox4.Text)
            {
                sql += string.Format("UPDATE between_repairs_res SET " +
                                "period_value = {0} " +
                                "WHERE unit_id = {1};",
                                textBox4.Text, m_id);

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    unitNum + " " + label9.Text,
                    "Межремонтный срок эксплуатации",
                    dtOld.Rows[0]["col_7"].ToString(),
                    textBox4.Text);
            }

            if (dtOld.Rows[0]["col_8"].ToString() != textBox3.Text)
            {
                sql += string.Format("UPDATE between_repairs_res SET " +
                                "operating_hours = {0} " +
                                "WHERE unit_id = {1};",
                                textBox3.Text, m_id);

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    unitNum + " " + label9.Text,
                    "Межремонтная наработка",
                    dtOld.Rows[0]["col_8"].ToString(),
                    textBox3.Text);
            }

            if (dtOld.Rows[0]["col_9"].ToString() != textBox6.Text)
            {
                sql += string.Format("UPDATE assigned_res SET " +
                                    "period_value = {0} " +
                                    "WHERE unit_id = {1};",
                                    textBox6.Text, m_id);

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    unitNum + " " + label9.Text,
                    "Назначенный срок эксплуатации",
                    dtOld.Rows[0]["col_9"].ToString(),
                    textBox6.Text);
            }
            if (dtOld.Rows[0]["col_10"].ToString() != textBox5.Text)
            {
                sql += string.Format("UPDATE assigned_res SET " +
                                    "operating_hours = {0} " +
                                    "WHERE unit_id = {1};",
                                    textBox5.Text, m_id);

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    unitNum + " " + label9.Text,
                    "Назначенная наработка",
                    dtOld.Rows[0]["col_10"].ToString(),
                    textBox5.Text);
            }
            if (dtOld.Rows[0]["col_11"].ToString() != textBox11.Text)
            {
                sql += string.Format("UPDATE refurbished_res SET " +
                     "period_value = {0} " +
                     "WHERE unit_id = {1};",
                    textBox11.Text, m_id);

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    unitNum + " " + label9.Text,
                    "Срок эксплуатации по техническому состоянию",
                    dtOld.Rows[0]["col_11"].ToString(),
                    textBox11.Text);
            }
            if (dtOld.Rows[0]["col_12"].ToString() != textBox7.Text)
            {
                sql += string.Format("UPDATE refurbished_res SET " +
                     "operating_hours = {0} " +
                     "WHERE unit_id = {1};",
                    textBox7.Text, m_id);

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    unitNum + " " + label9.Text,
                    "Наработка по техническому состояинию",
                    dtOld.Rows[0]["col_12"].ToString(),
                    textBox7.Text);
            }

            if (dtOld.Rows[0]["col_2"].ToString() != textBox10.Text)
            {
                sql += string.Format("UPDATE unit_info SET var_stor_period = '{0}' " +
                                 "WHERE unit_id = {1}; ",
                                 textBox10.Text, m_id);

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    unitNum + " " + label9.Text,
                    "Гарантийный срок хранения",
                    dtOld.Rows[0]["col_2"].ToString(),
                    textBox10.Text);
            }

            sql += string.Format("UPDATE unit_info SET " +
                                 "last_update = '{0}' " +
                                 "WHERE unit_id = {1}; ",
                                 DateTime.Today.ToString("yyyy-MM-dd"), m_id);

            sql += "COMMIT;";

            sqlJournal = sqlJournal.Remove(sqlJournal.Length - 2, 2);
            sqlJournal += ';';

            sqlJournal += "COMMIT;";

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sqlJournal);

            dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.ToString() != "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool insertResources()
        {
            string sql = "BEGIN;";
            string sqlJournal = "BEGIN; ";

            sqlJournal += "INSERT INTO journal_log " +
                "(date_time, user, operation, num_code, property, new_value) " +
                "VALUES ";

            sql += string.Format("INSERT INTO varranty_res " +
                                "(unit_id, " +
                                "period_value, " +
                                "operating_hours) " +
                                "VALUES ({0}, {1}, {2});",
                                m_id, textBox9.Text, textBox8.Text);

            sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                 "'{4}', '{5}'), ",
                 DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                 Form1.userName,
                 "Добавление",
                 unitNum + " " + label9.Text,
                 "Гарантийный срок эксплуатации",
                 textBox9.Text);

            sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                 "'{4}', '{5}'), ",
                 DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                 Form1.userName,
                 "Добавление",
                 unitNum + " " + label9.Text,
                 "Гарантийная наработка",
                 textBox8.Text);

            sql += string.Format("INSERT INTO bef_first_repair_res " +
                                "(bef_first_repair_res.unit_id, " +
                                "bef_first_repair_res.period_value, " +
                                "bef_first_repair_res.operating_hours) " +
                                "VALUES ({0}, {1}, {2});",
                                m_id, textBox2.Text, textBox1.Text);

            sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                 "'{4}', '{5}'), ",
                 DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                 Form1.userName,
                 "Добавление",
                 unitNum + " " + label9.Text,
                 "Срок эксплуатации до 1-го ремонта",
                 textBox2.Text);

            sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                 "'{4}', '{5}'), ",
                 DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                 Form1.userName,
                 "Добавление",
                 unitNum + " " + label9.Text,
                 "Наработка до 1-го ремонта",
                 textBox1.Text);

            sql += string.Format("INSERT INTO between_repairs_res " +
                                "(between_repairs_res.unit_id, " +
                                "between_repairs_res.period_value, " +
                                "between_repairs_res.operating_hours) " +
                                "VALUES ({0}, {1}, {2});",
                                 m_id, textBox4.Text, textBox3.Text);

            sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                 "'{4}', '{5}'), ",
                 DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                 Form1.userName,
                 "Добавление",
                 unitNum + " " + label9.Text,
                 "Межремонтный срок эксплуатации",
                 textBox4.Text);

            sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                 "'{4}', '{5}'), ",
                 DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                 Form1.userName,
                 "Добавление",
                 unitNum + " " + label9.Text,
                 "Межремонтная наработка",
                 textBox3.Text);

            sql += string.Format("INSERT INTO assigned_res " +
                                "(assigned_res.unit_id, " +
                                "assigned_res.period_value, " +
                                "assigned_res.operating_hours) " +
                                "VALUES ({0}, {1}, {2});",
                                m_id, textBox6.Text, textBox5.Text);

            sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                 "'{4}', '{5}'), ",
                 DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                 Form1.userName,
                 "Добавление",
                 unitNum + " " + label9.Text,
                 "Назначенный срок эксплуатации",
                 textBox6.Text);

            sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                 "'{4}', '{5}'), ",
                 DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                 Form1.userName,
                 "Добавление",
                 unitNum + " " + label9.Text,
                 "Назначенная наработка",
                 textBox5.Text);

            sql += string.Format("INSERT INTO refurbished_res " +
                                "(refurbished_res.unit_id, " +
                                "refurbished_res.period_value, " +
                                "refurbished_res.operating_hours) " +
                                "VALUES ({0}, {1}, {2});",
                                m_id, textBox11.Text, textBox7.Text);

            sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                 "'{4}', '{5}'), ",
                 DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                 Form1.userName,
                 "Добавление",
                 unitNum + " " + label9.Text,
                 "Срок эксплуатации по техническому состоянию",
                 textBox11.Text);

            sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                "'{4}', '{5}'), ",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                Form1.userName,
                "Добавление",
                unitNum + " " + label9.Text,
                "Наработка по техническому состояинию",
                textBox7.Text);

            sql += string.Format("UPDATE unit_info SET var_stor_period = '{0}', " +
                                 "last_update = '{1}'" +
                                 "WHERE unit_id = {2};",
                                 textBox10.Text, DateTime.Today.ToString("yyyy-MM-dd"), m_id);

            sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                "'{4}', '{5}'), ",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                Form1.userName,
                "Добавление",
                unitNum + " " + label9.Text,
                "Гарантийный срок хранения",
                textBox10.Text);

            sql += "COMMIT;";

            sqlJournal = sqlJournal.Remove(sqlJournal.Length - 2, 2);
            sqlJournal += ';';

            sqlJournal += "COMMIT;";

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sqlJournal);

            dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.ToString() != "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool insertContract()
        {
            string sql = string.Format("INSERT INTO contracts (unit_id, " +
                "contract_num, contract_date, associate, director_decision) " +
                "VALUES ({0}, '{1}', '{2}', '{3}', '{4}')",
                m_id,
                textBox12.Text,
                dateTimePicker1.Value.Date.ToString("yyyy-MM-d"),
                textBox14.Text,
                textBox15.Text);

            string sqlJournal = "BEGIN; ";

            sqlJournal += "INSERT INTO journal_log " +
                "(date_time, user, operation, num_code, property, new_value) " +
                "VALUES ";

            sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                 "'{4}', '{5}'), ",
                 DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                 Form1.userName,
                 "Добавление",
                 unitNum + " " + label9.Text,
                 "№ Договора",
                 textBox12.Text);

            sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                 "'{4}', '{5}'), ",
                 DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                 Form1.userName,
                 "Добавление",
                 unitNum + " " + label9.Text,
                 "Дата",
                 dateTimePicker1.Value.Date.ToString("yyyy-MM-d"));

            sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                 "'{4}', '{5}'), ",
                 DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                 Form1.userName,
                 "Добавление",
                 unitNum + " " + label9.Text,
                 "С кем заключен",
                 textBox14.Text);

            sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                 "'{4}', '{5}'), ",
                 DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                 Form1.userName,
                 "Добавление",
                 unitNum + " " + label9.Text,
                 "Распоряжение директора",
                 textBox15.Text);

            sqlJournal = sqlJournal.Remove(sqlJournal.Length - 2, 2);
            sqlJournal += ';';

            sqlJournal += "COMMIT;";

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sqlJournal);

            dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.ToString() != "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool updateContract()
        {
            string sqlOld = "SELECT * FROM contracts WHERE unit_id = " + m_id.ToString();

            DataTable dtOld = SQLCustom.SQL_Request(Form1.connection, sqlOld);

            string sql = "BEGIN;";
            string sqlJournal = "BEGIN; ";

            sqlJournal += "INSERT INTO journal_log " +
                "(date_time, user, operation, num_code, property, old_value, new_value) " +
                "VALUES ";

            if (dtOld.Rows[0]["contract_num"].ToString() != textBox12.Text)
            {
                sql += string.Format("UPDATE contracts SET " +
                   "contract_num = '{0}' " +
                   "WHERE unit_id = {1};",
                   textBox12.Text, m_id);

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    unitNum + " " + label9.Text,
                    "№ Договора",
                    dtOld.Rows[0]["contract_num"].ToString(),
                    textBox12.Text);
            }

            if (String.Format("{0:yyyy-MM-dd}", dtOld.Rows[0]["contract_date"]) != dateTimePicker1.
                Value.Date.ToString("yyyy-MM-dd"))               
            {
                sql += string.Format("UPDATE contracts SET " +
                   "contract_date = '{0}' " +
                   "WHERE unit_id = {1};",
                   dateTimePicker1.Value.Date.ToString("yyyy-MM-d"), m_id);

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    unitNum + " " + label9.Text,
                    "Дата",
                    String.Format("{0:yyyy-MM-dd}", dtOld.Rows[0]["contract_date"]),
                    dateTimePicker1.Value.Date.ToString("yyyy-MM-d"));
            }

            if (dtOld.Rows[0]["associate"].ToString() != textBox14.Text)
            {
                sql += string.Format("UPDATE contracts SET " +
                   "associate = '{0}' " +
                   "WHERE unit_id = {1};",
                   textBox14.Text, m_id);

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    unitNum + " " + label9.Text,
                    "С кем заключен",
                    dtOld.Rows[0]["associate"].ToString(),
                    textBox14.Text);
            }

            if (dtOld.Rows[0]["director_decision"].ToString() != textBox15.Text)
            {
                sql += string.Format("UPDATE contracts SET " +
                   "director_decision = '{0}' " +
                   "WHERE unit_id = {1};",
                   textBox15.Text, m_id);

                sqlJournal += string.Format("('{0}', '{1}', '{2}', '{3}', " +
                    "'{4}', '{5}', '{6}'), ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Form1.userName,
                    "Изменение",
                    unitNum + " " + label9.Text,
                    "Распоряжение директора",
                    dtOld.Rows[0]["director_decision"].ToString(),
                    textBox15.Text);
            }

            sql += "COMMIT;";

            sqlJournal = sqlJournal.Remove(sqlJournal.Length - 2, 2);
            sqlJournal += ';';

            sqlJournal += "COMMIT;";

            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sqlJournal);

            dt = SQLCustom.SQL_Request(Form1.connection, sql);

            if (dt.ToString() != "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void setTitle(int id)
        {
            string sql = "SELECT product_code, unit_num FROM unit_info WHERE unit_id = "
                         + id.ToString();
            DataTable dt = SQLCustom.SQL_Request(Form1.connection, sql);

            label9.Text = dt.Rows[0][0].ToString();
            unitNum = dt.Rows[0][1].ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string sql;
            DataTable dt;

            bool isSuccessful = false;

            if (!(isContractDataChanged || isResourcesDataChanged))
            {
                MessageBox.Show("Такая запись уже существует");
                return;
            }

            if (textBox1.Text != "" && textBox2.Text != "" &&
                textBox3.Text != "" && textBox4.Text != "" &&
                textBox5.Text != "" && textBox6.Text != "" &&
                textBox7.Text != "" && textBox8.Text != "" &&
                textBox9.Text != "" && textBox10.Text != "" &&
                textBox11.Text != "" && isResourcesDataChanged)
            {
                sql = "SELECT * FROM varranty_res WHERE unit_id = " + m_id;

                dt = SQLCustom.SQL_Request(Form1.connection, sql);

                if (dt.Rows.Count > 0)
                {
                    isSuccessful = updateResources();
                }
                else
                {
                    isSuccessful = insertResources();
                }
            }

            if (textBox12.Text != "" && textBox14.Text != "" &&
                textBox15.Text != "" && isContractDataChanged)
            {
                sql = "SELECT * FROM contracts WHERE unit_id = " + m_id;

                dt = SQLCustom.SQL_Request(Form1.connection, sql);

                if (dt.Rows.Count > 0)
                {
                    isSuccessful = updateContract();
                }
                else
                {
                    isSuccessful = insertContract();
                }
            }

            if (isSuccessful)
            {
                makeRequest(m_id);
                if (!m_parent.IsDisposed)
                {
                    m_parent.collectUnits();
                }
                MessageBox.Show("Данные успешно обновлены");
            }
            else
            {
                MessageBox.Show("Ошибка записи в базу данных!");
            }

        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            isContractDataChanged = true;
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            isResourcesDataChanged = true;
        }

        private void setAccessSettings()
        {
            switch (Form1.access)
            {
                case users.Control:
                case users.Peb:
                case users.Bras:
                case users.Bgir:
                case users.Bmtd:
                case users.Storehouse:
                    dateTimePicker1.Enabled = false;
                    textBox1.Enabled = false;
                    textBox2.Enabled = false;
                    textBox3.Enabled = false;
                    textBox4.Enabled = false;
                    textBox5.Enabled = false;
                    textBox6.Enabled = false;
                    textBox7.Enabled = false;
                    textBox8.Enabled = false;
                    textBox9.Enabled = false;
                    textBox10.Enabled = false;
                    textBox11.Enabled = false;
                    textBox12.Enabled = false;
                    textBox14.Enabled = false;
                    textBox15.Enabled = false;
                    button1.Enabled = false;
                    break;
                case users.Root:
                case users.SuperRoot:
                case users.Btk:
                    break;
                default:
                    break;
            }
        }
    }
}

