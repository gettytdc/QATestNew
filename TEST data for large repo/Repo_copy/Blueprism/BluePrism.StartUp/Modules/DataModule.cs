namespace BluePrism.StartUp.Modules
{
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;

    using Autofac;

    public class DataModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SqlConnection>().As<IDbConnection>().ExternallyOwned();
            builder.RegisterType<SqlCommand>().AsSelf().ExternallyOwned();
            builder.Register<IDbCommand>(
                (context, parameters) =>
                {
                    var enumeratedParameters = parameters.OfType<TypedParameter>().ToList();
                    var command = context.Resolve<SqlCommand>(enumeratedParameters[0]);
                    command.CommandType = enumeratedParameters.Count > 1
                        ? enumeratedParameters.TypedAs<CommandType>()
                        : CommandType.Text;

                    return command;
                }).ExternallyOwned();
        }
    }
}