using System;
using System.Collections.Generic;
using HotChocolate.Configuration;
using HotChocolate.Language;

namespace HotChocolate.Types.Factories
{
    internal sealed class InterfaceTypeFactory
        : ITypeFactory<InterfaceTypeDefinitionNode, InterfaceType>
    {
        public InterfaceType Create(
            IBindingLookup bindingLookup,
            IReadOnlySchemaOptions schemaOptions,
            InterfaceTypeDefinitionNode node)
        {
            if (bindingLookup is null)
            {
                throw new ArgumentNullException(nameof(bindingLookup));
            }

            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            ITypeBindingInfo bindingInfo =
                bindingLookup.GetBindingInfo(node.Name.Value);

            return new InterfaceType(d =>
            {
                d.SyntaxNode(schemaOptions.KeepSyntaxNodes ? node : null)
                    .Name(node.Name.Value)
                    .Description(node.Description?.Value);

                if (bindingInfo.SourceType != null)
                {
                    d.Extend().OnBeforeCreate(
                        t => t.RuntimeType = bindingInfo.SourceType);
                }

                foreach (DirectiveNode directive in node.Directives)
                {
                    d.Directive(directive);
                }

                DeclareInterfaces(d, node.Interfaces);

                DeclareFields(schemaOptions, d, node.Fields);
            });
        }

        private static void DeclareInterfaces(
            IInterfaceTypeDescriptor typeDescriptor,
            IReadOnlyCollection<NamedTypeNode> interfaceReferences)
        {
            foreach (NamedTypeNode typeNode in interfaceReferences)
            {
                typeDescriptor.Interface(typeNode);
            }
        }

        private static void DeclareFields(
            IReadOnlySchemaOptions schemaOptions,
            IInterfaceTypeDescriptor typeDescriptor,
            IReadOnlyCollection<FieldDefinitionNode> fieldDefinitions)
        {
            foreach (FieldDefinitionNode fieldDefinition in fieldDefinitions)
            {
                IInterfaceFieldDescriptor fieldDescriptor = typeDescriptor
                    .Field(fieldDefinition.Name.Value)
                    .Description(fieldDefinition.Description?.Value)
                    .Type(fieldDefinition.Type)
                    .SyntaxNode(schemaOptions.KeepSyntaxNodes ? fieldDefinition : null);

                foreach (DirectiveNode directive in fieldDefinition.Directives)
                {
                    if (!directive.IsDeprecationReason())
                    {
                        fieldDescriptor.Directive(directive);
                    }
                }

                string deprecactionReason = fieldDefinition.DeprecationReason();
                if (!string.IsNullOrEmpty(deprecactionReason))
                {
                    fieldDescriptor.Deprecated(deprecactionReason);
                }

                DeclareFieldArguments(schemaOptions, fieldDescriptor, fieldDefinition);
            }
        }

        private static void DeclareFieldArguments(
            IReadOnlySchemaOptions schemaOptions,
            IInterfaceFieldDescriptor fieldDescriptor,
            FieldDefinitionNode fieldDefinition)
        {
            foreach (InputValueDefinitionNode inputFieldDefinition in
                fieldDefinition.Arguments)
            {
                fieldDescriptor.Argument(inputFieldDefinition.Name.Value,
                    a =>
                    {
                        foreach (DirectiveNode directive in
                            inputFieldDefinition.Directives)
                        {
                            fieldDescriptor.Directive(directive);
                        }

                        a.Description(inputFieldDefinition.Description?.Value)
                            .Type(inputFieldDefinition.Type)
                            .DefaultValue(inputFieldDefinition.DefaultValue)
                            .SyntaxNode(schemaOptions.KeepSyntaxNodes
                                ? inputFieldDefinition
                                : null);
                    });
            }
        }
    }
}
