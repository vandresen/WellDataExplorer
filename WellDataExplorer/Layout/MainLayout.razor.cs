using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using WellDataExplorer.Models;
using WellDataExplorer.Pages;

namespace WellDataExplorer.Layout
{
    public partial class MainLayout
    {
        private string svgContent;
        private DotNetObjectReference<MainLayout>? dotNetHelper;

        [Inject]
        private HttpClient Http { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                foreach(var l in stateLoaderSource.StateLoaders)
                {

                }
                var svgResponse = await Http.GetStringAsync("us.svg");
                svgContent = HighlightStates(svgResponse, new[] { "TX", "KS" }, "fill:yellow;stroke:red;stroke-width:2");
                StateHasChanged();
                dotNetHelper = DotNetObjectReference.Create(this);
                await JS.InvokeVoidAsync("registerStateClickEvents", dotNetHelper);
            }
        }

        [JSInvokable]
        public async void LogStateId(string stateId)
        {
            Console.WriteLine($"State code is {stateId}");
            string stateFullName = GetStateFullName(stateId);
            StateInfo stateInfo = new StateInfo();
            stateInfo.WellCount = "Number of Wells: " + GetStateWellCount(stateId);
            stateInfo.LoaderSource = "Loader source code: " + GetLoaderSource(stateId); ;
            await OpenDialogAsync(stateFullName, stateInfo);
        }

        private Task OpenDialogAsync(string stateFullName, StateInfo stateInfo)
        {
            Console.WriteLine("Open Dialog");
            var parameters = new DialogParameters<StateDialog>
            {
                { x => x.Info, stateInfo }
            };
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
            String dialogTitle = stateFullName;
            return DialogService.ShowAsync<StateDialog>(dialogTitle, parameters, options);
        }

        private string HighlightStates(string svg, string[] stateIds, string style)
        {
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(svg);

            foreach (var stateId in stateIds)
            {
                var stateNode = xmlDoc.SelectSingleNode($"//*[@id='{stateId}']");
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

        private string GetStateFullName(string stateId)
        {
            string stateFullName = "";
            if (stateDictionary.States.TryGetValue(stateId, out var fullName))
            {
                stateFullName = fullName;
            }
            return stateFullName;
        }

        private string GetLoaderSource(string stateId)
        {
            string loaderSource = "N/A";
            if (stateLoaderSource.StateLoaders.TryGetValue(stateId, out var name))
            {
                loaderSource = name;
            }
            return loaderSource;
        }

        private string GetStateWellCount(string stateId)
        {
            string stateCount = "N/A";
            if (stateWellCount.WellCount.TryGetValue(stateId, out int count))
            {
                stateCount= count.ToString();
            }
            return stateCount;
        }

        public void Dispose()
        {
            dotNetHelper?.Dispose();
        }
    }
}