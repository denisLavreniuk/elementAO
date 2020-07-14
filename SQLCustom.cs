using System;
using MySql.Data.MySqlClient;

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
            //toolStripStatusLabel1.Text = "Request EROOR";
            return null;
        }
    }
    
}


