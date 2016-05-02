using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DtoGenerator.Logic.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;

namespace DtoGenerator.Logic.Infrastructure
{
    public class DtoBuilder
    {
        public static SyntaxTree BuildDto(EntityMetadata entity, SyntaxTree existingDto = null, string dtoNamespace = null)
        {
            CompilationUnitSyntax root = null;

            if (existingDto == null)
            {
                var rawTree = BuildOutline(dtoNamespace, entity);

                var commentAppender = new EmptyTreeCommentAppender();
                root = commentAppender.Visit(rawTree.GetRoot()) as CompilationUnitSyntax;
            }
            else
            {
                var existingRoot = existingDto.GetRoot();

                var finder = new CustomCodeLocator();
                finder.Visit(existingRoot);

                var rewriter = new GeneratedCodeRemover(finder);
                existingRoot = rewriter.Visit(existingRoot);

                if(rewriter.FirstCustomProperty == null)
                {
                    var commentAppender = new EmptyTreeCommentAppender();
                    root = commentAppender.Visit(existingRoot) as CompilationUnitSyntax;
                }
                else
                {
                    var commentWrapper = new CustomCodeCommentWrapper(rewriter);
                    root = commentWrapper.Visit(existingRoot) as CompilationUnitSyntax;
                }
            }

            var generatedPropertiesAppender = new GeneratedPropertiesAppender(entity);
            root = generatedPropertiesAppender.Visit(root) as CompilationUnitSyntax;

            return SyntaxFactory.SyntaxTree(root);
        }

        private static SyntaxTree BuildOutline(string dtoNamespace, EntityMetadata entity)
        {
            using (var stream = typeof(DtoBuilder).Assembly.GetManifestResourceStream($"DtoGenerator.Logic.Infrastructure.Template.Outline.cs"))
            {
                using (var reader = new StreamReader(stream))
                {
                    var sourceCode = reader.ReadToEnd();
                    sourceCode = sourceCode
                        .Replace("#Entity#", entity.Name)
                        .Replace("#Namespace#", dtoNamespace);

                    return CSharpSyntaxTree.ParseText(sourceCode);
                }
            }
        }
    }

    public class GeneratedPropertiesAppender : CSharpSyntaxRewriter
    {
        private EntityMetadata _metadata;

        public GeneratedPropertiesAppender(EntityMetadata metadata)
        {
            this._metadata = metadata;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if(node.Identifier.Text == this._metadata.Name + "DTO")
            {
                var membersList = node.Members;
                foreach (var prop in _metadata.Properties)
                    membersList = membersList.Add(prop.SyntaxNode);

                return node.WithMembers(membersList);
            }

            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            if(node.FirstAncestorOrSelf<PropertyDeclarationSyntax>().Identifier.Text == "SelectorExpression")
            {
                var expressions = new List<ExpressionSyntax>();

                foreach(var prop in this._metadata.Properties)
                {
                    var exp = SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName(prop.Name),
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("p"),
                            SyntaxFactory.IdentifierName(prop.Name)));

                    expressions.Add(exp);
                }

                return node.AddExpressions(expressions.ToArray());
            }

