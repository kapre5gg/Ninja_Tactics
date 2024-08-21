using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class DBManager : MonoBehaviour
{
    //메타버스에서 사용할 모든 데이터를 총괄
    //싱글톤 + 돈디스트로이
    #region 아바타
    public int hairIndex;
    public int faceIndex;
    public int topIndex;
    public int botIndex;
    public int balIndex;

    public int avatarIdx = 0;
    #endregion
    public string playerName;
    public string bubbleChat;
    public int PlayerNinjaType = -1;
    public MysqlManager sqlManager;
    public static DBManager instance;
    public NinjaController myCon = null;
    Dictionary<int, string> listClientIdx = new Dictionary<int, string>();


    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this);
            instance = this;
        }
        else
            Destroy(this);
    }

    //유저정보 다루기
    public class User
    {
        public int uuid;
        public string id, pw, nickName;

        public bool CheckNickName(string _id, string _pw) //sql 문법
        {
           try
           {
               DataTable tempData = DBManager.instance.sqlManager.SqlReceiveCmd($"SELECT NickName FROM player WHERE ID = '{_id}' AND PW = '{_pw}';");
               nickName = tempData.Rows[0][0].ToString();
               return true;
           }
           catch
           {
               return false;
           }
        }

        public void RegistID(string _id, string _pw, string _nick)
        {
            DBManager.instance.sqlManager.SqlSendCmd($"INSERT INTO `metaversedb`.`player` (`ID`, `PW`, `NickName`) VALUES ('{_id}', '{_pw}', '{_nick}');");
        }
    }

    public class Session
    {
        //방 정보 조회
        public DataTable LoadSessionInfo()
        {
            return DBManager.instance.sqlManager.SqlReceiveCmd($"SELECT * FROM `metaversedb`.`session` WHERE `RoomName` IS not NULL;");
        }
        //방 정보 갱신
        public void CreateNewRoom(int _uuid, string _roomName)
        {
            DBManager.instance.sqlManager.SqlSendCmd($"UPDATE `session` SET `RoomName` = '{_roomName}' WHERE UUID = {_uuid};");
        }
        //비어있는 방의 uuid 가져오기 
        public int LoadEmptyRoomNum() //데이터 테이블 하나는 int로 변환을 할 수 있다
        {
            DataTable data = DBManager.instance.sqlManager.SqlReceiveCmd($"SELECT `uuid` FROM `session` WHERE `RoomName` IS NULL ORDER BY UUID LIMIT 1;");
            return (int)data.Rows[0][0];
        }

        public void IncreasePlayerNum(string _port, string _id)
        {
            //====================
            // 들어온 클라이언트 누구인지 판별하는 코드
            //====================


            DBManager.instance.sqlManager.SqlSendCmd($"UPDATE `session` SET `PlayerNum` = `PlayerNum` + 1 WHERE `RoomPort` = {_port};");
        }

        //입장할때 인원수 증가 
        public void IncreasePlayerNum(int _uuid)
        {
            DBManager.instance.sqlManager.SqlSendCmd($"UPDATE `session` SET `PlayerNum` = `PlayerNum` + 1 WHERE UUID = {_uuid};");
        }
        public void IncreasePlayerNum(string _port)
        {
            DBManager.instance.sqlManager.SqlSendCmd($"UPDATE `session` SET `PlayerNum` = `PlayerNum` + 1 WHERE `RoomPort` = {_port};");
        }
        public void DecreasePlayerNum(int _uuid)
        {
            DBManager.instance.sqlManager.SqlSendCmd($"UPDATE `session` SET `PlayerNum` = `PlayerNum` - 1 WHERE UUID = {_uuid};");
        }
        public void DecreasePlayerNum(string _port)
        {
            DBManager.instance.sqlManager.SqlSendCmd($"UPDATE `session` SET `PlayerNum` = `PlayerNum` - 1 WHERE `RoomPort` = {_port};");
        }
        //방정보 초기화
        public void ResetRoomInfo(string _port)
        {
            DBManager.instance.sqlManager.SqlSendCmd($"UPDATE `session` SET `RoomName` = NULL, `PlayerNum` = 0 WHERE `RoomPort` = {_port};");
        }
        //uuid로 port 가져오기
        public string GetPort(int _uuid)
        {
            DataTable data = DBManager.instance.sqlManager.SqlReceiveCmd($"SELECT `RoomPort` FROM `session` WHERE UUID = {_uuid} ORDER BY `RoomPort` LIMIT 1;");
            return data.Rows[0][0].ToString();
        }
    }
}
