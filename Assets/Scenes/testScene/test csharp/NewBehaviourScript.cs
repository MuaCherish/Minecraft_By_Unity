//using System.Collections;
//using UnityEngine;

//public class NewBehaviourScript : MonoBehaviour
//{
//    public float angle = 50f;
//    public float cycleLength = 16f; // �������ڳ���
//    public float speed = 200f; // ����ʱ��������ٶ� 

//    void Update()
//    {
//        if (Input.GetMouseButtonDown(1))
//        {
//            StartCoroutine(Animation_Behurt());
//        }
//    }

//    IEnumerator Animation_Behurt()
//    {
//        Vector3 startRotation = transform.localRotation.eulerAngles;
//        Vector3 targetRotation = startRotation + new Vector3(0f, 0f, angle);

//        float elapsedTime = 0f;
//        while (elapsedTime < cycleLength / 2f)
//        {
//            elapsedTime += Time.deltaTime * speed;
//            transform.localRotation = Quaternion.Euler(Vector3.Lerp(startRotation, targetRotation, elapsedTime / (cycleLength / 2f)));
//            yield return null;
//        }

//        elapsedTime = 0f;
//        while (elapsedTime < cycleLength / 2f)
//        {
//            elapsedTime += Time.deltaTime * speed;
//            transform.localRotation = Quaternion.Euler(Vector3.Lerp(targetRotation, startRotation, elapsedTime / (cycleLength / 2f)));
//            yield return null;
//        }

//        transform.localRotation = Quaternion.Euler(startRotation); // ��֤��������ʱ����ص���ʼ�Ƕ�
//    }
//}