            return base.VisitInitializerExpression(node);
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            if (node.FirstAncestorOrSelf<MethodDeclarationSyntax>() != null && node.FirstAncestorOrSelf<MethodDeclarationSyntax>().Identifier.Text == "MapToModel")
            {
                var statements = node.Statements;
                foreach(var prop in _metadata.Properties)
                {
                    var statement = SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("model"),
                                SyntaxFactory.IdentifierName(prop.Name)),
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("dto"),
                                SyntaxFactory.IdentifierName(prop.Name))));

                    statements = statements.Add(statement);
                }

                return node.WithStatements(statements);
            }

            return base.VisitBlock(node);
        }
    }

    public class CustomCodeCommentWrapper : CSharpSyntaxRewriter
    {
        private GeneratedCodeRemover _remover;

        public CustomCodeCommentWrapper(GeneratedCodeRemover remover)
        {
            this._remover = remover;
        }

        private bool AreNodesEqual(SyntaxNode a, SyntaxNode b)
        {
            if (a.Kind() != b.Kind())
                return false;

            if (a.ToString() != b.ToString())
                return false;

            return true;
        }

        private SyntaxNode WrapWithComment(SyntaxNode node, string leadingTriviaComment, string trailingTriviaComment, SyntaxNode firstOccurance, SyntaxNode lastOccurance)
        {
            if (_remover.CustomPropertiesCount == 1)
            {
                if (AreNodesEqual(node, firstOccurance))
                {
                    var leadingTrivia = node.GetLeadingTrivia().AddRange(new[] { SyntaxFactory.Comment(leadingTriviaComment), SyntaxFactory.EndOfLine("\n") });
                    var trailingTrivia = node.GetTrailingTrivia().Add(SyntaxFactory.Comment(trailingTriviaComment));

                    return node
                        .WithLeadingTrivia(leadingTrivia)
                        .WithTrailingTrivia(trailingTrivia);
                }
            }
            else
            {
                if (AreNodesEqual(node, firstOccurance))
                {
                    var leadingTrivia = node.GetLeadingTrivia().Add(SyntaxFactory.Comment(leadingTriviaComment));

                    return node
                        .WithLeadingTrivia(leadingTrivia);
                }
                else if (AreNodesEqual(node, lastOccurance))
                {
                    var trailingTrivia = node.GetTrailingTrivia().Add(SyntaxFactory.Comment(trailingTriviaComment));

                    return node
                        .WithTrailingTrivia(trailingTrivia);
                }
            }

            return null;
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (node == null)
                return null;

            var result = this.WrapWithComment(
                node,
                "////BCPS/ BEGIN CUSTOM PROPERTY SECTION ",
                "////ECPS/ END CUSTOM PROPERTY SECTION ",
                this._remover.FirstCustomProperty,
                this._remover.LastCustomProperty);

            return result ?? base.VisitPropertyDeclaration(node);
        }

        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            if (node == null)
                return null;

            var result = this.WrapWithComment(
                node,
                "////BCMS/ BEGIN CUSTOM MAPPER SECTION ",
                "////ECMS/ END CUSTOM MAPPER SECTION ",
                this._remover.FirstCustomMapperStatement,
                this._remover.LastCustomMapperStatement);

            return result ?? base.VisitExpressionStatement(node);
        }

        public override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            if (node == null)
                return null;

            var result = this.WrapWithComment(
                node,
                "////BCSS/ BEGIN CUSTOM SELECTOR SECTION ",
                "////ECSS/ END CUSTOM SELECTOR SECTION ",
                this._remover.FirstCustomSelector,
                this._remover.LastCustomSelector);

            return result ?? base.VisitAssignmentExpression(node);
        }
    }


    public enum CommentTriviaType
    {
        Property,
        Selector,
        Mapper
    }

    public class EmptyTreeCommentAppender : CSharpSyntaxRewriter
    {
        private SyntaxTriviaList GenerateCommentsTrivia(CommentTriviaType type)
        {
            switch(type)
            {
                case CommentTriviaType.Mapper:
                    return SyntaxFactory.TriviaList(
                        new[]{
                            SyntaxFactory.Comment("////BCMS/ BEGIN CUSTOM MAPPER SECTION "),
                            SyntaxFactory.Comment("////ECMS/ END CUSTOM MAPPER SECTION")});

                case CommentTriviaType.Property:
                    return SyntaxFactory.TriviaList(
                        new[]{
                            SyntaxFactory.Comment("////BCPS/ BEGIN CUSTOM PROPERTY SECTION "),
                            SyntaxFactory.Comment("////ECPS/ END CUSTOM PROPERTY SECTION")});

                case CommentTriviaType.Selector:
                    return SyntaxFactory.TriviaList(
                        new[]{
                            SyntaxFactory.Comment("////BCSS/ BEGIN CUSTOM SELECTOR SECTION "),
                            SyntaxFactory.Comment("////ECSS/ END CUSTOM SELECTOR SECTION")});

            }

            return SyntaxTriviaList.Empty;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if(!node.Identifier.Text.Contains("Mapper"))
            {
                return node.ReplaceToken(node.OpenBraceToken, SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.OpenBraceToken, this.GenerateCommentsTrivia(CommentTriviaType.Property)));
            }

            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            return node.ReplaceToken(node.OpenBraceToken, SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.OpenBraceToken, this.GenerateCommentsTrivia(CommentTriviaType.Selector)));
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            if(node.FirstAncestorOrSelf<MethodDeclarationSyntax>() != null && node.FirstAncestorOrSelf<MethodDeclarationSyntax>().Identifier.Text == "MapToModel")
            {
                return node.ReplaceToken(node.OpenBraceToken, SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.OpenBraceToken, this.GenerateCommentsTrivia(CommentTriviaType.Mapper)));
            }

            return base.VisitBlock(node);
        }
    }

    public class CustomCodeLocator : CSharpSyntaxWalker
    {
        public int CustomPropertyBegin { get; private set; }
        public int CustomPropertyEnd { get; private set; }

        public int CustomSelectorBegin { get; private set; }
        public int CustomSelectorEnd { get; private set; }

        public int CustomMapperBegin { get; private set; }
        public int CustomMapperEnd { get; private set; }

        public CustomCodeLocator() : base(SyntaxWalkerDepth.Trivia)
        {
        }

        public override void VisitTrivia(SyntaxTrivia trivia)
        {
            if(trivia.Kind() == SyntaxKind.SingleLineCommentTrivia)
            {
                var text = trivia.ToString();

                if (text.Contains("////BCPS/"))
                    this.CustomPropertyBegin = trivia.Span.Start;
                if (text.Contains("////ECPS/"))
                    this.CustomPropertyEnd = trivia.Span.Start;

                if (text.Contains("////BCSS/"))
                    this.CustomSelectorBegin = trivia.Span.Start;
                if (text.Contains("////ECSS/"))
                    this.CustomSelectorEnd = trivia.Span.Start;

                if (text.Contains("////BCMS/"))
                    this.CustomMapperBegin = trivia.Span.Start;
                if (text.Contains("////ECMS/"))
                    this.CustomMapperEnd = trivia.Span.Start;
            }

            base.VisitTrivia(trivia);
        }
    }

    public class GeneratedCodeRemover : CSharpSyntaxRewriter
    {
        public int CustomPropertiesCount { get; set; }

        public SyntaxNode FirstCustomProperty { get; set; }
        public SyntaxNode LastCustomProperty { get; set; }

        public SyntaxNode FirstCustomSelector { get; set; }
        public SyntaxNode LastCustomSelector { get; set; }

        public SyntaxNode FirstCustomMapperStatement { get; set; }
        public SyntaxNode LastCustomMapperStatement { get; set; }

        private CustomCodeLocator _finder;

        public GeneratedCodeRemover(CustomCodeLocator finder)
        {
            this.CustomPropertiesCount = 0;

            this._finder = finder;
        }

        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
        {
            if (trivia.Kind() == SyntaxKind.SingleLineCommentTrivia)
            {
                var text = trivia.ToString();

                if (text.Contains("////BCPS/") || text.Contains("////ECPS/") || text.Contains("////BCSS/") || text.Contains("////ECSS/") || text.Contains("////BCMS/") || text.Contains("////ECMS/"))
                    return SyntaxFactory.Whitespace("");
            }

            return base.VisitTrivia(trivia);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (!node.FirstAncestorOrSelf<ClassDeclarationSyntax>().Identifier.Text.Contains("Mapper"))
            {
                // check if node is automatically generated (not wrapped inside custom comments)
                if (this.IsNodeAutoGenerated(node, this._finder.CustomPropertyBegin, this._finder.CustomPropertyEnd))
                {
                    return null;
                }
                else
                {
                    this.CustomPropertiesCount++;

                    if (this.FirstCustomProperty == null)
                        this.FirstCustomProperty = node;

                    this.LastCustomProperty = node;
                }
            }

            return base.VisitPropertyDeclaration(node);
        }

        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            if (node.FirstAncestorOrSelf<MethodDeclarationSyntax>() != null && node.FirstAncestorOrSelf<MethodDeclarationSyntax>().Identifier.Text == "MapToModel")
            {
                // this is mapper expression. Check if automatically generated..
                if (this.IsNodeAutoGenerated(node, this._finder.CustomMapperBegin, this._finder.CustomMapperEnd))
                {
                    return null;
                }
                else
                {
                    if (this.FirstCustomMapperStatement == null)
                        this.FirstCustomMapperStatement = node;

                    this.LastCustomMapperStatement = node;
                }
            }

            return base.VisitExpressionStatement(node);
        }

        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var customExpressions = node.Initializer.Expressions
                .Where(p => !this.IsNodeAutoGenerated(p, this._finder.CustomSelectorBegin, this._finder.CustomSelectorEnd))
                .Select(p => p.WithoutTrivia().WithTrailingTrivia(SyntaxFactory.EndOfLine("\n")))
                .ToList();

            var res = node.WithInitializer(node.Initializer.WithExpressions(SyntaxFactory.SeparatedList(customExpressions)));

            this.FirstCustomSelector = res.Initializer.Expressions.FirstOrDefault();
            this.LastCustomSelector = res.Initializer.Expressions.LastOrDefault();

            return res;
        }

        private bool IsNodeAutoGenerated(SyntaxNode node, int customCodeBegin, int customCodeEnd)
        {
            return node.GetLocation().SourceSpan.Start > customCodeEnd || node.GetLocation().SourceSpan.End < customCodeBegin;
        }
    }
}
