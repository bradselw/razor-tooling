﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.Host;
using Xunit;

namespace Microsoft.VisualStudio.Razor.IntegrationTests
{
    public class FormatDocumentTests : AbstractRazorEditorTest
    {
        private static readonly string s_projectPath = TestProject.GetProjectDirectory(typeof(FormatDocumentTests), useCurrentDirectory: true);

        // To add new formatting tests create a sample file of the "before" state
        // and place it in the TestFiles\Input folder.
        // If you know the "after" state then you can place that in a file of the same
        // name in the TestFiles\Expected folder, and run tests as normal.
        // If you want to generate the "after" state simple run the test without
        // creating the expected file, and it will be generated for you.
        //
        // Things that aren't (yet?) supported:
        //   * Formatting must change the input state or the test will hang
        //     ie. these tests cannot be used for pure validation
        //   * Test input is always placed in a .razor file, so .cshtml specific
        //     quirks can't be validated
        //
        // You'll just have to write tests for those ones :P

        [IdeTheory]
        [MemberData(nameof(GetFormattingInputFiles))]
        public async Task FormattingDocument(string inputResourceName, string expectedResourceName)
        {
            if (!TryGetResource(inputResourceName, out var input))
            {
                throw new Exception($"Could not get input resource data for '{inputResourceName}'");
            }

            // Open the file
            await TestServices.SolutionExplorer.OpenFileAsync(BlazorProjectName, CounterRazorFile, HangMitigatingCancellationToken);

            await TestServices.Editor.SetTextAsync(input, HangMitigatingCancellationToken);

            // Act
            await TestServices.Editor.InvokeFormatDocumentAsync(HangMitigatingCancellationToken);

            // Assert
            var actual = await TestServices.Editor.WaitForTextChangeAsync(input, HangMitigatingCancellationToken);

            if (!TryGetResource(expectedResourceName, out var expected))
            {
                // If there was no expected results file, we generate one, but still fail
                // the test so that its impossible to forget to commit the results.
                var path = Path.Combine(s_projectPath, "Formatting", "TestFiles", "Expected");
                var fileName = expectedResourceName.Split(new[] { '.' }, 8).Last();

                File.WriteAllText(Path.Combine(path, fileName), actual);

                throw new Exception("Test did not have expected results file so one has been generated. Running the test again should make it pass.");
            }

            Assert.Equal(expected, actual);
        }

        private static bool TryGetResource(string name, [NotNullWhen(true)] out string? value)
        {
            try
            {
                using var expectedStream = typeof(FormatDocumentTests).Assembly.GetManifestResourceStream(name);
                using var sr = new StreamReader(expectedStream);

                value = sr.ReadToEnd();
            }
            catch
            {
                value = null;
                return false;
            }

            return true;
        }

        private static IEnumerable<object[]> GetFormattingInputFiles()
        {
            var type = typeof(FormatDocumentTests);
            var assembly = type.Assembly;

            var basePath = $"{type.Namespace}.Formatting.TestFiles.Input";

            foreach (var name in assembly.GetManifestResourceNames())
            {
                if (name.StartsWith(basePath))
                {
                    var expectedStreamName = name.Replace(".Input.", ".Expected.");
                    yield return new[] { name, expectedStreamName };
                }
            }
        }
    }
}
