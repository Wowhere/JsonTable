﻿using System.IO;
using AvaloniaEdit.Document;
using Microsoft.CodeAnalysis.CSharp;
using System;
using ReactiveUI;
using System.Reactive;
using AvaloniaEdit.Document;
using TableJson.Models;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace TableJson.ViewModels
{
    public class MacrosCodeWindowViewModel : ViewModelBase
    {
        private TextDocument _SourceCode = new TextDocument("");
        public TextDocument SourceCode
        {
            get => _SourceCode;
            set => this.RaiseAndSetIfChanged(ref _SourceCode, value);
        }
        private string _SourceCodeRunOutputText = "";
        public string SourceCodeRunOutputText
        {
            get => _SourceCodeRunOutputText;
            set => this.RaiseAndSetIfChanged(ref _SourceCodeRunOutputText, value);
        }
        private string _CompileStatusText = "Status: No code was compiled";
        public string CompileStatusText
        {
            get => _CompileStatusText;
            set => this.RaiseAndSetIfChanged(ref _CompileStatusText, value);
        }
        public void CompileSourceCode()
        {
            var m = new Macros(false);
            var options = new CSharpCompilationOptions((OutputKind)LanguageVersion.Latest);
            var syntaxTree = CSharpSyntaxTree.ParseText(SourceCode.Text);
            var compilation = CSharpCompilation.Create("DynamicAssembly")
            .AddSyntaxTrees(syntaxTree)
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (result.Success == true) CompileStatusText = "Builded without errors";
                m.BinaryExecutable = ms.ToArray();
                if (m.IsSaved)
                {
                    using (var DataSource = new HelpContext())
                    {
                        DataSource.VoiceOperationTable.Attach(m);
                        DataSource.VoiceOperationTable.Update(m);
                        DataSource.SaveChanges();
                    }
                }
                else
                {
                    using (var DataSource = new HelpContext())
                    {
                        m.IsSaved = true;
                        DataSource.VoiceOperationTable.Attach(m);
                        DataSource.VoiceOperationTable.Add(m);
                        DataSource.SaveChanges();
                    }
                }
            }
        }
        public ReactiveCommand<Unit, Unit> CompileSourceCodeCommand { get; }
        public MacrosCodeWindowViewModel() {
            CompileSourceCodeCommand = ReactiveCommand.Create(CompileSourceCode);
        }
    }
}