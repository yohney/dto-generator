using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DtoGenerator.Logic.Infrastructure
{
    public static class SyntaxExtenders
    {
        public static NameSyntax SyntaxNameFromFullName(this string fullName)
        {
            if (fullName.Count(p => p == '.') == 0)
                return SyntaxFactory.IdentifierName(fullName);

            var parts = fullName.Split('.');

            if (fullName.Count(p => p == '.') == 1)
                return SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName(parts.First()), SyntaxFactory.IdentifierName(parts.Last()));

            var nameLeft = fullName.Substring(0, fullName.LastIndexOf('.')).SyntaxNameFromFullName();
            return SyntaxFactory.QualifiedName(nameLeft, SyntaxFactory.IdentifierName(parts.Last()));
        }

        public static UsingDirectiveSyntax ToUsing(this string @namespace)
        {
            return @namespace.SyntaxNameFromFullName().ToUsing();
        }

        public static UsingDirectiveSyntax ToUsing(this NameSyntax nameSyntaxNode)
        {
            return SyntaxFactory.UsingDirective(nameSyntaxNode);
        }

        public static NamespaceDeclarationSyntax ToNamespaceDirective(this string @namespace)
        {
            return SyntaxFactory.NamespaceDeclaration(@namespace.SyntaxNameFromFullName());
        }

        public static TypeSyntax ToGenericType(this string typeName, params TypeSyntax[] genArguments)
        {
            var result = new List<SyntaxNodeOrToken>();
            foreach (var baseArg in genArguments)
            {
                result.Add(baseArg);
                result.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
            }

            result.RemoveAt(result.Count - 1);

            return SyntaxFactory.GenericName(SyntaxFactory.Identifier(typeName))
                        .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList<TypeSyntax>(result.ToArray())));
        }

        public static TypeSyntax ToGenericType(this string typeName, params string[] genArguments)
        {
            return typeName.ToGenericType(genArguments.Select(p => SyntaxFactory.IdentifierName(p)).ToArray());
        }

        public static ClassDeclarationSyntax ToClassDeclaration(this string className, bool isPublic = true, string baseClassName = null, IEnumerable<string> baseClassGenericArguments = null)
        {
            var ret = SyntaxFactory.ClassDeclaration(className);

            if (isPublic)
                ret = ret.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

            if(baseClassName != null)
            {
                TypeSyntax baseClassTypeSyntax;

                if (baseClassGenericArguments == null)
                    baseClassTypeSyntax = SyntaxFactory.IdentifierName(baseClassName);
                else
                {
                    baseClassTypeSyntax = baseClassName.ToGenericType(baseClassGenericArguments.ToArray());
                }

                ret = ret.WithBaseList(
                        SyntaxFactory.BaseList(
                            SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                                SyntaxFactory.SimpleBaseType(baseClassTypeSyntax))));
            }

            return ret;
        }
    }
}
