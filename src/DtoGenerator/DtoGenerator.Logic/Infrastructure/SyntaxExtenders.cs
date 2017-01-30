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

        public static CompilationUnitSyntax AppendUsing(this CompilationUnitSyntax node, params string[] usingDirectives)
        {
            var existingUsings = node.DescendantNodes(p => !(p is ClassDeclarationSyntax))
                .OfType<UsingDirectiveSyntax>()
                .Select(p => p.Name.ToString())
                .ToList();

            var usings = node.Usings;

            foreach(var x in usingDirectives)
            {
                if (x == null || existingUsings.Contains(x))
                    continue;

                usings = usings.Add(x.ToUsing());
            }

            return node.WithUsings(usings);
        }

        public static UsingDirectiveSyntax ToUsing(this string @namespace)
        {
            return @namespace.SyntaxNameFromFullName().ToUsing();
        }

        public static UsingDirectiveSyntax ToUsing(this NameSyntax nameSyntaxNode)
        {
            return SyntaxFactory.UsingDirective(nameSyntaxNode.PrependWhitespace()).AppendNewLine();
        }


        public static TNode AppendNewLine<TNode>(this TNode node, bool preserveExistingTrivia = true)
            where TNode : SyntaxNode
        {
            var triviaList = preserveExistingTrivia == true ? node.GetTrailingTrivia() : SyntaxFactory.TriviaList();
            triviaList = triviaList.Add(SyntaxExtenders.EndOfLineTrivia);

            return node.WithTrailingTrivia(triviaList);
        }

        public static BaseListSyntax ToBaseClassList(this string baseClass)
        {
            return SyntaxFactory.BaseList(
                SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                    SyntaxFactory.SimpleBaseType(
                        SyntaxFactory.IdentifierName(baseClass).PrependWhitespace())))
                .AppendNewLine();
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

        public static ExpressionStatementSyntax InvocationStatement(string member, params string[] args)
        {
            return SyntaxFactory.ExpressionStatement(member.ToMethodInvocation(args.Select(p => p.ToMemberAccess()).ToArray()));
        }

        public static ExpressionStatementSyntax AssignmentStatement(string left, string right)
        {
            return SyntaxFactory.ExpressionStatement(AssignmentExpression(left, right))
                       .NormalizeWhitespace(elasticTrivia: true)
                       .AppendNewLine();
        }

        public static InvocationExpressionSyntax ToMethodInvocation(this string method, params ExpressionSyntax[] args)
        {
            return method.ToMemberAccess().ToMethodInvocation(args);
        }

        public static InvocationExpressionSyntax ToMethodInvocation(this ExpressionSyntax methodMember, params ExpressionSyntax[] args)
        {
            var argSyntaxes = args.Select(p => SyntaxFactory.Argument(p));

            return SyntaxFactory.InvocationExpression(methodMember)
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(argSyntaxes)));
        }

        public static SimpleNameSyntax ToName(this string identifier)
        {
            return SyntaxFactory.IdentifierName(identifier);
        }

        public static ExpressionSyntax ToMemberAccess(this string selector)
        {
            var parts = selector.Split('.');

            if(parts.Count() == 1)
            {
                return parts.First().ToName();
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

        public static SyntaxList<AttributeListSyntax> CreateAttributes(params string[] attributes)
        {
            var attrsList = SyntaxFactory.SeparatedList<AttributeSyntax>(attributes.Select(p => SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(p))));
            return SyntaxFactory.SingletonList(SyntaxFactory.AttributeList(attrsList));
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

        public static ExpressionSyntax WrapInConditional(this ExpressionSyntax expression, string propType)
        {
            var notNullExpressions = new List<BinaryExpressionSyntax>();
            
            var memberAcc = expression as MemberAccessExpressionSyntax;
            while(memberAcc != null && memberAcc.Expression is MemberAccessExpressionSyntax)
            {
                var notNullExp = SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression,
                    memberAcc.Expression,
                    SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));
                notNullExpressions.Add(notNullExp);

                memberAcc = memberAcc.Expression as MemberAccessExpressionSyntax;
            }

            notNullExpressions.Reverse();

            if (notNullExpressions.Count == 0)
                return expression;

            ExpressionSyntax current = notNullExpressions.First();
            for(int i = 1; i < notNullExpressions.Count; i++)
            {
                current = SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression,
                    current,
                    notNullExpressions[i]);
            }

            var fallbackExpression = propType == null ? (ExpressionSyntax)SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
               : SyntaxFactory.DefaultExpression(SyntaxFactory.ParseTypeName(propType));

            return SyntaxFactory.ConditionalExpression(current, expression, fallbackExpression).NormalizeWhitespace();
        }

        public static ExpressionSyntax AssignmentExpression(string left, string right, string propType = null, bool verifyRightNotNull = false)
        {
            var rightMemberAccess = right.ToMemberAccess();
            var rightExp = verifyRightNotNull ? rightMemberAccess.WrapInConditional(propType) : rightMemberAccess;

            return SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                left.ToMemberAccess().AppendWhitespace(),
                rightExp.PrependWhitespace());
        }

        public enum PropertyAccessorType
        {
            Get,
            Set
        }
    }
}
