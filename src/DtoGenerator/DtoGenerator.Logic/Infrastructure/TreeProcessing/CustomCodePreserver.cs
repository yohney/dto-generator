using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DtoGenerator.Logic.Infrastructure.TreeProcessing
{
    public class CustomCodePreserver : CSharpSyntaxRewriter
    {
        public const string CustomCodeCommentBegin = "////BCC/ BEGIN CUSTOM CODE SECTION ";
        public const string CustomCodeCommentEnd = "////ECC/ END CUSTOM CODE SECTION ";

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (!node.Identifier.Text.Contains("Mapper"))
            {
                // We want to wrap all members within //Custom code section within DTO class
                node = WrapMembersWithComment(node, node.Members);
            }
            else
            {
                // Mapper class - all members except SelectorExpression and MapToModel should be within same comment block
                var selectorExpressionMember = node.Members
                    .OfType<PropertyDeclarationSyntax>()
                    .Where(p => p.Identifier.ToString() == "SelectorExpression")
                    .FirstOrDefault();

                var mapToModelMember = node.Members
                    .OfType<MethodDeclarationSyntax>()
                    .Where(p => p.Identifier.ToString() == "MapToModel")
                    .FirstOrDefault();

                var otherMembers = node.Members
                    .Where(m => m != selectorExpressionMember)
                    .Where(m => m != mapToModelMember)
                    .ToList();

                node = WrapMembersWithComment(node, SyntaxFactory.List(otherMembers));

                var resultMembers = node.Members;

                if (selectorExpressionMember != null)
                    resultMembers = resultMembers.Add(selectorExpressionMember);

                if (mapToModelMember != null)
                    resultMembers = resultMembers.Add(mapToModelMember);

                node = node.WithMembers(resultMembers);
            }

            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            var containingProperty = node.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            if (containingProperty != null && containingProperty.Identifier.ToString() == "SelectorExpression" &&
                containingProperty.FirstAncestorOrSelf<ClassDeclarationSyntax>().Identifier.ToString().Contains("Mapper"))
            {
                if (node.Expressions.Count == 0)
                {
                    node = node.WithOpenBraceToken(GetOpenBraceTokenWithEmptyCustomCode());
                }
                else if (node.Expressions.Count == 1)
                {
                    var newExpression = AddLeadingTriviaComment(AddTrailingTriviaComment(node.Expressions.Single()));
                    node = node.WithExpressions(node.Expressions.RemoveAt(0).Add(newExpression));
                }
                else
                {
                    var firstExpression = AddLeadingTriviaComment(node.Expressions.First());

                    var lastExpression = AddTrailingTriviaComment(node.Expressions.Last());
                    var lastExpressionIndex = node.Expressions.Count - 1;

                    node = node.WithExpressions(node.Expressions.RemoveAt(lastExpressionIndex).RemoveAt(0).Insert(0, firstExpression).Add(lastExpression));
                }
            }

            return base.VisitInitializerExpression(node);
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            var containingMethod = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if(containingMethod != null && containingMethod.Identifier.ToString() == "MapToModel" && 
                containingMethod.FirstAncestorOrSelf<ClassDeclarationSyntax>().Identifier.ToString().Contains("Mapper"))
            {
                if(node.Statements.Count == 0)
                {
                    node = node.WithOpenBraceToken(GetOpenBraceTokenWithEmptyCustomCode());
                }
                else if(node.Statements.Count == 1)
                {
                    var newStatement = AddLeadingTriviaComment(AddTrailingTriviaComment(node.Statements.Single()));
                    node = node.WithStatements(node.Statements.RemoveAt(0).Add(newStatement));
                }
                else
                {
                    var firstStatement = AddLeadingTriviaComment(node.Statements.First());

                    var lastStatement = AddTrailingTriviaComment(node.Statements.Last());
                    var lastStatementIndex = node.Statements.Count - 1;

                    node = node.WithStatements(node.Statements.RemoveAt(lastStatementIndex).RemoveAt(0).Insert(0, firstStatement).Add(lastStatement));
                }
            }

            return base.VisitBlock(node);
        }

        private ClassDeclarationSyntax WrapMembersWithComment(ClassDeclarationSyntax node, SyntaxList<MemberDeclarationSyntax> members)
        {
            if (members.Count == 0)
            {
                node = node.WithOpenBraceToken(GetOpenBraceTokenWithEmptyCustomCode())
                    .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>());
            }
            else if (members.Count == 1)
            {
                var newMember = AddLeadingTriviaComment(AddTrailingTriviaComment(members.Single()));
                node = node.WithMembers(members.RemoveAt(0).Add(newMember));
            }
            else
            {
                var firstMember = AddLeadingTriviaComment(members.First());

                var lastMember = AddTrailingTriviaComment(members.Last());
                var lastMemberIdx = members.Count - 1;

                node = node.WithMembers(members.RemoveAt(lastMemberIdx).RemoveAt(0).Insert(0, firstMember).Add(lastMember));
            }

            return node;
        }

        private SyntaxToken GetOpenBraceTokenWithEmptyCustomCode()
        {
            return SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
                    .WithTrailingTrivia(SyntaxFactory.TriviaList(
                        SyntaxExtenders.EndOfLineTrivia,
                        SyntaxFactory.Comment(CustomCodeCommentBegin),
                        SyntaxExtenders.EndOfLineTrivia,
                        SyntaxFactory.Comment(CustomCodeCommentEnd),
                        SyntaxExtenders.EndOfLineTrivia));
        }

        private TNode AddLeadingTriviaComment<TNode>(TNode node)
            where TNode : SyntaxNode
        {
            var leadingTrivia = node.GetLeadingTrivia()
                    .Insert(0, SyntaxExtenders.EndOfLineTrivia)
                    .Insert(0, SyntaxFactory.Comment(CustomCodeCommentBegin))
                    .Insert(0, SyntaxExtenders.EndOfLineTrivia);

            return node
                .WithLeadingTrivia(leadingTrivia);
        }

        private TNode AddTrailingTriviaComment<TNode>(TNode node)
            where TNode : SyntaxNode
        {
            var trailingTrivia = node.GetTrailingTrivia()
                    .Add(SyntaxExtenders.EndOfLineTrivia)
                    .Add(SyntaxFactory.Comment(CustomCodeCommentEnd))
                    .Add(SyntaxExtenders.EndOfLineTrivia);

            return node
                .WithTrailingTrivia(trailingTrivia);
        }
    }
}
