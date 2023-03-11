using System;

namespace AeonFlux;

public class AeonFlux {

    int port;
    String PROPERTIES;

    ViewConfig viewConfig;
    PropertiesConfig propertiesConfig;
    Integer numberOfPartitions = 3;
    Integer numberOfRequestExecutors = 7;
    String securityAccessKlass;

    Socket listener;

    public AeonFlux(){
        this.port = 1301;
        this.PROPERTIES = "system.properties";
        this.viewConfig = new ViewConfig();
    }

    public AeonFlux(int port){
        this.port = port;
        this.PROPERTIES = "system.properties";
        this.viewConfig = new ViewConfig();
    }

    public void Start(){
        try {

            StartupAnnotationInspector startupAnnotationInspector = new StartupAnnotationInspector(new ComponentsHolder());
            startupAnnotationInspector.inspect();
            ComponentsHolder componentsHolder = startupAnnotationInspector.getComponentsHolder();

            if (propertiesConfig == null) {
                propertiesConfig = new PropertiesConfig();
                propertiesConfig.setPropertiesFile(PROPERTIES);
            }

            String propertiesFile = propertiesConfig.getPropertiesFile();
            RouteAttributesResolver routeAttributesResolver = new RouteAttributesResolver(propertiesFile);
            RouteAttributes routeAttributes = routeAttributesResolver.resolve();
            AnnotationComponent serverStartup = componentsHolder.getServerStartup();

            AeonHelper aeonHelper = new AeonHelper();

            String resourcesDirectory = viewConfig.getResourcesPath();
            Dictionary<String, byte[]> viewBytesMap = aeonHelper.getViewBytesMap(viewConfig);

            Log.info("Running startup routine, please wait...");
            if (serverStartup != null) {
                Method startupMethod = serverStartup.GetType().getMethod("startup");
                Object startupObject = serverStartup.GetType().getConstructor().newInstance();
                startupMethod.Invoke(startupObject);
            }

            Log.info("Registering network request negotiators, please wait...\n");
IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8900);

            try {

                // Create a Socket that will use Tcp protocol
                listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // A Socket must be associated with an endpoint using the Bind method
                listener.Bind(localEndPoint);
                // Specify how many requests a Socket can listen before it gives Server busy response.
                // We will listen 10 requests at a time
                listener.Listen(1);
                
                ThreadPool.SetMinThreads(numberOfRequestExecutors, numberOfRequestExecutors);
                
                for(int partitions = 0; partitions < numberOfPartitions; partitions++){
                    Console.WriteLine("registered request executor..");
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ExecuteRequest));
                }
                
            }catch (Exception e){
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("Ready!");

        }catch(Exception ex){
            ex.printStackTrace();
        }
    }
    public void ExecuteRequest(Object stateInfo){
        
        Console.WriteLine("Hello from the thread pool.");
        Socket handler = listener.Accept();

        // Incoming data from the client.
        string data = null;
        byte[] bytes = null;

        var utf8 = new UTF8Encoding();
        
        while (true){
            bytes = new byte[1024 * 3];
            int bytesRec = handler.Receive(bytes);
            string info = GetBytesToStringConverted(bytes);
            if(bytesRec < bytes.Length)break;
        }

        byte[] resp = utf8.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nhi");
        handler.Send(resp);
        handler.Close();

        ThreadPool.QueueUserWorkItem(new WaitCallback(ExecuteRequest));
    }    

    static string GetBytesToStringConverted(byte[] bytes){
        MemoryStream stream = new MemoryStream(bytes);
        StreamReader streamReader = new StreamReader(stream);
        return streamReader.ReadToEnd();
    }

    public void setPropertiesConfig(PropertiesConfig propertiesConfig){
        this.propertiesConfig = propertiesConfig;
    }

    public void setViewConfig(ViewConfig viewConfig) {
        this.viewConfig = viewConfig;
    }

    public void setSecurityAccess(Class<?> securityAccessKlass) {
        this.securityAccessKlass = securityAccessKlass;
    }

    public void setNumberOfPartitions(int numberOfPartitions){
        this.numberOfPartitions = numberOfPartitions;
    }

    public void setNumberOfRequestExecutors(int numberOfRequestExecutors){
        this.numberOfRequestExecutors = numberOfRequestExecutors;
    }

    public PLSAR addViewRenderer(Class<?> viewRenderer){
        this.viewRenderers.add(viewRenderer);
        return this;
    }

    public PLSAR setPersistenceConfig(PersistenceConfig persistenceConfig) {
        this.persistenceConfig = persistenceConfig;
        return this;
    }

}