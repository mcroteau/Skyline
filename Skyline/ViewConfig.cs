using System;
using Skyline.Schemes;

namespace Skyline{
    public class ViewConfig{
        public ViewConfig(){
            this.viewsPath = "Webapp";
            this.resourcesPath = "assets";
            this.viewExtension = ".asp";
            this.renderingScheme = RenderingScheme.RELOAD_EACH_REQUEST;
        }

        String viewsPath;
        String resourcesPath;
        String viewExtension;
        String renderingScheme;

        public String getViewsPath()
        {
            return this.viewsPath;
        }

        public void setViewsPath(String viewsPath)
        {
            this.viewsPath = viewsPath;
        }

        public String getResourcesPath()
        {
            return this.resourcesPath;
        }

        public void setResourcesPath(String resourcesPath)
        {
            this.resourcesPath = resourcesPath;
        }

        public String getViewExtension()
        {
            return this.viewExtension;
        }

        public void setViewExtension(String viewExtension)
        {
            this.viewExtension = viewExtension;
        }

        public String getRenderingScheme()
        {
            return this.renderingScheme;
        }

        public void setRenderingScheme(String renderingScheme)
        {
            this.renderingScheme = renderingScheme;
        }
    }
}
