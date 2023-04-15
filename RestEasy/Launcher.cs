
using Skyline;

public class Launcher{
    public static int Main(String[] args){
        SkylineServer server = new SkylineServer();
        server.setPorts(new Int32[]{2000, 3000, 4000});
        server.start();
        return 0;
    }
}