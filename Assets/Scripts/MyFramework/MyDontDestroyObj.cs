using UnityEngine;

namespace My
{
    public class MyDontDestroyObj : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}