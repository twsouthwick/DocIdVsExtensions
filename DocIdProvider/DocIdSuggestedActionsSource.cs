using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DocIdProvider
{
    internal class DocIdSuggestedActionsSource : ISuggestedActionsSource
    {
        private readonly ITextBuffer _buffer;

        public DocIdSuggestedActionsSource(ITextBuffer textBuffer)
        {
            _buffer = textBuffer;
        }

        public event EventHandler<EventArgs> SuggestedActionsChanged
        {
            add { }
            remove { }
        }

        public void Dispose()
        {
        }

        public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
            if (_buffer.GetWorkspace() is Workspace ws && ws.GetDocumentIdInCurrentContext(_buffer.AsTextContainer()) is DocumentId documentId)
            {
                if (ws.CurrentSolution.GetDocument(documentId) is Document doc && doc.TryGetSyntaxRoot(out var root))
                {
                    if (!doc.TryGetSemanticModel(out var model))
                    {
                        return Enumerable.Empty<SuggestedActionSet>();
                    }

                    var node = root.FindNode(new TextSpan(range.Span.Start, range.Span.Length), getInnermostNodeForTie: true);

                    if (GetDocId(model, node, cancellationToken) is string docId)
                    {
                        return new[]
                        {
                            new SuggestedActionSet("CopyDocId", new ISuggestedAction[]
                            {
                                new CopyDocIdSuggestedAction(docId),
                            })
                        };
                    }
                }
            }

            return Enumerable.Empty<SuggestedActionSet>();
        }

        private string GetDocId(SemanticModel model, SyntaxNode node, CancellationToken token)
        {
            if (node is null)
            {
                return null;
            }

            if (model.GetDeclaredSymbol(node, token) is ISymbol symbol)
            {
                return symbol.GetDocumentationCommentId();
            }

            if (GetDocId(model.GetOperation(node, token)) is string docId)
            {
                return docId;
            }

            return null;
        }

        private string GetDocId(IOperation operation)
            => operation switch
            {
                IInvocationOperation invocation => invocation.TargetMethod.GetDocumentationCommentId(),
                IExpressionStatementOperation expressionStatement => GetDocId(expressionStatement.Operation),
                _ => null,
            };

        public Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
            var actions = GetSuggestedActions(requestedActionCategories, range, cancellationToken);

            return Task.FromResult(actions.Any());
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            telemetryId = Guid.Empty;
            return false;
        }
    }
}
