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
    private void OnTriggerEnter(Collider other) //�ݶ��̴��� �ε������� �Ʒ� �ڵ� ����
    {
        if (other.CompareTag("Player")) //�÷��̾� �±� �����Ҷ�
        {
            player = other.transform; //�÷��̾�� Ʈ�������̶�� ��.
            switchUpdate(gameObject.tag); //switchUpdate�Լ��� ���� 

        }
    }
    // Update is called once per frame
    void Update()
    {

    }

    void switchUpdate(string _tag) //1. ��ٸ� ������, 2. ���� ������, 3. �����̵�
    {
        switch (_tag)
        {
            case "Ladder":
                DBManager.instance.myCon.ChangeAnim("Ladder"); //�±׸� �˼��ؼ� ��ٸ��̸� ��ٸ� �ִϸ��̼� ����
                teleportLocation.Teleport(player);  //teleportLocationŬ������ �ҷ��ͼ� �÷��̾� �̵� �Լ� ���
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
