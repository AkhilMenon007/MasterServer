using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using MasterServer.Services.Client;

namespace MasterServer.Services
{
    public static class ClientServiceManager
    {
        public static List<IClientService> RetrieveServices() 
        {
            List<IClientService> services = new List<IClientService>();

            var type = typeof(IClientService);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && type.Name.Contains("Service")&& !p.IsInterface);
            foreach (var item in types)
            {
                services.Add((IClientService)Activator.CreateInstance(item));
                Console.WriteLine(item.Name);
            }

            return services;
        }
    }
}
