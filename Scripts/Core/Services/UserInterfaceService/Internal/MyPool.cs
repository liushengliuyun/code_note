using Core.Services.ResourceService;
using Core.Services.ResourceService.Internal.UniPooling;

namespace Core.Services.UserInterfaceService.Internal
{
    public class MyPool
    {
        private Spawner spawner;

        public Spawner Spawner
        {
            get
            {
                if (spawner == null)
                {
                    spawner = UniPooling.CreateSpawner(ResourceSystem.DefaultPackage);
                }

                return spawner;
            }
            set { spawner = value; }
        }
    }
}