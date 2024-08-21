using Mirror;
using UnityEngine.SceneManagement;

public class RoomManager : NetworkBehaviour
{
    public void LeaveBtn()
    {
        DBManager.Session session = new DBManager.Session();
        if (MainManager.Instance.isRoomMaster) //방장이 나오면 모든 플레이어가 강퇴?
        {
            //db갱신 이름=null, 사람수 =0
            session.ResetRoomInfo(MainManager.Instance.connectionRoomPort);
            //서버에게 요청함 방에 모든 인원 퇴출
            //서버가 모든 참가자에게서 방에서 나가는 함수를 실행
            CmdAllLeaveRoom();
        }
        else
        {
            session.DecreasePlayerNum(MainManager.Instance.connectionRoomPort);
            //서버통신 없이 떠나기
            LeaveRoom();
        }
    }
    //방장이 서버에게 요청하는 함수
    [Command(requiresAuthority = false)] //불특정 다수에게 나가라고 할것임 ,방장 포함
    private void CmdAllLeaveRoom()
    {
        RpcLeaveRoom();
    }

    [ClientRpc]
    private void RpcLeaveRoom()
    {
        LeaveRoom();
    }


    private void LeaveRoom()//방에서 나가는 함수
    {
        MainManager.Instance.MoveScenePort = MainManager.LOBBYPORT;
        //서버, 호스트는 이동이 불다능 > 이동하면 이전 씬의 서버가 닫히므로
        //클라만 이동
        MainManager.Instance.nextSceneNumber = 2;
        //스탑을 하면 서버 연결을 끊고 오프라인 씬으로 넘어가는데, 오프라인에는 로딩씬이 등록되어있음
        //그래서 로딩씬으로 넘어갔다가 돌아가야함
        // 방의 인원수 감소, 방장이 나오면 모든 플레이어가 강퇴?, 방정보 초기화

        //현재 접속중이 uuid 초기화
        MainManager.Instance.connectionRoomPort = "";

        NetworkManager.singleton.StopClient();
        SceneManager.LoadSceneAsync("00 LOADING");
    }
}
