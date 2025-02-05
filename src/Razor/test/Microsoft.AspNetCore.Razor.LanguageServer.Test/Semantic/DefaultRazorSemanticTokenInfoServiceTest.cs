﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.LanguageServer.Common;
using Microsoft.AspNetCore.Razor.LanguageServer.Formatting;
using Microsoft.AspNetCore.Razor.LanguageServer.ProjectSystem;
using Microsoft.AspNetCore.Razor.LanguageServer.Semantic;
using Microsoft.AspNetCore.Razor.Test.Common;
using Microsoft.CodeAnalysis.Razor.ProjectSystem;
using Moq;
using OmniSharp.Extensions.JsonRpc;
using Xunit;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Range = Microsoft.VisualStudio.LanguageServer.Protocol.Range;

namespace Microsoft.AspNetCore.Razor.LanguageServer.Test.Semantic
{
    // Sets the FileName static variable.
    // Finds the test method name using reflection, and uses
    // that to find the expected input/output test files as Embedded resources.
    [IntializeTestFile]
    [UseExportProvider]
    public class DefaultRazorSemanticTokenInfoServiceTest : SemanticTokenTestBase
    {
        #region CSharp
        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_CSharp_RazorIfNotReady()
        {
            var documentText = @"<p></p>@{
    var d = ""t"";
}
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 3, Character = 0 }
            };

            var csharpTokens = new ProvideSemanticTokensResponse(tokens: Array.Empty<int>(), hostDocumentSyncVersion: 1);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens, documentVersion: 1);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_CSharpBlock_HTML()
        {
            var documentText = @"@{
    var d = ""t"";
    <p>HTML @d</p>
}
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 4, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_CSharp_Nested_HTML()
        {
            var documentText = @"@addTagHelper *, TestAssembly
<!--@{var d = ""string"";@<a></a>}-->
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_CSharp_VSCodeWorks()
        {
            var documentText = @"@addTagHelper *, TestAssembly
@{ var d = }
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = new ProvideSemanticTokensResponse(tokens: Array.Empty<int>(), hostDocumentSyncVersion: null);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens, documentVersion: 1);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_CSharp_Explicit()
        {
            var documentText = @$"@addTagHelper *, TestAssembly
@(DateTime.Now)
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_CSharp_Implicit()
        {
            var documentText = @"@addTagHelper *, TestAssembly
@{ var d = ""txt"";}
@d
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 3, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_CSharp_VersionMismatch()
        {
            var documentText = @"@addTagHelper *, TestAssembly
@{ var d = }
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens, documentVersion: 21);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_CSharp_FunctionAsync()
        {
            var documentText = @"@addTagHelper *, TestAssembly
@{ var d = }
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_CSharp_StaticModifier()
        {
            var documentText = @"@code
{
    static int x = 1;
}
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 4, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: true);
            await AssertSemanticTokensAsync(documentText, isRazorFile: true, razorRange, csharpTokens: csharpTokens);
        }
        #endregion

        #region HTML
        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_MultipleBlankLines()
        {
            var documentText = @"@addTagHelper *, TestAssembly

<p>first
second</p>
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 4, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_IncompleteTag()
        {
            var documentText = @"<str class='
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 1, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_MinimizedHTMLAttribute()
        {
            var documentText = @"<p attr />
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 1, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_MinimizedHTMLAsync()
        {
            var documentText = @"@addTagHelper *, TestAssembly
<input/>
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_HTMLCommentAsync()
        {
            var documentText = @"@addTagHelper *, TestAssembly
<!-- comment with comma's -->
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_PartialHTMLCommentAsync()
        {
            var documentText = @"@addTagHelper *, TestAssembly
<!-- comment
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: true);
            await AssertSemanticTokensAsync(documentText, isRazorFile: true, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_HTMLIncludesBang()
        {
            var documentText = @"@addTagHelper *, TestAssembly
<!input/>
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }
        #endregion

        #region TagHelpers
        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_HalfOfCommentAsync()
        {
            var documentText = @"@addTagHelper *, TestAssembly
@* comment
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: true);
            await AssertSemanticTokensAsync(documentText, isRazorFile: true, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_NoAttributesAsync()
        {
            var documentText = @"@addTagHelper *, TestAssembly
<test1></test1>
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_WithAttributeAsync()
        {
            var documentText = @"@addTagHelper *, TestAssembly
<test1 bool-val='true'></test1>
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_MinimizedAttribute_BoundAsync()
        {
            var documentText = @"@addTagHelper *, TestAssembly
<test1 bool-val></test1>
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_MinimizedAttribute_NotBoundAsync()
        {
            var documentText = @"@addTagHelper *, TestAssembly
<test1 notbound></test1>
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_IgnoresNonTagHelperAttributesAsync()
        {
            var documentText = @"@addTagHelper *, TestAssembly
<test1 bool-val='true' class='display:none'></test1>
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_TagHelpersNotAvailableInRazorAsync()
        {
            var documentText = @"@addTagHelper *, TestAssembly
<test1 bool-val='true' class='display:none'></test1>
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: true);
            await AssertSemanticTokensAsync(documentText, isRazorFile: true, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_DoesNotApplyOnNonTagHelpersAsync()
        {
            var documentText = @"@addTagHelper *, TestAssembly
<p bool-val='true'></p>
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }
        #endregion TagHelpers

        #region DirectiveAttributes
        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_Razor_MinimizedDirectiveAttributeParameters()
        {
            // Capitalized, non-well-known-HTML elements should not be marked as TagHelpers
            var documentText = @"@addTagHelper *, TestAssembly
}<NotATagHelp @minimized:something />
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: true);
            await AssertSemanticTokensAsync(documentText, isRazorFile: true, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_Razor_ComponentAttributeAsync()
        {
            var documentText = @"@addTagHelper *, TestAssembly
<Component1 bool-val=""true""></Component1>
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: true);
            await AssertSemanticTokensAsync(documentText, isRazorFile: true, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_Razor_DirectiveAttributesParametersAsync()
        {
            var documentText = @"@addTagHelper *, TestAssembly
<Component1 @test:something='Function'></Component1>
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: true);
            await AssertSemanticTokensAsync(documentText, isRazorFile: true, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_Razor_NonComponentsDoNotShowInRazorAsync()
        {
            var documentText = @"@addTagHelper *, TestAssembly
<test1 bool-val='true'></test1>
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: true);
            await AssertSemanticTokensAsync(documentText, isRazorFile: true, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_Razor_DirectivesAsync()
        {
            var documentText = @"@addTagHelper *, TestAssembly
<Component1 @test='Function'></Component1>
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: true);
            await AssertSemanticTokensAsync(documentText, isRazorFile: true, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_HandleTransitionEscape()
        {
            var documentText = @"@@text
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 1, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: true);
            await AssertSemanticTokensAsync(documentText, isRazorFile: true, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_Razor_DoNotColorNonTagHelpersAsync()
        {
            var documentText = @"
<p @test='Function'></p>
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: true);
            await AssertSemanticTokensAsync(documentText, isRazorFile: true, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_Razor_DoesNotApplyOnNonTagHelpersAsync()
        {
            var documentText = @"@addTagHelpers *, TestAssembly
<p></p>
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: true);
            await AssertSemanticTokensAsync(documentText, isRazorFile: true, razorRange, csharpTokens: csharpTokens);
        }
        #endregion DirectiveAttributes

        #region Directive
        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_Razor_CodeDirectiveAsync()
        {
            var documentText = @"@code {}
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 1, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: true);
            await AssertSemanticTokensAsync(documentText, isRazorFile: true, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_Razor_CodeDirectiveBodyAsync()
        {
            var documentText = @"@code {
    public void SomeMethod()
    {
@DateTime.Now
    }
}
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 6, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: true);
            await AssertSemanticTokensAsync(documentText, isRazorFile: true, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_Razor_UsingDirective()
        {
            var documentText = @"@using Microsoft.AspNetCore.Razor
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 1, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: true);
            await AssertSemanticTokensAsync(documentText, isRazorFile: true, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_Razor_FunctionsDirectiveAsync()
        {
            var documentText = @"@functions {}
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 1, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: true);
            await AssertSemanticTokensAsync(documentText, isRazorFile: true, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_Razor_NestedTextDirectives()
        {
            var documentText = @"@functions {
                private void BidsByShipment(string generatedId, int bids)
                {
                    if (bids > 0)
                    {
                        <a class=""Thing"">
                            @if(bids > 0)
                            {
                                <text>@DateTime.Now</text>
                            }
                        </a>
                    }
                }
            }
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 14, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_Razor_NestedTransitions()
        {
            var documentText = @"@functions {
                Action<object> abc = @<span></span>;
            }
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 3, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: true);
            await AssertSemanticTokensAsync(documentText, isRazorFile: true, razorRange, csharpTokens: csharpTokens);
        }
        #endregion

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_Razor_CommentAsync()
        {
            var documentText = @"@* A comment *@
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 1, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: true);
            await AssertSemanticTokensAsync(documentText, isRazorFile: true, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_Razor_MultiLineCommentMidlineAsync()
        {
            var documentText = @"<a />@* kdl
   skd
slf*@
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }

        [Fact(Skip = "https://github.com/dotnet/razor-tooling/issues/6395")]
        public async Task GetSemanticTokens_Razor_MultiLineCommentAsync()
        {
            var documentText = @$"@*stuff
things *@
";

            var razorRange = new Range
            {
                Start = new Position { Line = 0, Character = 0 },
                End = new Position { Line = 2, Character = 0 }
            };

            var csharpTokens = await GetCSharpSemanticTokensResponseAsync(documentText, razorRange, isRazorFile: false);
            await AssertSemanticTokensAsync(documentText, isRazorFile: false, razorRange, csharpTokens: csharpTokens);
        }

        private async Task AssertSemanticTokensAsync(
            string documentText,
            bool isRazorFile,
            Range range,
            RazorSemanticTokensInfoService? service = null,
            ProvideSemanticTokensResponse? csharpTokens = null,
            int? documentVersion = 0)
        {
            await AssertSemanticTokensAsync(new string[] { documentText }, new bool[] { isRazorFile }, range, service, csharpTokens, documentVersion);
        }

        private async Task AssertSemanticTokensAsync(
            string[] documentTexts,
            bool[] isRazorArray,
            Range range,
            RazorSemanticTokensInfoService? service = null,
            ProvideSemanticTokensResponse? csharpTokens = null,
            int? documentVersion = 0)
        {
            // Arrange
            if (documentVersion == 0 && csharpTokens != null)
            {
                documentVersion = (int?)csharpTokens.HostDocumentSyncVersion;
            }

            if (csharpTokens is null)
            {
                csharpTokens = new ProvideSemanticTokensResponse(tokens: null, documentVersion);
            }

            var (documentSnapshots, textDocumentIdentifiers) = CreateDocumentSnapshot(documentTexts, isRazorArray, DefaultTagHelpers);

            if (service is null)
            {
                service = GetDefaultRazorSemanticTokenInfoService(documentSnapshots, csharpTokens, documentVersion);
            }

            var textDocumentIdentifier = textDocumentIdentifiers.Dequeue();

            // Act
            var tokens = await service.GetSemanticTokensAsync(textDocumentIdentifier, range, CancellationToken.None);

            // Assert
            AssertSemanticTokensMatchesBaseline(tokens?.Data);
        }

        private RazorSemanticTokensInfoService GetDefaultRazorSemanticTokenInfoService(
            Queue<DocumentSnapshot> documentSnapshots,
            ProvideSemanticTokensResponse? csharpTokens = null,
            int? documentVersion = 0)
        {
            var responseRouterReturns = new Mock<IResponseRouterReturns>(MockBehavior.Strict);
            responseRouterReturns
                .Setup(l => l.Returning<ProvideSemanticTokensResponse?>(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(csharpTokens));

            var languageServer = new Mock<ClientNotifierServiceBase>(MockBehavior.Strict);
            languageServer
                .Setup(l => l.SendRequestAsync(LanguageServerConstants.RazorProvideSemanticTokensRangeEndpoint, It.IsAny<SemanticTokensParams>()))
                .Returns(Task.FromResult(responseRouterReturns.Object));

            var documentMappingService = new DefaultRazorDocumentMappingService(TestLoggerFactory.Instance);
            var loggingFactory = TestLoggerFactory.Instance;
            var projectSnapshotManagerDispatcher = Dispatcher;
            var documentResolver = new TestDocumentResolver(documentSnapshots);

            var documentVersionCache = new Mock<DocumentVersionCache>(MockBehavior.Strict);
            documentVersionCache.Setup(c => c.TryGetDocumentVersion(It.IsAny<DocumentSnapshot>(), out documentVersion))
                .Returns(true);

            var testClient = new TestClient();
            var errorReporter = new LanguageServerErrorReporter(loggingFactory);
            var semanticTokensRefreshPublisher = new DefaultWorkspaceSemanticTokensRefreshPublisher(testClient, errorReporter);

            return new DefaultRazorSemanticTokensInfoService(
                languageServer.Object,
                documentMappingService,
                projectSnapshotManagerDispatcher,
                documentResolver,
                documentVersionCache.Object,
                loggingFactory);
        }

        private class TestDocumentResolver : DocumentResolver
        {
            private readonly Queue<DocumentSnapshot> _documentSnapshots;

            public TestDocumentResolver(Queue<DocumentSnapshot> documentSnapshots)
            {
                _documentSnapshots = documentSnapshots;
            }

            public override bool TryResolveDocument(string documentFilePath, out DocumentSnapshot document)
            {
                document = _documentSnapshots.Count == 1 ? _documentSnapshots.Peek() : _documentSnapshots.Dequeue();

                return true;
            }
        }
    }
}
