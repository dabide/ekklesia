using Autofac;
using Ekklesia.Api.Common.Extensions;

namespace Ekklesia.Api.ServiceInterface
{
    public class ServiceInterfaceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(GetType().Assembly)
                .WithPublicConstructors()
                .AsDefaultInterface()
                .AsSelf();
        }
    }
}
