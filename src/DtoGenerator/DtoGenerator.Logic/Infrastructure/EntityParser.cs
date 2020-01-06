﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DtoGenerator.Logic.Infrastructure.TreeProcessing;
using DtoGenerator.Logic.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DtoGenerator.Logic.Infrastructure
{
    public class EntityParser
    {
        private static List<Type> _simpleTypes = new List<Type>()
        {
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid),
            typeof(byte), typeof(System.Byte),
            typeof(sbyte), typeof(System.SByte),
            typeof(char), typeof(System.Char),
            typeof(decimal), typeof(System.Decimal),
            typeof(double), typeof(System.Double),
            typeof(float), typeof(System.Single),
            typeof(int), typeof(System.Int32),
            typeof(uint), typeof(System.UInt32),
            typeof(long), typeof(System.Int64),
            typeof(ulong), typeof(System.UInt64),
            typeof(short), typeof(System.Int16),
            typeof(ushort), typeof(System.UInt16),
            typeof(string), typeof(System.String)
        };

        private static List<string> _classDataAnnotationToPreserve = new List<string>()
        {
            "MetadataType"
        };

        private static List<string> _attributDataAnnotationToPreserve = new List<string>()
        {
            "Key",
            "TimeStamp",
            "ConcurrencyCheck",
            "MaxLength",
            "MinLength",
            "ForeignKey",
            "DisplayName",
            "DisplayFormat",
            "Required",
            "StringLength",
            "RegularExpression",
            "Range",
            "DataType",
            "Validation"
        };

        public static EntityMetadata FromString(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            return FromSyntaxTree(syntaxTree);
        }

        public static async Task<EntityMetadata> FromDocument(Document doc, bool includeInherited = false)
        {
            var syntaxTree = doc.GetSyntaxTreeAsync().Result;
            var metadata = FromSyntaxTree(syntaxTree);

            if(includeInherited)
            {
                var baseDoc = await doc.GetRelatedEntityDocument(metadata.BaseClassName);

                if (baseDoc != null)
                {
                    var baseMetadata = await EntityParser.FromDocument(baseDoc, includeInherited: true);
                    foreach (var prop in baseMetadata.Properties)
                        prop.IsInherited = true;

                    foreach(var baseProp in baseMetadata.Properties)
                    {
                        if (metadata.Properties.Any(p => p.Name == baseProp.Name))
                            continue;

                        metadata.Properties.Add(baseProp);
                    }
                }
            }

            return metadata;
        }

        public static async Task<bool> HasBaseDto(Document existingDto, string baseDtoName)
        {
            if (existingDto == null)
                return false;

            var existingRoot = await existingDto.GetSyntaxRootAsync();

            var finder = new BaseDtoClassLocator();
            finder.Visit(existingRoot);

            return finder.BaseDtoName == baseDtoName;
        }

        public static async Task<bool> HasDataAnnotations(Document existingDto)
        {
            if (existingDto == null)
                return false;

            var existingRoot = await existingDto.GetSyntaxRootAsync();
            if (existingRoot.ToString().Contains("[MetadataType"))
                return true;
            
            var classNodes = GetClassNodes(existingRoot);
            var classNode = classNodes.First();
            var properties = GetProperties(classNode);

            return properties.Any(p => p.AttributeLists.Any(a => a.Attributes.Any(att => _attributDataAnnotationToPreserve.Contains(att.Name.ToString()))));
        }
        public static async Task<bool> HasStyleCop(Document existingDto)
        {
            if (existingDto == null)
                return false;

            var existingRoot = await existingDto.GetSyntaxRootAsync();
            if (existingRoot.ToString().Contains("#pragma warning disable CS1591"))
                return true;

            return false;
        }
        public static async Task<bool> HasDataContract(Document existingDto)
        {
            if (existingDto == null)
                return false;

            var existingRoot = await existingDto.GetSyntaxRootAsync();

            return existingRoot.ToString().Contains("[DataContract]");
        }
        public static async Task<bool> HasEntities(Document doc, Document existingDto, bool defaultValue)
        {
            if (existingDto == null)
                return defaultValue;

            var existingRoot = await existingDto.GetSyntaxRootAsync();

            var classNodes = GetClassNodes(existingRoot);
            var classNode = classNodes.First();
            var properties = GetProperties(classNode);

            if(defaultValue==true)
            {
                EntityMetadata originalMetadata = await EntityParser.FromDocument(doc, includeInherited: true);

                foreach (PropertyMetadata prop in originalMetadata.Properties)
                {
                    if (prop.IsRelation)
                    {
                        if (properties.Any(p => p.Identifier.Text.Length > prop.Name.Length && p.Identifier.Text.Substring(0, prop.Name.Length) == prop.Name) && !(properties.Any(p => (p.Identifier.Text == prop.Name) && (p.Type.ToString().Length > 3 && p.Type.ToString().Substring(p.Type.ToString().Length - 3, 3) == "DTO"))))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            else
            {
                return properties.Any(p => (p.Type.ToString().Length > 3 && p.Type.ToString().Substring(p.Type.ToString().Length - 3, 3) == "DTO")
                || (p.Type.ToString().Length > 14 && p.Type.ToString().Substring(0, 11) == "ICollection" && p.Type.ToString().Substring(p.Type.ToString().Length - 4, 3) == "DTO")
                );
            }
        }


        public static async Task<bool> HasMapEntitiesById(Document existingDto, bool defaultValue)
        {
            if (existingDto == null)
                return defaultValue;

            var existingRoot = await existingDto.GetSyntaxRootAsync();

            var classNodes = GetClassNodes(existingRoot);
            var classNode = classNodes.First();
            var properties = GetProperties(classNode);

            if (properties.Any(p => (p.Type.ToString().Length > 3 && p.Type.ToString().Substring(p.Type.ToString().Length - 3, 3) == "DTO")
                || (p.Type.ToString().Length > 14 && p.Type.ToString().Substring(0, 11) == "ICollection" && p.Type.ToString().Substring(p.Type.ToString().Length - 4, 3) == "DTO")
                ))
            {
                var propname = properties.Select(p => p.Identifier.ToString());

                return propname.Any(p => propname.Contains(p + "Id") || propname.Contains(p + "Ids"));
            }

            return defaultValue;
        }



        public static async Task<List<string>> GetAutoGeneratedProperties(Document existingDto)
        {
            if (existingDto == null)
                return null;

            var existingRoot = await existingDto.GetSyntaxRootAsync();

            var finder = new CustomCodeLocator();
            finder.Visit(existingRoot);

            var propFinder = new GeneratedPropertiesEnumerator(finder);
            propFinder.Visit(existingRoot);

            return propFinder.GeneratedProperties;
        }

        public static async Task<List<string>> GetRelatedMappedPoperties(Document existingDto)
        {
            List<string> mappedProperties = new List<string>();
            if (existingDto == null)
                return mappedProperties;

            var existingRoot = await existingDto.GetSyntaxRootAsync();
            var mapper = existingRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().Where(c => c.Identifier.Text.Contains("Mapper")).LastOrDefault();
            if (mapper == null)
                return mappedProperties;

            var selectorExpression = mapper.DescendantNodes().OfType<PropertyDeclarationSyntax>().Where(c => c.Identifier.ToString() == "SelectorExpression").LastOrDefault();
            if (mapper == null)
                return mappedProperties;

            mappedProperties.AddRange(ExtractPropertiesRecursif(selectorExpression));

            return mappedProperties;
        }

        private static List<string> ExtractPropertiesRecursif(SyntaxNode initContainer, string prefixInCode = "p.", string prefixInList = "")
        {
            List<string> mappedProperties = new List<string>();
            InitializerExpressionSyntax init = initContainer.DescendantNodes().OfType<InitializerExpressionSyntax>().FirstOrDefault();
            if (init != null)
            {
                bool parentAdded = false;
                foreach (var y in init.ChildNodes().OfType<AssignmentExpressionSyntax>())
                {
                    if (y.Right.ToString().Length > prefixInCode.Length && y.Right.ToString().Substring(0, prefixInCode.Length) == prefixInCode)
                    {
                        string propertySelected = y.Right.ToString().Substring(prefixInCode.Length);

                        if (propertySelected.IndexOf(".Select(") > 0)
                        {
                            string subProperties = prefixInList + propertySelected.Substring(0, propertySelected.IndexOf(".Select("));
                            //subProperties is a collection

                            string subPrefixInCode = propertySelected.Substring(propertySelected.IndexOf(".Select(") + 8);
                            subPrefixInCode = (subPrefixInCode.Substring(0, subPrefixInCode.IndexOf("=>"))).Trim();
                            //add properties of the collection
                            mappedProperties.AddRange(ExtractPropertiesRecursif(y, subPrefixInCode + ".", subProperties + "."));
                        }
                        else
                        {
                            string property = prefixInList + propertySelected;
                            if (!parentAdded)
                            {
                                if (property.LastIndexOf('.') > 0)
                                {
                                    string parentProperty = property.Substring(0, property.LastIndexOf('.'));
                                    mappedProperties.Add(parentProperty);
                                }
                                parentAdded = true;
                            }
                            mappedProperties.Add(property);
                        }
                    }
                    else
                    {
                        //mappedProperties.Add(prefixInList + y.Left);
                        mappedProperties.AddRange(ExtractPropertiesRecursif(y, prefixInCode, prefixInList));
                    }
                }
            }
  
            return mappedProperties;
        }

        private static EntityMetadata FromSyntaxTree(SyntaxTree syntaxTree)
        {
            var root = syntaxTree.GetRoot();

            var classNodes = GetClassNodes(root);

            if (classNodes.Count() != 1)
            {
                throw new ArgumentException("Source code to parse must contain exactly one class declaration!");
            }

            var namespaceNode = GetNamespaceNode(root);

            var classNode = classNodes
                .Single();

            var properties = GetProperties(classNode);

            var result = new EntityMetadata();
            result.Name = classNode.Identifier.Text;
            result.Namespace = namespaceNode.Name.ToString();

            if (classNode.BaseList != null && classNode.BaseList.Types.Count > 0)
            {
                var baseType = classNode.BaseList.Types.First().ToString();
                var isInterface = baseType.Length > 2 && baseType.StartsWith("I") && char.IsUpper(baseType[1]);

                if (!isInterface)
                {
                    result.BaseClassName = baseType;
                    result.BaseClassDtoName = baseType + "DTO";
                }
            }

            result.AttributesList = GetFilteredAttributeList(classNode.AttributeLists, _classDataAnnotationToPreserve);

            result.Properties = properties
                .Select(p => new PropertyMetadata()
                {
                    Type = p.Type.ToString(),
                    Name = p.Identifier.Text,
                    IsSimpleProperty = IsSimpleProperty(p),
                    IsCollection = IsCollection(p),
                    IsRelation = IsRelation(p),
                    RelatedEntityName = IsRelation(p) ? GetRelatedEntity(p) : null,
                    AttributesList = GetFilteredAttributeList(p.AttributeLists, _attributDataAnnotationToPreserve)
                })
                .ToList();

            return result;
        }

        public static NamespaceDeclarationSyntax GetNamespaceNode(SyntaxNode root)
        {
            return root
                            .DescendantNodes(p => !(p is NamespaceDeclarationSyntax))
                            .OfType<NamespaceDeclarationSyntax>()
                            .FirstOrDefault();
        }

        private static List<AttributeListSyntax> GetFilteredAttributeList(SyntaxList<AttributeListSyntax> attributeGroups, List<string> attributesToPreserve)
        {
            return attributeGroups
                .Where(a => a.Attributes
                    .Any(att => attributesToPreserve.Contains(att.Name.ToString())))
                .Select(a => a.RemoveNodes(a.Attributes.Where(att => !attributesToPreserve.Contains(att.Name.ToString())).ToArray(), SyntaxRemoveOptions.KeepNoTrivia))
                .ToList();
        }

        private static IEnumerable<ClassDeclarationSyntax> GetClassNodes(SyntaxNode root)
        {
            return root
                .DescendantNodes(p => !(p is ClassDeclarationSyntax))
                .OfType<ClassDeclarationSyntax>();
        }

        private static IEnumerable<PropertyDeclarationSyntax> GetProperties(ClassDeclarationSyntax classNode)
        {
            return classNode
                .DescendantNodes(p => !(p is PropertyDeclarationSyntax))
                .OfType<PropertyDeclarationSyntax>()
                .Where(p => p.Modifiers.Any(m => m.Kind() == SyntaxKind.PublicKeyword))
                .Where(p => p.FirstAncestorOrSelf<ClassDeclarationSyntax>() == classNode)
                .Where(p => p.AccessorList != null)
                .Where(p => p.AccessorList.Accessors.Any(a => a.Kind() == SyntaxKind.GetAccessorDeclaration))
                .Where(p => p.AccessorList.Accessors.Any(a => a.Kind() == SyntaxKind.SetAccessorDeclaration));
        }

        private static string GetRelatedEntity(PropertyDeclarationSyntax p)
        {
            try
            {
                if (p.Type is GenericNameSyntax)
                    return (p.Type as GenericNameSyntax).TypeArgumentList.Arguments.OfType<IdentifierNameSyntax>().Select(i => i.Identifier.Text).Single();

                if (p.Type is IdentifierNameSyntax)
                    return (p.Type as IdentifierNameSyntax).Identifier.Text;
            }
            catch(Exception)
            {
                return null;
            }

            return null;
        }

        private static bool IsRelation(PropertyDeclarationSyntax p)
        {
            return !IsSimpleProperty(p);
        }

        private static bool IsCollection(PropertyDeclarationSyntax p)
        {
            var genericSyntax = p.Type as GenericNameSyntax;
            if (genericSyntax != null && genericSyntax.Identifier.ToString() != "Nullable")
                return true;

            return false;
        }

        private static bool IsSimpleProperty(PropertyDeclarationSyntax propertyNode)
        {
            var nullableType = propertyNode.Type as NullableTypeSyntax;
            if(nullableType != null)
            {
                return IsSimpleType(nullableType.ElementType);
            }

            var nullableGenericType = propertyNode.Type as GenericNameSyntax;
            if (nullableGenericType != null && nullableGenericType.Identifier.ToString() == "Nullable")
                return IsSimpleType(nullableGenericType.TypeArgumentList.Arguments.First());

            return IsSimpleType(propertyNode.Type);
        }

        private static bool IsSimpleType(TypeSyntax type)
        {
            var simpleTypeList = _simpleTypes
                .Select(p => $"{p.Namespace}.{p.Name}")
                .Concat(_simpleTypes.Select(p => p.Name))
                .ToList();

            if (simpleTypeList.Contains(type.ToString()))
                return true;

            if (type is PredefinedTypeSyntax)
                return true;

            return false;
        }
    }
}
