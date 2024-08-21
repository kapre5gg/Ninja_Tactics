using Mirror;
using UnityEngine.SceneManagement;

public class RoomManager : NetworkBehaviour
{
    public void LeaveBtn()
    {
        DBManager.Session session = new DBManager.Session();
        if (MainManager.Instance.isRoomMaster) //������ ������ ��� �÷��̾ ����?
        {
            //db���� �̸�=null, ����� =0
            session.ResetRoomInfo(MainManager.Instance.connectionRoomPort);
            //�������� ��û�� �濡 ��� �ο� ����
            //������ ��� �����ڿ��Լ� �濡�� ������ �Լ��� ����
            CmdAllLeaveRoom();
        }
        else
        {
            session.DecreasePlayerNum(MainManager.Instance.connectionRoomPort);
            //������� ���� ������
            LeaveRoom();
        }
    }
    //������ �������� ��û�ϴ� �Լ�
    [Command(requiresAuthority = false)] //��Ư�� �ټ����� ������� �Ұ��� ,���� ����
    private void CmdAllLeaveRoom()
    {
        RpcLeaveRoom();
    }

    [ClientRpc]
    private void RpcLeaveRoom()
    {
        LeaveRoom();
    }


    private void LeaveRoom()//�濡�� ������ �Լ�
    {
        MainManager.Instance.MoveScenePort = MainManager.LOBBYPORT;
        //����, ȣ��Ʈ�� �̵��� �Ҵٴ� > �̵��ϸ� ���� ���� ������ �����Ƿ�
        //Ŭ�� �̵�
        MainManager.Instance.nextSceneNumber = 2;
        //��ž�� �ϸ� ���� ������ ���� �������� ������ �Ѿ�µ�, �������ο��� �ε����� ��ϵǾ�����
        //�׷��� �ε������� �Ѿ�ٰ� ���ư�����
        // ���� �ο��� ����, ������ ������ ��� �÷��̾ ����?, ������ �ʱ�ȭ

        //���� �������� uuid �ʱ�ȭ
        MainManager.Instance.connectionRoomPort = "";

        NetworkManager.singleton.StopClient();
        SceneManager.LoadSceneAsync("00 LOADING");
    }
}
