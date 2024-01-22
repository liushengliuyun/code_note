/*
 * This file is part of the CatLib package.
 *
 * (c) CatLib <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 *
 * Document: http://catlib.io/
 */

using System.Collections.Generic;
using System.Linq;
using CatLib;
using UnityEngine;

namespace Core.Runtime.Game
{
    /// <summary>
    /// Represents a service provider bootstrap.
    /// </summary>
    public sealed class BootstrapProviderRegister : IBootstrap
    {
        private readonly IServiceProvider[] _providers;
        private readonly Component _component;

        /// <summary>
        /// Initializes a new instance of the <see cref="BootstrapProviderRegister"/> class.
        /// </summary>
        /// <param name="component">Unity root GameObject object.</param>
        /// <param name="serviceProviders">An array of service providers.</param>
        public BootstrapProviderRegister(Component component, IServiceProvider[] serviceProviders = null)
        {
            _providers = serviceProviders;
            _component = component;
        }

        /// <inheritdoc />
        public void Bootstrap()
        {
            LoadUnityComponentProvider();
            RegisterProviders(_providers);
        }

        /// <summary>
        /// Service provider that loads Unity components.
        /// </summary>
        private void LoadUnityComponentProvider()
        {
            if (!_component)
            {
                return;
            }

            int GetChildIndex(Object com)
            {
                for (var i = 0; i < _component.transform.childCount; i++)
                {
                    if (_component.transform.GetChild(i) == com)
                    {
                        return i;
                    }
                }

                return -1;
            }

            var componentsInChildren = _component.GetComponentsInChildren<IServiceProvider>().ToList();

            componentsInChildren.Sort((a, b) =>
                GetChildIndex(((Component)a).transform) - GetChildIndex(((Component)b).transform));

            RegisterProviders(componentsInChildren);
        }

        /// <summary>
        /// Register service provider to the framework.
        /// </summary>
        private static void RegisterProviders(IEnumerable<IServiceProvider> providers)
        {
            foreach (var provider in providers)
            {
                if (provider == null)
                {   
                    continue;
                }

                if (!App.IsRegistered(provider))
                {
                    App.Register(provider);
                }
            }
        }
    }
}