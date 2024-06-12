using Sampo.AI;
using UnityEngine;


namespace Sampo.Weaponry.Melee
{
    [RequireComponent(typeof(Rigidbody))]
    public class Blade : MeleeTool, IDamageDealer
    {
        //TODO DESIGN (Когда будет вариативность Melee) : Добавить сюда понятие рукояти (И основного объекта, контроллирующего всё оружие, как следествие) и угловое движение относительно MeleeFighter.distanceFrom

        [Header("===Sword===")]
        public Transform upperPoint;
        public Transform downerPoint;
        [SerializeField]
        private Transform handle;

        [Header("lookonly")]
        public Vector3 AngularVelocityEuler;        

        [Header("Visuals")]
        public ParticleSystem sparkles;

        public Transform Handle { get => handle; private set => handle = value; }
        public Transform DamageFrom => _host;

        public struct Border
        {
            public Vector3 posUp;
            public Vector3 posDown;
            public Vector3 direction;
        }

        private void Start()
        {
            GameObject massCenterGo = new("MassCenter");
            massCenterGo.transform.parent = transform;
            body.centerOfMass = handle.localPosition;
            massCenterGo.transform.position = body.worldCenterOfMass;

            additionalMeleeReach = Vector3.Distance(upperPoint.position, handle.position) / 2;
        }

        /// <summary>
        /// Возвращает предсказание позиции меча
        /// </summary>
        /// <param name="withDistance">Расстояние, которое должно пройти лезвие</param>
        /// <returns>Все данные о позиции меча</returns>
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
            //TODO DESIGN : Добавить систему для проверки коллизии на высокой скорости
            AngularVelocityEuler = body.angularVelocity * 360 / (2 * Mathf.PI);
        }




        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(downerPoint.position, upperPoint.position);
        }
    }

}