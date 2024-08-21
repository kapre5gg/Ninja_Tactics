using MySql.Data.MySqlClient;
using System;
using System.Data;
using UnityEngine;

public class MysqlManager : MonoBehaviour
{
    //mysql (maria db) 에 접속할 유저의 정보
    private MySqlConnection sqlconn = null;
    //접속할 ip, 디비 이름, 계정 id, 비번
    readonly string sqlDB_ip = "192.168.100.38"; //cmd창에 ipconfig 라고 치면 IPv4 주소가 나온다
    //다른 pc에서 접근할려면 "localhost" 이름은 적합하지 않다. 그 컴퓨터의 로컬호스트로 들어가기 때문
    readonly string sqlDB_name = "metaversedb";
    readonly string sqlDB_id = "hanju";
    readonly string sqlDB_pw = "su4300185144";

    private void Start()
    {
        //DataTable data = SqlReceiveCmd("select * from hacseng where ban=1;"); // 마지막 ;은 생략 가능?

        //for (int i = 0; i < data.Rows.Count; i++)
        //{
        //    for (int j = 0; j < data.Columns.Count; j++)
        //    {
        //        Debug.Log(i + " : " + j);
        //        //print("j : "+j);
        //        Debug.Log(data.Rows[i][j]);
        //    }
        //} //order by를 안 쓰면 얘네가 멋대로 순서를 바꾸기도 함 그래서 order by를 꼭 쓰자
        //Debug.Log(data.Rows[0][0]);
        //Debug.Log(data.Rows[0]["bun"]); //어떤 열인지 스트링으로 쓸 수 도 있다.
        //Debug.Log(data.Rows[0][2]);
    }
    public void SqlConnectOpen()
    {
        string command = $"server={sqlDB_ip}; database={sqlDB_name}; Userid={sqlDB_id}; Password={sqlDB_pw};"; //대소문자 무관

        try
        {
            sqlconn = new MySqlConnection(command);
            sqlconn.Open();
            Debug.Log("접속 상태 : " + sqlconn.State);
        }
        catch (Exception msg) //접속에 실패하면 오류 디버그
        {
            Debug.Log(msg);
        }
    }
    public void SqlConnectClose()
    {
        sqlconn.Close();
        Debug.Log("접속 상태 : " + sqlconn.State);
    }

    //데이터를 유니티로 받아오기
    //명령어는 string으로 받아옴
    //명령어를 일방적으로 sql에 보내는 함수 (ex : update, create 등의 명령어(되돌려 받는 데이터가 없는 명령어들))
    public void SqlSendCmd(string _cmd)
    {
        SqlConnectOpen(); //시작과 끝은 서버와 연결하고 끊는 것

        MySqlCommand dbcmd = new MySqlCommand(_cmd, sqlconn);
        dbcmd.ExecuteNonQuery();

        SqlConnectClose();
    }

    public DataTable SqlReceiveCmd(string _cmd)
    {
        SqlConnectOpen();

        MySqlDataAdapter adapter = new MySqlDataAdapter(_cmd, sqlconn);

        DataTable dt = new DataTable(); //DataTable : 유니티 상에서 테이블 형식으로 저장하는 포맷
        adapter.Fill(dt); //서버 데이터를 dt 테이블에 넣기
        SqlConnectClose();

        //dt 정보를 밖에서도 사용할 수 있도록 리턴
        return dt;
    }
}
