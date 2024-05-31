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


    private string SendQuery(string queryStr, string tableName)
    {
        DataSet dataSet = OnSelectRequest(queryStr, tableName);
        return dataSet.GetXml().ToString();
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
        }

        Text_Log.text = string.Empty;

        string query = string.IsNullOrWhiteSpace(Input_Query.text) ? 
            "SELECT U_Name FROM user_info" :
            Input_Query.text;
        
        string resulStr = SendQuery(query, "user_info");
        
        Text_DBResult.text = resulStr;
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
