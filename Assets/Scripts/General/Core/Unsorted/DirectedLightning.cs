using System.Collections.Generic;
using UnityEngine;

namespace WingedCore.ToSort
{
    public class DirectedLightning : MonoBehaviour
    {
        [Header("Глобально")]
        [Tooltip("Угол проверки направленности молнии")]
        public float spreadDeg = 30;
        [Tooltip("Сколько кадров вся молния будет затухать визуально")]
        public float decayTimeSec = 2;
        [Tooltip("Максимальная длина фундаментальной линии молнии")]
        public float lightningMaxLength = 30;
        [Tooltip("Фрагменты изначальной ломаной линии")]
        public float fundamentalFragments = 5;
        [Tooltip("Количество линий продвижения за кадр")]
        public int proceedAmountInFrame = 1;
        [Header("Ветвь")]
        [Tooltip("Доля длины ветви относительно прародителя")]
        [Range(0,1)]
        public float branchLengthPerc = 0.7f;
        [Tooltip("Случайный разброс длины порождаемой ветви")]
        [Range(0, 1)]
        public float branchLengthRandomPerc = 0.2f;
        [Tooltip("Вероятность одного разветвления")]
        [Range(0, 1)]
        public float spreadPossibility = 0.2f;
        [Tooltip("Количество проверок на разветвление")]
        public float spreadMaxAmount = 6;
        //TODO: Контроль LineRenderer

        Vector3 direction;
        bool isDecaying = false;
        bool isStarted = false;

        private float currentDecayTime = 0;
        private Vector3 initialStartPoint;

        private struct Branch 
        {
            public Vector3 startPoint;
            public Vector3 direction;
            public float currentDecay;
            public List<Branch> subBranches;
        }

        public void InitiateLightning(Vector3 direction) 
        {
            isStarted = true;
            this.direction = direction;
            initialStartPoint = transform.position;

            DoLightningFundamental(transform.position, direction * lightningMaxLength);
        }

        private void Update()
        {
            if (!isStarted) return;

            if(isDecaying)
                currentDecayTime += Time.deltaTime;
            if(currentDecayTime >= decayTimeSec)            
                Destroy(gameObject);
        }
        /// <summary>
        /// Проведение прямой ломаной от начала до цели
        /// </summary>
        private void DoLightningFundamental(Vector3 from, Vector3 to) 
        {
            //TODO : Молния идёт постепенно, а затем, найдя разрядку, полностью высаживается в неё.

            RaycastHit target = GetNextHitPoint(from, to);

            //TODO : Нужно сделать кривую, по сути, вдоль которой ломаная пойдёт.
            /*Как это работает:
             * Сначала молния старается отдалится от фундаментального центра и от цели
             * Потом наоборот - приближается к цели
             */
            for(int i = 0; i < fundamentalFragments; i++) 
            {
                
            }
            
        }
        /// <summary>
        /// Создание ветвей молнии по её стволу
        /// </summary>
        private void DoBranchesStart(Vector3 from, Vector3 to, Vector3 target, float passedDistLeft) 
        {
            float toTargetDist = Vector3.Distance(from, target);
            float branchLength = toTargetDist * branchLengthPerc;
            float randomizationPerc = toTargetDist * branchLengthRandomPerc;
            float branchRandomization = Random.Range(-randomizationPerc, randomizationPerc);
            branchLength += branchRandomization;

            Vector3 getFromTargetDir = GetRandomDir((to - from).normalized, spreadDeg);

            RaycastHit hit = GetNextHitPoint(to, getFromTargetDir * branchLength);

            float passedDist = Vector3.Distance(from, hit.point);
            DoBranchesVisual(to, hit.point, target, passedDistLeft - passedDist);
        }
        /// <summary>
        /// Создание ветвей молнии от центрального ствола
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="target"></param>
        private void DoBranchesVisual(Vector3 from, Vector3 to, Vector3 target, float remainingDist) 
        {
            if (remainingDist < 0) return;

            float toTargetDist = Vector3.Distance(from, target);
            float branchLength = toTargetDist * branchLengthPerc;
            float randomizationPerc = toTargetDist * branchLengthRandomPerc;
            float branchRandomization = Random.Range(-randomizationPerc, randomizationPerc);
            branchLength += branchRandomization;

            //Изначальное направление
            Vector3 getFromTargetDir = (target - from).normalized;
            //Сдвигаем куда-нибудь случайно
            getFromTargetDir = GetRandomDir(getFromTargetDir, spreadDeg);
            Vector3 currentPoint = getFromTargetDir * branchLength;
            Vector3 nearestPoint = WingedCore.Core.Utility.MathHelper.NearestPointOnLine(to, getFromTargetDir, currentPoint);
            //Сдвигаем от центральной линии и цели дальше в сторону
            getFromTargetDir += -(currentPoint - nearestPoint).normalized;

            RaycastHit hit = GetNextHitPoint(to, getFromTargetDir * branchLength);

            float passedDist = Vector3.Distance(from, hit.point);
            DoBranchesVisual(to, hit.point, target, remainingDist - passedDist);
        }
        private RaycastHit GetNextHitPoint(Vector3 from, Vector3 to) 
        {
            RaycastHit[] hits = Physics.SphereCastAll(from, Vector3.Distance(from, to), direction);
            RaycastHit most = hits[0];
            //TODO : Изучить результат. Вдруг он как-то уже отсортирован? Позволит обрезать всё лишнее одним break.
            foreach (var hit in hits)
            {
                Vector3 dirToTarget = hit.point - from;
                float resAngle = Vector3.Angle(dirToTarget, direction);
                if (resAngle > spreadDeg) continue;

                if (Vector3.Distance(hit.point, from)
                    < Vector3.Distance(most.point, from))
                    most = hit;
            }

            if(hits.Length > 0)
                return most;
            else 
            {
                most = new RaycastHit();
                Vector3 dir = (to - from).normalized;
                most.point = initialStartPoint + GetRandomDir(dir, spreadDeg);
                return most;
            }
        }
        private Vector3 GetRandomDir(Vector3 initialDir, float spread) 
        {
            return Quaternion.Euler(Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                0)
                * initialDir;
        }
    }
}