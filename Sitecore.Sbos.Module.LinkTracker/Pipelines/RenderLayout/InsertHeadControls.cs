﻿using Sitecore.Mvc.Pipelines.Response.RenderRendering;

namespace Sitecore.Sbos.Module.LinkTracker.Pipelines.RenderLayout
{
    public class InsertHeadControls : RenderRenderingProcessor
    {
        public bool RenderJQuery { get; set; }

        public override void Process(RenderRenderingArgs args)
        {
            if (Context.Site.Name == "shell")
            {
                return;
            }

            if (!args.Writer.ToString().Contains(Data.Constants.LinkTrackerConstants.LinkTrackerMgdJSPath))
            {
                if (args.Writer.ToString().Contains("<title>"))
                {
                    if(RenderJQuery)
                        args.Writer.WriteLine(Data.Constants.LinkTrackerConstants.JQueryScript);

                    args.Writer.WriteLine(Data.Constants.LinkTrackerConstants.LinkTrackerMgrScript);
                }
            }
        }
    }
}