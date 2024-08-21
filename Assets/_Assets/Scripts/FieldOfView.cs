using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FieldOfView;
using static UnityEngine.UI.Image;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius; // �þ� �ݰ�
    [Range(0, 360)]
    public float viewAngle; // �þ� ����

    public LayerMask targetMask; // Ÿ�� ���̾�
    public LayerMask obstacleMask1; // �þ� ���� ���� ������Ʈ ���̾�
    public LayerMask obstacleMask2; // �þ� ���Ǻ� ���� ������Ʈ ���̾�(�ɾ����� ��)

    public List<Transform> visibleTargets = new List<Transform>(); // ������ Ÿ�� ����Ʈ

    public float meshResolution; // �޽� �ػ�

    Mesh viewMesh; // �þ� �޽�
    public MeshFilter viewMeshFilter; // �޽� ����

    Mesh viewMesh2; // �þ� �޽�
    public MeshFilter viewMeshFilter2; // �޽� ����
    public GameObject viewMeshGroup;

    public Material activeMaterial; // �þ� Ȱ�� ���¿��� ����� ���׸���
    public Material inactiveMaterial; // �þ� ��Ȱ�� ���¿��� ����� ���׸���
    public Material obtacleMaterial;

    private MeshRenderer meshRenderer; // �޽� ������
    private MeshRenderer meshRenderer2; // �޽� ������

    public int edgeResolveIterations; // ���� �ػ�
    public float edgeDstThreshold; // ���� �Ÿ� �Ӱ谪

    public float rotationSpeed = 30f; // ȸ�� �ӵ�
    public float maxRotationAngle = 45f; // �ִ� ȸ�� ����

    private float currentAngle; // ���� ����
    private bool rotatingRight = true; // ���������� ȸ�� ������ ����
    private NinjaController player; // �÷��̾� ��Ʈ�ѷ�

    public bool isViewMeshVisible = false;
    void Start()
    {
        player = FindObjectOfType<NinjaController>(); // �÷��̾� ��Ʈ�ѷ� ã��
        viewMesh = new Mesh(); // ���ο� �޽� ����
        viewMesh.name = "View Mesh"; // �޽� �̸� ����
        viewMeshFilter.mesh = viewMesh; // �޽� ���Ϳ� �޽� �Ҵ�

        viewMesh2 = new Mesh(); // ���ο� �޽� ����
        viewMesh2.name = "View Mesh2"; // �޽� �̸� ����
        viewMeshFilter2.mesh = viewMesh2; // �޽� ���Ϳ� �޽� �Ҵ�

        meshRenderer = viewMeshFilter.GetComponent<MeshRenderer>(); // �޽� ������ �ʱ�ȭ
        meshRenderer2 = viewMeshFilter2.GetComponent<MeshRenderer>(); // �޽� ������ �ʱ�ȭ
        meshRenderer.material = inactiveMaterial; // �ʱ� ���·� ��Ȱ�� ���׸��� ����
        meshRenderer2.material = obtacleMaterial; // �ʱ� ���·� ��Ȱ�� ���׸��� ����

        StartCoroutine(FindTargetsWithDelay(0.2f)); // Ÿ�� ������ ���� �ڷ�ƾ ����
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay); // �־��� �ð���ŭ ���
            FindVisibleTargets(); // Ÿ�� ����
        }
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear(); // ������ Ÿ�� ����Ʈ �ʱ�ȭ
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask); // �þ� �ݰ� �� Ÿ�� Ž��
        //Physics.OverlapSphere(���� ��ġ, ����, ����ũ ����)
        //transform.position���κ��� viewRadius ������ŭ�� ��ü �ݰ濡 �ִ� targetMask ���̾ ���� �ݶ��̴��� targetsInViewRadius �迭�� ����

        for (int i = 0; i < targetsInViewRadius.Length; i++) //Ž���� Ÿ�ٵ��� �˻�
        {
            Transform target = targetsInViewRadius[i].transform; // ���� �ݺ� ���� Ÿ���� Ʈ�������� ������.
            // ���� ��ü�� Ÿ�� ������ ���� ���͸� ���ϰ�, ����ȭ.-> ���� ��ü���� Ÿ������ ���� ����
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            //Vector3.Angle(Vector3 from, Vector3 to) �� ���� ������ ������ ����Ͽ� ��ȯ(0~180)
            //���� ��ü�� ����(transform.forward)�� Ÿ�� ���� ����(dirToTarget) ������ ������ ����ϰ�, �װ��� viewAngle�� ���ݺ��� ������ Ȯ��
            //-> Ÿ���� ��ü�� �þ߰� ���� �ִ��� Ȯ��(viewAngle�� ���ݺ��� ������ Ÿ���� �þ߰� ���� �����ִٰ� �Ǵ�)
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2) //Ÿ���� �þ߰��� ������
            {
                float dstToTarget = Vector3.Distance(transform.position, target.transform.position); // Ÿ�ٰ��� �Ÿ� ���

                //Physics.Raycast(��� ��ġ, ��� ����, �������� ��� �ִ� �Ÿ�, ���̾��ũ)
                // obstacleMask1 üũ
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask1)) //�þ߰��� obstacleMask1 ���̾ ���� ������Ʈ�� ���� x
                {
                    // obstacleMask2 üũ
                    if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask2)) //�þ߰��� obstacleMask2 ���̾ ���� ������Ʈ�� ���� x
                    {
                        //�þ߰��� �ƹ��� ��ֹ��� �������� ���� ��� (obstacleMask1 x, obstacleMask2 x)
                        visibleTargets.Add(target); // Ÿ�� ����
                    }
                    else //�þ߰��� obstacleMask2 ���̾ ���� ������Ʈ�� ���� o (obstacleMask1 x, obstacleMask2 o)
                    {
                        // Obstacle2�� ������ ����� ó��
                        bool isCrouch = player != null && player.isSitting; // isCrouch = �÷��̾ �ɾ��ִ� ����
                        if (!isCrouch) //�÷��̾ �ɾ��ִ� ���°� �ƴ� ��� ������.
                        {
                            visibleTargets.Add(target); // ������ �������� ������ Ÿ�� �߰�
                        }
                    }
                }
            }
        }
    }

    // y�� ���Ϸ� ���� 3���� ���� ���ͷ� ��ȯ�Ѵ�.
    // ������ ������ ��¦ �ٸ��� ����. ����� ����.
    public Vector3 DirFromAngle(float angleDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Cos((-angleDegrees + 90) * Mathf.Deg2Rad), 0, Mathf.Sin((-angleDegrees + 90) * Mathf.Deg2Rad)); // ������ ���� ���� ���� ���
    }

    public struct ViewCastInfo //  Raycast�� ���� �� ray�� �����ϴ� ��ġ�� ǥ���ϴ� struct
    {
        public bool hit; // raycast�� hit ��������
        public Vector3 point; // ray�� ���������� ������ ��ġ
        public float dst; // ray�� ����
        public float angle; // ray�� �̷�� ��

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle) 
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    ViewCastInfo ViewCast(float globalAngle) // raycast ����� ViewCastInfo�� ��ȯ�ϴ� �޼���
    {
        Vector3 dir = DirFromAngle(globalAngle, true); // �־��� ������ ���� ���� ���� ���
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask1))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle); // ���� ���� ������Ʈ�� �¾��� ��
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle); // ��ֹ��� ���� ��
        }
    }

    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution); // �þ߰��� meshResolution���� ������ �޽��� ���е��� ����. stepCount = �þ߰��� �� ���� �������� ������
        float stepAngleSize = viewAngle / stepCount; // �� ������ ���� ũ��
        List<Vector3> viewPoints = new List<Vector3>(); // �þ� ����Ʈ ����Ʈ
        ViewCastInfo prevViewCast = new ViewCastInfo(); // ���� �þ� ĳ��Ʈ ����

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i; // ���� ���� ���
            ViewCastInfo newViewCast = ViewCast(angle); // ���ο� �þ� ĳ��Ʈ

            //�þ߰��� �������� ������ ���� �������� ViewCast �Լ��� ȣ���Ͽ�, �� ������ ������ �����ɴϴ�.�� ������ viewPoints ����Ʈ�� ����

            // i�� 0�̸� prevViewCast�� �ƹ� ���� ���� ���� ������ �� �� �����Ƿ� �ǳʶڴ�.
            if (i != 0)
            {
                bool edgeDstThresholdExceed = Mathf.Abs(prevViewCast.dst - newViewCast.dst) > edgeDstThreshold; // ���� �Ÿ� ���� ��

                // �� �� �� raycast�� ��ֹ��� ������ �ʾҰų� �� raycast�� ���� �ٸ� ��ֹ��� hit �� ���̶��(edgeDstThresholdExceed ���η� ���)
                if (prevViewCast.hit != newViewCast.hit || (prevViewCast.hit && newViewCast.hit && edgeDstThresholdExceed))
                {
                    Edge e = FindEdge(prevViewCast, newViewCast); //FindEdge �Լ��� ȣ���Ͽ� ������ ã��. �� ������ �þ߰� �޽��� ��踦 ����

                    // zero�� �ƴ� ������ �߰���
                    if (e.PointA != Vector3.zero)
                    {
                        viewPoints.Add(e.PointA); // ���� ����Ʈ A �߰�
                    }

                    if (e.PointB != Vector3.zero)
                    {
                        viewPoints.Add(e.PointB); // ���� ����Ʈ B �߰�
                    }
                }
            }

            viewPoints.Add(newViewCast.point); // ���ο� �þ� ����Ʈ �߰�
            prevViewCast = newViewCast; // ���� �þ� ĳ��Ʈ ���� ������Ʈ
        }

        int vertexCount = viewPoints.Count + 1; // ���� �� ���
        Vector3[] vertices = new Vector3[vertexCount]; // ���� �迭
        int[] triangles = new int[(vertexCount - 2) * 3]; // �ﰢ�� �迭
        vertices[0] = Vector3.zero; // �߽���

        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]); // ���� ��ǥ�� ��ȯ�Ͽ� ���� �߰�
            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0; // �ﰢ�� ù ��° ��
                triangles[i * 3 + 1] = i + 1; // �ﰢ�� �� ��° ��
                triangles[i * 3 + 2] = i + 2; // �ﰢ�� �� ��° ��
            }
        }
        viewMesh.Clear(); // ���� �޽� ������ �ʱ�ȭ
        viewMesh.vertices = vertices; // �� ���� ������ ����
        viewMesh.triangles = triangles; // �� �ﰢ�� ������ ����
        viewMesh.RecalculateNormals(); // ��� ���� ����

        // viewMesh2 ���� (��ֹ��� �����ϰ� ������ ������)
        float extendedViewRadius = viewRadius * 1.1f;
        vertexCount = stepCount + 2; // ���� �� ��� (��ֹ��� �����ϰ� ����)
        Vector3[] vertices2 = new Vector3[vertexCount];
        int[] triangles2 = new int[(vertexCount - 2) * 3];
        vertices2[0] = Vector3.zero; // �߽���

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

        // �þ� ���¿� ���� ���׸��� ����
        if (isTarget())
        {
            meshRenderer.material = activeMaterial; // Ÿ�� ������
            meshRenderer2.material = activeMaterial; // Ÿ�� ������
        }
        else
        {
            meshRenderer.material = inactiveMaterial; // Ÿ�� �������� ����
            meshRenderer2.material = obtacleMaterial; // Ÿ�� ������
        }
    }
    private void OnMouseDown()
    {
        Debug.Log("Ŭ��");
        isViewMeshVisible = !isViewMeshVisible; // �þ� �޽��� Ȱ��ȭ ���¸� ���
        meshRenderer.enabled = isViewMeshVisible;
        meshRenderer2.enabled = isViewMeshVisible;
    }


    public struct Edge
    {
        public Vector3 PointA, PointB; // ������ �� ��
        public Edge(Vector3 _PointA, Vector3 _PointB)
        {
            PointA = _PointA;
            PointB = _PointB;
        }
    }

    Edge FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle; // �ּ� ����
        float maxAngle = maxViewCast.angle; // �ִ� ����
        Vector3 minPoint = Vector3.zero; // �ּ� ����Ʈ
        Vector3 maxPoint = Vector3.zero; // �ִ� ����Ʈ

        for (int i = 0; i < edgeResolveIterations; i++) // ���� ã�� �ݺ�
        {
            float angle = minAngle + (maxAngle - minAngle) / 2; // �߾� ���� ���
            ViewCastInfo newViewCast = ViewCast(angle); // ���ο� �þ� ĳ��Ʈ
            bool edgeDstThresholdExceed = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold; // ���� �Ÿ� ���� ��
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceed)
            {
                minAngle = angle; // �ּ� ���� ������Ʈ
                minPoint = newViewCast.point; // �ּ� ����Ʈ ������Ʈ
            }
            else
            {
                maxAngle = angle; // �ִ� ���� ������Ʈ
                maxPoint = newViewCast.point; // �ִ� ����Ʈ ������Ʈ
            }
        }

        return new Edge(minPoint, maxPoint); // ���� ���� ��ȯ
    }

    private void Update()
    {
        if (isViewMeshVisible)
        {
            viewMeshGroup.SetActive(true);
            DrawFieldOfView(); // �þ� �޽� �׸���
        }
        else
        {
            viewMeshGroup.SetActive(false);
        }
        /*if (rotatingRight) // ���������� ȸ�� ���� ���
        {
            currentAngle += rotationSpeed * Time.deltaTime; // ���� ���� ����
            if (currentAngle > maxRotationAngle) // �ִ� ���� �ʰ� �� ���� ����
            {
                currentAngle = maxRotationAngle;
                rotatingRight = false;
            }
        }
        else // �������� ȸ�� ���� ���
        {
            currentAngle -= rotationSpeed * Time.deltaTime; // ���� ���� ����
            if (currentAngle < -maxRotationAngle) // �ּ� ���� �ʰ� �� ���� ����
            {
                currentAngle = -maxRotationAngle;
                rotatingRight = true;
            }
        }

        transform.localEulerAngles = new Vector3(0, currentAngle, 0); // ���� ȸ�� ����*/
    }

    public bool isTarget()
    {
        if (visibleTargets.Count > 0) // ������ Ÿ���� �ִ� ���
        {
            return true;
        }
        else return false; // ������ Ÿ���� ���� ���
    }
}
