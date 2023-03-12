using System;
using Skyline;

namespace Skyline{
    public class StartupAnnotationInspector {

    String sourceDirectory;
    ComponentsHolder componentsHolder;

    public StartupAnnotationInspector(String sourceDirectory, ComponentsHolder componentsHolder){
        this.sourceDirectory = sourceDirectory;
        this.componentsHolder = componentsHolder;
    }

    public void Inspect(){
        inspectFilePath(sourceDirectory);
    }

    public void InspectFilePath(String filePath){
        
        String[] files = Directory.GetFiles(filePath);
        foreach(String file in files){

            if(Directory.Exists(file)){
                InspectFilePath(file);
                continue;
            }

            try {

                if(!file.endsWith(".cs"))continue;

                String separator = Path.PathSeparator;
                String[] klassPathParts = file.getPath().Split(separator);
                String klassPathSlashesRemoved =  klassPathParts[1].Replace("\\", ".");
                String klassPathPeriod = klassPathSlashesRemoved.Replace("/", ".");
                String klassPathBefore = klassPathPeriod.Replace("."+ "class", "");

                String klassPath = klassPathBefore.Replace("cs.", "");

                Console.WriteLine("1" + klassPath);
                Type klass = typeof(klassPath);

                if (klass.IsInterface) continue;

                if(klass.GetCustomAttributes(ServerStartup)) {
                    componentsHolder.setServerStartup(klass);
                }

            }catch (Exception ex){
                ex.printStackTrace();
            }
        }
    }

    public ComponentsHolder getComponentsHolder() {
        return this.componentsHolder;
    }
}

}