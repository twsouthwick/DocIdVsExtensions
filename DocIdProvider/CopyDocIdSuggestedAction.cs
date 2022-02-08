using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace DocIdProvider
{
    internal class CopyDocIdSuggestedAction : ISuggestedAction
    {
        private readonly string _docId;

        public CopyDocIdSuggestedAction(string docId)
        {
            _docId = docId;
        }

        public bool HasActionSets => false;

        public string DisplayText => "Copy DocId";

        public ImageMoniker IconMoniker => default;

        public string IconAutomationText => null;

        public string InputGestureText => null;

        public bool HasPreview => true;

        public void Dispose()
        {
        }

        public Task<IEnumerable<SuggestedActionSet>> GetActionSetsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<SuggestedActionSet>>(null);
        }

        public Task<object> GetPreviewAsync(CancellationToken cancellationToken)
        {
            var textBlock = new TextBlock
            {
                Padding = new Thickness(5),
                Inlines =
                {
                    new Run() { Text = _docId }
                }
            };

            return Task.FromResult<object>(textBlock);
        }

        public void Invoke(CancellationToken cancellationToken)
        {
            Clipboard.SetText(_docId);
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            telemetryId = Guid.Empty;
            return false;
        }
    }
}
