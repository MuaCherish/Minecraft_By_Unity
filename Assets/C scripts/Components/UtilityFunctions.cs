using UnityEngine;

namespace MCEntity
{
    //ʵ��ӿ�
    public interface IEntity
    {
        // ��ʼ��
        public void OnStartEntity();

        // ����
        public void OnEndEntity();
    }

    //һЩEntity��Ҫ�Ĺ��ú���
    public static class MC_UtilityFunctions
    {
        /// <summary>
        /// ���߼���ж��Ƿ񿴵�ĳ����
        /// </summary>
        public static bool IsTargetVisible(Vector3 _targetPos)
        {
            // ʵ�����߼����߼�
            return true;
        }

        /// <summary>
        /// ���������������ˮƽԲ�������ѡ��
        /// </summary>
        /// <param name="center">Բ������</param>
        /// <param name="radius">Բ�İ뾶</param>
        /// <returns>����������ɵ�ˮƽ��(Vector3)</returns>
        public static Vector3 GetRandomPointInCircle(Vector3 center, float radius)
        {
            // �������һ���Ƕȣ������ƣ�
            float angle = Random.Range(0f, Mathf.PI * 2);

            // �������һ�����룬ȷ���ڰ뾶��Χ��
            float randomRadius = Random.Range(0f, radius);

            // �����������ˮƽƽ���ϵ�x��z����
            float xOffset = Mathf.Cos(angle) * randomRadius;
            float zOffset = Mathf.Sin(angle) * randomRadius;

            // ������������������
            return new Vector3(center.x + xOffset, center.y, center.z + zOffset);
        }

    }

}
