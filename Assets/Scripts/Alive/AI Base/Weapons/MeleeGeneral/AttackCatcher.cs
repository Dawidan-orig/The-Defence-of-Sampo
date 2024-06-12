using Sampo.AI;
using Sampo.Melee;
using Sampo.Weaponry.Melee;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sampo.Weaponry
{
    public class AttackCatcher : MonoBehaviour
    {
        [Header("init-s")]
        public float minDistance = 0.3f;
        public bool debug_Draw = true;
        public bool blade_as_stuff = false;
        [Min(0.75f)]
        [Tooltip("����������� �� CriticalDistance. ��� ���� - ��� ������ ������������ ����� ������.")]
        public float ignoredDistance = 10;
        [Tooltip("����������� ����������� ��������, ������� � ������� ������ ���� ������")]
        public float ignoredImpulse = 5;

        [Header("Setup")]
        [SerializeField]
        [Tooltip("���� �������� - ������ ������� �����")]
        private Collider checker;
        [SerializeField]
        [Tooltip("����� ��������� ������� ��������� �� ������")]
        private Collider vital;
        [Header("lookonly")]
        [SerializeField]
        private List<Rigidbody> ignored = new List<Rigidbody>();
        [SerializeField]
        [Tooltip("��� �� �����, �� �������� ���� ������� � ������� ������� �����")]
        private List<GameObject> controlled = new();

        public List<GameObject> Controlled { get => new List<GameObject>(controlled); }

        public class AttackEventArgs : EventArgs
        {
            public Rigidbody body;
            public bool free; /// <summary> ����������, ��� ����������� �� ����� ��������. <\summary>
            public Vector3 start;
            public Vector3 end;
            public Vector3 direction;
            public float impulse;
        }

        public event EventHandler<AttackEventArgs> OnIncomingAttack;

        private void Update()
        {
            foreach (GameObject thing in controlled)
            {
                if (!thing) // ���������� ������ ��� �������� �������
                    continue;

                if (thing.TryGetComponent(out Faction f))
                {
                    if (!f.IsWillingToAttack(GetComponent<Faction>().FactionType))
                        continue;
                }

                // � thing �������������� ���� Rigidbody. ��� ������� ���������� � ������.
                Rigidbody rb = thing.GetComponent<Rigidbody>();
                if (ignored.Contains(rb))
                    continue;

                if (rb.velocity.magnitude * rb.mass < ignoredImpulse)
                    continue;

                if (thing.TryGetComponent(out Blade blade))
                    if (blade.Host != null || !blade_as_stuff)
                    {
                        BladeIncoming(blade);
                        continue;
                    }

                StuffIncoming(rb);
            }
        }

        public void AddIgnoredObject(Rigidbody toAdd)
        {
            ignored.Add(toAdd);
        }

        private void BladeIncoming(Blade blade)
        {
            // ��� ��������� ��� �����: ���� �������� ���?
            Vector3 center = vital.bounds.center;
            Blade.Border predition = blade.GetPrediction(Vector3.Distance(center, blade.transform.position) - minDistance);
            Vector3 bladeCenter = Vector3.Lerp(predition.posUp, predition.posDown, 0.5f);
            Vector3 toVital = vital.bounds.ClosestPoint(predition.posUp) - bladeCenter;
            if (Vector3.Dot(predition.direction, toVital) < 0)
                return;

            if (debug_Draw)
            {
                Debug.DrawLine(predition.posUp, predition.posDown, Color.yellow);
                Debug.DrawRay(bladeCenter, predition.direction * 0.1f, Color.green);
                Debug.DrawLine(vital.bounds.center, predition.posDown, Color.yellow * 0.3f);
                Debug.DrawLine(vital.bounds.center, predition.posUp, Color.yellow * 0.3f);
            }

            OnIncomingAttack?.Invoke(this,
                new AttackEventArgs { body = blade.body, free = false, start = predition.posUp, end = predition.posDown, direction = predition.direction, impulse = blade.body.mass * blade.body.velocity.magnitude });
        }

        private void StuffIncoming(Rigidbody rb)
        {
            if (Vector3.Dot(rb.velocity, vital.bounds.center - rb.position) < 0)
                return;
            // ����� ����� � ������� ����� transform

            Vector3 center = vital.bounds.center;

            if ((rb.position - center).magnitude >= ignoredDistance)
                return;
            // ����� ��� ������!

            if (rb.velocity.magnitude * rb.mass < ignoredImpulse)
                return;
            // � ����� ���������� ������� ��������

            Vector3 predictionPoint = rb.position + rb.velocity.normalized * (Vector3.Distance(center, rb.position) - minDistance);

            // �����������
            if (Vector3.Distance(predictionPoint, vital.ClosestPointOnBounds(predictionPoint)) < minDistance)
                predictionPoint = predictionPoint + (rb.position - predictionPoint).normalized * minDistance;

            if (debug_Draw)
            {
                Debug.DrawLine(rb.position, predictionPoint, Color.yellow);
            }

            OnIncomingAttack?.Invoke(this, new AttackEventArgs { body = rb, direction = rb.velocity.normalized, start = predictionPoint, end = predictionPoint, free = true, impulse = rb.mass * rb.velocity.magnitude });
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.TryGetComponent<Rigidbody>(out _))
                return;

            if (!debug_Draw)
                return;

            /*
            if (collision.transform.TryGetComponent(out Blade blade))
            {
                if (TryGetComponent(out SwordFighter_StateMachine s) && blade == s?.Blade)
                {
                    //Debug.Log($"Selfslash at speed {blade.body.velocity.magnitude}", collision.transform);
                    Debug.DrawLine(blade.downerPoint.position, blade.upperPoint.position, new Color(0.8f, 0.2f, 0), 3);
                }
                else
                {
                    //Debug.Log($"Skipped slash at speed {blade.body.velocity.magnitude}", collision.transform);
                    Debug.DrawLine(blade.downerPoint.position, blade.upperPoint.position, new Color(0.5f, 0, 0), 3);
                }
            }
            else
            {
                Utilities.DrawSphere(collision.GetContact(0).point, color: Color.red, duration: 3);
                //Debug.Log($"Blunt damage at speed {collision.rigidbody.velocity.magnitude}", collision.transform);
            }*/
        }

        private void OnTriggerEnter(Collider other)
        {
            controlled.RemoveAll(item => item == null);

            Rigidbody body = other.GetComponent<Rigidbody>();
            if (body == null)
                return;
            // ��� ����� ����� ����������� �������������� ������������.

            // ����� ��� ����������, ����� ��������� ����������� ������ � ���� ���������.
            controlled.Add(body.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            // ������ ����� �� ����, �� ��� ������ �� ����� ��������� ���������.
            controlled.Remove(other.gameObject);
        }
    }
}