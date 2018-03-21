using Sitecore.Pipelines.RenderLayout;
using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Sitecore.Sbos.Module.LinkTracker.Pipelines.RenderLayout
{
    public class InsertHeadControlsWebForms
    {
        public bool RenderJQuery { get; set; }

        public void Process(RenderLayoutArgs args)
        {
            // no need to run our code in the shell
            if (Sitecore.Context.Site.Name == "shell")
                return;

            Control head = WebUtil.FindControlOfType(Sitecore.Context.Page.Page, typeof(HtmlHead));
            if (head != null)
            {
                if(RenderJQuery)
                    head.Controls.Add(new LiteralControl(Data.Constants.LinkTrackerConstants.JQueryScript));

                head.Controls.Add(new LiteralControl(Data.Constants.LinkTrackerConstants.LinkTrackerMgrScript));
            }
        }
    }
}