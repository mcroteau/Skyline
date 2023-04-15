using System;
using System.Reflection;
using System.Text.RegularExpressions;

using Skyline;
using Skyline.Annotation;

namespace Skyline{
    public class StartupAnnotationResolver {

        ComponentsHolder componentsHolder;

        public StartupAnnotationResolver(ComponentsHolder componentsHolder){
            this.componentsHolder = componentsHolder;
        }

        public ComponentsHolder resolve(){
            String sourcesDirectory = Directory.GetCurrentDirectory() + 
            Path.DirectorySeparatorChar.ToString();

            InspectFilePath(sourcesDirectory, sourcesDirectory);
            return componentsHolder;
        }

        public void InspectFilePath(String sourcesDirectory, String filePath){

            if(File.Exists(filePath)){
                
                try {

                    if(filePath.EndsWith(".cs") && !filePath.Contains("bin") && !filePath.Contains("obj")){

                        Char separator = Path.DirectorySeparatorChar;;
                        String assembly = Assembly.GetEntryAssembly().GetName().Name;

                        int directoryIndex = filePath.IndexOf(assembly);
                        int directoryIndexWith = directoryIndex + 1;
                        int nextSeparatorIndex = filePath.IndexOf(separator, directoryIndexWith);
                        int directoryDiff = nextSeparatorIndex - directoryIndex;
                        
                        String directoryInfoBefore = filePath.Substring(directoryIndex, directoryDiff);
                        String directoryInfo = directoryInfoBefore.Replace(separator.ToString(), ".");
                        String directoryInfoFinal = directoryInfo.Replace(".", "");

                        int endDiff = filePath.Length - directoryIndex;
                        
                        String klassInfoBefore = filePath.Substring(directoryIndex, endDiff);
                        String klassInfo = klassInfoBefore.Replace(separator.ToString(), ".");
                        String klassDependencyBefore = klassInfo.Replace(".cs", "");
                        String klassDependency = klassDependencyBefore.Replace(directoryInfoFinal, "");
                        
                        var regex = new Regex(Regex.Escape("."));
                        var dependencyInfo = regex.Replace(klassDependency, "", 1);

                        Object klassInstance = Activator.CreateInstance(assembly, dependencyInfo).Unwrap();
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