using System.Linq;
using StrawberryShake.CodeGeneration.Analyzers.Models;

namespace StrawberryShake.CodeGeneration.Mappers
{
    public static class DependencyInjectionMapper
    {
        public static void Map(ClientModel model, IMapperContext context)
        {
            context.Register(
                new DependencyInjectionDescriptor(
                    context.ClientName,
                    context.Namespace,
                    context.EntityTypes.ToList(),
                    context.Operations.ToList(),
                    context.Types));
        }
    }
}
