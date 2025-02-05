﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.LanguageServer.Common.Extensions;
using Microsoft.AspNetCore.Razor.LanguageServer.EndpointContracts;
using Microsoft.AspNetCore.Razor.LanguageServer.Extensions;
using Microsoft.AspNetCore.Razor.LanguageServer.Formatting;
using Microsoft.AspNetCore.Razor.LanguageServer.ProjectSystem;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace Microsoft.AspNetCore.Razor.LanguageServer.AutoInsert
{
    internal class OnAutoInsertEndpoint : IVSOnAutoInsertEndpoint
    {
        private readonly ProjectSnapshotManagerDispatcher _projectSnapshotManagerDispatcher;
        private readonly DocumentResolver _documentResolver;
        private readonly AdhocWorkspaceFactory _workspaceFactory;
        private readonly IReadOnlyList<RazorOnAutoInsertProvider> _onAutoInsertProviders;
        private readonly ImmutableHashSet<string> _onAutoInsertTriggerCharacters;

        public OnAutoInsertEndpoint(
            ProjectSnapshotManagerDispatcher projectSnapshotManagerDispatcher,
            DocumentResolver documentResolver,
            IEnumerable<RazorOnAutoInsertProvider> onAutoInsertProvider,
            AdhocWorkspaceFactory workspaceFactory)
        {
            if (projectSnapshotManagerDispatcher is null)
            {
                throw new ArgumentNullException(nameof(projectSnapshotManagerDispatcher));
            }

            if (documentResolver is null)
            {
                throw new ArgumentNullException(nameof(documentResolver));
            }

            if (onAutoInsertProvider is null)
            {
                throw new ArgumentNullException(nameof(onAutoInsertProvider));
            }

            if (workspaceFactory is null)
            {
                throw new ArgumentNullException(nameof(workspaceFactory));
            }

            _projectSnapshotManagerDispatcher = projectSnapshotManagerDispatcher;
            _documentResolver = documentResolver;
            _workspaceFactory = workspaceFactory;
            _onAutoInsertProviders = onAutoInsertProvider.ToList();
            _onAutoInsertTriggerCharacters = _onAutoInsertProviders.Select(provider => provider.TriggerCharacter).ToImmutableHashSet();
        }

        public RegistrationExtensionResult GetRegistration(VSInternalClientCapabilities clientCapabilities)
        {
            const string AssociatedServerCapability = "_vs_onAutoInsertProvider";

            var registrationOptions = new VSInternalDocumentOnAutoInsertOptions()
            {
                TriggerCharacters = _onAutoInsertTriggerCharacters.ToArray(),
            };

            return new RegistrationExtensionResult(AssociatedServerCapability, registrationOptions);
        }

        public async Task<VSInternalDocumentOnAutoInsertResponseItem?> Handle(OnAutoInsertParamsBridge request, CancellationToken cancellationToken)
        {
            var document = await _projectSnapshotManagerDispatcher.RunOnDispatcherThreadAsync(() =>
            {
                _documentResolver.TryResolveDocument(request.TextDocument.Uri.GetAbsoluteOrUNCPath(), out var documentSnapshot);

                return documentSnapshot;
            }, cancellationToken).ConfigureAwait(false);

            if (document is null || cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            var codeDocument = await document.GetGeneratedOutputAsync();
            if (codeDocument.IsUnsupported())
            {
                return null;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var character = request.Character;

            var applicableProviders = new List<RazorOnAutoInsertProvider>();
            for (var i = 0; i < _onAutoInsertProviders.Count; i++)
            {
                var formatOnTypeProvider = _onAutoInsertProviders[i];
                if (formatOnTypeProvider.TriggerCharacter == character)
                {
                    applicableProviders.Add(formatOnTypeProvider);
                }
            }

            if (applicableProviders.Count == 0)
            {
                // There's currently a bug in the LSP platform where other language clients OnAutoInsert trigger characters influence every language clients trigger characters.
                // To combat this we need to pre-emptively return so we don't try having our providers handle characters that they can't.
                return null;
            }

            var uri = request.TextDocument.Uri;
            var position = request.Position;

            using (var formattingContext = FormattingContext.Create(uri, document, codeDocument, request.Options, _workspaceFactory))
            {
                for (var i = 0; i < applicableProviders.Count; i++)
                {
                    if (applicableProviders[i].TryResolveInsertion(position, formattingContext, out var textEdit, out var format))
                    {
                        return new VSInternalDocumentOnAutoInsertResponseItem()
                        {
                            TextEdit = textEdit,
                            TextEditFormat = format,
                        };
                    }
                }
            }

            // No provider could handle the text edit.
            return null;
        }
    }
}
