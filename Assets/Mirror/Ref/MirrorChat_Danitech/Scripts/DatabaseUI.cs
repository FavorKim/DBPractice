using UnityEngine;
using UnityEngine.UI;
using MySql.Data.MySqlClient;
using System;
using System.Data;

public class DatabaseUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] InputField Input_Query;
    [SerializeField] Text Text_DBResult;
    [SerializeField] Text Text_Log;

    [Header("ConnectionInfo")]
    [SerializeField] string _ip = "127.0.0.1";
    [SerializeField] string _DBName = "test";
    [SerializeField] string _uid = "root";
    [SerializeField] string _pwd = "1234";

    private bool _isConnectTestComplete;    //UI용

    private static MySqlConnection _dbConnection;



    private void Awake()
    {
        this.gameObject.SetActive(false);
    }


    private void SendQuery(string queryStr, string tableName)
    {
        // 있으면 SELECT관련 함수 호출
        if (queryStr.Contains("SELECT"))
        {
            DataSet dataSet = OnSelectRequest(queryStr,tableName);
            Text_DBResult.text = DeformatResult(dataSet);
        }
        // 없으면 INSERT 혹은 UPDATE관련 쿼리
        else 
        {
            Text_DBResult.text += OnInsertOnUpdateRequest(queryStr) ? "성공\n" : "실패\n";
        }
        
    }


    public static bool OnInsertOnUpdateRequest(string query)
    {
        try
        {
            MySqlCommand sqlCmd = new MySqlCommand();
            sqlCmd.Connection = _dbConnection;
            sqlCmd.CommandText = query;

            _dbConnection.Open();
            sqlCmd.ExecuteNonQuery();
            _dbConnection.Close();

            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
            return false;
        }
    }

    private string DeformatResult(DataSet dataSet)
    {
        string resultStr = string.Empty;

        foreach(DataTable table in dataSet.Tables)
        {
            foreach(DataRow row in table.Rows)
            {
                foreach(DataColumn col in table.Columns)
                    resultStr += $"{col.ColumnName}: {row[col]}\n";
            }
        }

        return resultStr;
    }

    public static DataSet OnSelectRequest(string query, string tableName)
    {
        try
        {
            _dbConnection.Open();
            MySqlCommand sqlCmd = new MySqlCommand();
            sqlCmd.Connection = _dbConnection;
            sqlCmd.CommandText = query;

            MySqlDataAdapter sd = new MySqlDataAdapter(sqlCmd);
            DataSet dataSet = new();
            sd.Fill(dataSet, tableName);

            _dbConnection.Close();
            return dataSet;
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.ToString());
            return null;
        }
    }

    private bool ConnectTest()
    {
        string connectStr = $"Server={_ip};Database={_DBName};Uid={_uid};Pwd={_pwd};";

        try
        {
            using(MySqlConnection conn = new MySqlConnection(connectStr)) 
            {
                _dbConnection = conn;
                conn.Open();
            }
            Text_Log.text = "DB 연결을 성공했습니다!";
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"e : {ex.ToString()}");
            Text_Log.text = "DB 연결 실패...";
            return false;
        }

    }

    public void OnClick_TestDBConnect()
    {
        _isConnectTestComplete = ConnectTest();
    }

    public void OnSubmit_SendQuery()
    {
        if (!_isConnectTestComplete)
        {
            Text_Log.text = "DB가 연결 되지 않았습니다. DB를 먼저 연결해주세요";
            return;
        }

        Text_Log.text = string.Empty;

        string query = string.IsNullOrWhiteSpace(Input_Query.text) ? 
            "SELECT U_Name,U_Password FROM user_info " :
            Input_Query.text;
        
        SendQuery(query, "user_info");
    }

    public void OnClick_OpenDatabaseUI()
    {
        this.gameObject.SetActive(true);
    }

    public void OnClick_CloseDatabaseUI()
    {
        this.gameObject.SetActive(false);
    }

}
