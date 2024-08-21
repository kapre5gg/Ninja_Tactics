using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FieldOfView;
using static UnityEngine.UI.Image;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius; // 시야 반경
    [Range(0, 360)]
    public float viewAngle; // 시야 각도

    public LayerMask targetMask; // 타겟 레이어
    public LayerMask obstacleMask1; // 시야 완전 차단 오브젝트 레이어
    public LayerMask obstacleMask2; // 시야 조건부 차단 오브젝트 레이어(앉아있을 때)

    public List<Transform> visibleTargets = new List<Transform>(); // 감지된 타겟 리스트

    public float meshResolution; // 메쉬 해상도

    Mesh viewMesh; // 시야 메쉬
    public MeshFilter viewMeshFilter; // 메쉬 필터

    Mesh viewMesh2; // 시야 메쉬
    public MeshFilter viewMeshFilter2; // 메쉬 필터
    public GameObject viewMeshGroup;

    public Material activeMaterial; // 시야 활성 상태에서 사용할 머테리얼
    public Material inactiveMaterial; // 시야 비활성 상태에서 사용할 머테리얼
    public Material obtacleMaterial;

    private MeshRenderer meshRenderer; // 메쉬 렌더러
    private MeshRenderer meshRenderer2; // 메쉬 렌더러

    public int edgeResolveIterations; // 엣지 해상도
    public float edgeDstThreshold; // 엣지 거리 임계값

    public float rotationSpeed = 30f; // 회전 속도
    public float maxRotationAngle = 45f; // 최대 회전 각도

    private float currentAngle; // 현재 각도
    private bool rotatingRight = true; // 오른쪽으로 회전 중인지 여부
    private NinjaController player; // 플레이어 컨트롤러

    public bool isViewMeshVisible = false;
    void Start()
    {
        player = FindObjectOfType<NinjaController>(); // 플레이어 컨트롤러 찾기
        viewMesh = new Mesh(); // 새로운 메쉬 생성
        viewMesh.name = "View Mesh"; // 메쉬 이름 설정
        viewMeshFilter.mesh = viewMesh; // 메쉬 필터에 메쉬 할당

        viewMesh2 = new Mesh(); // 새로운 메쉬 생성
        viewMesh2.name = "View Mesh2"; // 메쉬 이름 설정
        viewMeshFilter2.mesh = viewMesh2; // 메쉬 필터에 메쉬 할당

        meshRenderer = viewMeshFilter.GetComponent<MeshRenderer>(); // 메쉬 렌더러 초기화
        meshRenderer2 = viewMeshFilter2.GetComponent<MeshRenderer>(); // 메쉬 렌더러 초기화
        meshRenderer.material = inactiveMaterial; // 초기 상태로 비활성 머테리얼 설정
        meshRenderer2.material = obtacleMaterial; // 초기 상태로 비활성 머테리얼 설정

        StartCoroutine(FindTargetsWithDelay(0.2f)); // 타겟 감지를 위한 코루틴 시작
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay); // 주어진 시간만큼 대기
            FindVisibleTargets(); // 타겟 감지
        }
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear(); // 감지된 타겟 리스트 초기화
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask); // 시야 반경 내 타겟 탐지
        //Physics.OverlapSphere(기준 위치, 범위, 마스크 설정)
        //transform.position으로부터 viewRadius 범위만큼의 구체 반경에 있는 targetMask 레이어를 가진 콜라이더를 targetsInViewRadius 배열에 저장

        for (int i = 0; i < targetsInViewRadius.Length; i++) //탐지된 타겟들을 검사
        {
            Transform target = targetsInViewRadius[i].transform; // 현재 반복 중인 타겟의 트랜스폼을 가져옴.
            // 현재 객체와 타겟 사이의 방향 벡터를 구하고, 정규화.-> 현재 객체에서 타겟으로 가는 방향
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            //Vector3.Angle(Vector3 from, Vector3 to) 두 벡터 사이의 각도를 계산하여 반환(0~180)
            //현재 객체의 정면(transform.forward)과 타겟 방향 벡터(dirToTarget) 사이의 각도를 계산하고, 그것이 viewAngle의 절반보다 작은지 확인
            //-> 타겟이 객체의 시야각 내에 있는지 확인(viewAngle의 절반보다 작으면 타겟이 시야각 내에 들어와있다고 판단)
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2) //타겟이 시야각에 들어오면
            {
                float dstToTarget = Vector3.Distance(transform.position, target.transform.position); // 타겟과의 거리 계산

                //Physics.Raycast(쏘는 위치, 쏘는 방향, 레이저가 닿는 최대 거리, 레이어마스크)
                // obstacleMask1 체크
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask1)) //시야각에 obstacleMask1 레이어를 가진 오브젝트가 존재 x
                {
                    // obstacleMask2 체크
                    if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask2)) //시야각에 obstacleMask2 레이어를 가진 오브젝트가 존재 x
                    {
                        //시야각에 아무런 장애물이 존재하지 않을 경우 (obstacleMask1 x, obstacleMask2 x)
                        visibleTargets.Add(target); // 타겟 포착
                    }
                    else //시야각에 obstacleMask2 레이어를 가진 오브젝트가 존재 o (obstacleMask1 x, obstacleMask2 o)
                    {
                        // Obstacle2에 가려진 경우의 처리
                        bool isCrouch = player != null && player.isSitting; // isCrouch = 플레이어가 앉아있는 상태
                        if (!isCrouch) //플레이어가 앉아있는 상태가 아닐 경우 감지됨.
                        {
                            visibleTargets.Add(target); // 조건이 만족되지 않으면 타겟 추가
                        }
                    }
                }
            }
        }
    }

    // y축 오일러 각을 3차원 방향 벡터로 변환한다.
    // 원본과 구현이 살짝 다름에 주의. 결과는 같다.
    public Vector3 DirFromAngle(float angleDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Cos((-angleDegrees + 90) * Mathf.Deg2Rad), 0, Mathf.Sin((-angleDegrees + 90) * Mathf.Deg2Rad)); // 각도에 따른 방향 벡터 계산
    }

    public struct ViewCastInfo //  Raycast를 했을 때 ray가 도달하는 위치를 표현하는 struct
    {
        public bool hit; // raycast가 hit 판정인지
        public Vector3 point; // ray가 마지막으로 도달한 위치
        public float dst; // ray의 길이
        public float angle; // ray가 이루는 각

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle) 
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    ViewCastInfo ViewCast(float globalAngle) // raycast 결과를 ViewCastInfo로 반환하는 메서드
    {
        Vector3 dir = DirFromAngle(globalAngle, true); // 주어진 각도에 대한 방향 벡터 계산
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask1))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle); // 완전 차단 오브젝트에 맞았을 때
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle); // 장애물이 없을 때
        }
    }

    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution); // 시야각을 meshResolution으로 나눠서 메쉬의 정밀도를 결정. stepCount = 시야각을 몇 개의 스텝으로 나눌지
        float stepAngleSize = viewAngle / stepCount; // 각 스텝의 각도 크기
        List<Vector3> viewPoints = new List<Vector3>(); // 시야 포인트 리스트
        ViewCastInfo prevViewCast = new ViewCastInfo(); // 이전 시야 캐스트 정보

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i; // 현재 각도 계산
            ViewCastInfo newViewCast = ViewCast(angle); // 새로운 시야 캐스트

            //시야각을 기준으로 일정한 각도 간격으로 ViewCast 함수를 호출하여, 그 지점의 정보를 가져옵니다.각 지점은 viewPoints 리스트에 저장

            // i가 0이면 prevViewCast에 아무 값이 없어 정점 보간을 할 수 없으므로 건너뛴다.
            if (i != 0)
            {
                bool edgeDstThresholdExceed = Mathf.Abs(prevViewCast.dst - newViewCast.dst) > edgeDstThreshold; // 엣지 거리 차이 비교

                // 둘 중 한 raycast가 장애물을 만나지 않았거나 두 raycast가 서로 다른 장애물에 hit 된 것이라면(edgeDstThresholdExceed 여부로 계산)
                if (prevViewCast.hit != newViewCast.hit || (prevViewCast.hit && newViewCast.hit && edgeDstThresholdExceed))
                {
                    Edge e = FindEdge(prevViewCast, newViewCast); //FindEdge 함수를 호출하여 엣지를 찾음. 이 엣지는 시야각 메쉬의 경계를 형성

                    // zero가 아닌 정점을 추가함
                    if (e.PointA != Vector3.zero)
                    {
                        viewPoints.Add(e.PointA); // 엣지 포인트 A 추가
                    }

                    if (e.PointB != Vector3.zero)
                    {
                        viewPoints.Add(e.PointB); // 엣지 포인트 B 추가
                    }
                }
            }

            viewPoints.Add(newViewCast.point); // 새로운 시야 포인트 추가
            prevViewCast = newViewCast; // 이전 시야 캐스트 정보 업데이트
        }

        int vertexCount = viewPoints.Count + 1; // 정점 수 계산
        Vector3[] vertices = new Vector3[vertexCount]; // 정점 배열
        int[] triangles = new int[(vertexCount - 2) * 3]; // 삼각형 배열
        vertices[0] = Vector3.zero; // 중심점

        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]); // 로컬 좌표로 변환하여 정점 추가
            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0; // 삼각형 첫 번째 점
                triangles[i * 3 + 1] = i + 1; // 삼각형 두 번째 점
                triangles[i * 3 + 2] = i + 2; // 삼각형 세 번째 점
            }
        }
        viewMesh.Clear(); // 기존 메쉬 데이터 초기화
        viewMesh.vertices = vertices; // 새 정점 데이터 설정
        viewMesh.triangles = triangles; // 새 삼각형 데이터 설정
        viewMesh.RecalculateNormals(); // 노멀 벡터 재계산

        // viewMesh2 생성 (장애물과 무관하게 원뿔형 렌더링)
        float extendedViewRadius = viewRadius * 1.1f;
        vertexCount = stepCount + 2; // 정점 수 계산 (장애물과 무관하게 일정)
        Vector3[] vertices2 = new Vector3[vertexCount];
        int[] triangles2 = new int[(vertexCount - 2) * 3];
        vertices2[0] = Vector3.zero; // 중심점

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            Vector3 dir = DirFromAngle(angle, true);
            vertices2[i + 1] = transform.InverseTransformPoint(transform.position + dir * extendedViewRadius);
            if (i < stepCount)
            {
                triangles2[i * 3] = 0;
                triangles2[i * 3 + 1] = i + 1;
                triangles2[i * 3 + 2] = i + 2;
            }
        }
        viewMesh2.Clear();
        viewMesh2.vertices = vertices2;
        viewMesh2.triangles = triangles2;
        viewMesh2.RecalculateNormals();

        // 시야 상태에 따라 머테리얼 변경
        if (isTarget())
        {
            meshRenderer.material = activeMaterial; // 타겟 감지됨
            meshRenderer2.material = activeMaterial; // 타겟 감지됨
        }
        else
        {
            meshRenderer.material = inactiveMaterial; // 타겟 감지되지 않음
            meshRenderer2.material = obtacleMaterial; // 타겟 감지됨
        }
    }
    private void OnMouseDown()
    {
        Debug.Log("클릭");
        isViewMeshVisible = !isViewMeshVisible; // 시야 메쉬의 활성화 상태를 토글
        meshRenderer.enabled = isViewMeshVisible;
        meshRenderer2.enabled = isViewMeshVisible;
    }


    public struct Edge
    {
        public Vector3 PointA, PointB; // 엣지의 두 점
        public Edge(Vector3 _PointA, Vector3 _PointB)
        {
            PointA = _PointA;
            PointB = _PointB;
        }
    }

    Edge FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle; // 최소 각도
        float maxAngle = maxViewCast.angle; // 최대 각도
        Vector3 minPoint = Vector3.zero; // 최소 포인트
        Vector3 maxPoint = Vector3.zero; // 최대 포인트

        for (int i = 0; i < edgeResolveIterations; i++) // 엣지 찾기 반복
        {
            float angle = minAngle + (maxAngle - minAngle) / 2; // 중앙 각도 계산
            ViewCastInfo newViewCast = ViewCast(angle); // 새로운 시야 캐스트
            bool edgeDstThresholdExceed = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold; // 엣지 거리 차이 비교
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceed)
            {
                minAngle = angle; // 최소 각도 업데이트
                minPoint = newViewCast.point; // 최소 포인트 업데이트
            }
            else
            {
                maxAngle = angle; // 최대 각도 업데이트
                maxPoint = newViewCast.point; // 최대 포인트 업데이트
            }
        }

        return new Edge(minPoint, maxPoint); // 최종 엣지 반환
    }

    private void Update()
    {
        if (isViewMeshVisible)
        {
            viewMeshGroup.SetActive(true);
            DrawFieldOfView(); // 시야 메쉬 그리기
        }
        else
        {
            viewMeshGroup.SetActive(false);
        }
        /*if (rotatingRight) // 오른쪽으로 회전 중인 경우
        {
            currentAngle += rotationSpeed * Time.deltaTime; // 현재 각도 증가
            if (currentAngle > maxRotationAngle) // 최대 각도 초과 시 방향 변경
            {
                currentAngle = maxRotationAngle;
                rotatingRight = false;
            }
        }
        else // 왼쪽으로 회전 중인 경우
        {
            currentAngle -= rotationSpeed * Time.deltaTime; // 현재 각도 감소
            if (currentAngle < -maxRotationAngle) // 최소 각도 초과 시 방향 변경
            {
                currentAngle = -maxRotationAngle;
                rotatingRight = true;
            }
        }

        transform.localEulerAngles = new Vector3(0, currentAngle, 0); // 로컬 회전 설정*/
    }

    public bool isTarget()
    {
        if (visibleTargets.Count > 0) // 감지된 타겟이 있는 경우
        {
            return true;
        }
        else return false; // 감지된 타겟이 없는 경우
    }
}
