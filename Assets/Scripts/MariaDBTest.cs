using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MariaDBTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Connection Test : " + ConnectionTest());
    }

    public bool ConnectionTest()
    {
        string conStr = string.Format("Server={0};Database={1};Uid ={2};Pwd={3};",
            "127.0.0.1", "test", "root", "1004");
        try
        {
            using (MySqlConnection conn = new MySqlConnection(conStr))
            {
                conn.Open();
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("e : " + e.ToString());
            return false;
        }
    }
}