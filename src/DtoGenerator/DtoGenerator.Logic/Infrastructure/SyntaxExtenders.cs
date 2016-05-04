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
        public const string NewLine = "\r\n";

        public static SyntaxTrivia EndOfLineTrivia => SyntaxFactory.EndOfLine(NewLine);

        public static TypeSyntax ToCollectionType(this string type, string collectionType)
        {
            return SyntaxFactory.GenericName(
                    SyntaxFactory.Identifier(collectionType))
                .WithTypeArgumentList(
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                            SyntaxFactory.IdentifierName(type))));
        }

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

        public static TNode AppendNewLine<TNode>(this TNode node, bool preserveExistingTrivia = true)
            where TNode : SyntaxNode
        {
            var triviaList = preserveExistingTrivia == true ? node.GetTrailingTrivia() : SyntaxFactory.TriviaList();
            triviaList = triviaList.Add(SyntaxExtenders.EndOfLineTrivia);

            return node.WithTrailingTrivia(triviaList);
        }

        public static FieldDeclarationSyntax DeclareField(string type, bool autoCreateNew)
        {
            var fieldName = "_" + char.ToLower(type[0]) + type.Substring(1);

            return SyntaxFactory.FieldDeclaration(
                        SyntaxFactory.VariableDeclaration(
                            SyntaxFactory.IdentifierName(type))
                        .WithVariables(
                            SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                                SyntaxFactory.VariableDeclarator(
                                    SyntaxFactory.Identifier(fieldName))
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.ObjectCreationExpression(
                                            SyntaxFactory.IdentifierName(type))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList()))))))
                        .WithModifiers(
                            SyntaxFactory.TokenList(
                                SyntaxFactory.Token(SyntaxKind.PrivateKeyword)))
                            .NormalizeWhitespace(elasticTrivia: true)
                        .AppendNewLine();
        }

        public static ExpressionStatementSyntax AssignmentStatement(string left, string right)
        {
            return SyntaxFactory.ExpressionStatement(AssignmentExpression(left, right))
                       .NormalizeWhitespace(elasticTrivia: true)
                       .AppendNewLine();
        }

        public static InvocationExpressionSyntax ToMethodInvocation(this string method, params ExpressionSyntax[] args)
        {
            var argSyntaxes = args.Select(p => SyntaxFactory.Argument(p));

            return SyntaxFactory.InvocationExpression(
                            method.ToMemberAccess())
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(argSyntaxes)));
        }

        public static ExpressionSyntax ToMemberAccess(this string selector)
        {
            var parts = selector.Split('.');

            if(parts.Count() == 1)
            {
                return SyntaxFactory.IdentifierName(parts.First());
            }
            else if (parts.Count() == 2)
            {
                if(parts.First() == "this")
                {
                    return SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ThisExpression(),
                        SyntaxFactory.IdentifierName(parts.Last()));
                }
                    
                return SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(parts.First()),
                        SyntaxFactory.IdentifierName(parts.Last()));
            }
            else
            {
                var leftPart = string.Join(".", parts.Take(parts.Count() - 1));
                return SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        leftPart.ToMemberAccess(),
                        SyntaxFactory.IdentifierName(parts.Last()));
            }
        }

        public static PropertyDeclarationSyntax DeclareAutoProperty(TypeSyntax type, string identifier)
        {
            return SyntaxFactory.PropertyDeclaration(
                    type.AppendWhitespace(),
                    SyntaxFactory.Identifier(identifier).AppendWhitespace())
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword).AppendWhitespace()
                        ))
                .WithAccessorList(
                    SyntaxFactory.AccessorList(
                        SyntaxFactory.List<AccessorDeclarationSyntax>(
                            new AccessorDeclarationSyntax[]{
                                PropertyAccessor(PropertyAccessorType.Get),
                                PropertyAccessor(PropertyAccessorType.Set)
                            })))
                .AppendNewLine();
        }

        public static AccessorDeclarationSyntax PropertyAccessor(PropertyAccessorType type)
        {
            var syntaxKind = type == PropertyAccessorType.Get ? SyntaxKind.GetAccessorDeclaration : SyntaxKind.SetAccessorDeclaration;

            var accessor = SyntaxFactory.AccessorDeclaration(syntaxKind).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)).AppendWhitespace();

            if (type == PropertyAccessorType.Get)
                accessor = accessor.PrependWhitespace();

            return accessor;
        }

        public static TNode PrependWhitespace<TNode>(this TNode node)
            where TNode : SyntaxNode
        {
            return node.WithLeadingTrivia(node.GetLeadingTrivia().Add(SyntaxFactory.Whitespace(" ")));
        }

        public static TNode AppendWhitespace<TNode>(this TNode node)
            where TNode: SyntaxNode
        {
            return node.WithTrailingTrivia(node.GetTrailingTrivia().Add(SyntaxFactory.Whitespace(" ")));
        }

        public static SyntaxToken AppendWhitespace(this SyntaxToken token)
        {
            return token.WithTrailingTrivia(token.TrailingTrivia.Add(SyntaxFactory.Whitespace(" ")));
        }

        public static SyntaxToken AppendNewLine(this SyntaxToken token)
        {
            return token.WithTrailingTrivia(token.TrailingTrivia.Add(EndOfLineTrivia));
        }

        public static ExpressionSyntax AssignmentExpression(string left, string right)
        {
            return SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                left.ToMemberAccess().AppendWhitespace(),
                right.ToMemberAccess().PrependWhitespace());
        }

        public enum PropertyAccessorType
        {
            Get,
            Set
        }
    }
}
