using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TriggerPos : MonoBehaviour
{
    public NinjaTacticsManager tacticsManager;
    public int inSideCount = 0;
    public bool AllTogether;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tacticsManager.CmdCountAlivePlayers();
            inSideCount++;
            if (inSideCount == tacticsManager.AlivePlayers)
                AllTogether = true;
            else
                AllTogether = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tacticsManager.CmdCountAlivePlayers();
            inSideCount--;
            if (inSideCount != tacticsManager.AlivePlayers)
                AllTogether = false;
        }
    }
    public int CountGreaterThan(List<int> numbers)
    {
        return numbers.Count(n => n > 0); //현재 살아있는 멤버의 수 리턴
    }
}
