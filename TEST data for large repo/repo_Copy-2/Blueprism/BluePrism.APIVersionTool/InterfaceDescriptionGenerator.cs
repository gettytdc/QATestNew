using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Blueprism.APIVersionTool
{
    public class InterfaceDescriptionGenerator
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private IDictionary<string, Type> mDependantTypes = new SortedDictionary<string, Type>();

        private const int Indent = 4;

        public string GetDescription(Type type)
        {
            DiscoverTypes(type, includeMethods: true);
            mDependantTypes.Remove(type.FullName);

            var sb = new StringBuilder();
            sb.Append(TypeDescription(type, includeMethods: true));

            foreach (var dependentType in mDependantTypes.Values)
            {
                var description = TypeDescription(dependentType);
                if (description != null) sb.Append(description);
            }

            return sb.ToString();
        }

        private void DiscoverTypes(Type type, bool includeMethods = false)
        {
            if (type == null) return;

            string typeName = type.IsGenericType ? GenericClassName(type) : type.FullName;

            if (type.IsGenericType)
            {
                foreach (var genericType in type.GenericTypeArguments)
                {
                    DiscoverTypes(genericType);
                }
            }

            if (typeName == null || !IsBluePrismType(type) || mDependantTypes.ContainsKey(typeName))
                return;

            if (includeMethods)
            {
                if (!type.GetCustomAttributes<ServiceContractAttribute>().Any()) return;
            }
            else
            {
                if (!type.GetCustomAttributes<DataContractAttribute>().Any()) return;
            }

            if (type.IsArray || type.IsByRef)
            {
                DiscoverTypes(type.GetElementType());
                return;
            }

            mDependantTypes.Add(typeName, type);

            var knownTypes = type.GetCustomAttributes<KnownTypeAttribute>();
            foreach (var knownType in knownTypes)
            {
                if (knownType != null) DiscoverTypes(knownType.Type);
            }

            var fields = type.GetFields(Flags);
            foreach (var field in fields)
            {
                if (field.GetCustomAttributes<DataMemberAttribute>().Any()) DiscoverTypes(field.FieldType);
            }
            var properties = type.GetProperties(Flags);
            foreach (var prop in properties)
            {
                if (prop.GetCustomAttributes<DataMemberAttribute>().Any()) DiscoverTypes(prop.PropertyType);
            }
            if (includeMethods)
            {
                var methods = type.GetMethods();
                foreach (MethodInfo method in methods)
                {
                    foreach (var contract in method.GetCustomAttributes<FaultContractAttribute>())
                        DiscoverTypes(contract.DetailType);

                    DiscoverTypes(method.ReturnType);
                    var parameters = method.GetParameters();
                    foreach (var parameter in parameters)
                    {
                        DiscoverTypes(parameter.ParameterType);
                    }
                }
            }
            DiscoverTypes(type.BaseType);
            var interfaces = type.GetInterfaces();
            foreach (var iface in interfaces)
            {
                DiscoverTypes(iface, includeMethods);
            }
        }

        private bool IsBluePrismType(Type type)
        {
            return type.FullName.StartsWith("Automate", StringComparison.InvariantCultureIgnoreCase)
                || type.FullName.StartsWith("BluePrism", StringComparison.InvariantCultureIgnoreCase);
        }

        private string TypeDescription(Type type, bool includeMethods = false)
        {
            var sb = new StringBuilder();
            if (type.IsEnum)
            {
                sb.Append("enum ");
                sb.AppendLine(TypeSignature(type));
                sb.AppendLine("{");
                var names = type.GetEnumNames();
                foreach (var name in names)
                {
                    sb.Append(' ', Indent);
                    sb.Append(name);
                    sb.AppendLine(",");
                }
                sb.Length -= 3;
                sb.AppendLine();
                sb.AppendLine("};");
                return sb.ToString();
            }
            bool membersAdded = false;

            if (includeMethods)
                sb.Append(ServiceContractDescription(type));
            else
                sb.Append(TypeContractDescription(type));

            sb.Append("interface ");
            sb.Append(TypeSignature(type, includeTypeArguments: false));
            if (type.BaseType != null && type.BaseType != typeof(object))
            {
                sb.Append(" : ");
                var typeName = type.BaseType.FullName;
                if (type.IsGenericType)
                    typeName = GenericClassName(type);
                sb.Append(typeName);
            }
            var interfaces = type.GetInterfaces();
            foreach (var iface in interfaces)
            {
                sb.Append(", ");
                var typeName = iface.FullName;
                if (iface.IsGenericType)
                    typeName = GenericClassName(iface);
                sb.Append(typeName);
            }
            sb.AppendLine();
            sb.AppendLine("{");

            var fields = type.GetFields(Flags);
            foreach (var field in fields)
            {
                if (field.GetCustomAttributes<DataMemberAttribute>().Any())
                {
                    sb.Append(' ', Indent);
                    sb.Append(DataMemberDescription(field));
                    sb.Append(' ', Indent);
                    sb.Append(TypeSignature(field.FieldType));
                    sb.Append(" ");
                    sb.Append(field.Name);
                    sb.AppendLine(";");
                    membersAdded = true;
                }

            }
            var properties = type.GetProperties(Flags);
            foreach (var prop in properties)
            {
                if (prop.GetCustomAttributes<DataMemberAttribute>().Any())
                {
                    sb.Append(' ', Indent);
                    sb.Append(DataMemberDescription(prop));
                    sb.Append(' ', Indent);
                    sb.Append(TypeSignature(prop.PropertyType));
                    sb.Append(" ");
                    sb.Append(prop.Name);
                    sb.Append(" {");
                    if (prop.CanRead)
                    {
                        sb.Append(" get; ");
                    }
                    if (prop.CanWrite)
                    {
                        sb.Append(" set; ");
                    }
                    sb.AppendLine("};");
                    membersAdded = true;
                }

            }
            if (includeMethods)
            {
                var details = type.GetMethods().Select(m => new
                {
                    OperationContract = OperationContractDescription(m),
                    FaultContract = FaultContractDescription(m),
                    MethodSignature = MethodSignature(m)
                }
                ).OrderBy(x => x.MethodSignature);

                foreach (var detail in details)
                {
                    if (detail != null)
                    {
                        sb.Append(' ', Indent);
                        sb.Append(detail.OperationContract);
                        sb.Append(' ', Indent);
                        sb.Append(detail.FaultContract);
                        sb.Append(' ', Indent);
                        sb.AppendLine(detail.MethodSignature);
                        membersAdded = true;
                    }
                }
            }
            sb.AppendLine("};");
            if (!membersAdded) return null;

            return sb.ToString();
        }
        private string ServiceContractDescription(Type type)
        {
            var sb = new StringBuilder();
            foreach (var contract in type.GetCustomAttributes<ServiceContractAttribute>())
            {
                sb.Append("[ServiceContract(");
                sb.Append("SessionMode=");
                sb.Append(contract.SessionMode);
                sb.AppendLine(")]");
            }

            return sb.ToString();
        }

        private string OperationContractDescription(MethodInfo method)
        {
            var sb = new StringBuilder();
            foreach (var contract in method.GetCustomAttributes<OperationContractAttribute>())
            {
                sb.Append("[OperationContract(");
                if (!string.IsNullOrEmpty(contract.Name))
                {
                    sb.Append("Name=\"");
                    sb.Append(contract.Name);
                    sb.Append("\"");
                }
                sb.AppendLine(")]");
            }

            return sb.ToString();
        }

        private string FaultContractDescription(MethodInfo method)
        {
            var sb = new StringBuilder();
            foreach (var contract in method.GetCustomAttributes<FaultContractAttribute>())
            {
                sb.Append("[FaultContract(typeof(");
                sb.Append(TypeSignature(contract.DetailType));
                sb.AppendLine("))]");
            }

            return sb.ToString();
        }

        private string TypeContractDescription(Type type)
        {
            var sb = new StringBuilder();
            foreach (var contract in type.GetCustomAttributes<DataContractAttribute>())
            {
                sb.Append("[DataContract(");
                AddName(contract, sb);
                AddNamespace(contract, sb);
                AddReference(contract, sb);
                sb.AppendLine(")]");
            }

            return sb.ToString();
        }

        private static void AddReference(DataContractAttribute contract, StringBuilder sb)
        {
            if (contract.IsReferenceSetExplicitly)
            {
                if (contract.IsNamespaceSetExplicitly || contract.IsNameSetExplicitly)
                    sb.Append(",");
                sb.Append("IsReference=");
                sb.Append(contract.IsReference ? "true" : "false");
            }
        }

        private static void AddNamespace(DataContractAttribute contract, StringBuilder sb)
        {
            if (contract.IsNamespaceSetExplicitly)
            {
                if (contract.IsNameSetExplicitly)
                    sb.Append(",");
                sb.Append("Namespace=\"");
                sb.Append(contract.Namespace);
                sb.Append("\"");
            }
        }

        private static void AddName(DataContractAttribute contract, StringBuilder sb)
        {
            if (contract.IsNameSetExplicitly)
            {
                sb.Append("Name=\"");
                sb.Append(contract.Name);
                sb.Append("\"");
            }
        }

        private string DataMemberDescription(MemberInfo member)
        {
            var sb = new StringBuilder();
            foreach (var contract in member.GetCustomAttributes<DataMemberAttribute>())
            {
                sb.Append("[DataMember(");
                if (contract.IsNameSetExplicitly)
                {
                    sb.Append("Name=\"");
                    sb.Append(contract.Name);
                    sb.Append("\"");
                }

                if (!contract.IsDefaultAttribute())
                {
                    if (contract.IsNameSetExplicitly)
                        sb.Append(",");
                    sb.Append("EmitDefaultValue=");
                    sb.Append(contract.EmitDefaultValue ? "true" : "false");

                    sb.Append(",");
                    sb.Append("IsRequired=");
                    sb.Append(contract.IsRequired ? "true" : "false");
                }

                sb.AppendLine(")]");
            }

            return sb.ToString();
        }

        private string MethodSignature(MethodInfo method)
        {
            if (method.IsSpecialName) return null;
            if (!method.GetCustomAttributes<OperationContractAttribute>().Any()) return null;

            var sb = new StringBuilder();
            sb.Append(TypeSignature(method.ReturnType));
            sb.Append(" ");
            sb.Append(method.Name);
            var parameters = method.GetParameters();
            if (parameters.Count() > 0)
            {
                sb.Append("(");
                sb.Append(string.Join(",", parameters.Select(ParameterSignature)));
                sb.Append(")");
            }
            else
            {
                sb.Append("()");
            }
            sb.Append(";");
            return sb.ToString();
        }

        private string ParameterSignature(ParameterInfo parameter)
        {
            var sb = new StringBuilder();
            if (parameter.ParameterType.IsByRef)
            {
                sb.Append("ref ");
            }
            sb.Append(TypeSignature(parameter.ParameterType));
            sb.Append(" ");
            sb.Append(parameter.Name);
            if (parameter.HasDefaultValue)
            {
                if (parameter.DefaultValue == null)
                {
                    sb.Append("null");
                }
                else
                {
                    sb.Append("=default(");
                    sb.Append(TypeSignature(parameter.DefaultValue.GetType()));
                    sb.Append(")");
                }
            }
            return sb.ToString();
        }

        private string TypeSignature(Type type, bool includeTypeArguments = true)
        {
            if (type.IsByRef)
                return TypeSignature(type.GetElementType());

            if (!type.IsGenericType)
                return type.FullName;

            var sb = new StringBuilder();
            sb.Append(type.Namespace);
            sb.Append(".");
            sb.Append(GenericClassName(type));
            if (includeTypeArguments)
            {
                sb.Append("<");
                var arguments = type.GetGenericArguments();
                sb.Append(string.Join(",", arguments.Select(argument => TypeSignature(argument))));
                sb.Append(">");
            }
            else
            {
                sb.Append("<T>");
            }
            return sb.ToString();
        }

        private string GenericClassName(Type type)
        {
            string typeName = type.Name;
            int index = typeName.IndexOf("`");
            if (index < 0) return typeName;
            return typeName.Remove(index);
        }
    }
}