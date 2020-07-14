using System;
using MySql.Data.MySqlClient;
using System.Data;


static class SQLCustom
{
    public static DataTable SQL_Request(MySqlConnection connection, String request)
    {
        DataTable dt = new DataTable();
        try
        {
            connection.Open();
            using (MySqlCommand command = new MySqlCommand(request, connection))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                    connection.Close();
                    return dt;
                }
            }
        }
        catch (MySqlException err)
        {
            connection.Close();
            return new DataTable(err.Message);
            //toolStripStatusLabel1.Text = "Request EROOR";
            return null;
        }
    }
    
}


