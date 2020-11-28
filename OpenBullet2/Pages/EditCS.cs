﻿using BlazorMonaco;
using BlazorMonaco.Bridge;
using Microsoft.AspNetCore.Components;
using OpenBullet2.Helpers;
using OpenBullet2.Logging;
using OpenBullet2.Services;
using RuriLib.Helpers.Transpilers;
using RuriLib.Models.Configs;
using System;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class EditCS
    {
        [Inject] NavigationManager Nav { get; set; }
        [Inject] public OBLogger OBLogger { get; set; }
        [Inject] ConfigService ConfigService { get; set; }

        [Parameter] public Config Config { get; set; }
        private MonacoEditor _editor { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Config = ConfigService.SelectedConfig;

            // Transpile if not in CSharp mode
            if (Config != null && Config.Mode != ConfigMode.CSharp)
            {
                try
                {
                    var stack = Config.Mode == ConfigMode.Stack
                    ? Config.Stack
                    : new Loli2StackTranspiler().Transpile(Config.LoliCodeScript);
                    Config.CSharpScript = new Stack2CSharpTranspiler().Transpile(stack, Config.Settings);
                }
                catch (Exception ex)
                {
                    await OBLogger.LogException(ex);
                }
            }
        }

        private StandaloneEditorConstructionOptions EditorConstructionOptions(MonacoEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Minimap = new MinimapOptions { Enabled = false },
                ReadOnly = Config.Mode != ConfigMode.CSharp,
                Theme = "vs-dark",
                Language = "csharp",
                MatchBrackets = true,
                Value = Config.CSharpScript
            };
        }

        /*
        private async Task Transpile()
        {
            var stack = new Loli2StackTranspiler().Transpile(Config.LoliCodeScript);
            Config.CSharpScript = new Stack2CSharpTranspiler().Transpile(stack, Config.Settings);
            await _editor.SetValue(Config.CSharpScript);
        }
        */

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
                _editor.SetValue(Config.CSharpScript);
        }

        private async Task ConvertConfig()
        {
            var confirmed = await js.Confirm(Loc["WarningPleaseRead"], Loc["ConfirmConfigConversion"], Loc["Cancel"]);
            
            if (!confirmed)
                return;

            Config.ChangeMode(ConfigMode.CSharp);
            ConfigService.SelectedConfig = Config;
            Nav.NavigateTo("config/edit/code", true);
        }

        private async Task SaveScript()
        {
            Config.CSharpScript = await _editor.GetValue();
        }
    }
}
