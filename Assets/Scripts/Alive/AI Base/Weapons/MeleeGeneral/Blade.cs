using UnityEngine;


namespace Sampo.Weaponry.Melee
{
    [RequireComponent(typeof(Rigidbody))]
    public class Blade : MeleeTool, IDamageDealer
    {
        //TODO DESIGN (����� ����� ������������� Melee) : �������� ���� ������� ������� (� ��������� �������, ���������������� �� ������, ��� ����������) � ������� �������� ������������ MeleeFighter.distanceFrom

        [Header("===Sword===")]
        public Transform upperPoint;
        public Transform downerPoint;
        [SerializeField]
        private Transform handle;

        [Header("lookonly")]
        public Vector3 AngularVelocityEuler;
        public Faction faction;
        

        [Header("Visuals")]
        public ParticleSystem sparkles;

        public Transform Handle { get => handle; private set => handle = value; }

        public Transform DamageFrom => host;

        public struct Border
        {
            public Vector3 posUp;
            public Vector3 posDown;
            public Vector3 direction;
        }

        protected override void Awake()
        {
            base.Awake();
            faction = GetComponent<Faction>();
        }

        private void Start()
        {
            if (host)
                Physics.IgnoreCollision(GetComponent<Collider>(), host.GetComponent<IDamagable>().Vital);

            GameObject massCenterGo = new("MassCenter");
            massCenterGo.transform.parent = transform;
            body.centerOfMass = handle.localPosition;
            massCenterGo.transform.position = body.worldCenterOfMass;

            additionalMeleeReach = Vector3.Distance(upperPoint.position, handle.position) / 2;
        }

        private void Update()
        {
            if (faction)
            {
                if (host)
                    faction.ChangeFactionCompletely(host.GetComponent<Faction>().FactionType);
                else
                    faction.ChangeFactionCompletely(Faction.FType.aggressive);
            }
        }

        /// <summary>
        /// ���������� ������������ ������� ����
        /// </summary>
        /// <param name="withDistance">����������, ������� ������ ������ ������</param>
        /// <returns>��� ������ � ������� ����</returns>
        public Border GetPrediction(float withDistance)
        {
            Border border = new();

            border.direction = body.velocity.normalized;

            float timeToFly = withDistance / body.velocity.magnitude;

            Quaternion rotationIteration = Quaternion.Euler(AngularVelocityEuler * timeToFly);

            Vector3 rotatedPosUp = upperPoint.position - transform.position;
            rotatedPosUp = rotationIteration * rotatedPosUp;
            border.posUp = transform.position + rotatedPosUp + (body.velocity * timeToFly);

            Vector3 rotatedPosDown = downerPoint.position - transform.position;
            rotatedPosDown = rotationIteration * rotatedPosDown;
            border.posDown = transform.position + rotatedPosDown + (body.velocity * timeToFly);

            return border;
        }

        private void FixedUpdate()
        {
            //TODO DESIGN : �������� ������� ��� �������� �������� �� ������� ��������
            AngularVelocityEuler = body.angularVelocity * 360 / (2 * Mathf.PI);
        }




        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(downerPoint.position, upperPoint.position);
        }
    }

}