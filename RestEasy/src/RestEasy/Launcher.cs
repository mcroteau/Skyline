
using Skyline;

namespace RestEasy {
    public class Launcher{
        public static int Main(String[] args){
            SkylineServer server = new SkylineServer(4000, 10);
            server.start();
            return 0;
        }
    }
}