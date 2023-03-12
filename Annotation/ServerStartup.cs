using System;

namespace Skyline.Annotation {
    
    [System.AttributeUsage(System.AttributeTargets.Class |  
                        System.AttributeTargets.Struct)]  
    public class ServerStartupAttribute : System.Attribute {} 
}