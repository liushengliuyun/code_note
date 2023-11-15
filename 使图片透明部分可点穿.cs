namespace DefaultNamespace;



[RequireComponent(typeof(Image))]
public class 使图片透明部分可点穿: MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
    }
}