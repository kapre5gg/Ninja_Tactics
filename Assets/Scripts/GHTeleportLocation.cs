using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GHTeleportLocation : MonoBehaviour
{
    [SerializeField] private Transform player;
    TeleportLocation teleportLocation;


    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other) //콜라이더가 부딪혔을때 아래 코드 실행
    {
        if (other.CompareTag("Player")) //플레이어 태그 감지할때
        {
            player = other.transform; //플레이어는 트랜스폼이라고 함.
            switchUpdate(gameObject.tag); //switchUpdate함수를 실행 

        }
    }
    // Update is called once per frame
    void Update()
    {

    }

    void switchUpdate(string _tag) //1. 사다리 오르기, 2. 갈고리 오르기, 3. 동굴이동
    {
        switch (_tag)
        {
            case "Ladder":
                DBManager.instance.myCon.ChangeAnim("Ladder"); //태그를 검수해서 사다리이면 사다리 애니매이션 실행
                teleportLocation.Teleport(player);  //teleportLocation클래스를 불러와서 플레이어 이동 함수 사용
                break;

            case "Jump":
                DBManager.instance.myCon.ChangeAnim("Jump");
                teleportLocation.Teleport(player);
                break;

            case "Cave":
                teleportLocation.Teleport(player);
                break;
        }

    }

}
