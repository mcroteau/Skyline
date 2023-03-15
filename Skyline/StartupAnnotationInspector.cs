using System;
using System.Reflection;
using Skyline;
using Skyline.Annotation;

namespace Skyline{
    public class StartupAnnotationInspector {

        ComponentsHolder componentsHolder;

        public StartupAnnotationInspector(ComponentsHolder componentsHolder){
            this.componentsHolder = componentsHolder;
        }

        public ComponentsHolder Inspect(){
            String sourcesDirectory = Directory.GetCurrentDirectory() + 
                Path.DirectorySeparatorChar.ToString() + "Source" + Path.DirectorySeparatorChar.ToString();
            Console.WriteLine(sourcesDirectory);
            InspectFilePath(sourcesDirectory, sourcesDirectory);
            return componentsHolder;
        }

        public void InspectFilePath(String sourcesDirectory, String filePath){

            if(File.Exists(filePath)){
                
                try {

                    Char separator = Path.DirectorySeparatorChar;
                    String[] klassPathParts = filePath.Split(sourcesDirectory);
                    String klassPathSlashesRemoved =  klassPathParts[1].Replace("\\", ".");
                    String klassPathPeriod = klassPathSlashesRemoved.Replace("/", ".");
                    String klassPathBefore = klassPathPeriod.Replace("."+ "class", "");
                    String klassPath = klassPathBefore.Replace(".cs", "");

                    if(filePath.EndsWith(".cs")){
                        Object klassInstance = Activator.CreateInstance("Foo", klassPath).Unwrap();
                        Type klassType = klassInstance.GetType();
                        Object[] attrs = klassType.GetCustomAttributes(typeof(ServerStartup), true);
                        if(attrs.Length > 0) {
                            componentsHolder.setServerStartup(klassInstance);
                        }
                    }

                }catch (Exception ex){
                    Console.WriteLine(ex.ToString());
                }
           
            }

            if(Directory.Exists(filePath)){
                String[] files = Directory.GetFiles(filePath, "*", SearchOption.TopDirectoryOnly);
                foreach(String recursedFile in files){
                    InspectFilePath(sourcesDirectory, recursedFile);
                }
                
                String[] directories = Directory.GetDirectories(filePath, "*", SearchOption.TopDirectoryOnly);
                foreach(String directoryPath in directories){
                    InspectFilePath(sourcesDirectory, directoryPath);
                }
            }
        }
    }
}