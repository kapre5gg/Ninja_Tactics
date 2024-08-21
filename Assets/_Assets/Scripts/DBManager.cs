using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class DBManager : MonoBehaviour
{
    //��Ÿ�������� ����� ��� �����͸� �Ѱ�
    //�̱��� + ����Ʈ����
    #region �ƹ�Ÿ
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

    //�������� �ٷ��
    public class User
    {
        public int uuid;
        public string id, pw, nickName;

        public bool CheckNickName(string _id, string _pw) //sql ����
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
        //�� ���� ��ȸ
        public DataTable LoadSessionInfo()
        {
            return DBManager.instance.sqlManager.SqlReceiveCmd($"SELECT * FROM `metaversedb`.`session` WHERE `RoomName` IS not NULL;");
        }
        //�� ���� ����
        public void CreateNewRoom(int _uuid, string _roomName)
        {
            DBManager.instance.sqlManager.SqlSendCmd($"UPDATE `session` SET `RoomName` = '{_roomName}' WHERE UUID = {_uuid};");
        }
        //����ִ� ���� uuid �������� 
        public int LoadEmptyRoomNum() //������ ���̺� �ϳ��� int�� ��ȯ�� �� �� �ִ�
        {
            DataTable data = DBManager.instance.sqlManager.SqlReceiveCmd($"SELECT `uuid` FROM `session` WHERE `RoomName` IS NULL ORDER BY UUID LIMIT 1;");
            return (int)data.Rows[0][0];
        }

        public void IncreasePlayerNum(string _port, string _id)
        {
            //====================
            // ���� Ŭ���̾�Ʈ �������� �Ǻ��ϴ� �ڵ�
            //====================


            DBManager.instance.sqlManager.SqlSendCmd($"UPDATE `session` SET `PlayerNum` = `PlayerNum` + 1 WHERE `RoomPort` = {_port};");
        }

        //�����Ҷ� �ο��� ���� 
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
        //������ �ʱ�ȭ
        public void ResetRoomInfo(string _port)
        {
            DBManager.instance.sqlManager.SqlSendCmd($"UPDATE `session` SET `RoomName` = NULL, `PlayerNum` = 0 WHERE `RoomPort` = {_port};");
        }
        //uuid�� port ��������
        public string GetPort(int _uuid)
        {
            DataTable data = DBManager.instance.sqlManager.SqlReceiveCmd($"SELECT `RoomPort` FROM `session` WHERE UUID = {_uuid} ORDER BY `RoomPort` LIMIT 1;");
            return data.Rows[0][0].ToString();
        }
    }
}
