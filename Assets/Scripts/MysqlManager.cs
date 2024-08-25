using MySql.Data.MySqlClient;
using System;
using System.Data;
using UnityEngine;

public class MysqlManager : MonoBehaviour
{
    //mysql (maria db) �� ������ ������ ����
    private MySqlConnection sqlconn = null;
    //������ ip, ��� �̸�, ���� id, ���
    readonly string sqlDB_ip = "192.168.100.38"; //cmdâ�� ipconfig ��� ġ�� IPv4 �ּҰ� ���´�
    //�ٸ� pc���� �����ҷ��� "localhost" �̸��� �������� �ʴ�. �� ��ǻ���� ����ȣ��Ʈ�� ���� ����
    readonly string sqlDB_name = "metaversedb";
    readonly string sqlDB_id = "hanju";
    readonly string sqlDB_pw = "su4300185144";

    private void Start()
    {
        //DataTable data = SqlReceiveCmd("select * from hacseng where ban=1;"); // ������ ;�� ���� ����?

        //for (int i = 0; i < data.Rows.Count; i++)
        //{
        //    for (int j = 0; j < data.Columns.Count; j++)
        //    {
        //        Debug.Log(i + " : " + j);
        //        //print("j : "+j);
        //        Debug.Log(data.Rows[i][j]);
        //    }
        //} //order by�� �� ���� ��װ� �ڴ�� ������ �ٲٱ⵵ �� �׷��� order by�� �� ����
        //Debug.Log(data.Rows[0][0]);
        //Debug.Log(data.Rows[0]["bun"]); //� ������ ��Ʈ������ �� �� �� �ִ�.
        //Debug.Log(data.Rows[0][2]);
    }
    public void SqlConnectOpen()
    {
        string command = $"server={sqlDB_ip}; database={sqlDB_name}; Userid={sqlDB_id}; Password={sqlDB_pw};"; //��ҹ��� ����

        try
        {
            sqlconn = new MySqlConnection(command);
            sqlconn.Open();
            Debug.Log("���� ���� : " + sqlconn.State);
        }
        catch (Exception msg) //���ӿ� �����ϸ� ���� �����
        {
            Debug.Log(msg);
        }
    }
    public void SqlConnectClose()
    {
        sqlconn.Close();
        Debug.Log("���� ���� : " + sqlconn.State);
    }

    //�����͸� ����Ƽ�� �޾ƿ���
    //��ɾ�� string���� �޾ƿ�
    //��ɾ �Ϲ������� sql�� ������ �Լ� (ex : update, create ���� ��ɾ�(�ǵ��� �޴� �����Ͱ� ���� ��ɾ��))
    public void SqlSendCmd(string _cmd)
    {
        SqlConnectOpen(); //���۰� ���� ������ �����ϰ� ���� ��

        MySqlCommand dbcmd = new MySqlCommand(_cmd, sqlconn);
        dbcmd.ExecuteNonQuery();

        SqlConnectClose();
    }

    public DataTable SqlReceiveCmd(string _cmd)
    {
        SqlConnectOpen();

        MySqlDataAdapter adapter = new MySqlDataAdapter(_cmd, sqlconn);

        DataTable dt = new DataTable(); //DataTable : ����Ƽ �󿡼� ���̺� �������� �����ϴ� ����
        adapter.Fill(dt); //���� �����͸� dt ���̺� �ֱ�
        SqlConnectClose();

        //dt ������ �ۿ����� ����� �� �ֵ��� ����
        return dt;
    }
}
