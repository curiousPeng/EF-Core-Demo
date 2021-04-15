using Dal.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace EF_Core_Demo.Infrastructure.Extensions
{
    public static class DalsRegisterExtension
    {
        /// <summary>
        /// 请勿轻易修改dao和business的命名空间名称，否则无法注册
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterDals(this IServiceCollection services, IConfiguration configuration)
        {
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            ServerContext wlptServerContext = serviceProvider.GetService<ServerContext>();
            var dals = GetAllDals(configuration);
            foreach (var dal in dals)
            {
                services.AddScoped(dal, _ => System.Activator.CreateInstance(dal, args: wlptServerContext));
            }
            var blls = GetAllBusiness(configuration);
            foreach (var item in blls)
            {
                services.AddScoped(item.Key, item.Value);
            }

            return services;
        }
        private static Type[] GetAllDals(IConfiguration configuration)
        {
            var dalsName = configuration.GetSection("Dals").Value;
            var types = AppDomain.CurrentDomain.GetAssemblies()
                                 .SelectMany(a => a.GetTypes().Where(t => t.Namespace is not null && t.Namespace.Contains(dalsName)))
                                 .ToArray();
            return types;
        }
        private static Dictionary<Type, Type> GetAllBusiness(IConfiguration configuration)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            //.SelectMany(a => a.GetTypes().Where(t => t.GetNamespaces().Contains("WlptServer.Bll.Blls")))
            //.ToArray();
            Dictionary<Type, Type> result = new Dictionary<Type, Type>();
            Type[] interfaceBlls = new Type[0];
            Type[] blls = new Type[0];
            var bllsName = configuration.GetSection("Blls").Value;
            var modelName = configuration.GetSection("ModuleName").Value;
            var interfaceBllsName = configuration.GetSection("interfaceBlls").Value;
            foreach (var assembile in assemblies)
            {
                if (assembile.ManifestModule.Name == modelName)
                {
                    blls = assembile.GetTypes().Where(t => t.IsNested == false && t.Namespace is not null && t.Namespace.Contains(bllsName)).ToArray();
                }
                if (assembile.ManifestModule.Name == interfaceBllsName)
                {
                    interfaceBlls = assembile.GetTypes().Where(_ => _.IsInterface).ToArray();
                }
            }
            if (interfaceBlls.Count() < 1)
            {
                return result;
            }
            foreach (var item in interfaceBlls)
            {
                foreach (var bll in blls)
                {
                    if (bll.GetInterfaces().Contains(item) && item.Name.Contains(bll.Name))
                    {
                        result.Add(item, bll);
                    }
                }
            }
            return result;
        }
    }
}
