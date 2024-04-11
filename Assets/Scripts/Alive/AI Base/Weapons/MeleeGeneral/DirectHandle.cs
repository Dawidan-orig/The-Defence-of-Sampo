using UnityEngine;

public class DirectHandle : MonoBehaviour
    // �������� �� Rigidbody; ������������� ����������� ��������.
    //TODO OPTIMIZATION : 6% �� ����� ��� ������ ���������� ������
{
    public Transform bladeHandle;
    public Rigidbody bladeTarget;

    public float maxAngularVelocity = 20;
    public float angularPower = 0.95f;

    private Vector3 desirePoint;

    private void FixedUpdate()
    {
        desirePoint = transform.position;

        bladeTarget.velocity = (desirePoint - bladeHandle.position) / Time.fixedDeltaTime;
        //bladeTarget.transform.rotation = transform.rotation;

        
        bladeTarget.maxAngularVelocity = maxAngularVelocity;
        Quaternion detailRot = transform.rotation * Quaternion.Inverse(bladeTarget.transform.rotation);
        Vector3 euler = new(Mathf.DeltaAngle(0, detailRot.eulerAngles.x),
            Mathf.DeltaAngle(0, detailRot.eulerAngles.y),
            Mathf.DeltaAngle(0, detailRot.eulerAngles.z));
        euler *= angularPower;
        euler *= Mathf.Deg2Rad;
        bladeTarget.angularVelocity = euler / Time.fixedDeltaTime;
    }
}
