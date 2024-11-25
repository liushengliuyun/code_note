namespace My
{
    public class MyFramework: MyDontDestroyObj
    {
        public static MyFramework Instance;
        
        private void Start()
        {
            Instance = this;
        }
    }
}