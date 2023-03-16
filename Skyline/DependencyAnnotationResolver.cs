
using System;

using Skyline.Annotation;
using Skyline.Model;

namespace Skyline { 
    public class ComponentAnnotationResolver {

        ComponentsHolder componentsHolder;
        ApplicationAttributes applicationAttributes;

        public ComponentAnnotationResolver(ComponentsHolder componentsHolder){
            this.componentsHolder = componentsHolder;
        }

        public ComponentsHolder resolve(){
            String sourcesDirectory = Directory.GetCurrentDirectory() + 
                Path.DirectorySeparatorChar.ToString() + "Source" + Path.DirectorySeparatorChar.ToString();
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
                    int separatorIndex = klassPathBefore.IndexOf(".");
                    String assemblyCopy = klassPathBefore;
                    String assembly = assemblyCopy.Substring(0, separatorIndex);
                    String klassPath = klassPathBefore.Replace(".cs", "");

                    if(filePath.EndsWith(".cs")){
                        Object klassInstanceValidate = Activator.CreateInstance(assembly, klassPath).Unwrap();
                        Object[] attrs = klassInstanceValidate.GetType().GetCustomAttributes(typeof(Repository), true);
                        if(attrs.Length > 0) {
                            Object klassInstance = Activator.CreateInstance(klassInstanceValidate.GetType(), new Object[]{applicationAttributes}, new Object[]{});
                            String[] componentElements = klassInstance.GetType().Name.ToString().Split(".");
                            String dependencyKey = componentElements[componentElements.Length -1].ToLower();
                            componentsHolder.getRepositories().Add(dependencyKey, klassInstance);
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

        public void setApplicationAttributes(ApplicationAttributes applicationAttributes){
            this.applicationAttributes = applicationAttributes;
        }
    }
}