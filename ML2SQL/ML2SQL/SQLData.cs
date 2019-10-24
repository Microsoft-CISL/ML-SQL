using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace ML2SQL
{
    public interface SQLData
    {

        string GetHeader(string separator);


        string getData(string separator);


        string getMySQLData(string separator);


        string getSQLServerData(string separator);


        string getId();

        string getScores(string separator);



        void ReadDataFromMYSQL(MySqlDataReader reader);

        void ReadDataFromSQLServer(SqlDataReader reader);
        



    }
}
