using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Newtonsoft.Json;
using System;
using WellDataExplorer.Models;
using WellDataExplorer.Pages;
using static MudBlazor.Colors;

namespace WellDataExplorer.Layout
{
    public partial class MainLayout
    {
        private string svgContent;
        private DotNetObjectReference<MainLayout>? dotNetHelper;
        List<StateInfo> info;

        [Inject]
        private HttpClient Http { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    string url = @"https://welldataexplorer.blob.core.windows.net/explorerdata/StateInfo.json";
                    HttpResponseMessage response = await Http.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string jsonContent = await response.Content.ReadAsStringAsync();
                    info = JsonConvert.DeserializeObject<List<StateInfo>>(jsonContent);
                    var svgResponse = await Http.GetStringAsync("us.svg");
                    svgContent = HighlightStates(svgResponse, "fill:yellow;stroke:red;stroke-width:2");
                    StateHasChanged();
                    dotNetHelper = DotNetObjectReference.Create(this);
                    await JS.InvokeVoidAsync("registerStateClickEvents", dotNetHelper);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                }
            }
        }

        [JSInvokable]
        public async void LogStateId(string stateId)
        {
            Console.WriteLine($"State code is {stateId}");
            await OpenDialogAsync(stateId);
        }

        private Task OpenDialogAsync(string stateId)
        {
            Console.WriteLine("Open Dialog");
            StateInfo stateInfo = info.SingleOrDefault(s => s.StateId == stateId);
            var parameters = new DialogParameters<StateDialog>
            {
                { x => x.Info, stateInfo }
            };
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
            String dialogTitle = stateInfo.StateFullName;
            return DialogService.ShowAsync<StateDialog>(dialogTitle, parameters, options);
        }

        private string HighlightStates(string svg, string style)
        {
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(svg);

            foreach (var state in info)
            {
                var stateNode = xmlDoc.SelectSingleNode($"//*[@id='{state.StateId}']");
                if (stateNode != null)
                {
                    var styleAttr = stateNode.Attributes["style"];
                    if (styleAttr != null)
                    {
                        styleAttr.Value += ";" + style;
                    }
                    else
                    {
                        var newAttr = xmlDoc.CreateAttribute("style");
                        newAttr.Value = style;
                        stateNode.Attributes.Append(newAttr);
                    }
                }
                else
                {
                    Console.WriteLine("Could not find state");
                }
            }

            using (var stringWriter = new System.IO.StringWriter())
            using (var xmlTextWriter = System.Xml.XmlWriter.Create(stringWriter))
            {
                xmlDoc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }
        }

        public void Dispose()
        {
            dotNetHelper?.Dispose();
        }
    }
}