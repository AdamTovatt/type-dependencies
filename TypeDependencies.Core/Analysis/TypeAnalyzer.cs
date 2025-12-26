using Mono.Cecil;
using TypeDependencies.Core.Models;

namespace TypeDependencies.Core.Analysis
{
    public class TypeAnalyzer : ITypeAnalyzer
    {
        public DependencyGraph AnalyzeAssembly(string assemblyPath)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
                throw new ArgumentException("Assembly path cannot be null or empty.", nameof(assemblyPath));

            if (!File.Exists(assemblyPath))
                throw new FileNotFoundException($"Assembly not found: {assemblyPath}", assemblyPath);

            DependencyGraph dependencyGraph = new DependencyGraph();
            ReaderParameters readerParameters = new ReaderParameters
            {
                ReadWrite = false,
            };

            AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath, readerParameters);

            foreach (ModuleDefinition module in assemblyDefinition.Modules)
            {
                foreach (TypeDefinition type in module.Types)
                {
                    if (type.IsSpecialName || type.IsRuntimeSpecialName)
                        continue;

                    string typeName = GetFullTypeName(type);
                    AnalyzeType(type, typeName, dependencyGraph);
                }
            }

            return dependencyGraph;
        }

        private void AnalyzeType(TypeDefinition type, string typeName, DependencyGraph dependencyGraph)
        {
            // Base type
            if (type.BaseType != null && !IsSystemType(type.BaseType))
            {
                string baseTypeName = GetFullTypeName(type.BaseType);
                dependencyGraph.AddDependency(typeName, baseTypeName);
            }

            // Interfaces
            foreach (InterfaceImplementation interfaceImplementation in type.Interfaces)
            {
                if (!IsSystemType(interfaceImplementation.InterfaceType))
                {
                    string interfaceName = GetFullTypeName(interfaceImplementation.InterfaceType);
                    dependencyGraph.AddDependency(typeName, interfaceName);
                }
            }

            // Fields
            foreach (FieldDefinition field in type.Fields)
            {
                if (!IsSystemType(field.FieldType))
                {
                    string fieldTypeName = GetFullTypeName(field.FieldType);
                    dependencyGraph.AddDependency(typeName, fieldTypeName);
                }
            }

            // Properties
            foreach (PropertyDefinition property in type.Properties)
            {
                if (!IsSystemType(property.PropertyType))
                {
                    string propertyTypeName = GetFullTypeName(property.PropertyType);
                    dependencyGraph.AddDependency(typeName, propertyTypeName);
                }
            }

            // Methods
            foreach (MethodDefinition method in type.Methods)
            {
                // Return type
                if (method.ReturnType != null && !IsSystemType(method.ReturnType))
                {
                    string returnTypeName = GetFullTypeName(method.ReturnType);
                    dependencyGraph.AddDependency(typeName, returnTypeName);
                }

                // Parameter types
                foreach (ParameterDefinition parameter in method.Parameters)
                {
                    if (!IsSystemType(parameter.ParameterType))
                    {
                        string parameterTypeName = GetFullTypeName(parameter.ParameterType);
                        dependencyGraph.AddDependency(typeName, parameterTypeName);
                    }
                }
            }

            // Attributes
            foreach (CustomAttribute attribute in type.CustomAttributes)
            {
                if (!IsSystemType(attribute.AttributeType))
                {
                    string attributeTypeName = GetFullTypeName(attribute.AttributeType);
                    dependencyGraph.AddDependency(typeName, attributeTypeName);
                }

                // Constructor arguments
                foreach (CustomAttributeArgument argument in attribute.ConstructorArguments)
                {
                    if (argument.Type != null && !IsSystemType(argument.Type))
                    {
                        string argumentTypeName = GetFullTypeName(argument.Type);
                        dependencyGraph.AddDependency(typeName, argumentTypeName);
                    }
                }
            }

            // Generic arguments
            if (type.HasGenericParameters)
            {
                foreach (GenericParameter genericParameter in type.GenericParameters)
                {
                    foreach (GenericParameterConstraint constraint in genericParameter.Constraints)
                    {
                        if (constraint.ConstraintType != null && !IsSystemType(constraint.ConstraintType))
                        {
                            string constraintTypeName = GetFullTypeName(constraint.ConstraintType);
                            dependencyGraph.AddDependency(typeName, constraintTypeName);
                        }
                    }
                }
            }
        }

        private string GetFullTypeName(TypeReference typeReference)
        {
            if (typeReference == null)
                return string.Empty;

            // Handle generic types
            if (typeReference is GenericInstanceType genericInstanceType)
            {
                string baseName = GetFullTypeName(genericInstanceType.ElementType);
                return baseName;
            }

            // Handle array types
            if (typeReference is ArrayType arrayType)
            {
                return GetFullTypeName(arrayType.ElementType) + "[]";
            }

            // Handle pointer types
            if (typeReference is PointerType pointerType)
            {
                return GetFullTypeName(pointerType.ElementType) + "*";
            }

            // Handle by-reference types
            if (typeReference is ByReferenceType byReferenceType)
            {
                return GetFullTypeName(byReferenceType.ElementType) + "&";
            }

            // Handle generic parameters
            if (typeReference is GenericParameter genericParameter)
            {
                return genericParameter.Name;
            }

            // Get the full name, handling nested types
            string fullName = typeReference.FullName;

            // Remove assembly information if present
            int commaIndex = fullName.IndexOf(',');
            if (commaIndex >= 0)
            {
                fullName = fullName.Substring(0, commaIndex);
            }

            // Replace '/' with '+' for nested types (Mono.Cecil uses '/' but .NET uses '+')
            fullName = fullName.Replace('/', '+');

            return fullName;
        }

        private bool IsSystemType(TypeReference typeReference)
        {
            if (typeReference == null)
                return true;

            string namespaceName = typeReference.Namespace ?? string.Empty;
            return namespaceName.StartsWith("System", StringComparison.Ordinal) ||
                   namespaceName.StartsWith("Microsoft", StringComparison.Ordinal) ||
                   typeReference.FullName.StartsWith("System.", StringComparison.Ordinal) ||
                   typeReference.FullName.StartsWith("Microsoft.", StringComparison.Ordinal);
        }
    }
}

