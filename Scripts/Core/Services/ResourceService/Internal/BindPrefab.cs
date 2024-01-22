using System;

namespace Core.Services.ResourceService.Internal
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BindPrefab : Attribute
    {
        public string path;

        public BindPrefab(string path)
        {
            this.path = "Assets/" + path + ".prefab";
        }
    }
}